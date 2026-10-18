using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Network;
using Brix.Utils.Logging;
using Brix.Utils.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI.Damage;

[RequireComponent(typeof(BaseVoxelTransform))]
public class PlacedBlockDamageReceiver : NetworkGameComponent, IDamageReceiver, IDisposable
{
	private static BrixLogger logger = new BrixLogger(typeof(PlacedBlockDamageReceiver).FullName);

	public static Signal<BlockData, bool> OnBlockDamaged = new Signal<BlockData, bool>();

	public static Signal<BlockData, bool> OnBlockHealed = new Signal<BlockData, bool>();

	private Signal _onDeathSignal = new Signal();

	private bool _isDead;

	private CompCache<BaseVoxelTransform> _voxel;

	private BaseVoxelTransform BaseVoxel;

	private IBlockData _bd;

	private IDamageReceiver _referenceDamageReceiver;

	private bool _ready;

	[SerializeField]
	[DisplayRules(DisplayFlags.HideInEditMode)]
	[SyncVar(hook = "currentHpChanged")]
	private float _currentHP = 7f;

	public float MaxHP
	{
		get
		{
			return _referenceDamageReceiver.MaxHP;
		}
		set
		{
		}
	}

	public float OverkillThreshold
	{
		get
		{
			return _referenceDamageReceiver.OverkillThreshold;
		}
		set
		{
			_referenceDamageReceiver.OverkillThreshold = value;
		}
	}

	public bool IsDamaged => _bd.IsDamaged;

	public bool IsInvincible => _bd.IsInvincible;

	public CombatPriorityType CombatPriority => CombatPriorityType.OutOfCombat;

	public float CurrentHP
	{
		get
		{
			return _currentHP;
		}
		set
		{
			Network_currentHP = Mathf.Clamp(value, 0f, MaxHP);
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

	public float Network_currentHP
	{
		get
		{
			return _currentHP;
		}
		[param: In]
		set
		{
			ref float currentHP = ref _currentHP;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				currentHpChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref currentHP, 1u);
		}
	}

	private void currentHpChanged(float value)
	{
		float currentHP = _currentHP;
		if (value == currentHP)
		{
			return;
		}
		Network_currentHP = value;
		if (_ready)
		{
			float damage = (MaxHP - _currentHP) / MaxHP;
			_bd.Damage = damage;
			if (!IsDamaged)
			{
				OnBlockHealed.Invoke(_bd as BlockData, !_bd.BlockInfo.Groups.HasFlag(BlockGroups.Wood));
			}
			else if (_currentHP < currentHP)
			{
				OnBlockDamaged.Invoke(_bd as BlockData, !_bd.BlockInfo.Groups.HasFlag(BlockGroups.Wood));
			}
		}
		else
		{
			_ready = true;
		}
	}

	public void Heal(float value)
	{
		if (!_isDead)
		{
			CurrentHP += value;
		}
	}

	public DamageReaction GetReactionToAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 force)
	{
		if (CurrentHP <= value && !InvincibilityTracker.IsInvincible(_bd.Position))
		{
			return DamageReaction.Die;
		}
		return DamageReaction.Ignored;
	}

	public void ApplyDamage(float value, GameObject attacker)
	{
		ApplyDamage(value, DamageType.Magical, attacker);
	}

	public void ApplyDamage(float value, DamageType type, GameObject attacker)
	{
		if (!InvincibilityTracker.IsInvincible(_bd.Position))
		{
			CurrentHP -= value;
			if (CurrentHP <= 0f)
			{
				bool withParticles = type != DamageType.Explosive;
				Kill(withParticles, type);
			}
			SoundController soundController = ((!(attacker != null)) ? null : attacker.GetComponent<SoundController>());
			if (soundController != null)
			{
				soundController.BroadcastVoxelHitSound(base.transform.position, Voxel.GetSurfaceType(XYZ.FromVector3(base.transform.position)), _currentHP <= 0f);
			}
		}
	}

	public DamageReaction ReceiveAttack(float value, DamageType type, DamageReaction expectedReaction, Vector3 contact, Vector3 force, float lift, GameObject attacker = null)
	{
		if (_isDead)
		{
			logger.Warn($"A damage request was made on an already dead PlacedBlockDamageReceiver at {_voxel.Get.position}. Make sure the damager processed the OnDeathSignal.");
			return DamageReaction.Die;
		}
		DamageReaction reactionToAttack = GetReactionToAttack(value, type, expectedReaction, force);
		ApplyDamage(value, type, attacker);
		return reactionToAttack;
	}

	public override void Initialize()
	{
		base.Initialize();
		_voxel = new CompCache<BaseVoxelTransform>(base.gameObject);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		_isDead = false;
	}

	public void SetBlockData(IBlockData blockData)
	{
		_bd = (blockData.IsMultiBlock ? ((MultiBlockData)blockData).ChildBlocks.OfType<BlockData>().First((BlockData bd) => bd.PlacedBlock == base.gameObject).RootOrSelf : blockData);
		_referenceDamageReceiver = Factory.Peek(_bd.BlockInfo.Key).GetComponent<IDamageReceiver>();
		if (_referenceDamageReceiver == null)
		{
			throw new InvalidDataException($"Configuration error! Block '{GetComponent<AssetKeyHolder>().AssetKey.Name}' has no damagereceiver attached to it in the Factory.");
		}
		MaxHP = _referenceDamageReceiver.MaxHP;
		Network_currentHP = MaxHP * (1f - _bd.Damage);
	}

	protected override void OnDestroy()
	{
		InvalidateDamageReceiver(_voxel.Get.position);
	}

	private void InvalidateDamageReceiver(XYZ position)
	{
		if (position == _voxel.Get.position)
		{
			_isDead = true;
		}
	}

	public void Kill()
	{
		Kill(withParticles: true, DamageType.Absolute);
	}

	public void Kill(bool withParticles, DamageType type)
	{
		if (!_isDead)
		{
			ValidatorComponent component = GetComponent<ValidatorComponent>();
			if (!component.IsNullOrReleased() && type == DamageType.Explosive)
			{
				ValidationData vData = component.VData;
				component.SetUnstable(ref vData);
			}
			else
			{
				_bd.Damage = 1f;
				_bd.DestroyBlock(withParticles, withGarnotte: true);
				_onDeathSignal.Invoke();
			}
		}
	}

	public void Overkill(bool withParticles)
	{
		if (!_isDead)
		{
			_bd.Damage = 1f;
			_bd.DestroyBlock(withParticles, withGarnotte: false);
			_onDeathSignal.Invoke();
		}
	}

	public void Dispose()
	{
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_currentHP);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_currentHP);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			_currentHP = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			currentHpChanged(reader.ReadSingle());
		}
	}
}
