using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Batchers;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Game.Semantique;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Collections;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class LandscapeGoal : StorableGoal
{
	public enum Landscape
	{
		Dig,
		Fill,
		Invalid
	}

	private Landscape type;

	private LandscapeProject _landscapeProject;

	public Signal<LandscapeGoal> OnChangedType = new Signal<LandscapeGoal>();

	public Profile lookingFor = Adjectif.stones.Profile;

	private WorkNodeGenerator<LandscapeGoal> generator = new WorkNodeGenerator<LandscapeGoal>();

	private HashSet<Factory.AssetKey> _uniqueItemKeys;

	private const int INVENTORY_WEIGHT = 1073741823;

	private const int maxY = 1048;

	private const int HEIGHT_WEIGHT = 1024562;

	public override IconKey IconKey => IconKeys.Missing;

	public bool IsDig => type == Landscape.Dig;

	public bool IsFill => type == Landscape.Fill;

	private HashSet<Factory.AssetKey> uniqueItemKeys
	{
		get
		{
			if (_uniqueItemKeys == null)
			{
				_uniqueItemKeys = new HashSet<Factory.AssetKey>();
			}
			return _uniqueItemKeys;
		}
	}

	public Landscape GetLandscapeType()
	{
		return type;
	}

	public void LinkProject(LandscapeProject p)
	{
		_landscapeProject = p;
	}

	protected override void Start()
	{
		generator.PrepareAction = Prepare;
		generator.Animation = Working.WorkStates.Pick;
		generator.NodeWrappers.Add(WorkNodesWrappers.EatGarnotteWhileWorking);
		generator.WorkAction = Action;
		generator.WorkDoneCondition = VoxelWorkIsDone;
		generator.WorkInvalidCondition = (LandscapeGoal g, Labor l) => g.IsNullOrReleased();
		generator.ReleaseWhen = ReleaseNodeWhen.Ended;
		base.Start();
	}

	public void EvaluateTheshold(int threshold)
	{
		type = ((base.voxel.position.y <= threshold) ? Landscape.Fill : Landscape.Dig);
		uniqueItemKeys.Clear();
		OnChangedType.Invoke(this);
	}

	public override void UniqueItems(ref HashSet<Factory.AssetKey> keys)
	{
		if (!IsDig)
		{
			return;
		}
		if (uniqueItemKeys.Count == 0)
		{
			DigGoal.UniqueItem(base.voxel.position, ref _uniqueItemKeys);
		}
		foreach (Factory.AssetKey uniqueItemKey in uniqueItemKeys)
		{
			keys.Add(uniqueItemKey);
		}
	}

	private int BonusAccordingToInventory(Labor labor, int weight)
	{
		bool flag = AmountOfGarnotteCarried(labor) > 10;
		if ((IsFill && flag) || (IsDig && !flag))
		{
			return weight;
		}
		return 0;
	}

	private int DistanceAjustment(Labor labor)
	{
		return -Mathf.RoundToInt(Vector3.Distance(labor.transform.position, base.transform.position));
	}

	private int HeightAdjusmtmentToPriority(Landscape type, int weight)
	{
		int result = 0;
		if (IsDig)
		{
			result = base.voxel.position.y * weight;
		}
		else if (IsFill)
		{
			result = (1048 - base.voxel.position.y) * weight;
		}
		return result;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		int num = BonusAccordingToInventory(labor, 1073741823);
		int num2 = DistanceAjustment(labor);
		int num3 = HeightAdjusmtmentToPriority(type, 1024562);
		return num + num2 + num3;
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (this == null)
		{
			return false;
		}
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
		if (IsDig)
		{
			if (labor.recepteur.CanFitUpTo(RawResources.RawStone) < AmountGarnotteInGround(base.voxel.position))
			{
				errorDestination.AddDecisionError(SharedWorkException<Exceptions.BagTooFullToDig>.Get.Problem, labor);
				return false;
			}
			if (!labor.Occupation.Profession.CanBreakVoxels())
			{
				errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantDig>.Get.Problem, labor);
				return false;
			}
			time += 0.1f;
			if (!_landscapeProject.HasBothKindOfGoals() && !CouldStore(labor))
			{
				errorDestination.AddDecisionError(SharedWorkException<Exceptions.BagTooFullToDig>.Get.Problem, labor);
				return false;
			}
			return true;
		}
		if (IsFill)
		{
			if (Voxel.IsEmpty(base.voxel.position - XYZ.up))
			{
				return false;
			}
			if (!labor.recepteur.HasRoomFor(labor.Find(lookingFor)))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return generator.BuildNode(this, Worker);
	}

	private static Node Prepare(LandscapeGoal goal, Labor labor)
	{
		return Node.Selector.Label("Prepare LandscapeGoal").If((LandscapeGoal g) => g.IsFill, goal).Label("If FillGoal")
			.Do((LandscapeGoal _goal, Labor _labor) => _labor.FindAndPickUp(_goal.lookingFor, 20), goal, labor)
			.Label("FindAndPickUp")
			.ToNode;
	}

	private static Node Action(LandscapeGoal goal, Labor labor)
	{
		return Node.Selector.Label("Action LandscapeGoal").If((LandscapeGoal g) => g.IsDig, goal).Label("If DigGoal")
			.Do((LandscapeGoal g) => g.DigAction(labor), goal)
			.Label("Do DigAction")
			.If((LandscapeGoal g) => g.IsFill, goal)
			.Label("If FillGoal")
			.Do((LandscapeGoal g, Labor _labor) => g.FillAction(_labor), goal, labor)
			.Label("Do FillAction")
			.ToNode;
	}

	private StaticNode DigAction(Labor labor)
	{
		DamageReaction damageReaction = base.damageReceiver.ReceiveAttack(60f, DamageType.Melee, DamageReaction.Ignored, base.transform.position, base.transform.position, 0f, labor.gameObject);
		if (damageReaction == DamageReaction.Failed)
		{
			return Node.FailWork<Exceptions.VoxelHasBeenBuildOver>();
		}
		return Node.Empty;
	}

	private Node FillAction(Labor worker)
	{
		XYZ position = base.voxel.position;
		int p = AmountGarnotteMissingInGround(position);
		return Node.Sequence.Do(delegate(XYZ pos, int qty)
		{
			AddInGround(pos, qty);
		}, position, p).Label("AddInGround").Do(delegate(Labor labor, int qty)
		{
			RemoveGarnotes(labor, qty);
		}, worker, p)
			.Label("RemoveGarnotes")
			.ToNode;
	}

	private static void AddInGround(XYZ p, int qty)
	{
		VoxelRequestBatcher.Instance.CallRpcAddTerrainResources(p.x, p.y, p.z, VoxelTerrainType.Sand, qty, compacted: true, pushOut: true);
	}

	private static bool VoxelWorkIsDone(LandscapeGoal goal, Labor ignored)
	{
		XYZ position = goal.voxel.position;
		bool result = true;
		if (goal.IsDig)
		{
			result = Voxel.IsEmpty(position);
		}
		else if (goal.IsFill)
		{
			result = !Voxel.IsEmpty(position) && !Voxel.IsCrust(position);
		}
		return result;
	}

	private static void RemoveGarnotes(Labor labor, int nb)
	{
		List<IDescriptor> list = (from d in labor.recepteur.StoredItems.Slinq()
			where d.Description.Value<Stones>() == 1
			select d).Take(nb).ToPooledList();
		list.ForEach(delegate(IDescriptor g)
		{
			g.GameObject.SafeNetworkDestroy();
		});
		list.Dispose();
	}

	private int AmountGarnotteInGround(XYZ voxel)
	{
		return Voxel.Engine.GetTerrainResources(Voxel.Engine.GetVoxelHandle(voxel)).TotalAmount;
	}

	private int AmountGarnotteMissingInGround(XYZ voxel)
	{
		return 10 - AmountGarnotteInGround(voxel);
	}

	private int AmountOfGarnotteCarried(Labor labor)
	{
		return labor.recepteur.ContentDescription.Value<Stones>();
	}
}
