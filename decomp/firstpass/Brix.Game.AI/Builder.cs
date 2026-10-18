using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Lifecycle;
using Brix.External.Factories;
using Brix.Game.Audio;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Lua;
using Brix.UI.Icons;
using MoonSharp.Interpreter;
using Motus.Behavior;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Builder : Profession
{
	private static Dictionary<Occupation.Job, IconKey> classIcons = new Dictionary<Occupation.Job, IconKey>
	{
		{
			Occupation.Job.Builder,
			IconKeys._Bricktron
		},
		{
			Occupation.Job.Knight,
			IconKeys._Knight
		},
		{
			Occupation.Job.Archer,
			IconKeys._Archer
		},
		{
			Occupation.Job.Halberdier,
			IconKeys._Halberdier
		},
		{
			Occupation.Job.Arbalist,
			IconKeys._Arbalist
		},
		{
			Occupation.Job.Alchemist,
			IconKeys._Alchemist
		},
		{
			Occupation.Job.Artificer,
			IconKeys._Artificer
		}
	};

	public override bool UsesStamina => CanBlock();

	public override Sprite Icon32 => classIcons[labor.Occupation.CurrentJob].Get32();

	public override ProfessionAudio Audio => labor.Occupation.CurrentJob switch
	{
		Occupation.Job.Archer => SingletonProvider.Get<ArcherAudio>(), 
		Occupation.Job.Knight => SingletonProvider.Get<KnightAudio>(), 
		Occupation.Job.Halberdier => SingletonProvider.Get<HalberdierAudio>(), 
		Occupation.Job.Arbalist => SingletonProvider.Get<ArbalistAudio>(), 
		Occupation.Job.Artificer => SingletonProvider.Get<ArtificerAudio>(), 
		Occupation.Job.Alchemist => SingletonProvider.Get<AlchemistAudio>(), 
		Occupation.Job.Builder => SingletonProvider.Get<BuilderAudio>(), 
		_ => SingletonProvider.Get<BuilderAudio>(), 
	};

	public override float ProjectileSpeedMultiplier => (base.OccupationType != Occupation.Type.Archer) ? base.ProjectileSpeedMultiplier : CombatRules.ArrowSpeedMultiplier;

	protected override void Start()
	{
		base.Start();
		TryEquipScarf();
	}

	private void TryEquipScarf()
	{
		if ((Neo.IsMultiplayer || GeneralRules.ScarfsInSinglePlayer) && !toolbag.HasTool(Accessories.Scarf))
		{
			toolbag.EquipMagicalTool(Accessories.Scarf);
		}
	}

	public override bool CanWieldAxe()
	{
		return true;
	}

	public override bool CanWieldHammer()
	{
		return true;
	}

	public override bool CanWieldPick()
	{
		return true;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		GetComponent<Occupation>().SetOccupation(Occupation.Type.Builder);
	}

	public override void EquipGear()
	{
		switch (base.OccupationType)
		{
		case Occupation.Type.Archer:
			toolbag.EquipTool(Accessories.Bow);
			toolbag.EquipTool(Accessories.Quiver);
			toolbag.EquipTool(Accessories.Cap);
			break;
		case Occupation.Type.Knight:
			toolbag.EquipTool(Accessories.Sword);
			toolbag.EquipTool(Accessories.Helmet);
			toolbag.EquipTool(Accessories.Shield);
			break;
		}
	}

	protected override ConflictResolver.ResolutionModel GetConflictResolutionModel()
	{
		if (base.OccupationType != Occupation.Type.Builder)
		{
			return base.GetConflictResolutionModel();
		}
		GameObject currentTaskGiver = labor.GetCurrentTaskGiver();
		return (!currentTaskGiver.IsNullOrReleased() && currentTaskGiver.GetComponent<BuildGoal>() != null) ? ConflictResolver.ResolutionModel.Construction : ConflictResolver.ResolutionModel.Mining;
	}

	public override bool CanRunWhileDefensive()
	{
		return labor.Occupation.CurrentJob != Occupation.Job.Artificer && labor.Occupation.CurrentJob != Occupation.Job.Knight;
	}

	public override bool IsWorker()
	{
		return labor.Occupation.CurrentOccupation == Occupation.Type.Builder;
	}

	public override bool CanPassThrough()
	{
		return labor.Activity != Activity.Fighting && labor.Activity != Activity.Chilling && (IsWorker() || labor.hackedDigGoal.IsNullOrReleased());
	}

	public override bool CanTransferTo(Labor other)
	{
		return labor.Project.ConflictResolver.HasValidTransfer(labor, other);
	}

	public override MeleeAttackVariation ChooseAttackVariation()
	{
		if (toolbag.HasToolFor(Adjectif.blockAngle, out var _))
		{
			return MeleeAttackVariation.BricktronFist;
		}
		if (toolbag.GetToolLimbPair(Slot.RightHand).Tool != null)
		{
			return MeleeAttackVariation.BricktronWeapon;
		}
		return MeleeAttackVariation.BricktronFist;
	}

	private static bool SubjectCanFightEnemy(Labor subject, GameObject enemy)
	{
		return SubjectCanFightEnemyInMelee(subject, enemy) || SubjectCanFightEnemyRanged(subject, enemy);
	}

	private static bool SubjectCanFightEnemyInMelee(Labor subject, GameObject enemy)
	{
		return subject.Profession.IsStrongInMelee() || (subject.Profession.HasMeleeAttack() && Location.Of(enemy).TargetInCloseRange(subject.transform.position));
	}

	private static bool SubjectCanFightEnemyRanged(Labor subject, GameObject enemy)
	{
		return subject.Profession.ProjectileAttackAvailable();
	}

	public override Node GetDangerReactionNode(Labor subject, GameObject enemy)
	{
		if (enemy == null)
		{
			return Node.Empty;
		}
		if (IsStrongInMelee())
		{
			return subject.GoAttack(enemy.gameObject);
		}
		if (ProjectileAttackAvailable())
		{
			return subject.HoldPositionAndAttackWithPriority(enemy);
		}
		return (StaticNode)Node.Sequence.Do(() => subject.HoldPositionAndAttack());
	}

	private bool CanCreateTool()
	{
		return toolbag.IsMagical && CanSwitchTools();
	}

	public override bool CanChopTrees()
	{
		return (CanCreateTool() || base.WoodCutting > 0) && labor.recepteur.IsEmpty();
	}

	public override bool CanBreakVoxels()
	{
		return (CanCreateTool() || BlockBreaking > 0) && !labor.recepteur.RequiresBothHands();
	}

	public override bool CanHarvestMinerals()
	{
		return (CanCreateTool() || base.MineralHarvesting > 0) && !labor.recepteur.RequiresBothHands();
	}

	[LuaDrivable("EquipScarf")]
	private static DynValue lua_EquipScarf(ScriptExecutionContext context, CallbackArguments args)
	{
		Builder builder = args.AsComponent<Builder>(0, "EquipScarf");
		builder.toolbag.EquipMagicalTool(Accessories.Scarf);
		return DynValue.Void;
	}

	[LuaDrivable("SetScarfColor")]
	private static DynValue lua_SetScarfColor(ScriptExecutionContext context, CallbackArguments args)
	{
		Builder builder = args.AsComponent<Builder>(0, "EquipScarf");
		string text = args.AsString(1, "EquipScarf");
		ScarfFactionColoring component = builder.toolbag.Neck.Tool.GetComponent<ScarfFactionColoring>();
		switch (text)
		{
		case "Orange":
			component.UpdateColor(new Color(1f, 0.78f, 0.31f, 1f));
			break;
		case "Red":
			component.UpdateColor(new Color(0.855f, 0f, 0.216f, 1f));
			break;
		case "Blue":
			component.UpdateColor(new Color(0f, 0.53f, 1f, 1f));
			break;
		case "Green":
			component.UpdateColor(new Color(0f, 0.8f, 0.2f, 1f));
			break;
		case "Sun":
			component.UpdateColor(new Color(200f, 20f, 20f, 255f));
			break;
		case "Test":
			component.UpdateColor(new Color(3f, 3f, 3f, 255f));
			break;
		default:
			component.UpdateColor(new Color(0f, 0f, 0f, 255f));
			break;
		}
		return DynValue.Void;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
