using System;
using System.Collections.Generic;
using Brix.Engine.Abstract;
using Brix.Engine.Trackers;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Batchers;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

public class MultiBlockData : IBlockData, IVoxel, IPoolable, IResetable, IDisposable
{
	private class MultiBlockInfo : BlockInfo
	{
		protected MultiBlockData _mbd;

		public MultiBlockInfo()
		{
			ID = -1;
			Groups = BlockGroups.Block | BlockGroups.Structure | BlockGroups.Item | BlockGroups.Wood | BlockGroups.Terrain;
			Surface = VoxelSurface.Wood;
			Type = BlockType.Block;
			BlueprintKey = Blueprints.Stone_Brick;
			IsPartialBloc = true;
			MergeableBloc = true;
		}

		public void UpdateBlockInfo(MultiBlockData mbd)
		{
			Groups = (BlockGroups)(from ibd in mbd.ChildBlocks.Slinq()
				select ibd.BlockInfo.Groups).Aggregate(0, (int current, BlockGroups bg) => current | (int)bg);
			Ressource = (ResourceType)(from ibd in mbd.ChildBlocks.Slinq()
				select ibd.BlockInfo.Ressource).Aggregate(0, (int current, ResourceType r) => current | (int)r);
			_mbd = mbd;
		}

		public override bool IsOfType(int blockTypeId)
		{
			return _mbd.IsOfType(blockTypeId);
		}

		public override int CalculateNiveaux(IBlockData bd)
		{
			int num = 0;
			foreach (IBlockData childBlock in _mbd.ChildBlocks)
			{
				int niveaux = childBlock.Niveaux;
				if (childBlock.BlockInfo.OverrideMultiBlockNiveaux)
				{
					return niveaux;
				}
				num = Math.Max(num & 3, niveaux & 3) | Math.Max(num & 0x30, niveaux & 0x30) | Math.Max(num & 0x300, niveaux & 0x300) | Math.Max(num & 0x3000, niveaux & 0x3000);
			}
			return num;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(MultiBlockData).FullName);

	private XYZ _position;

	private List<IBlockData> _blocks = new List<IBlockData>(2);

	private MultiBlockInfo _blockInfo = new MultiBlockInfo();

	private MultiBlockVolume _volume;

	IVoxel IVoxel.RootOrSelf => RootOrSelf;

	IBlockData IBlockData.RootOrSelf => RootOrSelf;

	IVolume IVoxel.Volume => Volume;

	public List<IBlockData> ChildBlocks => _blocks;

	public XYZ Position => _position;

	public Quaternion Rotation => _blocks[0].Rotation;

	public Quaternion CanonicalRotation => _blocks[0].Rotation;

	public Vector3 AbsolutePivotPosition => _blocks[0].AbsolutePivotPosition;

	public IBlockData Root => null;

	public MultiBlockData RootOrSelf => this;

	public BlockInfo BlockInfo => (BlockInfo)(((object)_blockInfo) ?? ((object)NullBlockInfo.Instance));

	public MegaVoxel MegaVoxel => Architecte.TryGetMegaVoxel(Brix.Engine.Abstract.Voxel.GetGScalaire(Position.ToScalaire()));

	public int Niveaux => _blockInfo.CalculateNiveaux(this);

	public bool IsDamaged => _blocks.Slinq().Any((IBlockData ibd) => ibd.IsDamaged);

	public float Damage
	{
		get
		{
			return (from ibd in _blocks.Slinq()
				select ibd.Damage).Average();
		}
		set
		{
		}
	}

	public IDamageReceiver DamageReceiver => MultiBlockDamageReceiver.Make(this);

	public virtual MultiBlockVolume Volume => _volume;

	public ValidationData ValidationData { get; private set; }

	public bool IsCrust => _blocks.Slinq().Any((IBlockData ibd) => ibd.IsCrust);

	public bool IsNull => false;

	public bool IsTerrain => _blocks.Slinq().Any((IBlockData ibd) => ibd.IsTerrain);

	public bool IsInvincible => InvincibilityTracker.IsInvincible(_position);

	public bool IsMultiBlock => true;

	public MultiBlockData()
	{
		_volume = new MultiBlockVolume(this);
	}

	public void DestroyBlock(bool withParticles, bool withGarnotte)
	{
		List<IBlockData> list = _blocks.Slinq().ToPooledList();
		list.ForEach(delegate(IBlockData ibd)
		{
			ibd.DestroyBlock(withParticles, withGarnotte);
		});
		list.Dispose();
	}

	public void DeBlock(int blockId = -1)
	{
		if (blockId == -1)
		{
			VoxelRequestBatcher.Instance.QueueDebloc(_position.x, _position.y, _position.z, -1);
			return;
		}
		_blocks.Slinq().FirstOrDefault((IBlockData ibd) => ibd.BlockInfo.ID == blockId)?.DeBlock();
	}

	public void DeBlockClient(int blockId = -1)
	{
		if (blockId == -1)
		{
			List<IBlockData> list = _blocks.Slinq().ToPooledList();
			list.ForEach(delegate(IBlockData ibd)
			{
				ibd.DeBlockClient(blockId);
			});
			list.Dispose();
		}
		else
		{
			_blocks.Slinq().FirstOrDefault((IBlockData ibd) => ibd.BlockInfo.ID == blockId)?.DeBlockClient(blockId);
		}
	}

	public bool MatchesGameObject(GameObject go)
	{
		return _blocks.Slinq().Any((IBlockData ibd) => ibd.MatchesGameObject(go));
	}

	public bool IsOfType(Factory.AssetKey assetKey)
	{
		return _blocks.Slinq().Any((IBlockData bd) => bd.BlockInfo.Key == assetKey);
	}

	public bool IsOfType(int blockInfoTypeId)
	{
		return _blocks.Slinq().Any((IBlockData bd) => bd.BlockInfo.ID == blockInfoTypeId);
	}

	public bool RemoveBlockWithMatchingRoot(BlockData rootBD)
	{
		int count = _blocks.Count;
		_blocks.Slinq().Where((IBlockData ibd, BlockData rbd) => ibd.RemoveBlockWithMatchingRoot(rbd), rootBD).Remove();
		if (_blocks.Count <= 1)
		{
			IBlockData a = _blocks.Slinq().FirstOrDefault();
			ReleaseToPool();
			BlockData.BlockDataCreated.Invoke(a);
			return true;
		}
		return count != _blocks.Count;
	}

	public void RedrawVoxel(MaterialCombineDispatcher materialCombineDispatcher)
	{
		_blocks.Slinq().ForEach(delegate(IBlockData ibd)
		{
			ibd.RedrawVoxel(materialCombineDispatcher);
		});
	}

	public void Physicalize()
	{
		List<IBlockData> list = _blocks.Slinq().ToPooledList();
		list.ForEach(delegate(IBlockData ibd)
		{
			ibd.Physicalize();
		});
		list.Dispose();
	}

	public void ResetObject()
	{
		_blocks.Clear();
	}

	public void Dispose()
	{
	}

	public void ReleaseToPool()
	{
		logger.DebugFormat("Removing Multiblock at position '{0}'", _position);
		BlockData.BlockDataReleased.Invoke(this);
		ObjectPool<MultiBlockData>.Release(this);
	}

	public static MultiBlockData Make()
	{
		return ObjectPool<MultiBlockData>.Reserve();
	}

	public static MultiBlockData Make(IBlockData originalBD, IBlockData newBD)
	{
		MultiBlockData multiBlockData = originalBD as MultiBlockData;
		if (multiBlockData != null)
		{
			multiBlockData.AddBlockData(newBD);
		}
		else
		{
			multiBlockData = Make();
			multiBlockData.AddBlockData(originalBD);
			multiBlockData.AddBlockData(newBD);
		}
		multiBlockData._blockInfo.UpdateBlockInfo(multiBlockData);
		multiBlockData.Volume.Reset();
		return multiBlockData;
	}

	private void AddBlockData(IBlockData newBd)
	{
		_blocks.Add(newBd);
		_position = newBd.Position;
		_volume.Reset();
	}
}
