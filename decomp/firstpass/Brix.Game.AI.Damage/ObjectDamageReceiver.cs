using Brix.Engine;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI.Damage;

public class ObjectDamageReceiver : DamageReceiver
{
	private static readonly UpdatableNetworkProfiler<ObjectDamageReceiver> UpdatableProfiler = new UpdatableNetworkProfiler<ObjectDamageReceiver>();

	[SerializeField]
	[LuaDrivable("isInvincible")]
	[JsonProperty]
	protected bool Invincible;

	[SerializeField]
	[JsonProperty]
	private CombatPriorityType _combatPriority = CombatPriorityType.OutOfCombat;

	public VoxelSurface OnHitSurfaceSound;

	[SerializeField]
	[JsonProperty]
	private bool _corruptronDmgReductionEnabled;

	private float _corruptronDmgModifier = 0.5f;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public override bool IsInvincible => Invincible;

	public override CombatPriorityType CombatPriority => _combatPriority;

	public override void OnStartClient()
	{
		base.OnStartClient();
		base.EventClientDamageReaction += OnEventClientDamageReaction;
	}

	private void OnEventClientDamageReaction(DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		ApplyReactionClient(reaction, contact, force, lift, attacker);
	}

	private void ApplyReactionClient(DamageReaction reaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		SoundController soundController = ((!(attacker != null)) ? null : attacker.GetComponent<SoundController>());
		if (soundController != null)
		{
			soundController.BroadcastVoxelHitSound(contact, OnHitSurfaceSound, base.CurrentHP <= 0f);
		}
	}

	public override DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		DamageReaction damageReaction = expectedReaction;
		if (Invincible)
		{
			damageReaction = DamageReaction.Ignored;
		}
		if (value >= base.CurrentHP)
		{
			damageReaction = DamageReaction.Die;
		}
		WillReceiveDamage.Invoke(damageReaction);
		return damageReaction;
	}

	public override void ApplyDamage(float value, GameObject attacker = null)
	{
		ApplyDamage(value, DamageType.Absolute);
	}

	public void ApplyDamage(float value, DamageType type)
	{
		value = ApplyResistances(value, type);
		base.CurrentHP -= value;
		_lastDamageTimestamp.Reset();
		HasReceivedDamage.Invoke();
		if (base.CurrentHP <= 0f)
		{
			Kill();
		}
	}

	public override void Heal(float value)
	{
		_lastHealTimestamp.Reset();
		base.CurrentHP += value;
	}

	public override DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		if (_corruptronDmgReductionEnabled && !attacker.IsNullOrReleased())
		{
			Profession component = attacker.GetComponent<Profession>();
			if (!component.IsNullOrReleased() && component.IsCorruptron())
			{
				value *= _corruptronDmgModifier;
			}
		}
		DamageReaction reactionToAttack = GetReactionToAttack(value, type, expectedReaction, force);
		if (!Invincible)
		{
			ApplyDamage(value, type);
		}
		InvokeDamageReactionEvent(reactionToAttack, contact, force, lift, attacker);
		return reactionToAttack;
	}

	public override void Kill()
	{
		base.Kill();
		if (!this.IsNullOrReleasedIgc())
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	[ContextMenu("Take 1 Damage (Melee)")]
	private void DebugMeleeDamage()
	{
		ReceiveAttack(1f, DamageType.Melee, DamageReaction.Stun, base.transform.position, base.transform.forward, 0f);
	}

	[ContextMenu("Take 1 Damage (Explosive)")]
	private void DebugExplosiveDamage()
	{
		ReceiveAttack(1f, DamageType.Explosive, DamageReaction.Smash, base.transform.position, base.transform.forward, 0f);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
