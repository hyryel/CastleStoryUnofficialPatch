using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Factories;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Engine.Blocks;

[RequireComponent(typeof(DamagedBlockTracker))]
public class BlockEngine : BrixSingleton<BlockEngine>
{
	private static BrixLogger logger = new BrixLogger(typeof(BlockEngine).FullName);

	private readonly HashSet<RootBlockData> _rootBlockData = new HashSet<RootBlockData>();

	private readonly Dictionary<XYZ, IBlockData> _blocks = new Dictionary<XYZ, IBlockData>();

	private int maxSpawnPerYield = 350;

	private List<XYZ> __changedVoxel = new List<XYZ>(27);

	public DamagedBlockTracker BlockTracker { get; private set; }

	public HashSet<RootBlockData> RootBlocks => _rootBlockData;

	public BlockDataStruct[] AllRootBlockData()
	{
		BlockDataStruct[] array = new BlockDataStruct[_rootBlockData.Count];
		Slinq<BlockDataStruct, SelectContext<BlockDataStruct, RootBlockData, HashSetSlinqContext<RootBlockData>>> slinq = from rbd in _rootBlockData.Slinq()
			select rbd.ToSaveableStruct();
		for (int num = 0; num < _rootBlockData.Count; num++)
		{
			ref BlockDataStruct reference = ref array[num];
			reference = slinq.current.value;
			slinq.Skip();
		}
		return array;
	}

	public IEnumerator LoadBlockData(BlockDataStruct[] placedBlockData)
	{
		int i = 0;
		foreach (BlockDataStruct bds in placedBlockData)
		{
			i++;
			RegisterBlockDataStruct(bds);
			if (i >= maxSpawnPerYield)
			{
				i = 0;
				yield return new WaitForFixedUpdate();
			}
		}
		yield return null;
	}

	private void RegisterBlockDataStruct(BlockDataStruct bds)
	{
		CreateBlock(bds.P, bds.R, bds.type.BlockInfoIDToBlockInfo(), bds.Dmg);
		Architecte.Changement(bds.P);
	}

	protected override void Awake()
	{
		base.Awake();
		BlockTracker = GetComponent<DamagedBlockTracker>();
		RootBlockData.RootBlockCreated.Connect(delegate(RootBlockData rbd)
		{
			_rootBlockData.Add(rbd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		RootBlockData.RootBlockCreated.Connect(delegate(RootBlockData rbd)
		{
			OnBlockDataCreated(rbd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		RootBlockData.RootBlockReleased.Connect(delegate(RootBlockData rbd)
		{
			_rootBlockData.Remove(rbd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		RootBlockData.RootBlockReleased.Connect(delegate(RootBlockData rbd)
		{
			OnBlockDataReleased(rbd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		BlockData.BlockDataReleased.Connect(delegate(IBlockData ibd)
		{
			OnBlockDataReleased(ibd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		BlockData.BlockDataCreated.Connect(delegate(IBlockData ibd)
		{
			OnBlockDataCreated(ibd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	public GameObject CreateBlock(XYZ rootPosition, Quaternion rotation, BlockInfo bi, float dmg = 0f)
	{
		if (bi.Type != BlockType.Block)
		{
			string message = $"Invalid BlockInfo argument for CreateBlock request. BlockInfo '{bi.Key.Name}' must be of BlockType 'Block'. Current type is '{Enum.GetName(typeof(BlockType), bi.Type)}'";
			throw new ArgumentException(message);
		}
		return SpawnPlacedBlockGO(bi, rootPosition, rotation, dmg);
	}

	public void RegisterBlock(XYZ rootPosition, int rotationIndex, BlockInfo bi, GameObject placedBlock, float dmg = 0f)
	{
		logger.DebugFormat("Registered block {0} for block type {1}", rootPosition, bi.ID);
		bi.GenerateBlocData(rootPosition, rotationIndex, placedBlock, dmg);
		GameSignals.Invoke(GameSignals.BlockTypeChanged, rootPosition);
	}

	private GameObject SpawnPlacedBlockGO(BlockInfo blockInfo, XYZ position, Quaternion rotation, float dmg)
	{
		GameObject gameObject = Factory.Peek(blockInfo.BlueprintKey);
		Vector3 position2 = position.ToVector3() - rotation * gameObject.GetComponent<BaseVoxelTransform>().CenterOffset;
		Factory.AssetKey blueprintResult = gameObject.GetComponent<Blueprint>().BlueprintResult;
		ObjectPoolSingleton.Request(out var go, blueprintResult, position2, rotation);
		go.GetComponent<VoxelBlockRequestComponent>().dmg = dmg;
		return go;
	}

	public List<XYZ> RemoveBlock(RootBlockData rbd)
	{
		__changedVoxel.Clear();
		rbd.Volume.GetCases(VProperty.occupy).Slinq().Where((XYZ p, XYZ rp) => rp != p, rbd.Position)
			.ForEach(delegate(XYZ p, RootBlockData _rbd)
			{
				RemoveEntry(p, _rbd);
			}, rbd);
		RemoveEntry(rbd.Position, rbd);
		return __changedVoxel;
	}

	private void RemoveEntry(XYZ p, RootBlockData rbd)
	{
		__changedVoxel.Add(p);
		IBlockData blockData;
		try
		{
			blockData = _blocks[p];
		}
		catch (KeyNotFoundException)
		{
			logger.ErrorFormat("Tried to remove an non-existing block at position '{0}'", p);
			return;
		}
		blockData.RemoveBlockWithMatchingRoot(rbd);
	}

	public void OnBlockDataCreated(IBlockData bd)
	{
		if (_blocks.TryGetValue(bd.Position, out var value))
		{
			if (value.BlockInfo.MergeableBloc && bd.BlockInfo.MergeableBloc)
			{
				value = MultiBlockData.Make(value, bd);
			}
			else
			{
				value.DeBlock();
				value = bd;
			}
		}
		else
		{
			value = bd;
		}
		_blocks[bd.Position] = value;
	}

	private void OnBlockDataReleased(IBlockData ibd)
	{
		if (_blocks[ibd.Position] == ibd)
		{
			_blocks.Remove(ibd.Position);
		}
	}

	public bool IsBlock(XYZ position)
	{
		return _blocks.ContainsKey(position);
	}

	public int GetBlockID(XYZ position)
	{
		IBlockData value;
		return _blocks.TryGetValue(position, out value) ? value.BlockInfo.ID : 0;
	}

	public IBlockData GetBlockData(XYZ position)
	{
		_blocks.TryGetValue(position, out var value);
		return value;
	}

	public IEnumerable<BlockData> GetDamagedBlocksIn(Bounds bounds)
	{
		foreach (BlockData bd in BlockTracker.DamagedBlocks)
		{
			if (bounds.Contains(bd.Position.ToVector3()))
			{
				yield return bd;
			}
		}
	}
}
