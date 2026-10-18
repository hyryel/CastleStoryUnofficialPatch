using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.AI.Damage;

internal class DieOnDamage_DamageReceiver : IDamageReceiver, IPoolable, IDisposable, IResetable
{
	private GameObject _go;

	private Signal _onDeathSignal = new Signal();

	public float MaxHP
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public float CurrentHP
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public float OverkillThreshold
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public bool IsDamaged => _go != null;

	public virtual bool IsInvincible => false;

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

	public GameObject GameObject => _go;

	public void Heal(float value)
	{
	}

	public void Kill()
	{
		Architecte.NetworkAsymmetricDestroy(_go);
		_onDeathSignal.Invoke();
	}

	public void Dispose()
	{
		ObjectPool<DieOnDamage_DamageReceiver>.Release(this);
	}

	public void ResetObject()
	{
		_go = null;
		_onDeathSignal.Clear();
	}

	public static DieOnDamage_DamageReceiver Make(GameObject go)
	{
		DieOnDamage_DamageReceiver dieOnDamage_DamageReceiver = ObjectPool<DieOnDamage_DamageReceiver>.Reserve();
		dieOnDamage_DamageReceiver._go = go;
		return dieOnDamage_DamageReceiver;
	}

	DamageReaction IDamageReceiver.GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		return DamageReaction.Die;
	}

	DamageReaction IDamageReceiver.ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		Kill();
		return DamageReaction.Die;
	}

	public void ApplyDamage(float value, GameObject attacker = null)
	{
		Kill();
	}
}
