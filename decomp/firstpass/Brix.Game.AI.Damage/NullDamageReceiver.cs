using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.AI.Damage;

public class NullDamageReceiver : IDamageReceiver, IDisposable
{
	private static NullDamageReceiver _instance = new NullDamageReceiver();

	public static NullDamageReceiver Get => _instance;

	public float MaxHP
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float CurrentHP
	{
		get
		{
			return 0f;
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

	public float ProjectileMultiplier { get; set; }

	public IReadOnlySignal OnDeathSignal => NullSignal.Get;

	public bool IsDamaged => false;

	public bool IsInvincible => true;

	public CombatPriorityType CombatPriority => CombatPriorityType.CompletelyIgnored;

	public GameObject GameObject => null;

	private NullDamageReceiver()
	{
	}

	public void Anticipate(float value, DamageType type, Vector3 contact, Vector3 force)
	{
	}

	DamageReaction IDamageReceiver.ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		return DamageReaction.Ignored;
	}

	DamageReaction IDamageReceiver.GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		return DamageReaction.Ignored;
	}

	public void ApplyDamage(float value, GameObject attacker = null)
	{
	}

	public void Heal(float value)
	{
	}

	public void Kill()
	{
	}

	public void Dispose()
	{
	}
}
