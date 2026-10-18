using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Lua;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI.Damage;

public class BricktronDamageReceiver : DamageReceiver
{
	private static readonly UpdatableNetworkProfiler<BricktronDamageReceiver> UpdatableProfiler = new UpdatableNetworkProfiler<BricktronDamageReceiver>();

	[SerializeField]
	private Locomotion4 Locomotion;

	[SerializeField]
	private Labor Labor;

	[SerializeField]
	private LuaDriver Ld;

	public float RequiredStaminaForShieldBlock = 1f;

	public float RequiredStaminaForShieldBlockWhileCranking = 2.5f;

	public float StaminaRegenPerSecond = 1f;

	public float RequiredExplosionForceForSmash = 1f;

	public Signal<float, float> StaminaChangedSignal = new Signal<float, float>();

	public bool Unwavering;

	[SyncVar]
	[SerializeField]
	private float _maxStamina = 5f;

	[JsonProperty]
	protected SerializableTimestamp _lastStaminaLostTimestamp;

	[JsonProperty]
	protected SerializableTimestamp _lastStaminaHealing;

	[JsonProperty]
	[SyncVar(hook = "OnStaminaChanged")]
	private float _stamina;

	[SerializeField]
	private float _meleeBlockResist;

	[SerializeField]
	private float _piercingBlockResist;

	[SerializeField]
	private float _magicBlockResist;

	[SerializeField]
	private float _explosiveBlockResist;

	[LuaDrivable("hk_OnKilled")]
	private static LuaHook<LuaDriver, LuaDriver> lua_OnKilled = new LuaHook<LuaDriver, LuaDriver>();

	[LuaDrivable("hk_OnStaminaChanged")]
	private static LuaHook<LuaDriver> lua_OnStaminaChanged = new LuaHook<LuaDriver>();

	[LuaDrivable("hk_OnHpChanged")]
	private static LuaHook<LuaDriver> lua_OnHpChanged = new LuaHook<LuaDriver>();

	[LuaDrivable("stamina")]
	private float lua_stamina;

	[LuaDrivable("hp")]
	private float lua_hp;

	[LuaDrivable("killable")]
	public bool lua_killable = true;

	[LuaDrivable("maxStamina")]
	private float lua_maxStamina;

	[LuaDrivable("maxHp")]
	private float lua_maxHp;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public override CombatPriorityType CombatPriority => Labor.Profession.CombatPriority;

	public float MaxStamina
	{
		get
		{
			return _maxStamina;
		}
		set
		{
			if (_maxStamina != value)
			{
				float maxStamina = _maxStamina;
				Network_maxStamina = value;
				CurrentStamina = Mathf.Clamp(CurrentStamina + (value - maxStamina), 1f, value);
			}
		}
	}

	public float CurrentStamina
	{
		get
		{
			return _stamina;
		}
		set
		{
			if (value > _stamina)
			{
				Network_stamina = Mathf.Min(value, MaxStamina);
				if (_lastStaminaHealing != null)
				{
					_lastStaminaHealing.Reset();
				}
			}
			else if (value < _stamina)
			{
				Network_stamina = Mathf.Max(value, 0f);
				if (_lastStaminaLostTimestamp != null)
				{
					_lastStaminaLostTimestamp.Reset();
				}
			}
		}
	}

	public bool FullStamina => CurrentStamina == MaxStamina;

	public float Network_maxStamina
	{
		get
		{
			return _maxStamina;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _maxStamina, 4u);
		}
	}

	public float Network_stamina
	{
		get
		{
			return _stamina;
		}
		[param: In]
		set
		{
			ref float stamina = ref _stamina;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnStaminaChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref stamina, 8u);
		}
	}

	private void luaOnStaminaChanged()
	{
		lua_stamina = CurrentStamina;
		lua_maxStamina = MaxStamina;
		LuaHookBase.InvokeFromCLR(lua_OnStaminaChanged, Ld);
	}

	protected override void luaOnHpChanged()
	{
		base.luaOnHpChanged();
		lua_hp = base.CurrentHP;
		lua_maxHp = base.MaxHP;
		LuaHookBase.InvokeFromCLR(lua_OnHpChanged, Ld);
	}

	public void OnStaminaChanged(float newStamina)
	{
		float stamina = _stamina;
		Network_stamina = newStamina;
		if (stamina != newStamina)
		{
			StaminaChangedSignal.Invoke(stamina, _stamina);
			luaOnStaminaChanged();
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		base.EventClientDamageReaction += OnEventClientDamageReaction;
	}

	private void OnEventClientDamageReaction(DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		ApplyReactionClient(reaction, contact, force, lift);
	}

	protected override void Awake()
	{
		base.Awake();
		if (_lastStaminaLostTimestamp == null)
		{
			CurrentStamina = MaxStamina;
		}
		_lastStaminaLostTimestamp = _lastStaminaLostTimestamp ?? new SerializableTimestamp();
		_lastStaminaHealing = _lastStaminaHealing ?? new SerializableTimestamp();
	}

	protected override void Start()
	{
		base.Start();
		_lastStaminaLostTimestamp.InitTimestamp();
		_lastStaminaHealing.InitTimestamp();
	}

	protected override void BrixFixedUpdate()
	{
		if (!(_lastRegenTick.Delta >= 1f))
		{
			return;
		}
		if (base.EffectiveRegenPerSecond != 0f)
		{
			if (base.EffectiveRegenPerSecond < 0f)
			{
				ApplyDamage(0f - base.EffectiveRegenPerSecond);
			}
			else if (HurtDelayPassed() || ExternalRegenerator != null)
			{
				Heal(base.EffectiveRegenPerSecond);
			}
		}
		if (StaminaRegenPerSecond != 0f && (HurtDelayPassed() || StaminaRegenPerSecond < 0f))
		{
			CurrentStamina += StaminaRegenPerSecond;
		}
		_lastRegenTick.Reset();
	}

	private bool HurtDelayPassed()
	{
		return _lastDamageTimestamp.Delta > _delayBeforeRegenPops && _lastStaminaLostTimestamp.Delta > _delayBeforeRegenPops;
	}

	public override DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		DamageReaction damageReaction = expectedReaction;
		if (expectedReaction == DamageReaction.Smash)
		{
			damageReaction = ((!DoesAttackSmash(type, force)) ? DamageReaction.Stun : DamageReaction.Smash);
		}
		if (Unwavering && (damageReaction == DamageReaction.Stun || damageReaction == DamageReaction.Twitch))
		{
			damageReaction = DamageReaction.Endure;
		}
		if (CanBlockAttack(type, force) && damageReaction != DamageReaction.Smash)
		{
			damageReaction = DamageReaction.Block;
		}
		else if (value >= base.CurrentHP || type == DamageType.Suicide)
		{
			damageReaction = DamageReaction.Die;
		}
		WillReceiveDamage.Invoke(damageReaction);
		return damageReaction;
	}

	public override void Heal(float value)
	{
		_lastHealTimestamp.Reset();
		base.CurrentHP += value;
	}

	public override void ApplyDamage(float value, GameObject attacker = null)
	{
		ApplyDamage(value, DamageType.Absolute, DamageReaction.Ignored, Vector3.zero, Vector3.zero, 0f);
	}

	public void ApplyDamage(float value, DamageType type, DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		if (CanBlockAttack(type, force))
		{
			value = ApplyBlockResist(value, type);
			if (CurrentStamina >= value)
			{
				if (reaction == DamageReaction.Smash)
				{
					CurrentStamina = 0f;
				}
				else
				{
					CurrentStamina -= value;
				}
				value = 0f;
			}
			else
			{
				float num = value - CurrentStamina;
				CurrentStamina = 0f;
				value = num;
			}
		}
		value = ApplyResistances(value, type);
		if (_lastDamageTimestamp != null && _lastStaminaLostTimestamp != null)
		{
			_lastDamageTimestamp.Reset();
			_lastStaminaLostTimestamp.Reset();
		}
		base.CurrentHP -= value;
		HasReceivedDamage.Invoke();
		if (base.CurrentHP <= 0f)
		{
			_Kill(contact, force, lift, attacker);
		}
	}

	private void ApplyReactionServer(DamageReaction reaction, Vector3 contact, Vector3 force, float lift)
	{
		switch (reaction)
		{
		case DamageReaction.Twitch:
			Locomotion.SmallHurt(contact, force);
			break;
		case DamageReaction.Stun:
			Locomotion.BigHurt(contact, force);
			break;
		case DamageReaction.Smash:
			Locomotion.Smash(contact, force, lift);
			break;
		case DamageReaction.Block:
			Locomotion.DoStartBlocking(0.5f);
			break;
		}
	}

	private void ApplyReactionClient(DamageReaction reaction, Vector3 contact, Vector3 force, float lift)
	{
		switch (reaction)
		{
		case DamageReaction.Endure:
		case DamageReaction.Twitch:
		case DamageReaction.Stun:
		case DamageReaction.Smash:
			SoundEngine.Play(Labor.Sounds.Hurt, Labor.gameObject);
			break;
		case DamageReaction.Die:
			SoundEngine.Play(Labor.Sounds.Death, Labor.gameObject);
			break;
		}
		GameplayAudio.AddBattleIntensity(1f, faction.IsAlliedToMe);
	}

	public override DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		DamageReaction reactionToAttack = GetReactionToAttack(value, type, expectedReaction, force);
		InvokeDamageReactionEvent(reactionToAttack, contact, force, lift, attacker);
		ApplyDamage(value, type, reactionToAttack, contact, force, lift, attacker);
		ApplyReactionServer(reactionToAttack, contact, force, lift);
		return reactionToAttack;
	}

	public override void Kill()
	{
		_Kill(Vector3.zero, Vector3.zero, 1f);
	}

	private void _Kill(Vector3 contact, Vector3 force, float upwardsEffect, GameObject attacker = null)
	{
		base.Kill();
		if (attacker != null)
		{
			SoundEngine.Play(Labor.Sounds.AttackKill, attacker);
		}
		Locomotion.Kill(contact, force, upwardsEffect);
		LuaDriver component = GetComponent<LuaDriver>();
		if (component != null)
		{
			LuaDriver arg = ((!(attacker == null)) ? attacker.GetComponent<LuaDriver>() : null);
			LuaHookBase.InvokeFromCLR(lua_OnKilled, component, arg);
		}
		GameSignals.Invoke(GameSignals.UnitDied, base.gameObject, attacker);
	}

	public bool CanBlockAttack(DamageType type, Vector3 force)
	{
		if (Unwavering)
		{
			return false;
		}
		if (CurrentStamina < RequiredStaminaForShieldBlock)
		{
			return false;
		}
		if (!Labor.Occupation.Profession.CanBlock())
		{
			return false;
		}
		if (!Labor.Defending)
		{
			if (type != DamageType.Melee)
			{
				return false;
			}
			if (!Locomotion.IsReadyToBlock())
			{
				return false;
			}
			if (Locomotion.IsCranking() && CurrentStamina < RequiredStaminaForShieldBlockWhileCranking)
			{
				return false;
			}
		}
		if (type == DamageType.Explosive)
		{
			return true;
		}
		float num = Vector3.Angle(force, -base.transform.forward);
		return num < Labor.Occupation.Profession.BlockingAngle;
	}

	private bool DoesAttackSmash(DamageType type, Vector3 force)
	{
		if (type == DamageType.Explosive)
		{
			return force.magnitude >= RequiredExplosionForceForSmash;
		}
		return !Unwavering;
	}

	protected float ApplyBlockResist(float damage, DamageType type)
	{
		return type switch
		{
			DamageType.Melee => damage * (1f - _meleeBlockResist), 
			DamageType.Piercing => damage * (1f - _piercingBlockResist), 
			DamageType.Magical => damage * (1f - _magicBlockResist), 
			DamageType.Explosive => damage * (1f - _explosiveBlockResist), 
			_ => damage, 
		};
	}

	[ContextMenu("Take 1 Damage (Melee)")]
	private void DebugMeleeDamage()
	{
		ReceiveAttack(1f, DamageType.Melee, DamageReaction.Stun, base.transform.position, base.transform.forward, 0f);
	}

	[LuaDrivable("Kill")]
	private static DynValue lua_Kill(ScriptExecutionContext context, CallbackArguments args)
	{
		BricktronDamageReceiver bricktronDamageReceiver = args.AsComponent<BricktronDamageReceiver>(0, "Kill");
		GameObject attacker = ((args.Count < 2) ? null : args.AsUserData<LuaDriver>(1, "Kill").gameObject);
		bricktronDamageReceiver._Kill(Vector3.zero, Vector3.zero, 1f, attacker);
		return DynValue.Void;
	}

	[LuaDrivable("ToggleDisplayCircle")]
	public static DynValue lua_ToggleDisplayCircle(ScriptExecutionContext context, CallbackArguments args)
	{
		BricktronDamageReceiver bricktronDamageReceiver = args.AsComponent<BricktronDamageReceiver>(0, "ToggleDisplayCircle");
		bool showing = args.AsBool(1, "ToggleDisplayCircle");
		if (!bricktronDamageReceiver.GetComponentInChildren<CircleHighlight>(includeInactive: true))
		{
			bricktronDamageReceiver.DebugMeleeDamage();
		}
		if ((bool)bricktronDamageReceiver.GetComponentInChildren<CircleHighlight>(includeInactive: true))
		{
			Component[] componentsInChildren = bricktronDamageReceiver.GetComponentsInChildren<CircleHighlight>(includeInactive: true);
			Component[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				CircleHighlight circleHighlight = (CircleHighlight)array[i];
				circleHighlight.Showing = showing;
			}
		}
		return DynValue.Void;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_maxStamina);
			writer.Write(_stamina);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_maxStamina);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_stamina);
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
			_maxStamina = reader.ReadSingle();
			_stamina = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 4) != 0)
		{
			_maxStamina = reader.ReadSingle();
		}
		if ((num & 8) != 0)
		{
			OnStaminaChanged(reader.ReadSingle());
		}
	}
}
