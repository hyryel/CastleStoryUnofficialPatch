using System.Collections.Generic;

namespace Brix.Game.Components.Volumes.Drawers;

public class VolumeRegistryDrawer : GameComponent
{
	public static List<IVolumetricSettingsProvider> vSettingsProvider = new List<IVolumetricSettingsProvider>();
}
