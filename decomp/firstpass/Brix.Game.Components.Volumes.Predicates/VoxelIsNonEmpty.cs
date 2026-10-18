using Brix.Engine;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelIsNonEmpty : VoxelPredicateComponent
{
	public override bool HasProperty(XYZ p)
	{
		return Voxel.IsNonEmpty(p);
	}
}
