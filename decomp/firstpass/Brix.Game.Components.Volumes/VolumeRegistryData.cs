using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Game.Components.Volumes;

public class VolumeRegistryData
{
	public Dictionary<XYZ, RegistryEntry> IndexedEntries = new Dictionary<XYZ, RegistryEntry>();

	public Dictionary<IVolume, BaseVolume> RegisteredVolumesData = new Dictionary<IVolume, BaseVolume>();

	public void Reset()
	{
		IndexedEntries.Clear();
		RegisteredVolumesData.Clear();
	}
}
