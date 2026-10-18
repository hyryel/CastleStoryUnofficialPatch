using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI.Damage;

public class MultiBlockDamageReceiver : IDamageReceiver, IPoolable, IDisposable, IResetable
{
	private static BrixLogger logger = new BrixLogger(typeof(MultiBlockDamageReceiver).FullName);

	private XYZ _position;

	private Signal _onDeathSignal = new Signal();

	private MultiBlockData _cachedMultiBlockData;

	private bool IsDead => !ChildDamageReceivers.Slinq().Any();

	private IEnumerable<IDamageReceiver> ChildDamageReceivers
	{
		get
		{
			using IVoxel ibd = Voxel.GetVoxel(_position);
			if (!ibd.IsNull && ibd != _cachedMultiBlockData)
			{
				yield return ibd.DamageReceiver;
				yield break;
			}
			foreach (IBlockData childBlock in _cachedMultiBlockData.ChildBlocks)
			{
				yield return childBlock.DamageReceiver;
			}
		}
	}

	public float MaxHP
	{
		get
		{
			return (from dr in ChildDamageReceivers.Slinq()
				select dr.MaxHP).Sum();
		}
		set
		{
		}
	}

	public float CurrentHP
	{
		get
		{
			return (from dr in ChildDamageReceivers.Slinq()
				select dr.CurrentHP).Sum();
		}
		set
		{
		}
	}

	public float OverkillThreshold
	{
		get
		{
			return (from dr in ChildDamageReceivers.Slinq()
				select dr.OverkillThreshold).Average();
		}
		set
		{
		}
	}

	public float RegenPerSecond
	{
		get
		{
			return (from dr in ChildDamageReceivers.Slinq()
				select dr.RegenPerSecond).Sum();
		}
		set
		{
		}
	}

	public IReadOnlySignal OnDeathSignal => _onDeathSignal;

	public bool IsDamaged => ChildDamageReceivers.Slinq().Any((IDamageReceiver ibd) => ibd.IsDamaged);

	public bool IsInvincible => ChildDamageReceivers.Slinq().All((IDamageReceiver ibd) => ibd.IsInvincible);

	public CombatPriorityType CombatPriority => CombatPriorityType.OutOfCombat;

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

	public GameObject GameObject => null;

	public DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		if ((from child in ChildDamageReceivers.Slinq()
			select child.CurrentHP).Sum() <= value)
		{
			return DamageReaction.Die;
		}
		return DamageReaction.Ignored;
	}

	public void ApplyDamage(float value, GameObject attacker)
	{
		float num = value;
		List<IDamageReceiver> list = ChildDamageReceivers.Slinq().ToPooledList();
		foreach (IDamageReceiver item in list)
		{
			if (num <= 0f)
			{
				break;
			}
			if (item.CurrentHP > num)
			{
				item.ApplyDamage(num, attacker);
				num = 0f;
			}
			else
			{
				num -= item.CurrentHP;
				item.ApplyDamage(item.CurrentHP, attacker);
			}
		}
		list.Dispose();
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		if (IsDead)
		{
			logger.Warn($"A damage request was made on an already dead MultiBlockDamageReceiver at {_position}. Make sure the damager processed the OnDeathSignal.");
			return DamageReaction.Die;
		}
		DamageReaction reactionToAttack = GetReactionToAttack(value, type, expectedReaction, force);
		float num = value;
		int num2 = 0;
		List<IDamageReceiver> list = ChildDamageReceivers.Slinq().ToPooledList();
		while (num > 0f && num2 < list.Count)
		{
			IDamageReceiver damageReceiver = list[num2];
			if (damageReceiver.CurrentHP > num)
			{
				damageReceiver.ReceiveAttack(num, type, expectedReaction, contact, force, lift, attacker);
				num = 0f;
			}
			else
			{
				num -= damageReceiver.CurrentHP;
				damageReceiver.ReceiveAttack(damageReceiver.CurrentHP, type, expectedReaction, contact, force, lift, attacker);
			}
			damageReceiver.Dispose();
			num2++;
		}
		list.Dispose();
		return reactionToAttack;
	}

	public void Heal(float value)
	{
		if (IsDead)
		{
			logger.Warn($"A damage request was made on an already dead MultiBlockDamageReceiver at {_position}. Make sure the damager processed the OnDeathSignal.");
			return;
		}
		float num = value;
		int num2 = 0;
		List<IDamageReceiver> list = ChildDamageReceivers.Slinq().ToPooledList();
		while (num > 0f && num2 < list.Count)
		{
			IDamageReceiver damageReceiver = list[num2];
			if (damageReceiver.MaxHP - damageReceiver.CurrentHP > num)
			{
				damageReceiver.Heal(num);
				num = 0f;
			}
			else
			{
				num -= damageReceiver.MaxHP - damageReceiver.CurrentHP;
				damageReceiver.CurrentHP = damageReceiver.MaxHP;
			}
			damageReceiver.Dispose();
			num2++;
		}
		list.Dispose();
	}

	public void Kill()
	{
		Kill(withParticles: true);
	}

	public void Kill(bool withParticles)
	{
		using (IVoxel voxel = Voxel.GetVoxel(_position))
		{
			voxel.DestroyBlock(withParticles, withGarnotte: true);
		}
		_onDeathSignal.Invoke();
	}

	private void Init(MultiBlockData mbd)
	{
		_position = mbd.Position;
		_cachedMultiBlockData = mbd;
	}

	public void Dispose()
	{
		ObjectPool<MultiBlockDamageReceiver>.Release(this);
	}

	public void ResetObject()
	{
		_position = XYZ.zero;
		_onDeathSignal.Clear();
	}

	public static MultiBlockDamageReceiver Make(MultiBlockData mbd)
	{
		MultiBlockDamageReceiver multiBlockDamageReceiver = ObjectPool<MultiBlockDamageReceiver>.Reserve();
		multiBlockDamageReceiver.Init(mbd);
		return multiBlockDamageReceiver;
	}
}
