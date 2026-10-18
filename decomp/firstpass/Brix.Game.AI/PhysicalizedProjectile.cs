using Brix.Engine.Abstract;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class PhysicalizedProjectile : Projectile
{
	private static BrixLogger logger = new BrixLogger(typeof(PhysicalizedProjectile).FullName);

	public ProjectileAttack Attack;

	public Signal<Collision> CollisionSignal = new Signal<Collision>();

	protected Rigidbody Rigidbody;

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

	public void Thrust(Vector3 initialVelocity, float inaccuracy, bool addTorque = true)
	{
		initialVelocity = Projectile.GetInaccurateVelocity(initialVelocity, inaccuracy);
		Rigidbody.AddForce(initialVelocity, ForceMode.VelocityChange);
		if (addTorque)
		{
			AddRandomTorque();
		}
	}

	protected void AddRandomTorque()
	{
		int scalaire = Voxel.GetScalaire(base.transform.position);
		Random.State state = Random.state;
		Random.InitState(scalaire);
		Rigidbody.AddTorque(Random.insideUnitSphere * 10f, ForceMode.VelocityChange);
		Random.state = state;
	}

	protected virtual void OnCollisionEnter(Collision collision)
	{
		logger.Info("CollisionEnter");
		CollisionSignal.Invoke(collision);
		DamageReceiver component = collision.gameObject.GetComponent<DamageReceiver>();
		if (component != null)
		{
			Attack.Apply(component);
		}
		Object.Destroy(this);
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
