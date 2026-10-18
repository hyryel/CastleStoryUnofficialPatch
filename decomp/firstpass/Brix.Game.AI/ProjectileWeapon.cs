using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Network;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

[DisallowMultipleComponent]
public class ProjectileWeapon : Weapon
{
	private static BrixLogger logger = new BrixLogger(typeof(ProjectileWeapon).FullName);

	public Factory.AssetKey ammoAssetKey;

	public Transform Origin;

	public float Power = 50f;

	public float PreferredAngle = 45f;

	public float MinRange = 0.5f;

	public float MaxRange = 80f;

	[JsonProperty]
	[SerializeField]
	private int AmmoCount;

	public int StaminaCost;

	public int AmmoCountGetter
	{
		get
		{
			return AmmoCount;
		}
		set
		{
			AmmoCount = value;
			ToolParentTracker component = GetComponent<ToolParentTracker>();
			if (component != null)
			{
				component.ParentRecepteur.WithComponent(delegate(CharacterState state)
				{
					state.AmmoCount = value;
				});
			}
		}
	}

	public LayerMask AmmoCollisionLayer => Factory.Peek(ammoAssetKey).GetComponent<Projectile>().LayerCollisionMask;

	protected override void Awake()
	{
		base.Awake();
		if (Origin == null)
		{
			Origin = base.transform.parent;
		}
	}

	public bool HasEnoughStamina(Labor labor)
	{
		return labor.GetComponent<BricktronDamageReceiver>().CurrentStamina >= (float)StaminaCost;
	}

	public bool HasAmmo()
	{
		return AmmoCountGetter > 0;
	}

	public bool HasThrowableUnit(Labor labor)
	{
		return labor.navigation.Locomotion.Common.Grabbing.GrabbedTarget != null;
	}

	public bool IsAvailableFor(Labor labor)
	{
		return (HasAmmo() || HasThrowableUnit(labor)) && HasEnoughStamina(labor);
	}

	public bool IsFarEnough(Vector3 target)
	{
		return (target - base.transform.position).sqrMagnitude >= MinRange * MinRange;
	}

	public bool IsInRangeToShoot(Vector3 target)
	{
		Vector3 a = base.transform.position - target;
		float magnitude = a.Flatten().magnitude;
		return magnitude >= MinRange && magnitude <= MaxRange;
	}

	public ProjectileAttack Fire(Vector3 origin, Vector3 aimOrigin, Vector3 target, GameObject source, Location targetLocation, GameObject thrown, float powerMultiplier = 1f, float inaccuracy = 0f, float hitPointDamage = 60f)
	{
		if (TryThrowUnit(out var attack, target, source, thrown, powerMultiplier, inaccuracy))
		{
			return attack;
		}
		if (TryThrowThing(out attack, target, source, thrown, powerMultiplier, hitPointDamage))
		{
			return attack;
		}
		if (TryThrowBullet(out attack, origin, aimOrigin, target, source, targetLocation, powerMultiplier, inaccuracy, hitPointDamage))
		{
			return attack;
		}
		logger.WarnFormat("ProjectileWeapon.Fire failed to create a valid projectile object:\n'{0}'\n'{1}'\n'{2}'\n'{3}'\n'{4}'\n'{5}'.", origin, target, source, targetLocation, inaccuracy, ammoAssetKey);
		return null;
	}

	private bool TryThrowUnit(out ProjectileAttack attack, Vector3 target, GameObject source, GameObject thrown, float powerMultiplier = 1f, float inaccuracy = 0f)
	{
		attack = null;
		if (thrown == null || thrown.GetComponent<Locomotion4>() == null)
		{
			return false;
		}
		Locomotion4 component = thrown.GetComponent<Locomotion4>();
		Rigidbody component2 = component.FlailingCollider.GetComponent<Rigidbody>();
		GameObject gameObject = component2.gameObject;
		thrown.transform.parent = null;
		Locomotion4 component3 = source.GetComponent<Locomotion4>();
		if (component3 != null && component3.ArcheryStartPoint != null)
		{
			Transform archeryStartPoint = component3.ArcheryStartPoint;
			thrown.transform.position = source.transform.TransformPoint(archeryStartPoint.localPosition);
			thrown.transform.rotation = archeryStartPoint.rotation;
		}
		Collider[] componentsInChildren = thrown.GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (!collider.enabled)
			{
				continue;
			}
			Collider[] componentsInChildren2 = source.GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider2 in componentsInChildren2)
			{
				if (collider2.enabled)
				{
					Physics.IgnoreCollision(collider, collider2, ignore: true);
				}
			}
		}
		Vector3 relativeTarget = target - thrown.transform.position;
		float power = Mathf.Min(ProjectilePhysics.ComputeFiringVelocity(relativeTarget, PreferredAngle).magnitude, Power * powerMultiplier);
		float elevationAngle = ProjectilePhysics.ComputeElevationAngle(power, relativeTarget);
		Vector3 velocity = ProjectilePhysics.ComputeFiringVelocity(relativeTarget, elevationAngle);
		velocity = Projectile.GetInaccurateVelocity(velocity, inaccuracy);
		CharacterProjectile characterProjectile = gameObject.AddComponent<CharacterProjectile>();
		characterProjectile.MakeAttack(source);
		component.DoGetThrown(velocity);
		characterProjectile.CollisionSignal.ConnectOnce(delegate
		{
			if (!(thrown == null) && !(source == null))
			{
				Collider[] componentsInChildren3 = thrown.GetComponentsInChildren<Collider>(includeInactive: true);
				foreach (Collider collider3 in componentsInChildren3)
				{
					if (collider3.enabled)
					{
						Collider[] componentsInChildren4 = source.GetComponentsInChildren<Collider>(includeInactive: true);
						foreach (Collider collider4 in componentsInChildren4)
						{
							if (collider4.enabled)
							{
								Physics.IgnoreCollision(collider3, collider4, ignore: false);
							}
						}
					}
				}
			}
		});
		attack = characterProjectile.GetAttack();
		return true;
	}

	private bool TryThrowThing(out ProjectileAttack attack, Vector3 target, GameObject source, GameObject thrown, float powerMultiplier = 1f, float inaccuracy = 0f)
	{
		attack = null;
		if (thrown == null)
		{
			return false;
		}
		thrown.transform.parent = null;
		Collider[] componentsInChildren = thrown.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (!collider.enabled)
			{
				continue;
			}
			Collider[] componentsInChildren2 = source.GetComponentsInChildren<Collider>();
			foreach (Collider collider2 in componentsInChildren2)
			{
				if (collider2.enabled)
				{
					Physics.IgnoreCollision(collider, collider2, ignore: true);
				}
			}
		}
		thrown.GetComponent<Rigidbody>().isKinematic = false;
		thrown.GetComponent<Rigidbody>().useGravity = true;
		Vector3 initialVelocity = ProjectilePhysics.ComputeFiringVelocity(Power * powerMultiplier, target - base.transform.position);
		if (Architecte.editorMode)
		{
			ProjectilePhysics.DebugTrajectory(ProjectilePhysics.ComputeTrajectory(base.transform.position, initialVelocity, target, 20), Color.blue);
		}
		PhysicalizedProjectile physicalizedProjectile = thrown.AddComponent<PhysicalizedProjectile>();
		physicalizedProjectile.Thrust(initialVelocity, inaccuracy);
		physicalizedProjectile.CollisionSignal.ConnectOnce(delegate
		{
			Collider[] componentsInChildren3 = thrown.GetComponentsInChildren<Collider>();
			foreach (Collider collider3 in componentsInChildren3)
			{
				if (collider3.enabled)
				{
					Collider[] componentsInChildren4 = source.GetComponentsInChildren<Collider>();
					foreach (Collider collider4 in componentsInChildren4)
					{
						if (collider4.enabled)
						{
							Physics.IgnoreCollision(collider3, collider4, ignore: false);
						}
					}
				}
			}
		});
		attack = physicalizedProjectile.GetAttack();
		return true;
	}

	private bool TryThrowBullet(out ProjectileAttack attack, Vector3 origin, Vector3 aimOrigin, Vector3 target, GameObject source, Location targetLocation, float powerMultiplier = 1f, float inaccuracy = 0f, float hitPointDamage = 60f)
	{
		attack = null;
		if (ammoAssetKey.IsNullOrInvalid())
		{
			return false;
		}
		KinematicProjectile kinematicProjectile = KinematicProjectile.FireAt(ammoAssetKey, source, origin, aimOrigin, targetLocation, target, Power * powerMultiplier, inaccuracy, Mathf.RoundToInt(hitPointDamage));
		MagicProjectile component = kinematicProjectile.GetComponent<MagicProjectile>();
		if ((bool)component)
		{
			component.Init(origin, targetLocation, 30f);
		}
		NetworkServer.Spawn(kinematicProjectile.gameObject, ammoAssetKey.AssetId);
		attack = kinematicProjectile.GetAttack();
		SpendStamina(source);
		AmmoCountGetter--;
		return true;
	}

	private void SpendStamina(GameObject source)
	{
		BricktronDamageReceiver component = source.GetComponent<BricktronDamageReceiver>();
		if ((bool)component)
		{
			component.CurrentStamina -= StaminaCost;
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
