using Brix.Game.AI.Damage;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI;

public class MeleeAttack : IAttack
{
	public enum Phase
	{
		Startup,
		Anticipation,
		Resolution,
		Finished
	}

	private static BrixLogger logger = new BrixLogger(typeof(MeleeAttack).FullName);

	public GameObject Attacker;

	public AttackTemplate Attack;

	public IDamageReceiver Target;

	public Phase CurrentPhase;

	public MeleeAttackVariation Variation;

	float IAttack.DamageValue => Attack.damage;

	GameObject IAttack.Attacker => Attacker;

	private Vector3 HorizontalDirectionToTarget => (Target.GameObject.transform.position - Attacker.transform.position).Flatten().normalized;

	public Vector3 Force => HorizontalDirectionToTarget * Attack.ForceFactor;

	public Vector3 Contact
	{
		get
		{
			Labor component = Target.GameObject.GetComponent<Labor>();
			Vector3 vector = ((!(component != null)) ? Vector3.zero : component.Occupation.Profession.AimAtOffset);
			return Target.GameObject.transform.position + vector + HorizontalDirectionToTarget * 0.5f;
		}
	}

	public MeleeAttack Set(GameObject o, AttackTemplate a, IDamageReceiver t)
	{
		Attacker = o;
		Attack = a;
		Target = t;
		CurrentPhase = Phase.Anticipation;
		Variation = Attacker.GetComponent<Labor>().Profession.ChooseAttackVariation();
		return this;
	}

	public void Reset()
	{
		Attacker = null;
		Attack = null;
		Target = null;
	}

	public DamageReaction GetTargetReaction()
	{
		return Target.GetReactionToAttack(Attack.damage, Attack.type, Attack.Reaction, Force);
	}

	public void Apply(IDamageReceiver target)
	{
		if (!target.IsNullOrReleased() && target != Target)
		{
			logger.InfoFormat("Collateral damage! {0}", target.GameObject.name);
			Target = target;
		}
		Target.ReceiveAttack(Attack.damage, Attack.type, Attack.Reaction, Contact, Force, Attack.Lift, Attacker);
	}
}
