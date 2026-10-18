using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.DisplayDataGenerator;

public class LogVoxelDisplayDataGenerator : GameComponent, IVoxelDisplayDataGenerator
{
	private DefaultBlockMeshSelector _logUnculled;

	[SerializeField]
	private Factory.AssetKey _logUnculledAssetKey;

	private DefaultBlockMeshSelector _logTopFaceCulled;

	[SerializeField]
	private Factory.AssetKey _logTopFaceCulledAssetKey;

	private DefaultBlockMeshSelector _logTopBottomFaceCulled;

	[SerializeField]
	private Factory.AssetKey _logTopBottomFaceCulledAssetKey;

	private DefaultBlockMeshSelector _logBottomFaceCulledLog;

	[SerializeField]
	private Factory.AssetKey _logBottomFaceCulledLogAssetKey;

	private DefaultBlockMeshSelector _beamPlugZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _beamPlugZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZNegativeCulledAssetKey;

	private DefaultBlockMeshSelector _plankPlugZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _plankPlugZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _plankPlugZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _plankPlugZNegativeCulledAssetKey;

	private List<VoxelDisplayData> _voxelDisplayDataList = new List<VoxelDisplayData>(5);

	public override void Initialize()
	{
		base.Initialize();
		_logUnculled = Factory.Peek(_logUnculledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_logTopFaceCulled = Factory.Peek(_logTopFaceCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_logTopBottomFaceCulled = Factory.Peek(_logTopBottomFaceCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_logBottomFaceCulledLog = Factory.Peek(_logBottomFaceCulledLogAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamPlugZPositiveCulled = Factory.Peek(_beamPlugZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamPlugZNegativeCulled = Factory.Peek(_beamPlugZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankPlugZPositiveCulled = Factory.Peek(_plankPlugZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_plankPlugZNegativeCulled = Factory.Peek(_plankPlugZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		_voxelDisplayDataList.Clear();
		DefaultBlockMeshSelector defaultBlockMeshSelector = InferLogMeshSelectorFromSurrounding(bd);
		_voxelDisplayDataList.Add(new VoxelDisplayData(defaultBlockMeshSelector.name, defaultBlockMeshSelector.MeshVariantFor(bd, XYZ.up), defaultBlockMeshSelector.Material, bd.AbsolutePivotPosition, bd.Rotation));
		GenerateBeamPlugDisplayData(bd);
		GeneratePlankPlugDisplayData(bd);
		_voxelDisplayDataList.Slinq().ForEach(delegate(VoxelDisplayData v, IVoxelDispatcher d)
		{
			d.Dispatch(v);
		}, dispatcher);
	}

	private void GenerateBeamPlugDisplayData(BlockData bd)
	{
		Factory.AssetKey wood_Beam = Blocks.Wood_Beam;
		TryAddVoxelDisplayData(bd, XYZ.forward, _beamPlugZPositiveCulled, wood_Beam, Quaternion.identity);
		TryAddVoxelDisplayData(bd, -XYZ.forward, _beamPlugZNegativeCulled, wood_Beam, Quaternion.identity);
		TryAddVoxelDisplayData(bd, XYZ.right, _beamPlugZPositiveCulled, wood_Beam, Quaternion.Euler(0f, 90f, 0f));
		TryAddVoxelDisplayData(bd, -XYZ.right, _beamPlugZNegativeCulled, wood_Beam, Quaternion.Euler(0f, 90f, 0f));
	}

	private void GeneratePlankPlugDisplayData(BlockData bd)
	{
		Factory.AssetKey wood_SinglePlank = Blocks.Wood_SinglePlank;
		TryAddVoxelDisplayData(bd, XYZ.forward, _plankPlugZPositiveCulled, wood_SinglePlank, Quaternion.identity);
		TryAddVoxelDisplayData(bd, -XYZ.forward, _plankPlugZNegativeCulled, wood_SinglePlank, Quaternion.identity);
		TryAddVoxelDisplayData(bd, XYZ.right, _plankPlugZPositiveCulled, wood_SinglePlank, Quaternion.Euler(0f, 90f, 0f));
		TryAddVoxelDisplayData(bd, -XYZ.right, _plankPlugZNegativeCulled, wood_SinglePlank, Quaternion.Euler(0f, 90f, 0f));
	}

	private void TryAddVoxelDisplayData(BlockData logBD, XYZ deltaP, DefaultBlockMeshSelector meshSelector, Factory.AssetKey key, Quaternion r)
	{
		using IVoxel voxel = Voxel.GetVoxel(logBD.Position + deltaP);
		if (voxel.IsOfType(key) && IsBlockRelativeZAxisFacing(key, voxel, deltaP.ToVector3()))
		{
			VoxelDisplayData item = new VoxelDisplayData(meshSelector.name, meshSelector.MeshVariantFor(logBD, deltaP), meshSelector.Material, logBD.AbsolutePivotPosition, r);
			_voxelDisplayDataList.Add(item);
		}
	}

	private bool IsBlockRelativeZAxisFacing(Factory.AssetKey assetKey, IVoxel ibd, Vector3 referenceForward)
	{
		if (ibd is MultiBlockData multiBlockData)
		{
			ibd = multiBlockData.ChildBlocks.Slinq().First((IBlockData bd) => bd.IsOfType(assetKey));
		}
		return Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
	}

	private DefaultBlockMeshSelector InferLogMeshSelectorFromSurrounding(BlockData bd)
	{
		using IVoxel voxel = Voxel.GetVoxel(bd.Position + XYZ.up);
		using IVoxel voxel2 = Voxel.GetVoxel(bd.Position - XYZ.up);
		bool flag = voxel.IsNull || voxel.IsTerrain || voxel is MultiBlockData || voxel.BlockInfo.BlueprintKey != bd.BlockInfo.BlueprintKey;
		bool flag2 = voxel2.IsNull || voxel2.IsTerrain || voxel2.BlockInfo.BlueprintKey != bd.BlockInfo.BlueprintKey;
		if (flag)
		{
			if (flag2)
			{
				return _logUnculled;
			}
			return _logBottomFaceCulledLog;
		}
		if (flag2)
		{
			return _logTopFaceCulled;
		}
		return _logTopBottomFaceCulled;
	}
}
