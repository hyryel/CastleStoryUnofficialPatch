using System;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Motus.Behavior;
using Motus.Behavior.Extensions;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class WorkNode
{
	private static void SetupDigGoal(DigGoal goal)
	{
		goal.DestroyIfAbandonned = true;
		goal.Highlight = true;
		goal.RebuildNode();
	}

	public static Node CutTree(this Labor labor, Location location)
	{
		StaticNode toNode = Node.Sequence.Do((Func<Labor, WoodCutting, Node>)ToolNode.GoEquipBestToolOfType, labor, Adjectif.woodCutting).Do((Labor _labor, Factory.AssetKey _goal, Vector3 _pos, Quaternion _rot) => _labor.GetComponent<Transactor>().SpawnNode<TreeGoal>(_goal, _labor.faction, _pos, _rot, new SpawnPostProcessor[0]), labor, Test.TreeGoal, location.Position, Quaternion.identity).Process((TreeGoal goal) => Wait.ForLateSignal(goal.GoalStarted))
			.Process((TreeGoal goal, Labor l) => goal.GetWorkNode(l), labor)
			.ToNode;
		return toNode.FailUnless(SharedWorkException<Exceptions.CharacterCantCutTrees>.Get).Condition((Labor l) => l.Profession.CanChopTrees(), labor).FailWhen(SharedWorkException<Exceptions.InventoryIsFull>.Get)
			.Condition((Labor l) => l.recepteur.RequiresBothHands(), labor);
	}

	public static Node Dig(this Labor labor, Location toDig)
	{
		if (labor.recepteur.RequiresBothHands())
		{
			return Node.FailWork<Exceptions.CharacterCantDig>();
		}
		StaticNode n = Node.Sequence.Do((Factory.AssetKey _key, Vector3 _pos, Quaternion _rot) => ObjectPoolSingleton.Request_Unsafe(_key, _pos, _rot).GetComponent<DigGoal>(), Test.DigGoal, toDig.Position, Quaternion.identity).Process((DigGoal goal) => Wait.ForLateSignal(goal.GoalStarted)).Process(delegate(DigGoal goal)
		{
			SetupDigGoal(goal);
		})
			.Process((DigGoal goal, Labor _labor) => _labor.hackedDigGoal = goal, labor)
			.Process((DigGoal goal, Labor _labor) => goal.GetWorkNode(_labor), labor)
			.Do(Node.WaitForTicks(20));
		return n.FailUnless(SharedWorkException<Exceptions.CharacterCantDig>.Get).Condition((Labor l) => l.Profession.CanBreakVoxels(), labor);
	}

	private static void SetupRepairGoal(RepairGoal goal)
	{
		goal.DestroyIfAbandoned = true;
		goal.WithHighlight = true;
		goal.RebuildNode();
	}

	public static Node Repair(this Labor labor, Location toRepair)
	{
		StaticNode n = Node.Sequence.Do((Location _toRepair) => ObjectPoolSingleton.Request_Unsafe(Test.RepairGoal, _toRepair.Position, Quaternion.identity).GetComponent<RepairGoal>(), toRepair).Process((RepairGoal goal) => Wait.ForLateSignal(goal.GoalStarted)).Process(delegate(RepairGoal goal)
		{
			SetupRepairGoal(goal);
		})
			.Process((RepairGoal goal, Labor _labor) => goal.GetWorkNode(_labor), labor)
			.Do(Node.WaitForTicks(20));
		return n.FailUnless(SharedWorkException<Exceptions.CharacterCantRepair>.Get).Condition((Labor l) => l.Profession.VoxelRepair > 0, labor);
	}

	public static Node Detonate(this Labor labor, GameComponent gameComponent)
	{
		WorkNodeGenerator<GameComponent> workNodeGenerator = new WorkNodeGenerator<GameComponent>();
		workNodeGenerator.WorkAction = (GameComponent l, Labor o) => (StaticNode)Node.Sequence.Do(delegate(GameComponent g)
		{
			g.GetComponent<Explosifs>().CallRpcStartFuse();
		}, gameComponent);
		workNodeGenerator.WorkDoneCondition = (GameComponent component, Labor l) => component.IsNullOrReleased();
		workNodeGenerator.Animation = Working.WorkStates.Hammer;
		workNodeGenerator.NodeWrappers.Add(WorkNodesWrappers.HighlightTargetRed);
		workNodeGenerator.OnWorkEnded = (GameComponent l, Labor o) => (StaticNode)Node.Sequence.Do(delegate(Labor _labor, int _time)
		{
			_labor.SetTemporaryThought(_time, Thought.Reactions.Run);
		}, labor, 2);
		workNodeGenerator.LoopAction = false;
		workNodeGenerator.ReleaseWhen = ReleaseNodeWhen.Dont;
		return workNodeGenerator.BuildNode(gameComponent, labor);
	}
}
