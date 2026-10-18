using System.Collections.Generic;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class NatureHarvestGoal : StorableGoal
{
	private NatureData NData;

	public bool DestroyIfAbandonned;

	public bool Highlight;

	public bool RequireStorage = true;

	private WorkNodeGenerator<NatureHarvestGoal> workNodeGenerator;

	private NatureDamageReceiver natureDamageReceiver;

	public LaborInstruction<GameObject> GoHarvest;

	public NatureType NatureType => NData.Type;

	public override IconKey IconKey => IconKeys.Missing;

	public override Accessoire PreferedTool => NData.ToolToHarvest;

	protected override void Awake()
	{
		base.Awake();
		GoHarvest = new LaborInstruction<GameObject>("NatureHarvestGoal", IconKey, (Labor labor, GameObject value) => GetWorkNode(labor));
	}

	protected override void Start()
	{
		SetupBuilder();
		base.Start();
	}

	private void SetupBuilder()
	{
		workNodeGenerator = new WorkNodeGenerator<NatureHarvestGoal>();
		workNodeGenerator.WorkAction = (NatureHarvestGoal g, Labor l) => g.HarvestAction(l);
		workNodeGenerator.WorkDoneCondition = (NatureHarvestGoal g, Labor l) => !g.NData.Registered();
		workNodeGenerator.WorkInvalidCondition = (NatureHarvestGoal g, Labor l) => g.IsWorkInvalid(l);
		workNodeGenerator.OnWorkInvalid = (NatureHarvestGoal g, Labor l) => g.OnWorkInvalid(l);
		workNodeGenerator.Animation = NData.WorkAnim();
		workNodeGenerator.ReleaseWhen = ReleaseNodeWhen.Ended;
		workNodeGenerator.NodeWrappers.Add(WorkNodesWrappers.EatGarnotteWhileWorking);
		if (!DestroyIfAbandonned)
		{
			return;
		}
		workNodeGenerator.Finally = delegate(NatureHarvestGoal g, Labor l)
		{
			if (!g.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(g.gameObject);
			}
		};
	}

	public void RebuildNode()
	{
		SetupBuilder();
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return workNodeGenerator.BuildNode(this, Worker);
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (!NData.IsValid)
		{
			return false;
		}
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		if (NData is PlantData && !labor.Occupation.Profession.CanCollectPlants())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantHarvestPlant>.Get.Problem, labor);
			return false;
		}
		if ((NData is MineralData || NData is BoulderData) && !labor.Occupation.Profession.CanHarvestMinerals())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantHarvestMinerals>.Get.Problem, labor);
			return false;
		}
		if (labor.recepteur.CanFitUpTo(NData.RepresentativeKey()) < NData.ExpectedQte())
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
		time += 0.05f;
		if (!CouldStore(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.BagTooFullToDig>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		return DistanceAjustment(labor);
	}

	private int DistanceAjustment(Labor labor)
	{
		return -Mathf.RoundToInt(Vector3.Distance(labor.transform.position, base.transform.position));
	}

	private Node HarvestAction(Labor labor)
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
			Knowledge.Instance.ReserveUntil(labor.gameObject, labor.signal.WorkEnded);
		}).Do(Node.WaitForTicks(1)).Do(delegate
		{
			HarvestHitAction(labor);
		})
			.Label("Harvest Nature");
	}

	private void HarvestHitAction(Labor labor)
	{
		float value = 1f;
		if (NData is BoulderData boulderData)
		{
			value = BoulderRegistry.GetMaxHp(boulderData) / (float)boulderData.MaxRessources * (float)boulderData.ExpectedQte();
		}
		natureDamageReceiver.ApplyDamage(value, labor.gameObject);
		NatureRegistry.HarvestAt(NData.RootPos);
	}

	private bool IsWorkInvalid(Labor labor)
	{
		if (RequireStorage && !CouldStore(labor))
		{
			return true;
		}
		return labor.recepteur.IsFull(NData.RepresentativeRessource());
	}

	private Node OnWorkInvalid(Labor labor)
	{
		if (RequireStorage && !CouldStore(labor))
		{
			return Node.Fail(SharedWorkException<Exceptions.NoCorrespondingStorage>.Get);
		}
		if (labor.recepteur.IsFull(NData.RepresentativeRessource()))
		{
			return Node.Fail(SharedWorkException<Exceptions.InventoryIsFull>.Get);
		}
		return Node.Empty;
	}

	public override void UniqueItems(ref HashSet<Factory.AssetKey> keys)
	{
		Factory.AssetKey assetKey = NData.RepresentativeKey();
		if (assetKey != null)
		{
			keys.Add(assetKey);
		}
	}

	public static NatureHarvestGoal Request(NatureData nature, Transform parent)
	{
		ObjectPoolSingleton.RequestComponent<NatureHarvestGoal>(out var t, nature.GetGoalAsset(), nature.GoalPosition, Quaternion.identity);
		t.DestroyIfAbandonned = false;
		t.NData = nature;
		t.natureDamageReceiver = NatureDamageReceiver.Make(nature.RootPos);
		t.transform.parent = parent;
		t.RebuildNode();
		return t;
	}
}
