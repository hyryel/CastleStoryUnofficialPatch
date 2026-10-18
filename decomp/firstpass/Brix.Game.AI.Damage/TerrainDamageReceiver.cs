using System;
using System.IO;
using Brix.Engine;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI.Damage;

public class TerrainDamageReceiver : IDamageReceiver, IPoolable, IDisposable, IResetable
{
	public delegate void DamageReactionEvent(DamageReaction reaction, Vector3 contact, Vector3 force, float lift);

	private static BrixLogger logger = new BrixLogger(typeof(TerrainDamageReceiver).FullName);

	public static Signal<XYZ, bool> OnTerrainDamaged = new Signal<XYZ, bool>();

	private TerrainDamageTracker _damageTracker;

	private XYZ _position;

	private Signal _onDeathSignal = new Signal();

	private float _maxHP;

	private float _currentHP;

	public float _overkillThreshold;

	private bool IsDead
	{
		get
		{
			using IVoxel voxel = Voxel.GetTerrainBlockData(_position);
			return voxel.IsNull;
		}
	}

	private float VoxelDamage
	{
		get
		{
			return _damageTracker.GetDamageAt(_position);
		}
		set
		{
			if (!IsDead)
			{
				_damageTracker.SetDamageAt(_position, value);
			}
		}
	}

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
			return _maxHP - _maxHP * VoxelDamage;
		}
		set
		{
			_currentHP = Mathf.Clamp(value, 0f, _maxHP);
			VoxelDamage = (_maxHP - _currentHP) / _maxHP;
			if (value >= _maxHP)
			{
				_damageTracker.ClearDamageAt(_position);
			}
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

	public bool IsDamaged
	{
		get
		{
			using IVoxel voxel = Voxel.GetTerrainBlockData(_position);
			return voxel.IsDamaged;
		}
	}

	public bool IsInvincible
	{
		get
		{
			using IVoxel voxel = Voxel.GetTerrainBlockData(_position);
			return voxel.IsInvincible;
		}
	}

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
		if (CurrentHP <= value && !InvincibilityTracker.IsInvincible(_position))
		{
			return (expectedReaction != DamageReaction.ForceOverkill) ? DamageReaction.Die : expectedReaction;
		}
		return DamageReaction.Ignored;
	}

	public void ApplyDamage(float value, GameObject attacker)
	{
		ApplyDamage(value, DamageType.Magical, attacker, DamageReaction.Ignored);
	}

	public void ApplyDamage(float value, DamageType type, GameObject attacker, DamageReaction reaction)
	{
		if (!InvincibilityTracker.IsInvincible(_position))
		{
			bool flag = type != DamageType.Explosive;
			if (CurrentHP - value <= OverkillThreshold)
			{
				CurrentHP = Math.Max(CurrentHP - value, 0f);
				Overkill(flag);
			}
			else
			{
				CurrentHP = Math.Max(CurrentHP - value, 0f);
				if (_currentHP > 0f)
				{
					OnTerrainDamaged.Invoke(_position, flag);
				}
				else if (reaction == DamageReaction.ForceOverkill)
				{
					Overkill(flag);
				}
				else
				{
					Kill(flag);
				}
			}
		}
		SoundController soundController = ((!(attacker != null)) ? null : attacker.GetComponent<SoundController>());
		if (soundController != null)
		{
			soundController.BroadcastVoxelHitSound(_position.ToVector3(), Voxel.GetSurfaceType(_position), _currentHP <= 0f);
		}
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker)
	{
		if (IsDead)
		{
			if (!Voxel.IsEmpty(_position) && !Voxel.IsTerrain(_position))
			{
				logger.WarnFormat("A damage request was made on a TerrainDamageReceiver at {0} while a non-terrain block occupies the space.", _position);
				return DamageReaction.Failed;
			}
			logger.WarnFormat("A damage request was made on an already dead TerrainDamageReceiver at {0}. Make sure the damager processed the OnDeathSignal.", _position);
			return DamageReaction.Die;
		}
		DamageReaction reactionToAttack = GetReactionToAttack(value, type, expectedReaction, force);
		ApplyDamage(value, type, attacker, reactionToAttack);
		if (attacker != null && attacker.GetComponent<Labor>() != null && reactionToAttack == DamageReaction.Die)
		{
			StatsTracker.DugVoxel(attacker);
		}
		return reactionToAttack;
	}

	public void Heal(float value)
	{
		float currentHP = CurrentHP;
		CurrentHP += value;
		if (CurrentHP != currentHP)
		{
			OnTerrainDamaged.Invoke(_position, b: false);
		}
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

	public void Overkill(bool withParticles)
	{
		using (IVoxel voxel = Voxel.GetVoxel(_position))
		{
			voxel.DestroyBlock(withParticles, withGarnotte: false);
		}
		_onDeathSignal.Invoke();
	}

	public void Dispose()
	{
		ObjectPool<TerrainDamageReceiver>.Release(this);
	}

	public void ResetObject()
	{
		_position = XYZ.zero;
		_onDeathSignal.Clear();
		_maxHP = 0f;
		_currentHP = 0f;
	}

	public static TerrainDamageReceiver Make(TerrainBlockData bd)
	{
		TerrainDamageReceiver terrainDamageReceiver = ObjectPool<TerrainDamageReceiver>.Reserve();
		terrainDamageReceiver.Init(bd, TerrainDamageTracker.Instance);
		return terrainDamageReceiver;
	}

	private void Init(TerrainBlockData tbd, TerrainDamageTracker damageTracker)
	{
		_damageTracker = damageTracker;
		_position = tbd.Position;
		SetMaxHPAndOverkillFromFactory(tbd);
		_currentHP = CurrentHP;
	}

	private void SetMaxHPAndOverkillFromFactory(TerrainBlockData tbd)
	{
		IDamageReceiver component = Factory.Peek(tbd.BlockInfo.Key).GetComponent<IDamageReceiver>();
		if (component == null)
		{
			throw new InvalidDataException($"Configuration error! Block '{tbd.BlockInfo.Key.Name}' has no Damage Receiver attached to it in the Factory.");
		}
		MaxHP = component.MaxHP;
		OverkillThreshold = component.OverkillThreshold;
	}
}
