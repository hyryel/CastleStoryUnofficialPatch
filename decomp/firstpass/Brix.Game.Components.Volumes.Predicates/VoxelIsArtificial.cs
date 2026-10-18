using Brix.Engine;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelIsArtificial : VoxelPredicateComponent
{
	public override bool HasProperty(XYZ p)
	{
		using IVoxel voxel = Voxel.GetVoxel(p);
		return voxel.BlockInfo.Type == BlockType.Block;
	}
}
