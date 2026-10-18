using System;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Game.AI.Damage;

public class SimpleDamageReceiver : MonoBehaviour, IDamageReceiver, IDisposable
{
	private Signal _onDeathSignal = new Signal();

	private bool _dead;

	[SerializeField]
	private float _maxHP;

	[SerializeField]
	private float _currentHP;

	[SerializeField]
	public float _overkillThreshold;

	public GameObject GameObject => (!(this != null)) ? null : base.gameObject;

	public float MaxHP
	{
		get
		{
			return _maxHP;
		}
		set
		{
			_maxHP = value;
		}
	}

	public float CurrentHP
	{
		get
		{
			return _currentHP;
		}
		set
		{
			_currentHP = Math.Max(Math.Min(value, MaxHP), 0f);
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

	public float ProjectileMultiplier
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public bool IsDamaged => _currentHP < MaxHP;

	public bool IsInvincible => false;

	public CombatPriorityType CombatPriority => CombatPriorityType.OutOfCombat;

	public IReadOnlySignal OnDeathSignal => _onDeathSignal;

	public void Heal(float value)
	{
		if (!_dead)
		{
			CurrentHP += value;
		}
	}

	public DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		if (CurrentHP <= value)
		{
			return DamageReaction.Die;
		}
		return DamageReaction.Ignored;
	}

	public void ApplyDamage(float value, GameObject attacker = null)
	{
		CurrentHP -= value;
		if (CurrentHP <= 0f)
		{
			Kill();
		}
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		if (_dead)
		{
			return DamageReaction.Ignored;
		}
		DamageReaction reactionToAttack = GetReactionToAttack(value, type, expectedReaction, force);
		ApplyDamage(value);
		return reactionToAttack;
	}

	public void Kill()
	{
		_dead = true;
		_onDeathSignal.Invoke();
		if ((bool)base.gameObject)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	public void Dispose()
	{
	}
}
