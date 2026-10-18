using Brix.Engine;

namespace Brix.Game.Components.Volumes.Generators;

public class VoxelVolumeCustomComponent : VoxelVolumeComponent
{
	public IVoxelVolume volume;

	public override IVoxelVolume Volume => volume;
}
