using Brix.Audio;
using Brix.Game.AI;
using Brix.Legacy;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultProjectile : PhysicalizedProjectile
{
	public Catapult sourceCatapult;

	public override Vector3 GetVelocity()
	{
		return Rigidbody.velocity;
	}

	protected override void Awake()
	{
		base.Awake();
		if (NetworkServer.active)
		{
			if (Rigidbody == null)
			{
				Rigidbody = base.gameObject.AddComponent<Rigidbody>();
				Rigidbody.angularDrag = 0f;
				Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
			Rigidbody.isKinematic = false;
			Rigidbody.velocity = Vector3.zero;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (GetComponent<Explosifs>() != null)
		{
			GetComponent<Explosifs>().Emit(b: true);
		}
		SoundEngine.Play("Missile_Fall", base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SoundEngine.Play("Missile_Hit", base.gameObject);
	}

	protected override void OnCollisionEnter(Collision collision)
	{
		if (sourceCatapult == null || collision.gameObject.GetComponentInParent<Catapult>() != sourceCatapult)
		{
			GetComponent<Explosifs>().Explode(500f);
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
