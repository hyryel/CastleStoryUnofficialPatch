using System;
using Brix.Engine.Abstract;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Batchers;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine;

public class TerrainBlockData : IVoxel, IPoolable, IResetable, IDisposable
{
	private XYZ _position;

	private TerrainBlockInfoMeta.TerrainBlockInfo _blockInfo;

	private VoxelHandle handle;

	IVoxel IVoxel.RootOrSelf => RootOrSelf;

	BlockInfo IVoxel.BlockInfo => BlockInfo;

	IVolume IVoxel.Volume => Volume;

	public XYZ Position => _position;

	public Quaternion Rotation => Quaternion.identity;

	public Quaternion CanonicalRotation => Quaternion.identity;

	public Vector3 AbsolutePivotPosition => _position.ToVector3();

	public TerrainBlockData RootOrSelf => this;

	public BlockInfo BlockInfo => (BlockInfo)(((object)_blockInfo) ?? ((object)NullBlockInfo.Instance));

	public MegaVoxel MegaVoxel => Architecte.TryGetMegaVoxel(Brix.Engine.Abstract.Voxel.GetGScalaire(_position.ToScalaire()));

	public int Niveaux => Voxel.Engine.ComputeVoxelLegacyLevels(VoxelHandle);

	public bool OverrideMultiBlockNiveaux => false;

	public bool IsDamaged => TerrainDamageTracker.Instance.HasDamageAt(_position);

	public float Damage
	{
		get
		{
			return TerrainDamageTracker.Instance.GetDamageAt(_position);
		}
		set
		{
			TerrainDamageTracker.Instance.SetDamageAt(_position, value);
		}
	}

	public IDamageReceiver DamageReceiver => TerrainDamageReceiver.Make(this);

	public virtual FreeVolume Volume => FreeVolume.Make(BlockInfo.Key, Position);

	public virtual ValidationData ValidationData => (_blockInfo == null) ? ValidationData.None : _blockInfo.ValidationData(this);

	public bool IsNull => false;

	public bool IsTerrain => true;

	public bool IsMultiBlock => false;

	public bool IsInvincible => InvincibilityTracker.IsInvincible(_position);

	public VoxelHandle VoxelHandle
	{
		get
		{
			if (!Voxel.Engine.IsValidVoxelHandle(handle))
			{
				return handle = Voxel.Engine.GetVoxelHandle(_position);
			}
			Voxel.Engine.AssignVoxelHandle(handle, _position);
			return handle;
		}
	}

	public bool IsCrust => Voxel.Engine.IsVoxelSmooth(VoxelHandle);

	public int TerrainVolume => Voxel.Engine.GetVoxelVolume(VoxelHandle);

	public bool IsFull => (Niveaux & 0x3333) == 13107;

	private void Init(XYZ position, TerrainBlockInfoMeta.TerrainBlockInfo terrainBlockInfo)
	{
		_position = position;
		_blockInfo = terrainBlockInfo;
	}

	public bool IsOfType(Factory.AssetKey assetKey)
	{
		return BlockInfo.Key == assetKey;
	}

	public bool IsOfType(int blockInfoTypeId)
	{
		return BlockInfo.ID == blockInfoTypeId;
	}

	public void RedrawVoxel(MaterialCombineDispatcher materialCombineDispatcher)
	{
	}

	public void Physicalize()
	{
	}

	public ValidationResult Validate()
	{
		using FreeVolume freeVolume = Volume;
		return freeVolume.Validate();
	}

	public void DestroyBlock(bool withParticles, bool withGarnotte)
	{
		if (!InvincibilityTracker.IsInvincible(_position))
		{
			GameSignals.Invoke(GameSignals.VoxelDestroyed, this, withParticles, withGarnotte);
			DeBlock();
		}
	}

	public void DeBlock(int blockId = -1)
	{
		VoxelRequestBatcher.Instance.QueueDebloc(_position.x, _position.y, _position.z, BlockInfo.ID);
	}

	public void DeBlockClient(int id = -1)
	{
		if (id == BlockInfo.ID || id == -1)
		{
			TerrainDamageTracker.Instance.ClearDamageAt(_position);
			Voxel.RemoveTerrain(this);
			GameSignals.Invoke(GameSignals.VoxelChanged, _position);
			GameSignals.Invoke(GameSignals.TerrainRemoved, _position);
			GameSignals.Invoke(GameSignals.TerrainRemovedLua, _position.ToVector3());
		}
	}

	public bool MatchesGameObject(GameObject go)
	{
		return false;
	}

	public bool RemoveBlockWithMatchingRoot(BlockData rootBD)
	{
		return false;
	}

	public void ResetObject()
	{
		_position = XYZ.zero;
		_blockInfo = null;
	}

	public void Dispose()
	{
		ReleaseToPool();
	}

	public void ReleaseToPool()
	{
		ObjectPool<TerrainBlockData>.Release(this);
	}

	public static TerrainBlockData Make(XYZ position, TerrainBlockInfoMeta.TerrainBlockInfo info)
	{
		TerrainBlockData terrainBlockData = ObjectPool<TerrainBlockData>.Reserve();
		terrainBlockData.Init(position, info);
		return terrainBlockData;
	}
}
