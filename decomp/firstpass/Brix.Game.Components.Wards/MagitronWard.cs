using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Locomotion;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Wards;

public class MagitronWard : ProjectileWeapon
{
	public delegate void TargetChangedDelegate();

	private HashSet<DamageReceiver> _targets = new HashSet<DamageReceiver>();

	public float HeightOffset;

	public float AttacksPerSecond;

	public float HeatPerShot = 0.05f;

	public float CooldownPerSecond = 0.2f;

	public float WakeUpDelay = 2f;

	public float GlowIntensity = 0.485f;

	[SyncVar(hook = "OnHeatChanged")]
	private float _currentHeat;

	public Signal HeatChangedSignal = new Signal();

	private bool Overheated;

	private bool CoolingDown = true;

	private bool Sleeping = true;

	public int ProjectileLayerMask;

	private float lastAttack;

	private float lastCooldown;

	private float lastWakeUp;

	private static readonly RaycastHit[] Hits;

	private static int kEventEventTargetChanged;

	public float CurrentHeat => _currentHeat;

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

	[SyncEvent]
	public event TargetChangedDelegate EventTargetChanged;

	public override void Initialize()
	{
		base.Initialize();
		EventTargetChanged += TargetChanged;
		ProjectileLayerMask = Factory.Peek(ammoAssetKey).GetComponent<Projectile>().LayerCollisionMask;
	}

	private void TargetChanged()
	{
		SoundEngine.Play("Sentinel_Ward_Spot_Enemy", base.gameObject);
	}

	private void OnHeatChanged(float heat)
	{
		Network_currentHeat = heat;
		HeatChangedSignal.Invoke();
	}

	public void UnitsRefresh(HashSet<GameObject> units)
	{
		HashSet<DamageReceiver> hashSet = CollectionPool.RequestHashSet<DamageReceiver>();
		foreach (DamageReceiver target in _targets)
		{
			if (target.IsNullOrReleased() || !units.Contains(target.gameObject) || !CanFireAt(target.gameObject))
			{
				hashSet.Add(target);
			}
		}
		foreach (DamageReceiver item in hashSet)
		{
			_targets.Remove(item);
		}
		hashSet.Dispose();
		foreach (GameObject unit in units)
		{
			DamageReceiver component = unit.GetComponent<DamageReceiver>();
			if (!component.IsNullOrReleased() && !_targets.Contains(component) && CanFireAt(unit))
			{
				_targets.Add(component);
			}
		}
	}

	private void FixedUpdate()
	{
		if (CoolingDown && (double)Time.fixedTime > (double)lastCooldown + 0.05 && CurrentHeat > 0f)
		{
			Network_currentHeat = Mathf.Max(CurrentHeat - CooldownPerSecond * 0.05f, 0f);
			lastCooldown = Time.fixedTime;
		}
		if (Overheated && CurrentHeat <= 0f)
		{
			Overheated = false;
		}
		if (!(Time.fixedTime > lastAttack + 1f / AttacksPerSecond))
		{
			return;
		}
		if (_targets.Count >= 3)
		{
			if (Sleeping)
			{
				lastWakeUp = Time.fixedTime;
				Sleeping = false;
			}
			else if (!Overheated && Time.fixedTime > lastWakeUp + WakeUpDelay)
			{
				lastAttack = Time.fixedTime;
				foreach (DamageReceiver target in _targets)
				{
					if (!target.IsNullOrReleased())
					{
						Location location = Location.Of(target.gameObject);
						Fire(FiringPosition, FiringPosition, location.RangeHitPosition, base.gameObject, location, null);
						Network_currentHeat = _currentHeat + HeatPerShot;
					}
				}
				CoolingDown = false;
				if (CurrentHeat >= 1f)
				{
					Overheated = true;
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
		}
	}

	private bool CanFireAt(GameObject unit)
	{
		DamageReceiver component = unit.GetComponent<DamageReceiver>();
		if (!faction.IsEnemyTo(unit))
		{
			return false;
		}
		if (!component || component.IsInvincible)
		{
			return false;
		}
		float num = Vector3.Distance(unit.transform.position, base.transform.position);
		if (num > MaxRange || num < MinRange)
		{
			return false;
		}
		if (!LocalFactionVision.ObjectIsVisibleForFaction(unit, faction))
		{
			return false;
		}
		if (!CheckLinesOfSight(Location.Of(unit).RangeHitPosition, ProjectileLayerMask, unit))
		{
			return false;
		}
		return true;
	}

	private bool CheckLinesOfSight(Vector3 targetPosition, int layerMask, GameObject targetGameObject)
	{
		Vector3 firingPosition = FiringPosition;
		if (CheckLineOfSight(firingPosition, targetPosition, layerMask, targetGameObject))
		{
			return true;
		}
		Vector3 vector = GetCornerAdjustment(firingPosition.Rounded(), targetPosition.Rounded()) * 0.99f;
		if (vector != Vector3.zero)
		{
			if (CheckLineOfSight(firingPosition + vector, targetPosition, layerMask, targetGameObject))
			{
				return true;
			}
			if (CheckLineOfSight(firingPosition, targetPosition - vector, layerMask, targetGameObject))
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
				if (gameObject != target && !gameObject.WithComponent((Labor l) => l.faction.IsAlliedTo(base.gameObject), _default: false))
				{
					return false;
				}
			}
		}
		return true;
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

	static MagitronWard()
	{
		Hits = new RaycastHit[20];
		kEventEventTargetChanged = 368508180;
		NetworkBehaviour.RegisterEventDelegate(typeof(MagitronWard), kEventEventTargetChanged, InvokeSyncEventEventTargetChanged);
		NetworkCRC.RegisterBehaviour("MagitronWard", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventTargetChanged(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Event EventTargetChanged called on server.");
		}
		else if (((MagitronWard)obj).EventTargetChanged != null)
		{
			((MagitronWard)obj).EventTargetChanged();
		}
	}

	public void CallEventTargetChanged()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Event EventTargetChanged called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventTargetChanged);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendEventInternal(networkWriter, 0, "EventTargetChanged");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_currentHeat);
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
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnHeatChanged(reader.ReadSingle());
		}
	}
}
