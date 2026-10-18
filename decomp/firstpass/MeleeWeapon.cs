using Brix.Game.AI;
using Brix.Game.AI.Damage;
using UnityEngine;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class MeleeWeapon : Weapon
{
	public int Range = 2;

	public bool CanStrike = true;

	public static MeleeAttack CreateAttack(GameObject attacker, IDamageReceiver target)
	{
		AttackTemplate attackTemplate = new AttackTemplate();
		attackTemplate.damage = attacker.GetComponent<Labor>().Profession.MeleeDamage;
		attackTemplate.ForceFactor = attacker.GetComponent<Labor>().Profession.MeleeWeight;
		attackTemplate.Lift = AttackTemplate.DefaultMelee.Lift;
		attackTemplate.Reaction = ((!(attackTemplate.ForceFactor < 5f)) ? DamageReaction.Smash : AttackTemplate.DefaultMelee.Reaction);
		attackTemplate.type = AttackTemplate.DefaultMelee.type;
		return attackTemplate.MakeMeleeAttack(attacker, target);
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
