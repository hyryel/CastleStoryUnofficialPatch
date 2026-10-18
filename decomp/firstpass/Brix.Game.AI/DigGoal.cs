using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class DigGoal : StorableGoal
{
	public bool UseHeightAdjustment = true;

	public bool DestroyIfAbandonned;

	public bool EatGarnotteWhileWorking = true;

	public bool DigOnlyWhatFitsInBag = true;

	public bool Highlight;

	public Signal FinishedSignal = new Signal();

	public IDigGoalResourceSetting ResourceSetting;

	private HashSet<Factory.AssetKey> _uniqueItemKeys = new HashSet<Factory.AssetKey>();

	private WorkNodeGenerator<DigGoal> workNodeGenerator;

	public LaborInstruction<GameObject> GoDig;

	public override IconKey IconKey => IconKeys.Missing;

	public override Accessoire PreferedTool => Adjectif.blockBreaking;

	public override void UniqueItems(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (Factory.AssetKey uniqueItemKey in _uniqueItemKeys)
		{
			keys.Add(uniqueItemKey);
		}
	}

	public static void UniqueItem(XYZ position, ref HashSet<Factory.AssetKey> keys)
	{
		Voxel.GetGarnotteValue(position, keys);
	}

	protected override void Awake()
	{
		base.Awake();
		GoDig = new LaborInstruction<GameObject>("DigGoal", IconKey, (Labor labor, GameObject value) => GetWorkNode(labor));
		ResourceSetting = DefaultDigGoalResourceSetting.Get;
	}

	protected override void Start()
	{
		UniqueItem(base.voxel.position, ref _uniqueItemKeys);
		SetupBuilder();
		base.Start();
	}

	private void SetupBuilder()
	{
		workNodeGenerator = new WorkNodeGenerator<DigGoal>();
		workNodeGenerator.WorkAction = DigAction;
		workNodeGenerator.WorkInvalidCondition = VoxelWorkIsDone;
		workNodeGenerator.Animation = Working.WorkStates.Pick;
		workNodeGenerator.OnWorkEnded = InvokeFinished;
		workNodeGenerator.OnWorkInvalid = InvokeFinished;
		workNodeGenerator.ReleaseWhen = (ReleaseNodeWhen)(5 | (DestroyIfAbandonned ? 2 : 0));
		if (EatGarnotteWhileWorking && !MustOverkill())
		{
			workNodeGenerator.ActionWrappers.Add(WorkNodesWrappers.EatGarnotteWhileWorking);
		}
		if (Highlight)
		{
			workNodeGenerator.NodeWrappers.Add(WorkNodesWrappers.HighlightAtYellow);
		}
	}

	private Node InvokeFinished(Goal g, Labor l)
	{
		return (StaticNode)Node.Sequence.Do(delegate(Signal _sig)
		{
			_sig.Invoke();
		}, FinishedSignal);
	}

	public void RebuildNode()
	{
		if (!base.Released)
		{
			SetupBuilder();
		}
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return workNodeGenerator.BuildNode(this, Worker);
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!labor.Occupation.Profession.CanBreakVoxels())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantDig>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (labor.recepteur.IsFull() || (DigOnlyWhatFitsInBag && SpaceRemainingInInventory(labor) < AmountGarnotteInGround(base.voxel.position)))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.BagTooFullToDig>.Get.Problem, labor);
			return false;
		}
		time += 0.1f;
		if (!MustOverkill() && !CouldStore(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.NoCorrespondingStorage>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			return int.MinValue;
		}
		int num = -(int)Vector3.Distance(labor.transform.position, base.transform.position);
		int num2 = (int)base.transform.position.y * 10000;
		int num3 = (HasOtherWorkerAssigned(labor) ? (-100) : 0);
		return num + (UseHeightAdjustment ? 1 : 0) * num2 + num3;
	}

	private int AmountGarnotteInGround(XYZ voxel)
	{
		return Voxel.Engine.GetTerrainResources(Voxel.Engine.GetVoxelHandle(voxel)).TotalAmount;
	}

	private int SpaceRemainingInInventory(Labor labor)
	{
		return 20 - AmountOfGarnotteCarried(labor);
	}

	private int AmountOfGarnotteCarried(Labor labor)
	{
		int num = labor.recepteur.ContentDescription.Value<Stones>();
		num += labor.recepteur.ContentDescription.Value<BlueCrystal>();
		num += labor.recepteur.ContentDescription.Value<OrangeCrystal>();
		return num + labor.recepteur.ContentDescription.Value<RawIron>();
	}

	private Node DigAction(DigGoal goal, Labor labor)
	{
		return (StaticNode)Node.Sequence.Do(delegate(DigGoal self, DigGoal g, Labor l)
		{
			self.ApplyDamage(l);
		}, this, goal, labor).Label("dammage voxel");
	}

	private void ApplyDamage(Labor labor)
	{
		DamageReaction expectedReaction = DamageReaction.Ignored;
		if (MustOverkill())
		{
			expectedReaction = DamageReaction.ForceOverkill;
		}
		float value = ((Voxel.GetBlockInfo(XYZ.FromVector3(base.transform.position)).Type != BlockType.Terrain) ? ((float)labor.Occupation.Profession.BlockBreaking) : ((float)labor.Occupation.Profession.TerrainDamage));
		base.damageReceiver.ReceiveAttack(value, DamageType.Siege, expectedReaction, Vector3.zero, Vector3.zero, 0f, labor.gameObject);
	}

	private GroundResource RessourceKeyToGroundResource(Factory.AssetKey key)
	{
		if (key == RawResources.RawStone)
		{
			return GroundResource.Stone;
		}
		if (key == RawResources.RawIron)
		{
			return GroundResource.Iron;
		}
		if (key == RawResources.RawOrangeCrystal)
		{
			return GroundResource.Brimstone;
		}
		return GroundResource.None;
	}

	private bool MustOverkill()
	{
		return MustOverkill(Voxel.GetBlockInfo(base.voxel.position).ResourceKey);
	}

	private bool MustOverkill(Factory.AssetKey key)
	{
		GroundResource groundResource = RessourceKeyToGroundResource(key);
		return groundResource != GroundResource.None && !ResourceSetting.TypesToHarvest.HasFlag(groundResource);
	}

	private bool VoxelWorkIsDone(DigGoal goal, Labor ignored)
	{
		XYZ position = goal.voxel.position;
		return Voxel.IsEmpty(position) || Voxel.IsInvincible(position);
	}
}
