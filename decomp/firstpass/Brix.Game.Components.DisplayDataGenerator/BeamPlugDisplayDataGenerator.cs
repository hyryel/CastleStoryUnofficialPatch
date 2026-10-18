using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.DisplayDataGenerator;

public class BeamPlugDisplayDataGenerator : GameComponent, IVoxelDisplayDataGenerator
{
	[SerializeField]
	private Mesh _thisMesh;

	[SerializeField]
	private Material[] _thisMaterial;

	private DefaultBlockMeshSelector _beamPlugZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _beamPlugZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _beamPlugZNegativeCulledAssetKey;

	[SerializeField]
	public CrackMaterialHolder CrackMatHolder;

	private Material[] damagedMat1 = new Material[1];

	private Material[] damagedMat2 = new Material[1];

	private Material[] damagedMat3 = new Material[1];

	private Material[] damagedMat4 = new Material[1];

	private List<VoxelDisplayData> _voxelDisplayDataList = new List<VoxelDisplayData>(5);

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		_voxelDisplayDataList.Clear();
		Material[] thisMaterial = _thisMaterial;
		if (bd.Damage > 0.75f)
		{
			thisMaterial = damagedMat4;
		}
		else if (bd.Damage > 0.5f)
		{
			thisMaterial = damagedMat3;
		}
		else if (bd.Damage > 0.25f)
		{
			thisMaterial = damagedMat2;
		}
		else if (bd.Damage > 0f)
		{
			thisMaterial = damagedMat1;
		}
		_voxelDisplayDataList.Add(new VoxelDisplayData("BrickPlug", _thisMesh, thisMaterial, bd.AbsolutePivotPosition, bd.Rotation));
		GenerateBeamPlugDisplayData(bd);
		_voxelDisplayDataList.Slinq().ForEach(delegate(VoxelDisplayData v, IVoxelDispatcher d)
		{
			d.Dispatch(v);
		}, dispatcher);
	}

	public override void Initialize()
	{
		base.Initialize();
		_beamPlugZPositiveCulled = Factory.Peek(_beamPlugZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamPlugZNegativeCulled = Factory.Peek(_beamPlugZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		if (CrackMatHolder != null && CrackMatHolder.DamagedMats.Length == 4)
		{
			damagedMat1[0] = CrackMatHolder.DamagedMats[0];
			damagedMat2[0] = CrackMatHolder.DamagedMats[1];
			damagedMat3[0] = CrackMatHolder.DamagedMats[2];
			damagedMat4[0] = CrackMatHolder.DamagedMats[3];
		}
	}

	private void GenerateBeamPlugDisplayData(BlockData bd)
	{
		Factory.AssetKey wood_Beam = Blocks.Wood_Beam;
		TryAddVoxelDisplayData(bd, XYZ.forward, _beamPlugZPositiveCulled, wood_Beam, Quaternion.identity);
		TryAddVoxelDisplayData(bd, -XYZ.forward, _beamPlugZNegativeCulled, wood_Beam, Quaternion.identity);
		TryAddVoxelDisplayData(bd, XYZ.right, _beamPlugZPositiveCulled, wood_Beam, Quaternion.Euler(0f, 90f, 0f));
		TryAddVoxelDisplayData(bd, -XYZ.right, _beamPlugZNegativeCulled, wood_Beam, Quaternion.Euler(0f, 90f, 0f));
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
}
