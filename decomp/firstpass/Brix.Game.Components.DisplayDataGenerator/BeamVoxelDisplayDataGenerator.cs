using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.Blocks.MeshSelector;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.DisplayDataGenerator;

public class BeamVoxelDisplayDataGenerator : GameComponent, IVoxelDisplayDataGenerator
{
	private DefaultBlockMeshSelector _beamUnculled;

	[SerializeField]
	private Factory.AssetKey _beamUnculledAssetKey;

	private DefaultBlockMeshSelector _beamZPositiveCulled;

	[SerializeField]
	private Factory.AssetKey _beamZPositiveCulledAssetKey;

	private DefaultBlockMeshSelector _beamZNegativeCulled;

	[SerializeField]
	private Factory.AssetKey _beamZNegativeCulledAssetKey;

	private DefaultBlockMeshSelector _beamCulled;

	[SerializeField]
	private Factory.AssetKey _beamCulledAssetKey;

	private static HashSet<Factory.AssetKey> _brickPlugKeys = new HashSet<Factory.AssetKey>
	{
		Blocks.Stone_BeamPlug_AllSides,
		Blocks.Stone_BeamPlug_Corner,
		Blocks.Stone_BeamPlug_OneSide,
		Blocks.Stone_BeamPlug_TwoSides,
		Blocks.Stone_BeamPlug_ThreeSides
	};

	public override void Initialize()
	{
		base.Initialize();
		_beamUnculled = Factory.Peek(_beamUnculledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamZPositiveCulled = Factory.Peek(_beamZPositiveCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamZNegativeCulled = Factory.Peek(_beamZNegativeCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
		_beamCulled = Factory.Peek(_beamCulledAssetKey).GetComponent<DefaultBlockMeshSelector>();
	}

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		AssetKeyHolder component = GetComponent<AssetKeyHolder>();
		Mesh mesh = SelectMeshFromSurrounding(bd);
		dispatcher.Dispatch(new VoxelDisplayData(component.AssetKey.ToString(), mesh, _beamCulled.Material, bd.AbsolutePivotPosition, bd.CanonicalRotation));
	}

	private Mesh SelectMeshFromSurrounding(BlockData bd)
	{
		Vector3 referenceForward = bd.CanonicalRotation * Vector3.forward;
		XYZ xYZ = XYZ.FromVector3(bd.CanonicalRotation * Vector3.forward);
		using IBlockData ibd = Voxel.GetBlockData(bd.Position + xYZ);
		using IBlockData ibd2 = Voxel.GetBlockData(bd.Position - xYZ);
		if (HasMatchingBlock(bd, ibd, referenceForward))
		{
			if (HasMatchingBlock(bd, ibd2, referenceForward))
			{
				return _beamCulled.MeshVariantFor(bd, xYZ);
			}
			return _beamZPositiveCulled.MeshVariantFor(bd, xYZ);
		}
		if (HasMatchingBlock(bd, ibd2, referenceForward))
		{
			return _beamZNegativeCulled.MeshVariantFor(bd, xYZ);
		}
		return _beamUnculled.MeshVariantFor(bd, xYZ);
	}

	private bool HasMatchingBlock(BlockData bd, IBlockData ibd, Vector3 referenceForward)
	{
		return ibd.IsOfType(Blocks.Wood_SingleLog) || ibd.IsOfType(Blocks.Wood_VerticalBeam) || HasMatchingBrickPlug(bd, ibd) || HasMatchingBeam(bd, ibd, referenceForward);
	}

	private bool HasMatchingBrickPlug(BlockData bd, IBlockData ibd)
	{
		return _brickPlugKeys.Slinq().Any((Factory.AssetKey key, IBlockData b) => b.IsOfType(key), ibd) && ibd.Volume.HasPropertyAt(VProperty.canHookBeam, bd.Position);
	}

	private bool HasMatchingBeam(BlockData bd, IBlockData ibd, Vector3 referenceForward)
	{
		if (ibd is MultiBlockData multiBlockData)
		{
			ibd = multiBlockData.ChildBlocks.Slinq().FirstOrDefault((IBlockData childBD) => childBD.IsOfType(bd.BlockInfo.Key));
			return ibd != null && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
		}
		return ibd.IsOfType(bd.BlockInfo.Key) && Mathf.Abs(Vector3.Dot(ibd.Rotation * Vector3.forward, referenceForward)) > 0.5f;
	}
}
