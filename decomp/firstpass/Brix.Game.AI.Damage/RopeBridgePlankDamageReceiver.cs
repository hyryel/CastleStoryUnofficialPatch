using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI.Damage;

public class RopeBridgePlankDamageReceiver : NetworkGameComponent, IDamageReceiver, IDisposable
{
	private IDamageReceiver _dr;

	private static BrixLogger logger = new BrixLogger(typeof(RopeBridgePlankDamageReceiver).FullName);

	public float MaxHP
	{
		get
		{
			return _dr.MaxHP;
		}
		set
		{
			_dr.MaxHP = value;
		}
	}

	public float CurrentHP
	{
		get
		{
			return _dr.CurrentHP;
		}
		set
		{
			_dr.CurrentHP = value;
		}
	}

	public float OverkillThreshold
	{
		get
		{
			return _dr.OverkillThreshold;
		}
		set
		{
			_dr.OverkillThreshold = value;
		}
	}

	public float RegenPerSecond
	{
		get
		{
			return _dr.RegenPerSecond;
		}
		set
		{
			_dr.RegenPerSecond = value;
		}
	}

	public float ProjectileMultiplier
	{
		get
		{
			return _dr.ProjectileMultiplier;
		}
		set
		{
			_dr.ProjectileMultiplier = value;
		}
	}

	public bool IsDamaged => _dr.IsDamaged;

	public bool IsInvincible => _dr.IsInvincible;

	public CombatPriorityType CombatPriority => _dr.CombatPriority;

	public IReadOnlySignal OnDeathSignal => _dr.OnDeathSignal;

	public override void OnRequested()
	{
		base.OnRequested();
		_dr = NullDamageReceiver.Get;
	}

	public override void ResetComponent()
	{
		_dr = NullDamageReceiver.Get;
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		GameSignals.LateConnectOnce(GameSignals.GamemodeLoaded, delegate
		{
			RefreshRopeBridgeFrameDamageReceiver();
		});
	}

	private void RefreshRopeBridgeFrameDamageReceiver()
	{
		RopeBridgeConfigurator ropeBridgeFromPlankAt = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(base.voxel.position);
		if (ropeBridgeFromPlankAt != null)
		{
			_dr = ropeBridgeFromPlankAt.GetComponent<ObjectDamageReceiver>();
			return;
		}
		_dr = NullDamageReceiver.Get;
		logger.WarnFormat("Could not find bridge frame for {0}", base.gameObject.name);
	}

	public void Dispose()
	{
	}

	public DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		return _dr.GetReactionToAttack(value, type, expectedReaction, force);
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		return _dr.ReceiveAttack(value, type, expectedReaction, contact, force, lift, attacker);
	}

	public void ApplyDamage(float value, GameObject attacker)
	{
		_dr.ApplyDamage(value, attacker);
	}

	public void Heal(float value)
	{
		_dr.Heal(value);
	}

	public void Kill()
	{
		_dr.Kill();
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
