using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Legacy;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class TreeGoal : Goal
{
	private DamagedTree _tree;

	private Labor _worker;

	private WorkNodeGenerator<TreeGoal> generator = new WorkNodeGenerator<TreeGoal>();

	public override IconKey IconKey => IconKeys.Missing;

	public override Accessoire PreferedTool => Adjectif.woodCutting;

	protected override void Awake()
	{
		base.Awake();
		generator.Animation = Working.WorkStates.Axe;
		generator.WorkAction = SpawnAndAttackTree;
		generator.WorkDoneCondition = WorkDone;
		generator.ReleaseWhen = ReleaseNodeWhen.Ended | ReleaseNodeWhen.Invalid;
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return generator.BuildNode(this, Worker);
	}

	private Node SpawnAndAttackTree(TreeGoal goal, Labor labor)
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
			SpawnTreeIfNeeded();
		}).Do(Node.WaitForTicks(1)).Do(delegate
		{
			Faction result = labor.affiliation.faction;
			_tree.GetOrAddComponent<AffiliationComponent>().faction = result;
			return result;
		})
			.Do(delegate
			{
				_tree.Strike(base.voxel.index, labor.Occupation.Profession.WoodCutting);
			});
	}

	private static bool WorkDone(TreeGoal g, Labor ignored)
	{
		return TreeRegistry.Instance.GetTree(g.voxel.position) == 0 || (!g._tree.IsNullOrReleased() && g._tree.IsDying);
	}

	public override bool IsValid()
	{
		bool flag = TreeRegistry.Instance.HasTree(base.voxel.position);
		bool flag2 = _tree != null && _tree.Hp > 0;
		return flag || flag2;
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!labor.Occupation.Profession.CanChopTrees())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantCutTrees>.Get.Problem, labor);
			return false;
		}
		if (labor.recepteur.IsFull())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.InventoryIsFull>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReachTree>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		int num = Mathf.RoundToInt(Vector3.Distance(labor.transform.position, base.gameObject.transform.position));
		int num2 = 0;
		if (Knowledge.Instance.IsReserved(base.gameObject) || HasOtherWorkerAssigned(labor))
		{
			num2 = 100;
		}
		return -num - num2;
	}

	private void SpawnTreeIfNeeded()
	{
		_tree = DamagedTree.GetAtOrCreate(base.voxel.position);
	}
}
