using Brix.Components;
using Brix.Engine;
using Brix.External.Signals;

namespace Brix.Game.Components.Volumes;

public abstract class VoxelVolumeComponent : BrixComponent
{
	public Signal VolumeChanged = new Signal();

	public abstract IVoxelVolume Volume { get; }
}
public abstract class VoxelVolumeComponent<T> : VoxelVolumeComponent where T : IVoxelVolume
{
	public abstract T VolumeNoLeak { get; }
}
