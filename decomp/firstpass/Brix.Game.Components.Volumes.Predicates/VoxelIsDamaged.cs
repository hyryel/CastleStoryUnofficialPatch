using Brix.Engine;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelIsDamaged : VoxelPredicateComponent
{
	public override bool HasProperty(XYZ p)
	{
		using IVoxel voxel = Voxel.GetVoxel(p);
		return voxel.IsDamaged;
	}
}
