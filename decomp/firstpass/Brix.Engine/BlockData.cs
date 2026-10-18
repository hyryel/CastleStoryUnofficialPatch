using System;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine;

public class BlockData : IBlockData, IVoxel, IPoolable, IResetable, IDisposable
{
	private static BrixLogger logger = new BrixLogger(typeof(BlockData).FullName);

	public static Signal<IBlockData> BlockDataReleased = new Signal<IBlockData>();

	public static Signal<IBlockData> BlockDataCreated = new Signal<IBlockData>();

	protected XYZ _position;

	protected RootBlockData _root;

	IVoxel IVoxel.RootOrSelf => RootOrSelf;

	IBlockData IBlockData.RootOrSelf => RootOrSelf;

	IVolume IVoxel.Volume => Volume;

	public XYZ Position => _position;

	public virtual Quaternion Rotation => _root.Rotation;

	public virtual int RotationIndex => _root.RotationIndex;

	public virtual Quaternion CanonicalRotation => _root.CanonicalRotation;

	public virtual Vector3 AbsolutePivotPosition => _root.AbsolutePivotPosition;

	public Vector3 OffsetedPosition => RootOrSelf.BlockInfo.OffsetedPosition(_position, Rotation);

	public virtual RootBlockData RootOrSelf => _root;

	public virtual BlockInfo BlockInfo => _root.BlockInfo;

	public virtual MegaVoxel MegaVoxel => _root.MegaVoxel;

	public virtual int Niveaux => RootOrSelf.BlockInfo.CalculateNiveaux(this);

	public virtual IDamageReceiver DamageReceiver => _root.DamageReceiver;

	public virtual IVolume Volume => _root.Volume;

	public virtual ValidationData ValidationData => _root.ValidationData;

	public bool IsNull => false;

	public bool IsTerrain => false;

	public bool IsMultiBlock => false;

	public virtual GameObject PlacedBlock => _root.PlacedBlock;

	public virtual bool IsDamaged => _root.IsDamaged;

	public virtual bool IsInvincible => _root.IsInvincible;

	public virtual float Damage
	{
		get
		{
			return _root.Damage;
		}
		set
		{
			_root.Damage = value;
		}
	}

	public bool IsCrust => false;

	protected void Init(XYZ position, RootBlockData root)
	{
		_position = position;
		_root = root;
		BlockDataCreated.Invoke(this);
	}

	public virtual bool IsOfType(int blockInfoTypeId)
	{
		if (_root == null)
		{
			return false;
		}
		return _root.IsOfType(blockInfoTypeId);
	}

	public virtual void RedrawVoxel(MaterialCombineDispatcher materialCombineDispatcher)
	{
		_root.RedrawVoxel(materialCombineDispatcher);
	}

	public virtual bool IsOfType(Factory.AssetKey assetKey)
	{
		return _root.IsOfType(assetKey);
	}

	public virtual void DestroyBlock(bool withParticles, bool withGarnotte)
	{
		_root.DestroyBlock(withParticles, withGarnotte);
	}

	public virtual void DeBlock(int blockId = -1)
	{
		_root.DeBlock(blockId);
	}

	public virtual void DeBlockClient(int id)
	{
		_root.DeBlockClient(id);
	}

	public virtual bool MatchesGameObject(GameObject go)
	{
		return _root.MatchesGameObject(go);
	}

	public bool RemoveBlockWithMatchingRoot(BlockData rootBD)
	{
		bool flag = RootOrSelf == rootBD;
		if (flag)
		{
			ReleaseToPool();
		}
		return flag;
	}

	public virtual void Physicalize()
	{
		_root.Physicalize();
	}

	public virtual void ReleaseToPool()
	{
		logger.DebugFormat("Removing block data at position '{0}'", _position);
		BlockDataReleased.Invoke(this);
		ObjectPool<BlockData>.Release(this);
	}

	public virtual void ResetObject()
	{
		_position = XYZ.zero;
		_root = null;
	}

	public void Dispose()
	{
	}

	public static BlockData Make(XYZ position, RootBlockData root)
	{
		BlockData blockData = ObjectPool<BlockData>.Reserve();
		blockData.Init(position, root);
		return blockData;
	}
}
