using Brix.Engine.Lifecycle;
using Brix.External.Factories;
using Brix.Game.Audio;
using Brix.Game.Rules;
using Brix.UI.Icons;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Magitron : Profession
{
	public float StaminaUsedPerProjectile;

	public override Sprite Icon32 => IconKeys._Warlock.Get32();

	public override int BlockBreaking => Mathf.RoundToInt((float)base.BlockBreaking * CombatRules.UnitToVoxelDamageMultiplier);

	public override float CombatAggro => 3f;

	public override float RaidProjectAggro => 18f;

	public override float Size => 1.9f;

	public override ProfessionAudio Audio => SingletonProvider.Get<MagitronAudio>();

	public override bool UsesStamina => false;

	public override bool CanJumpOff()
	{
		return true;
	}

	public override bool CanMakeWay()
	{
		return false;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		GetComponent<Occupation>().SetOccupation(Occupation.Type.Magitron);
	}

	public override void EquipGear()
	{
		toolbag.EquipTool(Accessories.WarlockBlast);
	}

	public bool EquipOneMoreTrap()
	{
		Toolbag component = GetComponent<Toolbag>();
		if (!component.HasTool(Slot.RightFoot))
		{
			component.EquipTool(Accessories.BearTrapShoes, Slot.RightFoot);
			return true;
		}
		if (!component.HasTool(Slot.LeftFoot))
		{
			component.EquipTool(Accessories.BearTrapShoes, Slot.LeftFoot);
			return true;
		}
		return false;
	}

	public override MeleeAttackVariation ChooseAttackVariation()
	{
		return MeleeAttackVariation.BricktronFist;
	}

	public override bool WillPursueWithRangeAttack()
	{
		return true;
	}

	public override bool CanSack()
	{
		return false;
	}

	public override bool HasMeleeAttack()
	{
		return false;
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
