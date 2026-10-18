using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.Audio;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Game.Rules;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class KinematicProjectile : Projectile
{
	public delegate void HitTargetEvent();

	public enum AttackArchetype
	{
		ThrownRock,
		MagicArrow,
		Arrow,
		Bolt,
		WarlockBlast,
		ArtificerShot,
		AlchemistBomb
	}

	public Signal<Vector3> ImpactSignal = new Signal<Vector3>();

	[SerializeField]
	private MagicProjectile magic;

	[SerializeField]
	private Rigidbody rigidBody;

	[SerializeField]
	private Collider garnotteCollider;

	[SerializeField]
	private Sticky sticky;

	[SerializeField]
	private TimedDestruction destruction;

	private ProjectileAttack Attack = new ProjectileAttack();

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool StartedMoving;

	public bool DestroyOnHit;

	public bool UseMagicProjectileComponent;

	public bool IgnoreOtherCollisions;

	public AttackArchetype Archetype = AttackArchetype.Arrow;

	[SerializeField]
	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	[SyncVar]
	protected Vector3 FiredPosition;

	[SerializeField]
	[SyncVar]
	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	protected Vector3 FiredVelocity;

	[SyncVar]
	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	[SerializeField]
	protected GameObject Attacker;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public GameObject Target;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 AnticipatedHitPosition;

	[SerializeField]
	private Explosifs explosif;

	[SyncVar(hook = "physicalized_hook")]
	private bool physicalize;

	[SerializeField]
	private string OnHitSoundEvent;

	private Vector3 aimOrigin;

	private const int TrajectoryAnticipationIterationCount = 1;

	private NetworkInstanceId ___AttackerNetId;

	private static int kEventEventHitTarget;

	private GameObject Origin => Attacker;

	public Vector3 NetworkFiredPosition
	{
		get
		{
			return FiredPosition;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref FiredPosition, 1u);
		}
	}

	public Vector3 NetworkFiredVelocity
	{
		get
		{
			return FiredVelocity;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref FiredVelocity, 2u);
		}
	}

	public GameObject NetworkAttacker
	{
		get
		{
			return Attacker;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref Attacker, 4u, ref ___AttackerNetId);
		}
	}

	public bool Networkphysicalize
	{
		get
		{
			return physicalize;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref physicalize;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				physicalized_hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 8u);
		}
	}

	[SyncEvent]
	private event HitTargetEvent EventHitTarget;

	private void OnHitTargetEvent()
	{
		if (!string.IsNullOrEmpty(OnHitSoundEvent))
		{
			SoundEngine.Play(OnHitSoundEvent, base.gameObject);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (NetworkServer.active)
		{
			NetworkServer.UnSpawn(base.gameObject);
		}
		StartedMoving = false;
		NetworkFiredPosition = Vector3.zero;
		NetworkFiredVelocity = Vector3.zero;
		NetworkAttacker = null;
		Target = null;
		AnticipatedHitPosition = Vector3.zero;
		if ((bool)destruction)
		{
			destruction.enabled = false;
		}
		base.enabled = true;
		Attack.Reset();
	}

	public override void Initialize()
	{
		base.Initialize();
		if ((bool)destruction)
		{
			destruction.enabled = false;
		}
		EventHitTarget += OnHitTargetEvent;
	}

	public static KinematicProjectile FireAt(Factory.AssetKey ammoAssetKey, GameObject attacker, Vector3 origin, Vector3 aimOrigin, Location target, Vector3 anticipatedTargetPosition, float power, float inaccuracy, int hitPointDamage)
	{
		Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, attacker.transform.forward);
		ObjectPoolSingleton.RequestComponent<KinematicProjectile>(out var t, ammoAssetKey, origin, rotation);
		Vector3 vector = ProjectilePhysics.ComputeFiringVelocity(power, anticipatedTargetPosition - origin);
		if (Architecte.editorMode)
		{
			ProjectilePhysics.DebugTrajectory(ProjectilePhysics.ComputeTrajectory(origin, vector, anticipatedTargetPosition, 20), Color.green);
		}
		Vector3 inaccurateVelocity = Projectile.GetInaccurateVelocity(vector, inaccuracy);
		t.InitBeforeSpawn(origin, aimOrigin, inaccurateVelocity, attacker, target.Object, anticipatedTargetPosition, hitPointDamage);
		t.enabled = true;
		return t;
	}

	private void InitBeforeSpawn(Vector3 origin, Vector3 aimOrigin, Vector3 velocity, GameObject attacker, GameObject target, Vector3 anticipatedHitPosition, int hitPointDamage = -1)
	{
		NetworkAttacker = attacker;
		Target = target;
		this.aimOrigin = aimOrigin;
		NetworkFiredPosition = origin;
		NetworkFiredVelocity = velocity;
		if (CheckStartCollision(attacker.transform, aimOrigin, origin, out var hit))
		{
			velocity = ProjectilePhysics.ComputeFiringVelocity(velocity.magnitude, hit.point - attacker.transform.position);
			origin = hit.point;
		}
		base.transform.position = origin;
		base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, velocity);
		NetworkFiredPosition = origin;
		NetworkFiredVelocity = velocity;
		AnticipatedHitPosition = anticipatedHitPosition;
		(Archetype switch
		{
			AttackArchetype.ThrownRock => AttackTemplate.ThrownRock, 
			AttackArchetype.MagicArrow => AttackTemplate.MagicProjectile, 
			AttackArchetype.Bolt => AttackTemplate.Bolt, 
			AttackArchetype.WarlockBlast => AttackTemplate.WarlockBlast, 
			AttackArchetype.ArtificerShot => AttackTemplate.ArtificerShot, 
			AttackArchetype.AlchemistBomb => AttackTemplate.AlchemistBomb, 
			_ => AttackTemplate.Arrow, 
		}).CopyTo(ref Attack, this, attacker);
	}

	public override Vector3 GetVelocity()
	{
		if ((bool)magic && UseMagicProjectileComponent)
		{
			return magic.GetVelocity();
		}
		return ProjectilePhysics.ComputeVelocity(FiredVelocity, base.transform.position - FiredPosition);
	}

	public override Vector3 GetImpactVector()
	{
		if ((bool)garnotteCollider)
		{
			return GetVelocity().Flatten().normalized;
		}
		return GetVelocity().normalized;
	}

	public override ProjectileAttack GetAttack()
	{
		return Attack;
	}

	public Vector3 GetAnticipatedTargetPosition(ref Vector3 firingVelocity)
	{
		if (Target == null)
		{
			return AnticipatedHitPosition;
		}
		Locomotion4 component = Target.GetComponent<Locomotion4>();
		if (component == null)
		{
			return AnticipatedHitPosition;
		}
		Vector3 position = base.transform.position;
		Vector3 anticipatedHitPosition = AnticipatedHitPosition;
		firingVelocity = ProjectilePhysics.ComputeFiringVelocity(firingVelocity.magnitude, anticipatedHitPosition - position);
		float projectedTime = ProjectilePhysics.ComputeFlightDuration(firingVelocity, anticipatedHitPosition - position);
		for (int i = 0; i < 1; i++)
		{
			Vector3 projectedPosition = component.GetProjectedPosition(projectedTime);
			firingVelocity = ProjectilePhysics.ComputeFiringVelocity(firingVelocity.magnitude, projectedPosition - position);
			projectedTime = ProjectilePhysics.ComputeFlightDuration(firingVelocity, projectedPosition - position);
		}
		Vector3 projectedPosition2 = component.GetProjectedPosition(projectedTime);
		if (Architecte.editorMode)
		{
			ProjectilePhysics.DebugTrajectory(ProjectilePhysics.ComputeTrajectory(position, firingVelocity, projectedPosition2, 20), Color.green);
		}
		return projectedPosition2;
	}

	private void StartMoving()
	{
		StartedMoving = true;
		if ((bool)garnotteCollider)
		{
			garnotteCollider.enabled = true;
		}
		if (rigidBody != null)
		{
			rigidBody.detectCollisions = false;
			rigidBody.isKinematic = true;
		}
		AnticipatedHitPosition = GetAnticipatedTargetPosition(ref FiredVelocity);
		if (NetworkServer.active && CheckStartCollision(Attacker.transform, aimOrigin, FiredPosition, out var hit))
		{
			HandleCollision(hit);
		}
	}

	private void Update()
	{
		if (FiredPosition == Vector3.zero)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (!(deltaTime <= 0f))
		{
			if (!StartedMoving)
			{
				StartMoving();
			}
			DeleteIfOutOfBounds();
			if ((rigidBody == null || rigidBody.isKinematic) && !CheckCollisions(deltaTime))
			{
				Move(deltaTime);
			}
		}
	}

	private void DeleteIfOutOfBounds()
	{
		if (base.transform.position.y <= -16f)
		{
			NetworkServer.UnSpawn(base.gameObject);
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	private bool AcceptCollision(RaycastHit hit)
	{
		if (IgnoreOtherCollisions && !Target.IsNullOrReleased())
		{
			GameObject gameObject = ProjectileAiming.GetSignificantParent(hit.collider).gameObject;
			if (gameObject == Target)
			{
				return true;
			}
			return Vector3.Dot((Target.transform.position - hit.point).Flatten(), FiredVelocity.Flatten()) < 0f;
		}
		if (CombatRules.ArrowShootThroughAllies && Attacker != null)
		{
			GameObject gameObject2 = ProjectileAiming.GetSignificantParent(hit.collider).gameObject;
			if (gameObject2 == Target)
			{
				return true;
			}
			bool flag = Affiliation.IsAllied(Attacker, gameObject2);
			return !flag;
		}
		return true;
	}

	private bool CheckStartCollision(Transform attacker, Vector3 aimOrigin, Vector3 projectileOrigin, out RaycastHit hit)
	{
		Ray ray = new Ray(aimOrigin, projectileOrigin - aimOrigin);
		float num = (projectileOrigin - aimOrigin).magnitude;
		while (num > 0f)
		{
			if (!Physics.Raycast(ray, out hit, num, LayerCollisionMask, QueryTriggerInteraction.Ignore))
			{
				return false;
			}
			float num2 = hit.distance + 0.01f;
			num -= num2;
			ray.origin += ray.direction * num2;
			if (hit.transform.IsChildOf(attacker) || !AcceptCollision(hit))
			{
				continue;
			}
			return true;
		}
		hit = default(RaycastHit);
		return false;
	}

	private bool CheckCollisions(float deltaTime)
	{
		if (!NetworkServer.active)
		{
			return false;
		}
		Vector3 velocity = GetVelocity();
		float distanceMax = velocity.magnitude * deltaTime;
		ProjectilePhysics.CollisionTrajectory collisionTrajectory = ProjectilePhysics.ComputeExpectedCollision(base.transform.position, velocity, 0f, distanceMax, 1f, LayerCollisionMask, (RaycastHit h, KinematicProjectile self) => self.AcceptCollision(h), this);
		if (!collisionTrajectory.IsNull)
		{
			HandleCollision(collisionTrajectory.hit);
			return true;
		}
		return false;
	}

	private void HandleCollision(RaycastHit hit)
	{
		if (HittingItself(hit))
		{
			return;
		}
		ProcessCollision(hit);
		ImpactSignal.Invoke(hit.point);
		CallEventHitTarget();
		if (DestroyOnHit)
		{
			NetworkServer.UnSpawn(base.gameObject);
			if (!base.gameObject.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(base.gameObject);
			}
		}
		else
		{
			Deactivate();
		}
	}

	private bool HittingItself(RaycastHit hit)
	{
		return Origin != null && hit.collider.transform.IsChildOf(Origin.transform);
	}

	protected virtual void Move(float deltaTime)
	{
		if (this == null)
		{
			return;
		}
		if ((bool)magic && UseMagicProjectileComponent)
		{
			magic.Move(deltaTime);
			return;
		}
		Vector3 currentVelocity = GetVelocity();
		base.transform.position = ProjectilePhysics.ComputeNextPosition(base.transform.position, ref currentVelocity, deltaTime);
		if (currentVelocity.sqrMagnitude >= 0.0001f)
		{
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, currentVelocity);
		}
	}

	public void MoveToImpact(RaycastHit hit)
	{
		float value = ProjectilePhysics.ComputeFlightDuration(GetVelocity(), hit.point - base.transform.position);
		value = Mathf.Clamp(value, 0f, Time.fixedDeltaTime);
		Move(value);
	}

	private void ProcessCollision(RaycastHit hit)
	{
		DamageReceiver damageReceiver = SignificantParent<DamageReceiver>.Of(hit);
		if (!Attack.Attacker.IsNullOrReleased())
		{
			RevealAttackerToTargets(Attack.Attacker);
		}
		Attack.Apply(damageReceiver);
		if ((bool)explosif)
		{
			explosif.Explode();
		}
		else if (!sticky || !sticky.TryStick(hit, damageReceiver))
		{
			Physicalize();
		}
		PlayArcherSound(hit, damageReceiver);
	}

	private void RevealAttackerToTargets(GameObject attacker)
	{
		LocalFactionVision component = attacker.GetComponent<LocalFactionVision>();
		if (component == null)
		{
			return;
		}
		foreach (TrackingRegistry.TrackingEntry collision in TrackingRegistry.GetCollisions(base.transform.position, 20f, TrackingLayer.Character, component.faction.EnemyOrNeutralFactions))
		{
			if (!collision.GameObject.IsNullOrReleased())
			{
				component.TemporaryRevealForFaction(collision.gameComponent.faction, 5f);
				Labor component2 = collision.GameObject.GetComponent<Labor>();
				if (component2 != null)
				{
					component2.TemporaryFocusOnAttacker(attacker, 5f);
				}
			}
		}
	}

	private void PlayArcherSound(RaycastHit hit, DamageReceiver victim)
	{
		if (!(Origin == null))
		{
			Occupation component = Origin.GetComponent<Occupation>();
			if (component != null && component.CurrentOccupation == Occupation.Type.Archer)
			{
				BricktronSoundEffects sounds = component.Profession.Sounds;
				bool flag = ShotWasSuccessful(hit, victim);
				SoundEngine.Play(sounds.ArrowHit, Origin);
				SoundEngine.Play((!flag) ? sounds.TargetMiss : sounds.TargetHit, Origin);
			}
		}
	}

	private bool ShotWasSuccessful(RaycastHit hit, DamageReceiver victim)
	{
		if (Target == null)
		{
			return Vector3.SqrMagnitude(hit.point - AnticipatedHitPosition) < 0.25f;
		}
		return victim != null && victim.GameObject == Target;
	}

	public void Deactivate()
	{
		if ((bool)destruction && NetworkServer.active)
		{
			destruction.enabled = true;
		}
		base.enabled = false;
	}

	private void physicalized_hook(bool value)
	{
		Networkphysicalize = value;
		if (physicalize)
		{
			Physicalize();
		}
	}

	public void Physicalize()
	{
		if ((bool)garnotteCollider)
		{
			garnotteCollider.enabled = true;
		}
		if (rigidBody != null)
		{
			rigidBody.isKinematic = false;
			rigidBody.detectCollisions = true;
		}
		Deactivate();
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventHitTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Event EventHitTarget called on server.");
		}
		else if (((KinematicProjectile)obj).EventHitTarget != null)
		{
			((KinematicProjectile)obj).EventHitTarget();
		}
	}

	public void CallEventHitTarget()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Event EventHitTarget called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventHitTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendEventInternal(networkWriter, 0, "EventHitTarget");
	}

	static KinematicProjectile()
	{
		kEventEventHitTarget = -1487936778;
		NetworkBehaviour.RegisterEventDelegate(typeof(KinematicProjectile), kEventEventHitTarget, InvokeSyncEventEventHitTarget);
		NetworkCRC.RegisterBehaviour("KinematicProjectile", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(FiredPosition);
			writer.Write(FiredVelocity);
			writer.Write(Attacker);
			writer.Write(physicalize);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(FiredPosition);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(FiredVelocity);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(Attacker);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(physicalize);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			FiredPosition = reader.ReadVector3();
			FiredVelocity = reader.ReadVector3();
			___AttackerNetId = reader.ReadNetworkId();
			physicalize = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			FiredPosition = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			FiredVelocity = reader.ReadVector3();
		}
		if ((num & 4) != 0)
		{
			Attacker = reader.ReadGameObject();
		}
		if ((num & 8) != 0)
		{
			physicalized_hook(reader.ReadBoolean());
		}
	}

	public override void PreStartClient()
	{
		if (!___AttackerNetId.IsEmpty())
		{
			NetworkAttacker = ClientScene.FindLocalObject(___AttackerNetId);
		}
	}
}
