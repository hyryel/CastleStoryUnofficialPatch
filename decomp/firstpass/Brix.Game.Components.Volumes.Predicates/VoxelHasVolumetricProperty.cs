using Brix.Engine;

namespace Brix.Game.Components.Volumes.Predicates;

public class VoxelHasVolumetricProperty : VoxelPredicateComponent
{
	public VProperty[] properties;

	public override bool HasProperty(XYZ p)
	{
		VPropertyMask vPropertyMask = ~VPropertyMask.All;
		VProperty[] array = properties;
		foreach (VProperty p2 in array)
		{
			vPropertyMask |= p2.ToMask();
		}
		return VolumeRegistry.HasVolumesWithPropertyAt(p, vPropertyMask);
	}
}
