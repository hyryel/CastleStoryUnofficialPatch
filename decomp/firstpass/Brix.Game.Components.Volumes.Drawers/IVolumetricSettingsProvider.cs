using System.Collections.Generic;

namespace Brix.Game.Components.Volumes.Drawers;

public interface IVolumetricSettingsProvider
{
	bool DrawVSettingsGUI { get; set; }

	HashSet<VolumetricEntry> VolumesDrawn { get; }

	VPropertyMask VPropertiesToDraw { get; set; }

	void Clear();

	void VSettingsForVolumetricEntry(ref VPropertyMask mask, VolumetricEntry ve);

	void Redraw();
}
