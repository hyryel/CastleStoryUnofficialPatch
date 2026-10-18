using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Input;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class CharacterProjectile : Projectile
{
	private static BrixLogger logger = new BrixLogger(typeof(CharacterProjectile).FullName);

	public ProjectileAttack Attack;

	public Signal<Collision> CollisionSignal = new Signal<Collision>();

	protected Rigidbody Rigidbody;

	public static LayerBundle DamagedLayers = new LayerBundle(10, 16, 12, 14, 19);

	public override ProjectileAttack GetAttack()
	{
		return Attack;
	}

	public override Vector3 GetVelocity()
	{
		return Rigidbody.velocity;
	}

	protected override void Awake()
	{
		base.Awake();
		Rigidbody = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (Rigidbody.isKinematic)
		{
			Object.Destroy(this);
		}
	}

	public void MakeAttack(GameObject origin)
	{
		Attack = AttackTemplate.ThrownUnit.MakeProjectileAttack(this, origin);
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		logger.Info("CollisionEnter");
		DamageReceiver componentInParent = collision.gameObject.GetComponentInParent<DamageReceiver>();
		if (!(componentInParent == null) || DamagedLayers.ContainsAny(1 << collision.gameObject.layer))
		{
			CollisionSignal.Invoke(collision);
			Attack.Apply(componentInParent);
			Object.Destroy(this);
		}
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
