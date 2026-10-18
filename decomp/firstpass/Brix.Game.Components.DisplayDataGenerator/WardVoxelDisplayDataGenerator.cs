using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.Engine.Blocks.MeshSelector;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.Components.DisplayDataGenerator;

public class WardVoxelDisplayDataGenerator : MonoBehaviour, IVoxelDisplayDataGenerator
{
	[SerializeField]
	private DefaultBlockMeshSelector _wallTorch;

	[SerializeField]
	private DefaultBlockMeshSelector _floorTorch;

	public void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher)
	{
		DefaultBlockMeshSelector defaultBlockMeshSelector = InferMeshFromSurrounding(bd);
		Quaternion canonicalRotation = bd.CanonicalRotation;
		Vector3 v = canonicalRotation * Vector3.forward;
		XYZ dominantAxis = XYZ.FromVector3(v);
		defaultBlockMeshSelector.MeshVariantFor(bd.Position, dominantAxis);
		dispatcher.Dispatch(new VoxelDisplayData(defaultBlockMeshSelector.name, defaultBlockMeshSelector.MeshVariantFor(bd, XYZ.up), defaultBlockMeshSelector.Material, bd.AbsolutePivotPosition, bd.Rotation));
	}

	private DefaultBlockMeshSelector InferMeshFromSurrounding(BlockData voxel)
	{
		DefaultBlockMeshSelector result = _floorTorch;
		if (VolumeRegistry.HasVolumesWithPropertyAt(voxel.Position + XYZ.forward, VProperty.stoneSupport) || VolumeRegistry.HasVolumesWithPropertyAt(voxel.Position + -XYZ.forward, VProperty.stoneSupport) || VolumeRegistry.HasVolumesWithPropertyAt(voxel.Position + -XYZ.right, VProperty.stoneSupport) || VolumeRegistry.HasVolumesWithPropertyAt(voxel.Position + XYZ.right, VProperty.stoneSupport))
		{
			result = _wallTorch;
		}
		return result;
	}
}
