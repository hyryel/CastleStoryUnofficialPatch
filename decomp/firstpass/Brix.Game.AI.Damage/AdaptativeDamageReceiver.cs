using System;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI.Damage;

[RequireComponent(typeof(VoxelTransform))]
public class AdaptativeDamageReceiver : BrixComponent, IDamageReceiver, IDisposable
{
	private static BrixLogger logger = new BrixLogger(typeof(AdaptativeDamageReceiver).FullName);

	private bool _isFocused;

	private Signal _onDeathSignal = new Signal();

	private IDamageReceiver _proxiedDamageReceiver;

	protected SignalConnection OnDeathConnection;

	public GameObject GameObject => (!(this != null)) ? null : base.gameObject;

	public float ProjectileMultiplier
	{
		get
		{
			return ProxiedDamageReceiver.ProjectileMultiplier;
		}
		set
		{
			ProxiedDamageReceiver.ProjectileMultiplier = value;
		}
	}

	public float MaxHP
	{
		get
		{
			return ProxiedDamageReceiver.MaxHP;
		}
		set
		{
			ProxiedDamageReceiver.MaxHP = value;
		}
	}

	public float CurrentHP
	{
		get
		{
			return ProxiedDamageReceiver.CurrentHP;
		}
		set
		{
			ProxiedDamageReceiver.CurrentHP = value;
		}
	}

	public float OverkillThreshold
	{
		get
		{
			return ProxiedDamageReceiver.OverkillThreshold;
		}
		set
		{
			ProxiedDamageReceiver.OverkillThreshold = value;
		}
	}

	public float RegenPerSecond
	{
		get
		{
			return ProxiedDamageReceiver.RegenPerSecond;
		}
		set
		{
			ProxiedDamageReceiver.RegenPerSecond = value;
		}
	}

	public CombatPriorityType CombatPriority => ProxiedDamageReceiver.CombatPriority;

	public IReadOnlySignal OnDeathSignal => _onDeathSignal;

	public IDamageReceiver ProxiedDamageReceiver => _proxiedDamageReceiver ?? ResolveProxiedDamageReceiver();

	public bool IsDamaged => ProxiedDamageReceiver.IsDamaged;

	public bool IsInvincible => ProxiedDamageReceiver.IsInvincible;

	public DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		return ProxiedDamageReceiver.GetReactionToAttack(value, type, expectedReaction, force);
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		return ProxiedDamageReceiver.ReceiveAttack(value, type, expectedReaction, contact, force, lift, attacker);
	}

	public void ApplyDamage(float value, GameObject attacker = null)
	{
		ProxiedDamageReceiver.ApplyDamage(value, attacker);
	}

	public void Heal(float value)
	{
		ProxiedDamageReceiver.Heal(value);
	}

	public void Kill()
	{
		ProxiedDamageReceiver.Kill();
	}

	public void FocusOn(IBlockData bd)
	{
		SetProxiedDamageReceiver(bd.DamageReceiver);
		_isFocused = true;
	}

	protected override void Start()
	{
		base.Start();
		if (_proxiedDamageReceiver == null)
		{
			ResolveProxiedDamageReceiver();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Dispose();
	}

	public void SetProxiedDamageReceiver(IDamageReceiver dr)
	{
		if (_proxiedDamageReceiver != null)
		{
			_proxiedDamageReceiver.Dispose();
		}
		_proxiedDamageReceiver = dr;
		OnDeathConnection.Disconnect();
		OnDeathConnection = _proxiedDamageReceiver.OnDeathSignal.ConnectOnce(OnProxiedDamageReceiverDeath);
	}

	private IDamageReceiver ResolveProxiedDamageReceiver()
	{
		VoxelTransform component = GetComponent<VoxelTransform>();
		_proxiedDamageReceiver = FindProxiedDamageReceiver(component.position);
		OnDeathConnection.Disconnect();
		OnDeathConnection = _proxiedDamageReceiver.OnDeathSignal.ConnectOnce(OnProxiedDamageReceiverDeath);
		return _proxiedDamageReceiver;
	}

	protected virtual IDamageReceiver FindProxiedDamageReceiver(XYZ position)
	{
		NatureData natureData = NatureRegistry.NatureAt(position, (NatureType)(-2));
		if (natureData.IsValid)
		{
			return natureData.DamageReceiver;
		}
		IVoxel voxel = Voxel.GetVoxel(position);
		if (!voxel.IsNull)
		{
			return voxel.DamageReceiver;
		}
		IVolume volume = VolumeRegistry.VolumesWithPropertyAt(position, VProperty.occupy).Slinq().FirstOrDefault();
		if (volume != null)
		{
			IDamageReceiver damageReceiver = volume.DamageReceiver;
			if (damageReceiver == null)
			{
				logger.WarnFormat("No damage receivers found on GameObject '{0}'. Will use a DieOnDamage_DamageReceiver!", volume.Name);
				return DieOnDamage_DamageReceiver.Make(volume.GameObject);
			}
			return damageReceiver;
		}
		logger.InfoFormat("Nothing damageable was found at '{0}'. A NullDamageReceiver will be used!", position);
		return NullDamageReceiver.Get;
	}

	private void OnProxiedDamageReceiverDeath()
	{
		if (GetComponent<Blueprint>() != null)
		{
			base.gameObject.SafeNetworkDestroy();
		}
		_onDeathSignal.Invoke();
		_proxiedDamageReceiver.Dispose();
		if (_isFocused)
		{
			_proxiedDamageReceiver = NullDamageReceiver.Get;
		}
		else
		{
			ResolveProxiedDamageReceiver();
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

	public void Dispose()
	{
		if (_proxiedDamageReceiver != null)
		{
			IDamageReceiver proxiedDamageReceiver = _proxiedDamageReceiver;
			_proxiedDamageReceiver = null;
			OnDeathConnection.Disconnect();
			proxiedDamageReceiver.Dispose();
		}
	}
}
