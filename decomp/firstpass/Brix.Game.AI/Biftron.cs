using Brix.Engine.Lifecycle;
using Brix.External.Factories;
using Brix.Game.Audio;
using Brix.Game.Rules;
using Brix.UI.Icons;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Biftron : Profession
{
	public override float CombatAggro => 2f;

	public override float RaidProjectAggro => 12f;

	public override ProfessionAudio Audio => SingletonProvider.Get<BiftronAudio>();

	public override Sprite Icon32 => IconKeys._Biftron.Get32();

	public override bool AvoidsFriendlyFire => false;

	public override Vector3 AimAtOffset => Vector3.up * 1.5f;

	public override float Size => 1.9f;

	public override bool UsesStamina => false;

	public override int BlockBreaking => Mathf.RoundToInt((float)base.BlockBreaking * CombatRules.BiftronToObjectDamageMultiplier);

	public override void OnRequested()
	{
		base.OnRequested();
		GetComponent<Occupation>().SetOccupation(Occupation.Type.Biftron);
	}

	public override void EquipGear()
	{
		toolbag.EquipTool(Accessories.BiftronFist);
	}

	public override bool CanJumpOff()
	{
		return true;
	}

	public override bool CanMakeWay()
	{
		return true;
	}

	public override bool CanRun()
	{
		return false;
	}

	public override bool CanMoveAttack()
	{
		return false;
	}

	public override bool CanSack()
	{
		return true;
	}

	public override Tool GetMeleeWeapon()
	{
		return GetComponentInChildren<MeleeWeapon>();
	}

	public override MeleeAttackVariation ChooseAttackVariation()
	{
		return MeleeAttackVariation.BricktronFist;
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
