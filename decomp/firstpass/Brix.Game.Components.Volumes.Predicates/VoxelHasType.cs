using Brix.Engine;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelHasType : VoxelPredicateComponent
{
	public VoxelTerrainType[] acceptedTypes;

	public override bool HasProperty(XYZ p)
	{
		IVoxel ibd = Voxel.GetVoxel(p);
		try
		{
			return acceptedTypes.Slinq().Any((VoxelTerrainType t) => ibd.BlockInfo.IsOfType((int)t));
		}
		finally
		{
			if (ibd != null)
			{
				ibd.Dispose();
			}
		}
	}
}
