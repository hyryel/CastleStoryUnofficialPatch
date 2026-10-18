using Brix.Engine.Lifecycle;
using Brix.External.Factories;
using Brix.Game.Audio;
using Brix.Game.Rules;
using Brix.UI.Icons;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Corruptron : Profession
{
	public float StaminaUsedPerProjectile = 2.5f;

	public override Sprite Icon32 => IconKeys._Corruptron.Get32();

	public override int BlockBreaking => Mathf.RoundToInt((float)base.BlockBreaking * CombatRules.UnitToVoxelDamageMultiplier);

	public override float CombatAggro => 0.33f;

	public override float RaidProjectAggro => 3f;

	public override ProfessionAudio Audio => SingletonProvider.Get<CorruptronAudio>();

	public override bool UsesStamina => false;

	public override bool CanJumpOff()
	{
		return true;
	}

	public override bool CanMakeWay()
	{
		return true;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		GetComponent<Occupation>().SetOccupation(Occupation.Type.Corruptron);
	}

	public override void EquipGear()
	{
		toolbag.EquipTool(Accessories.CorruptronClaw);
	}

	public override MeleeAttackVariation ChooseAttackVariation()
	{
		return (!(Random.value < 0.6f)) ? MeleeAttackVariation.BricktronWeapon : MeleeAttackVariation.BricktronFist;
	}

	public override bool CanSack()
	{
		return true;
	}

	public override bool HasMeleeAttack()
	{
		return true;
	}

	protected override bool TargetIsWeakToProjectileAttack(Location target)
	{
		if (!(target is CharacterLocation characterLocation) || characterLocation.Character == null)
		{
			return false;
		}
		if (characterLocation.Character.Profession.OccupationType != Occupation.Type.Archer)
		{
			return false;
		}
		if (characterLocation.Locomotion.IsFlailing())
		{
			return false;
		}
		return base.TargetIsWeakToProjectileAttack(target);
	}

	public override Tool GetMeleeWeapon()
	{
		return GetComponentInChildren<MeleeWeapon>();
	}

	public override bool IsCorruptron()
	{
		return true;
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
