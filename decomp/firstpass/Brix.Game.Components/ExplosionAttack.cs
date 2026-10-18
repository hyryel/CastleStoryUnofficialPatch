using Brix.Game.AI;
using Brix.Game.AI.Damage;
using UnityEngine;

namespace Brix.Game.Components;

public class ExplosionAttack : IAttack
{
	public ExplosionGameplay Explosion;

	public int DamageValue;

	public Vector3 Thrust;

	float IAttack.DamageValue => DamageValue;

	public GameObject Attacker => Explosion.gameObject;

	public virtual void Apply(IDamageReceiver receiver)
	{
		receiver.ReceiveAttack(DamageValue, DamageType.Explosive, DamageReaction.Smash, Explosion.transform.position, Thrust, 1f, Attacker);
	}

	public void ApplyAsAbsolute(IDamageReceiver receiver)
	{
		receiver.ReceiveAttack(DamageValue, DamageType.Absolute, DamageReaction.Smash, Explosion.transform.position, Thrust, 1f, Attacker);
	}
}
