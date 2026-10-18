using Brix.Game.AI.Damage;
using Brix.Game.Rules;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class ProjectileAttack : IAttack
{
	protected Projectile projectile;

	protected GameObject origin;

	private AttackTemplate attack;

	private Vector3 Force => projectile.GetImpactVector() * attack.ForceFactor;

	public float DamageValue => attack.damage;

	public GameObject Attacker => origin;

	public ProjectileAttack Set(Projectile p, GameObject o, AttackTemplate a)
	{
		projectile = p;
		origin = o;
		attack = a;
		return this;
	}

	public void Reset()
	{
		projectile = null;
		origin = null;
		attack = null;
	}

	public virtual void Apply(IDamageReceiver receiver)
	{
		if (NetworkServer.active && receiver != null)
		{
			receiver.ReceiveAttack(Mathf.FloorToInt(DamageValue * receiver.ProjectileMultiplier * CombatRules.ArrowDamageMultiplier), attack.type, attack.Reaction, projectile.transform.position, Force, attack.Lift, Attacker);
			if (receiver.GameObject.GetComponent<Labor>() != null && receiver.GameObject.GetComponent<Labor>().Project is RaidProject)
			{
				(receiver.GameObject.GetComponent<Labor>().Project as RaidProject).OnHit(origin);
			}
		}
	}
}
