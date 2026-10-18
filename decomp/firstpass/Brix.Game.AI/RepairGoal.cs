using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Locomotion;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class RepairGoal : Goal
{
	public bool WithHighlight;

	public bool DestroyIfAbandoned;

	[SerializeField]
	private MeshRenderer visual;

	private WorkNodeGenerator<RepairGoal> generator = new WorkNodeGenerator<RepairGoal>();

	private void SetupBuilder()
	{
		generator.WorkAction = (RepairGoal g, Labor l) => (StaticNode)Node.Sequence.Do(delegate
		{
			base.damageReceiver.Heal(l.Profession.VoxelRepair);
		});
		generator.Animation = Working.WorkStates.Pick;
		generator.WorkDoneCondition = (RepairGoal g, Labor l) => !base.damageReceiver.IsDamaged;
		generator.ReleaseWhen = (ReleaseNodeWhen)(4 | (DestroyIfAbandoned ? 2 : 0));
		if (WithHighlight)
		{
			generator.NodeWrappers.Add(WorkNodesWrappers.HighlightTargetYellow);
		}
	}

	public void Show(bool value)
	{
		if (visual != null)
		{
			visual.enabled = value;
		}
	}

	public void RebuildNode()
	{
		SetupBuilder();
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return generator.BuildNode(this, Worker);
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		if (!labor.recepteur.IsEmpty())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.InventoryIsFull>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReach>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		return -Mathf.RoundToInt(Vector3.Distance(labor.transform.position, base.transform.position));
	}

	protected override void Start()
	{
		SetupBuilder();
		base.Start();
	}
}
