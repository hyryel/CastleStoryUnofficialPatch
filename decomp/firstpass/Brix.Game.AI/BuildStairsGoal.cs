using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Lifecycle.Pooling;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class BuildStairsGoal : Goal
{
	public Signal FinishedSignal = new Signal();

	private WorkNodeGenerator<BuildStairsGoal> workNodeGenerator;

	public override IconKey IconKey => IconKeys.Missing;

	protected override void Start()
	{
		SetupBuilder();
		base.Start();
	}

	private void SetupBuilder()
	{
		workNodeGenerator = new WorkNodeGenerator<BuildStairsGoal>();
		workNodeGenerator.WorkAction = BuildStair;
		workNodeGenerator.WorkInvalidCondition = IsDone;
		workNodeGenerator.Animation = Working.WorkStates.Pick;
		workNodeGenerator.ReleaseWhen = ReleaseNodeWhen.Ended | ReleaseNodeWhen.Invalid;
		workNodeGenerator.OnWorkEnded = InvokeFinished;
		workNodeGenerator.OnWorkInvalid = InvokeFinished;
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return workNodeGenerator.BuildNode(this, Worker);
	}

	private Node BuildStair(BuildStairsGoal g, Labor l)
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
			RemoveCroute();
		}).Do(delegate
		{
			SpawnStairs();
		});
	}

	private bool IsDone(BuildStairsGoal g, Labor ignored)
	{
		return QuarryGoalProvider2.IsStair(g.voxel.position);
	}

	private Node InvokeFinished(Goal g, Labor l)
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
			FinishedSignal.Invoke();
		});
	}

	private void SpawnStairs()
	{
		XYZ position = base.voxel.position;
		Quaternion rotation;
		Factory.AssetKey key;
		if (base.transform.forward == Vector3.up)
		{
			key = Blueprints.Mine_Stairs_Block;
			rotation = Quaternion.identity;
		}
		else
		{
			key = Blueprints.Mine_Stairs;
			rotation = base.transform.rotation;
		}
		key = Factory.Peek(key).GetComponent<Blueprint>().BlueprintResult;
		ObjectPoolSingleton.Request_Unsafe(key, position.ToVector3(), rotation);
	}

	private void RemoveCroute()
	{
		IVoxel voxel = Voxel.GetVoxel(base.voxel.position + XYZ.up);
		if (voxel.IsCrust)
		{
			voxel.DestroyBlock(withParticles: true, withGarnotte: true);
		}
	}

	public override bool IsValid()
	{
		return Voxel.IsEmpty(base.voxel.position + XYZ.up);
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReach>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!labor.Profession.CanBreakVoxels())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantDig>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		return 10000000 - Mathf.RoundToInt(Vector3.Distance(labor.transform.position, base.transform.position));
	}
}
