using System;
using Brix.Components;
using Brix.Engine.Abstract;
using Brix.Engine.Blocks;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI.Damage;
using Brix.Game.Batchers;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Brix.Util;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Engine;

public class RootBlockData : BlockData, IBlockData, IVoxel, IPoolable, Brix.Utils.IResetable, IDisposable
{
	public static Signal<RootBlockData> RootBlockCreated = new Signal<RootBlockData>();

	public static Signal<RootBlockData> RootBlockReleased = new Signal<RootBlockData>();

	protected BlockInfo _blockInfo;

	protected float _damage;

	protected FixedVolume _volume;

	private int _rotationIndex;

	IVoxel IVoxel.RootOrSelf => RootOrSelf;

	IBlockData IBlockData.RootOrSelf => RootOrSelf;

	IVolume IVoxel.Volume => Volume;

	public override int RotationIndex => _rotationIndex;

	public override Quaternion Rotation => PreprocessedRotations.DiscreteRotationForIndexes[_rotationIndex];

	public override Quaternion CanonicalRotation => Quaternion.LookRotation(Calcule.AbsVecteur(Rotation * Vector3.forward), Rotation * Vector3.up);

	public override Vector3 AbsolutePivotPosition => base.OffsetedPosition;

	public override RootBlockData RootOrSelf => this;

	public override BlockInfo BlockInfo => _blockInfo ?? NullBlockInfo.Instance;

	public override MegaVoxel MegaVoxel => Architecte.TryGetMegaVoxel(Brix.Engine.Abstract.Voxel.GetGScalaire(_position.ToScalaire()));

	public override IDamageReceiver DamageReceiver => _volume.GetComponent<IDamageReceiver>();

	public override IVolume Volume => (!(_volume != null)) ? _blockInfo.Volume(_position, _rotationIndex) : _volume;

	public override ValidationData ValidationData => _blockInfo.GenerateVData(this);

	public override GameObject PlacedBlock => _volume.gameObject;

	public override bool IsDamaged => _damage > 0f;

	public override bool IsInvincible => InvincibilityTracker.IsInvincible(_position);

	public override float Damage
	{
		get
		{
			return _damage;
		}
		set
		{
			_damage = value;
		}
	}

	protected void Init(XYZ position, int rotationIndex, BlockInfo info, GameObject blockGo, float damage)
	{
		_root = null;
		_position = position;
		_rotationIndex = rotationIndex;
		_blockInfo = info;
		_damage = damage;
		_volume = blockGo.GetComponent<FixedVolume>();
		RootBlockCreated.Invoke(this);
	}

	public static RootBlockData Make(XYZ position, int rotationIndex, BlockInfo info, GameObject blockGO, float dommage = 0f)
	{
		RootBlockData rootBlockData = ObjectPool<RootBlockData>.Reserve();
		rootBlockData.Init(position, rotationIndex, info, blockGO, dommage);
		return rootBlockData;
	}

	public override void RedrawVoxel(MaterialCombineDispatcher materialCombineDispatcher)
	{
		_blockInfo.VoxelDrawer.RedrawVoxel(this, materialCombineDispatcher);
	}

	public override bool IsOfType(Factory.AssetKey assetKey)
	{
		return BlockInfo.Key == assetKey;
	}

	public override bool IsOfType(int blockInfoTypeId)
	{
		return _blockInfo.ID == blockInfoTypeId;
	}

	public override void DestroyBlock(bool withParticles, bool withGarnotte)
	{
		GameSignals.Invoke(GameSignals.VoxelDestroyed, this, withParticles, withGarnotte);
		DeBlock();
	}

	public override void DeBlock(int blockId = -1)
	{
		VoxelRequestBatcher.Instance.QueueDebloc(_position.x, _position.y, _position.z, _blockInfo.ID);
	}

	public override void DeBlockClient(int id)
	{
		if (id == _blockInfo.ID || id == -1)
		{
			GameSignals.Invoke(GameSignals.BeforeBlockRemoved, RootOrSelf);
			XYZ position = _position;
			Quaternion rotation = Rotation;
			BlockInfo blockInfo = _blockInfo;
			BrixSingleton<BlockEngine>.Instance.RemoveBlock(this).Slinq().ForEach(delegate(XYZ t)
			{
				GameSignals.Invoke(GameSignals.VoxelChanged, t);
			});
			GameSignals.Invoke(GameSignals.AfterBlockRemoved, position, rotation, blockInfo);
		}
	}

	public override bool MatchesGameObject(GameObject go)
	{
		return _volume != null && _volume.gameObject == go;
	}

	public override void Physicalize()
	{
		Vector3 position = PlacedBlock.transform.position;
		if (NetworkServer.active)
		{
			Transactor.SpawnUNet(_blockInfo.Key, Faction.DefaultNeutral, position, Rotation);
		}
		DeBlockClient(_blockInfo.ID);
	}

	public override void ReleaseToPool()
	{
		RootBlockReleased.Invoke(this);
		ObjectPool<RootBlockData>.Release(this);
	}

	public override void ResetObject()
	{
		base.ResetObject();
		_position = XYZ.zero;
		_rotationIndex = 0;
		_blockInfo = null;
		_damage = 0f;
		if (!_volume.IsNullOrReleased())
		{
			FixedVolume volume = _volume;
			_volume = null;
			ObjectPoolSingleton.Release(volume.gameObject);
		}
	}

	public BlockDataStruct ToSaveableStruct()
	{
		return new BlockDataStruct(_position, Rotation, _damage, BlockInfo.ID);
	}
}
