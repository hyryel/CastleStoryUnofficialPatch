using System;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using Motus.Behavior;
using Motus.Behavior.Extensions;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class HarvestNode
{
	private static Factory.AssetKey LogsTransformInto = Blocks.Wood_SinglePlank;

	private static int RessourcePerLogs = 4;

	public static Node CleanUpAnythingAround(Labor labor, Location location)
	{
		return _CleanUpAround(labor, location);
	}

	public static Node CleanUpSimilarObjectsAround(Labor labor, GameObject objetToClean)
	{
		return _CleanUpAround(labor, Location.LeadingTo(objetToClean), objetToClean);
	}

	private static Node _CleanUpAround(Labor labor, Location location, GameObject objetToClean = null)
	{
		if ((bool)objetToClean)
		{
			location = Location.LeadingTo(objetToClean.transform.position);
		}
		return Node.Parallel.Do(Node.Selector.When((Labor worker, Location area) => Vector3.SqrMagnitude(worker.transform.position - area.Position) <= 9f, labor, location).Do(delegate(Labor _labor, GameObject toClean)
		{
			_labor.eater.EatOnlySimilarsTo(toClean);
		}, labor, objetToClean).Else()
			.Do(Node.Forever)).Do(Node.Sequence.Do((Func<Labor, Location, StaticNode>)NavigationNodes.RunToExactly, labor, location).Do(Node.WaitFor.Ticks(20))).Finally(delegate(Labor l)
		{
			l.eater.Starve();
		}, labor)
			.ToNode;
	}

	private static void SetupNatureGoal(NatureHarvestGoal goal)
	{
		goal.DestroyIfAbandonned = true;
		goal.Highlight = true;
		goal.RequireStorage = false;
		goal.RebuildNode();
	}

	public static Node HarvestNatureAt(Labor labor, Vector3 location)
	{
		NatureData nData = NatureRegistry.NatureAt(XYZ.FromVector3(location));
		if (!nData.IsValid)
		{
			return Node.Fail(SharedWorkException<Exceptions.NullTarget>.Get);
		}
		StaticNode toNode = Node.Sequence.Do((NatureData _nData) => NatureHarvestGoal.Request(nData, null), nData).Process((NatureHarvestGoal g) => Wait.ForLateSignal(g.GoalStarted)).Process(delegate(NatureHarvestGoal g)
		{
			SetupNatureGoal(g);
		})
			.Process((NatureHarvestGoal g, Labor _labor) => g.GetWorkNode(_labor), labor)
			.ToNode;
		return AppendCorrectFailToHarvestNode(toNode, nData, labor);
	}

	public static Node AppendCorrectFailToHarvestNode(Node node, NatureData nature, Labor labor)
	{
		if (nature is PlantData)
		{
			return node.FailUnless(SharedWorkException<Exceptions.CharacterCantHarvestPlant>.Get).Condition((Labor l) => l.Profession.CanCollectPlants(), labor);
		}
		return node.FailUnless(SharedWorkException<Exceptions.CharacterCantHarvestMinerals>.Get).Condition((Labor l) => l.Profession.CanHarvestMinerals(), labor);
	}

	private static void SetupTransformGoal(TransformGoal goal, GameObject objectToTransform, Factory.AssetKey transformInto, int Qty)
	{
		goal.DestroyIfAbandonned = true;
		goal.Highlight = true;
		goal.RebuildNode(objectToTransform, transformInto, Qty);
	}

	public static Node TransformLog(Labor labor, GameObject log)
	{
		StaticNode toNode = Node.Sequence.Do((GameObject _log) => ObjectPoolSingleton.Request_Unsafe(Test.TransformGoal, _log.transform.position, Quaternion.identity).GetComponent<TransformGoal>(), log).Process((TransformGoal goal) => Wait.ForLateSignal(goal.GoalStarted)).Process(delegate(TransformGoal goal, GameObject _log)
		{
			SetupTransformGoal(goal, _log, LogsTransformInto, RessourcePerLogs);
		}, log)
			.Process((TransformGoal goal, Labor _labor) => goal.GetWorkNode(_labor), labor)
			.Do(Node.WaitForTicks(20))
			.ToNode;
		return toNode.FailUnless(SharedWorkException<Exceptions.CharacterCantChopWood>.Get).Condition((Labor l) => l.Profession.CanTranformLog(), labor);
	}
}
