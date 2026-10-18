using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.AI.Damage;

public interface IDamageReceiver : IDisposable
{
	float MaxHP { get; set; }

	float CurrentHP { get; set; }

	float OverkillThreshold { get; set; }

	float RegenPerSecond { get; set; }

	float ProjectileMultiplier { get; set; }

	bool IsDamaged { get; }

	bool IsInvincible { get; }

	CombatPriorityType CombatPriority { get; }

	GameObject GameObject { get; }

	IReadOnlySignal OnDeathSignal { get; }

	DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force);

	DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker);

	void ApplyDamage(float value, GameObject attacker);

	void Heal(float value);

	void Kill();
}
