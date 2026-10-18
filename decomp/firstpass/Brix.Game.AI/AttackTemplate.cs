using Brix.Game.AI.Damage;
using Brix.Game.Rules;
using UnityEngine;

namespace Brix.Game.AI;

public class AttackTemplate
{
	public float damage;

	public DamageType type;

	public float ForceFactor;

	public DamageReaction Reaction;

	public float Lift = 1f;

	public static AttackTemplate ThrownRock = new AttackTemplate
	{
		damage = 20f,
		type = DamageType.Piercing,
		ForceFactor = 1f,
		Reaction = ((!CombatRules.StoneKnockback) ? DamageReaction.Twitch : DamageReaction.Smash),
		Lift = 0.25f
	};

	public static AttackTemplate Arrow = new AttackTemplate
	{
		damage = 20f,
		type = DamageType.Piercing,
		ForceFactor = 4f,
		Reaction = DamageReaction.Twitch,
		Lift = 0f
	};

	public static AttackTemplate Bolt = new AttackTemplate
	{
		damage = 45f,
		type = DamageType.Piercing,
		ForceFactor = 4f,
		Reaction = DamageReaction.Twitch,
		Lift = 0f
	};

	public static AttackTemplate WarlockBlast = new AttackTemplate
	{
		damage = 60f,
		type = DamageType.Magical,
		ForceFactor = 4f,
		Reaction = DamageReaction.Twitch,
		Lift = 0f
	};

	public static AttackTemplate ArtificerShot = new AttackTemplate
	{
		damage = 30f,
		type = DamageType.Magical,
		ForceFactor = 4f,
		Reaction = DamageReaction.Twitch,
		Lift = 0f
	};

	public static AttackTemplate MagicProjectile = new AttackTemplate
	{
		damage = 12f,
		type = DamageType.Magical,
		ForceFactor = 4f,
		Reaction = DamageReaction.Ignored,
		Lift = 0f
	};

	public static AttackTemplate ThrownUnit = new AttackTemplate
	{
		damage = 80f,
		type = DamageType.Explosive,
		ForceFactor = 0.8f,
		Reaction = DamageReaction.Smash,
		Lift = 0.25f
	};

	public static AttackTemplate AlchemistBomb = new AttackTemplate
	{
		damage = 20f,
		type = DamageType.Explosive,
		ForceFactor = 3f,
		Reaction = DamageReaction.Twitch,
		Lift = 0.25f
	};

	public static AttackTemplate DefaultMelee = new AttackTemplate
	{
		damage = 5f,
		type = DamageType.Melee,
		ForceFactor = 1f,
		Reaction = DamageReaction.Twitch,
		Lift = 1f
	};

	public void CopyTo(ref ProjectileAttack attack, Projectile projectile, GameObject origin)
	{
		attack.Set(projectile, origin, this);
	}

	public void CopyTo(ref MeleeAttack attack, GameObject origin, IDamageReceiver target)
	{
		attack.Set(origin, this, target);
	}

	public ProjectileAttack MakeProjectileAttack(Projectile projectile, GameObject origin)
	{
		return new ProjectileAttack().Set(projectile, origin, this);
	}

	public MeleeAttack MakeMeleeAttack(GameObject origin, IDamageReceiver target)
	{
		return new MeleeAttack().Set(origin, this, target);
	}
}
