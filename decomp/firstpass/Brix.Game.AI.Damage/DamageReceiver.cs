using System;
using System.Runtime.InteropServices;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Utils.Unity;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI.Damage;

[JsonObject(MemberSerialization.OptIn)]
public abstract class DamageReceiver : UpdatableNetworkGameComponent, IDamageReceiver, IDisposable
{
	public delegate void DamageReactionDelegate(DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker);

	public string OnHitSoundSwitch;

	public Signal<DamageReaction> WillReceiveDamage = new Signal<DamageReaction>();

	public Signal HasReceivedDamage = new Signal();

	public Signal<float, float> HPChangedSignal = new Signal<float, float>();

	private Signal _onDeathSignal = new Signal();

	[SyncVar]
	[SerializeField]
	private float _maxHP = 3f;

	[SerializeField]
	public float _overkillThreshold;

	[SerializeField]
	[DisplayRules(DisplayFlags.HideInEditMode)]
	[SyncVar(hook = "OnHpChanged")]
	[JsonProperty]
	private float _hp;

	public RegenAura ExternalRegenerator;

	[SerializeField]
	private float _regenPerSecond = 1f;

	private float _projectileMultiplier = 1f;

	public GameObject[] StickyParts;

	public float _delayBeforeRegenPops = 10f;

	[JsonProperty]
	public SerializableTimestamp _lastHealTimestamp;

	[JsonProperty]
	protected SerializableTimestamp _lastDamageTimestamp;

	[JsonProperty]
	protected SerializableTimestamp _lastRegenTick;

	[SerializeField]
	private float _meleeResistance;

	[SerializeField]
	private float _piercingResistance;

	[SerializeField]
	private float _magicResistance;

	[SerializeField]
	private float _explosiveResistance;

	[SerializeField]
	private float _siegeResistance;

	private static int kEventEventClientDamageReaction;

	public IReadOnlySignal OnDeathSignal => _onDeathSignal;

	public float MaxHP
	{
		get
		{
			return _maxHP;
		}
		set
		{
			if (_maxHP != value)
			{
				float maxHP = _maxHP;
				Network_maxHP = value;
				CurrentHP = Mathf.Clamp(CurrentHP + (value - maxHP), 1f, value);
			}
		}
	}

	public float OverkillThreshold
	{
		get
		{
			return _overkillThreshold;
		}
		set
		{
			_overkillThreshold = value;
		}
	}

	public float CurrentHP
	{
		get
		{
			return _hp;
		}
		set
		{
			float hp = _hp;
			float num = Mathf.Clamp(value, 0f, MaxHP);
			if (num != hp)
			{
				Network_hp = num;
			}
		}
	}

	[LuaDrivable("hp")]
	private float lua_HP => _hp;

	public float RegenPerSecond
	{
		get
		{
			return _regenPerSecond;
		}
		set
		{
			_regenPerSecond = value;
		}
	}

	public float EffectiveRegenPerSecond => (!(ExternalRegenerator != null)) ? RegenPerSecond : Mathf.Max(ExternalRegenerator.RegenPerSecond, RegenPerSecond);

	public float ProjectileMultiplier
	{
		get
		{
			return _projectileMultiplier;
		}
		set
		{
			_projectileMultiplier = value;
		}
	}

	public bool IsDamaged => _hp != _maxHP;

	public virtual bool IsInvincible => false;

	public virtual CombatPriorityType CombatPriority => CombatPriorityType.OutOfCombat;

	protected override bool HasFixedUpdate => NetworkServer.active;

	public float Network_maxHP
	{
		get
		{
			return _maxHP;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _maxHP, 1u);
		}
	}

	public float Network_hp
	{
		get
		{
			return _hp;
		}
		[param: In]
		set
		{
			ref float hp = ref _hp;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnHpChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref hp, 2u);
		}
	}

	[SyncEvent]
	public event DamageReactionDelegate EventClientDamageReaction;

	protected virtual void luaOnHpChanged()
	{
	}

	protected void InvokeDamageReactionEvent(DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		CallEventClientDamageReaction(reaction, contact, force, lift, attacker);
	}

	private void OnHpChanged(float value)
	{
		float hp = _hp;
		Network_hp = value;
		if (hp != _hp && _lastDamageTimestamp != null)
		{
			HPChangedSignal.Invoke(hp, value);
			luaOnHpChanged();
		}
	}

	public bool HasStickyParts()
	{
		return StickyParts.Length != 0;
	}

	public virtual void Kill()
	{
		_onDeathSignal.Invoke();
	}

	public abstract DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force);

	public abstract DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null);

	public abstract void ApplyDamage(float value, GameObject attacker = null);

	public abstract void Heal(float value);

	protected override void Awake()
	{
		base.Awake();
		if (_lastDamageTimestamp == null)
		{
			Network_hp = _maxHP;
		}
		_lastHealTimestamp = _lastHealTimestamp ?? new SerializableTimestamp();
		_lastDamageTimestamp = _lastDamageTimestamp ?? new SerializableTimestamp();
		_lastRegenTick = _lastRegenTick ?? new SerializableTimestamp();
	}

	protected override void Start()
	{
		base.Start();
		_lastHealTimestamp.InitTimestamp();
		_lastDamageTimestamp.InitTimestamp();
		_lastRegenTick.InitTimestamp();
	}

	protected override void BrixFixedUpdate()
	{
		if (RegenPerSecond != 0f && _lastRegenTick.Delta >= 1f)
		{
			if (_regenPerSecond < 0f)
			{
				ApplyDamage(0f - _regenPerSecond);
			}
			else if (_lastDamageTimestamp.Delta > _delayBeforeRegenPops)
			{
				Heal(_regenPerSecond);
			}
			_lastRegenTick.Reset();
		}
	}

	protected float ApplyResistances(float damage, DamageType type)
	{
		return type switch
		{
			DamageType.Melee => damage * (1f - _meleeResistance), 
			DamageType.Piercing => damage * (1f - _piercingResistance), 
			DamageType.Magical => damage * (1f - _magicResistance), 
			DamageType.Explosive => damage * (1f - _explosiveResistance), 
			DamageType.Siege => damage * (1f - _siegeResistance), 
			_ => damage, 
		};
	}

	public virtual void Dispose()
	{
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventClientDamageReaction(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Event EventClientDamageReaction called on server.");
		}
		else if (((DamageReceiver)obj).EventClientDamageReaction != null)
		{
			((DamageReceiver)obj).EventClientDamageReaction((DamageReaction)reader.ReadInt32(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadSingle(), reader.ReadGameObject());
		}
	}

	public void CallEventClientDamageReaction(DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Event EventClientDamageReaction called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventClientDamageReaction);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write((int)reaction);
		networkWriter.Write(contact);
		networkWriter.Write(force);
		networkWriter.Write(lift);
		networkWriter.Write(attacker);
		SendEventInternal(networkWriter, 0, "EventClientDamageReaction");
	}

	static DamageReceiver()
	{
		kEventEventClientDamageReaction = -1359185119;
		NetworkBehaviour.RegisterEventDelegate(typeof(DamageReceiver), kEventEventClientDamageReaction, InvokeSyncEventEventClientDamageReaction);
		NetworkCRC.RegisterBehaviour("DamageReceiver", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_maxHP);
			writer.Write(_hp);
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
			writer.Write(_maxHP);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_hp);
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
			_maxHP = reader.ReadSingle();
			_hp = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			_maxHP = reader.ReadSingle();
		}
		if ((num & 2) != 0)
		{
			OnHpChanged(reader.ReadSingle());
		}
	}
}
