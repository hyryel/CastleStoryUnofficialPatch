using System;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.AI.Damage;

public class NatureDamageReceiver : IDamageReceiver, IPoolable, IDisposable, IResetable
{
	public static Signal<XYZ, bool> OnNatureDamaged = new Signal<XYZ, bool>();

	private TerrainDamageTracker _damageTracker;

	private XYZ _position;

	private Signal _onDeathSignal = new Signal();

	public float _overkillThreshold;

	private bool IsDead => !NatureRegistry.HasNatureAt(_position) || NatureRegistry.GetCurrentHpAt(_position) == 0f;

	public float MaxHP
	{
		get
		{
			return NatureRegistry.GetMaxHpAt(_position);
		}
		set
		{
		}
	}

	public float CurrentHP
	{
		get
		{
			return NatureRegistry.GetCurrentHpAt(_position);
		}
		set
		{
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

	public float ProjectileMultiplier { get; set; }

	public bool IsDamaged => NatureRegistry.GetDamageAt(_position) > 0f;

	public bool IsInvincible => false;

	public CombatPriorityType CombatPriority => CombatPriorityType.CompletelyIgnored;

	public float RegenPerSecond
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public IReadOnlySignal OnDeathSignal => _onDeathSignal;

	public GameObject GameObject => null;

	public DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		return DamageReaction.Ignored;
	}

	public void ApplyDamage(float value, GameObject attacker = null)
	{
		NatureRegistry.DamageAt(_position, value);
		SoundController soundController = ((!(attacker != null)) ? null : attacker.GetComponent<SoundController>());
		if (soundController != null)
		{
			bool destroyed = NatureRegistry.GetCurrentHpAt(_position) - value <= 0f;
			soundController.BroadcastVoxelHitSound(_position.ToVector3(), Voxel.GetSurfaceType(_position), destroyed);
		}
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		if (IsDead)
		{
			_onDeathSignal.Invoke();
			return DamageReaction.Die;
		}
		ApplyDamage(value);
		return DamageReaction.Ignored;
	}

	public void Heal(float value)
	{
	}

	public void Kill()
	{
		NatureRegistry.BreakAt(_position);
		_onDeathSignal.Invoke();
	}

	public void Overkill()
	{
		NatureRegistry.BreakAt(_position);
		_onDeathSignal.Invoke();
	}

	public void Dispose()
	{
		ObjectPool<NatureDamageReceiver>.Release(this);
	}

	public void ResetObject()
	{
		_position = XYZ.zero;
		_onDeathSignal.Clear();
	}

	public static NatureDamageReceiver Make(XYZ pos)
	{
		NatureDamageReceiver natureDamageReceiver = ObjectPool<NatureDamageReceiver>.Reserve();
		natureDamageReceiver.Init(pos);
		return natureDamageReceiver;
	}

	private void Init(XYZ pos)
	{
		_position = pos;
	}
}
