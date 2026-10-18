using System;
using Brix.Audio;
using Brix.Game.AI.Behaviors;
using Brix.Game.Locomotion;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class AnimationNodes
{
	public static StaticNode Once(Node actionNode, Working.WorkStates workType, Transform target, Labor worker)
	{
		return Work(workType, actionNode, target, worker, loop: false);
	}

	public static StaticNode Loop(Node actionNode, Working.WorkStates workType, Transform target, Labor worker)
	{
		return Work(workType, actionNode, target, worker, loop: true);
	}

	public static StaticNode Loop(Working.WorkStates workType, Transform target, Labor worker)
	{
		return Work(workType, Node.Empty, target, worker, loop: true);
	}

	private static StaticNode Work(Working.WorkStates state, Node actionNode, Transform target, Labor worker, bool loop)
	{
		return Node.Sequence.Label("Workbench Anim").Do(delegate(Labor _worker, Working.WorkStates _state)
		{
			_worker.toolbag.EquipMagicalTool(Working.RecommendedTools[_state]);
		}, worker, state).Label("WieldHammer")
			.Do((Labor _worker) => UtilityNodes.WaitForTimeOrCondition((Labor __worker) => __worker.navigation.Locomotion.IsInPlace(), _worker, 2f, failOnDelay: true), worker)
			.Label("Wait for Idle")
			.Do(delegate(Labor _labor, Working.WorkStates _state, Transform _target)
			{
				_labor.navigation.Locomotion.DoWork(_state, Location.Of(_target));
			}, worker, state, target)
			.Label("StarAnim")
			.Do(Action(actionNode, worker, loop))
			.Finally(delegate(Labor w)
			{
				w.navigation.Locomotion.StopWork();
			}, worker);
	}

	private static StaticNode ActionOnce(Node actionNode, Labor worker)
	{
		return Node.Sequence.Do((Func<Labor, Node>)WaitForHit, worker).Label("wait for hit").Do(delegate(Labor _worker)
		{
			SendWorkIntensityAudioEvent(_worker);
		}, worker)
			.Label("Send Audio event")
			.Do(actionNode as StaticNode)
			.Label("Hit Reaction");
	}

	private static StaticNode Action(Node actionNode, Labor worker, bool loop)
	{
		StaticNode staticNode = ActionOnce(actionNode, worker);
		if (loop)
		{
			staticNode = staticNode.Loop();
		}
		return staticNode;
	}

	private static Node WaitForHit(Labor worker)
	{
		StaticNode n = Node.WaitForCondition(worker.animationEventBoard.OnEvent, (Brix.Game.AI.Behaviors.AnimationEvent ev, Labor l) => l != null && ev == l.interaction.ToolHitEvent, worker);
		Sequencer.Builder n2 = Node.Sequence.Do(Node.WaitFor.Milliseconds(2000)).Do(Node.Fail(SharedWorkException<Exceptions.NotReady>.Get));
		return (StaticNode)Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(n).Do(n2);
	}

	private static void SendWorkIntensityAudioEvent(Labor worker)
	{
		GameplayAudio.AddWorkIntensity(worker.transform.position, 0.5f);
	}
}
