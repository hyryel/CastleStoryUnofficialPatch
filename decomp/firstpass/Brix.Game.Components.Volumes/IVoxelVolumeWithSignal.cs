using Brix.Engine;
using Brix.External.Signals;

namespace Brix.Game.Components.Volumes;

public interface IVoxelVolumeWithSignal
{
	IVoxelVolume Volume { get; }

	IReadOnlySignal VolumeChanged { get; }
}
