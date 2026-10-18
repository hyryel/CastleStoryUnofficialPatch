using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Rules;
using Brix.Utils.Factories;
using Brix.Utils.Locomotion;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Wards;

public class WardAttack : ProjectileWeapon
{
	private Trackable myTarget;

	public Animator animator;

	public ParticleSystem fireParticles;

	public ParticleSystem overheatParticles;

	public float HeightOffset;

	public float AttacksPerSecond;

	public float ProjectileSpeedMultiplier = 1f;

	public bool AimLikeAnArcher;

	public float HeatPerShot = 0.05f;

	public float CooldownPerSecond = 0.2f;

	public float WakeUpDelay = 3f;

	public float GlowIntensity = 0.485f;

	[SyncVar(hook = "OnHeatChanged")]
	private float _currentHeat;

	public Signal HeatChangedSignal = new Signal();

	private bool _overheated;

	private bool CoolingDown = true;

	private bool Sleeping = true;

	private float InactiveRotationSpeed;

	private float StandbyRotationSpeed = 0.3f;

	private float TargetingRotationSpeed = 0.5f;

	private ProjectileAiming _aiming;

	[SyncVar(hook = "OnSpeedChanged")]
	private float anim_speed;

	[SyncVar(hook = "OnGlowChanged")]
	private bool anim_glow;

	[SyncVar(hook = "OnOverheatChanged")]
	private bool anim_overheat;

	[SyncVar(hook = "OnFireChanged")]
	private bool anim_fire;

	private WardController _controller;

	public int ProjectileLayerMask;

	private float lastAttack;

	private float lastCooldown;

	private float lastWakeUp;

	private float nextWakeUpDelay;

	private static readonly RaycastHit[] Hits = new RaycastHit[20];

	public float AttackRange => _controller.EffectRange;

	public float CurrentHeat => _currentHeat;

	public bool IsHeated => _currentHeat != 0f;

	public bool IsOverHeated => _overheated;

	public bool IsNewlyOverHeated => _overheated && !CoolingDown;

	private Vector3 FiringPosition => base.transform.position + HeightOffset * Vector3.up;

	public float Network_currentHeat
	{
		get
		{
			return _currentHeat;
		}
		[param: In]
		set
		{
			ref float currentHeat = ref _currentHeat;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnHeatChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref currentHeat, 1u);
		}
	}

	public float Networkanim_speed
	{
		get
		{
			return anim_speed;
		}
		[param: In]
		set
		{
			ref float fieldValue = ref anim_speed;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnSpeedChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	public bool Networkanim_glow
	{
		get
		{
			return anim_glow;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref anim_glow;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnGlowChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 4u);
		}
	}

	public bool Networkanim_overheat
	{
		get
		{
			return anim_overheat;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref anim_overheat;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnOverheatChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 8u);
		}
	}

	public bool Networkanim_fire
	{
		get
		{
			return anim_fire;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref anim_fire;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnFireChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 16u);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		ProjectileLayerMask = Factory.Peek(ammoAssetKey).GetComponent<Projectile>().LayerCollisionMask;
		_aiming = new ProjectileAiming(base.transform);
	}

	private void TargetChanged()
	{
		SoundEngine.Play("Sentinel_Ward_Spot_Enemy", base.gameObject);
	}

	protected override void Awake()
	{
		base.Awake();
		_controller = GetComponent<WardController>();
		nextWakeUpDelay = WakeUpDelay;
	}

	private void OnHeatChanged(float heat)
	{
		Network_currentHeat = heat;
		HeatChangedSignal.Invoke();
	}

	private void OnSpeedChanged(float speed)
	{
		Networkanim_speed = speed;
		animator.SetFloat("Speed", speed);
	}

	private void OnGlowChanged(bool glow)
	{
		Networkanim_glow = glow;
		animator.SetBool("Glow", glow);
		animator.SetFloat("Glow Amount", (!glow) ? 0f : GlowIntensity);
		SoundEngine.Play("Sentinel_Ward_Charge_up", base.gameObject);
	}

	private void OnOverheatChanged(bool overheat)
	{
		Networkanim_overheat = overheat;
		if (overheat)
		{
			overheatParticles.Play();
			SoundEngine.Play("Sentinel_Ward_Overheat_Start", base.gameObject);
		}
		else
		{
			overheatParticles.Stop();
			SoundEngine.Play("Sentinel_Ward_Overheat_Stop", base.gameObject);
		}
	}

	private void OnFireChanged(bool nobodyCares)
	{
		animator.SetTrigger("Fire");
		SoundEngine.Play("Sentinel_Ward_Shoot", base.gameObject);
		fireParticles.Play();
	}

	public void UnitsRefresh(HashSet<Trackable> units)
	{
		if (!myTarget.IsNullOrReleased() && (!units.Contains(myTarget) || !CanFireAt(myTarget.gameObject)))
		{
			myTarget = null;
		}
		if (myTarget.IsNullOrReleased())
		{
			Trackable trackable = units.Slinq().Where((Trackable u, WardAttack self) => self.CanFireAt(u.gameObject), this).FirstOrDefault();
			if ((bool)trackable)
			{
				myTarget = trackable;
				TargetChanged();
			}
		}
	}

	private void FixedUpdate()
	{
		bool flag = anim_glow;
		bool flag2 = anim_overheat;
		if (CoolingDown && (double)Time.fixedTime > (double)lastCooldown + 0.05 && CurrentHeat > 0f)
		{
			Network_currentHeat = Mathf.Max(CurrentHeat - CooldownPerSecond * 0.05f, 0f);
			lastCooldown = Time.fixedTime;
		}
		if (_overheated && CurrentHeat <= 0f)
		{
			_overheated = false;
			flag2 = false;
			if (!myTarget.IsNullOrReleased())
			{
				flag = true;
			}
		}
		if (Time.fixedTime > lastAttack + 1f / AttacksPerSecond)
		{
			if (!myTarget.IsNullOrReleased())
			{
				if (Sleeping)
				{
					lastWakeUp = Time.fixedTime;
					Sleeping = false;
					Networkanim_speed = TargetingRotationSpeed;
					flag = true;
					nextWakeUpDelay = WakeUpDelay + UnityEngine.Random.Range(-0.25f, 0.25f);
				}
				else if (!_overheated && Time.fixedTime > lastWakeUp + nextWakeUpDelay)
				{
					lastAttack = Time.fixedTime;
					Location location = Location.Of(myTarget.gameObject);
					Fire(FiringPosition, FiringPosition, location.RangeHitPosition, base.gameObject, location, null);
					Networkanim_fire = true;
					CoolingDown = false;
					Network_currentHeat = _currentHeat + HeatPerShot;
					if (CurrentHeat >= 1f)
					{
						_overheated = true;
						flag = false;
						flag2 = true;
					}
				}
				else
				{
					CoolingDown = true;
				}
			}
			else
			{
				CoolingDown = true;
				Sleeping = true;
				Networkanim_speed = StandbyRotationSpeed;
				flag = false;
			}
		}
		if (anim_glow != flag)
		{
			Networkanim_glow = flag;
		}
		if (anim_overheat != flag2)
		{
			Networkanim_overheat = flag2;
		}
	}

	private bool CanFireAt(GameObject unit)
	{
		DamageReceiver component = unit.GetComponent<DamageReceiver>();
		if (!component || component.IsInvincible)
		{
			return false;
		}
		if (Vector3.Distance(unit.transform.position, base.transform.position) > AttackRange)
		{
			return false;
		}
		if (!LocalFactionVision.ObjectIsVisibleForFaction(unit, faction))
		{
			return false;
		}
		if (!CheckLinesOfSight(ProjectileLayerMask, unit))
		{
			return false;
		}
		return true;
	}

	private bool CheckLinesOfSight(int layerMask, GameObject targetGameObject)
	{
		if (AimLikeAnArcher)
		{
			_aiming.SetupFire(Location.Of(targetGameObject), this, ProjectileSpeedMultiplier, avoidAllies: true);
			return !_aiming.IsProjectileObstructed(FiringPosition, layerMask);
		}
		Vector3 firingPosition = FiringPosition;
		Vector3 rangeHitPosition = Location.Of(targetGameObject).RangeHitPosition;
		if (CheckLineOfSight(firingPosition, rangeHitPosition, layerMask, targetGameObject))
		{
			return true;
		}
		Vector3 vector = GetCornerAdjustment(firingPosition.Rounded(), rangeHitPosition.Rounded()) * 0.99f;
		if (vector != Vector3.zero)
		{
			if (CheckLineOfSight(firingPosition + vector, rangeHitPosition, layerMask, targetGameObject))
			{
				return true;
			}
			if (CheckLineOfSight(firingPosition, rangeHitPosition - vector, layerMask, targetGameObject))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckLineOfSight(Vector3 firingPosition, Vector3 targetPosition, int layerMask, GameObject targetGameObject = null)
	{
		Vector3 vector = targetPosition - firingPosition;
		int num = Physics.RaycastNonAlloc(new Ray(firingPosition, vector.normalized), Hits, vector.magnitude, layerMask, QueryTriggerInteraction.Ignore);
		bool result = ReachesTarget(Hits, num, targetGameObject);
		Array.Clear(Hits, 0, num);
		return result;
	}

	private bool ReachesTarget(RaycastHit[] hits, int nHits, GameObject target)
	{
		for (int i = 0; i < nHits; i++)
		{
			RaycastHit hit = hits[i];
			if (!(hit.collider == null))
			{
				GameObject gameObject = SignificantParent.Of(hit);
				if (gameObject != target && !CanShootThrough(gameObject))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool CanShootThrough(GameObject go)
	{
		if (!CombatRules.ArrowShootThroughAllies)
		{
			return false;
		}
		return go.WithComponent((Labor l) => l.faction.IsAlliedTo(base.gameObject), _default: false) || go.WithComponent((WardAttack c) => c.faction.IsAlliedTo(base.gameObject), _default: false);
	}

	private static Vector3 GetCornerAdjustment(Vector3 v, Vector3 lookAt)
	{
		Vector3 zero = Vector3.zero;
		if (!Mathf.Approximately(lookAt.x, v.x))
		{
			zero.x = ((!(lookAt.x < v.x)) ? 0.5f : (-0.5f));
		}
		if (!Mathf.Approximately(lookAt.z, v.z))
		{
			zero.z = ((!(lookAt.z < v.z)) ? 0.5f : (-0.5f));
		}
		return zero;
	}

	private void OnDisable()
	{
		Networkanim_speed = InactiveRotationSpeed;
		Networkanim_glow = false;
		overheatParticles.Stop();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_currentHeat);
			writer.Write(anim_speed);
			writer.Write(anim_glow);
			writer.Write(anim_overheat);
			writer.Write(anim_fire);
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
			writer.Write(_currentHeat);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(anim_speed);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(anim_glow);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(anim_overheat);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(anim_fire);
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
			_currentHeat = reader.ReadSingle();
			anim_speed = reader.ReadSingle();
			anim_glow = reader.ReadBoolean();
			anim_overheat = reader.ReadBoolean();
			anim_fire = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnHeatChanged(reader.ReadSingle());
		}
		if ((num & 2) != 0)
		{
			OnSpeedChanged(reader.ReadSingle());
		}
		if ((num & 4) != 0)
		{
			OnGlowChanged(reader.ReadBoolean());
		}
		if ((num & 8) != 0)
		{
			OnOverheatChanged(reader.ReadBoolean());
		}
		if ((num & 0x10) != 0)
		{
			OnFireChanged(reader.ReadBoolean());
		}
	}
}
