using System.Collections.Generic;
using Brix.External.Signals;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Drawers;

public class GlobalDebugDrawer : IVolumetricSettingsProvider
{
	private SignalConnection _volumetricEntryDirtiedSignalConnection;

	private bool _enabled = true;

	public HashSet<VolumetricEntry> VolumesDrawn { get; private set; }

	public VPropertyMask VPropertiesToDraw { get; set; }

	public bool DrawVSettingsGUI { get; set; }

	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if ((_enabled = value) != _enabled)
			{
				Redraw();
			}
		}
	}

	public GlobalDebugDrawer()
	{
		_volumetricEntryDirtiedSignalConnection = VolumetricEntry.VolumetricEntryDirtiedSignal.Connect(OnVolumetricEntryDirtied);
		VolumesDrawn = new HashSet<VolumetricEntry>();
		VPropertiesToDraw = ~VPropertyMask.All;
	}

	public void VSettingsForVolumetricEntry(ref VPropertyMask mask, VolumetricEntry ve)
	{
		if (Enabled)
		{
			mask |= VPropertiesToDraw;
		}
	}

	public void Redraw()
	{
		VolumesDrawn.Slinq().ForEach(delegate(VolumetricEntry ve)
		{
			ve.Draw();
		});
	}

	public void Clear()
	{
		VolumesDrawn.Clear();
		_volumetricEntryDirtiedSignalConnection.Disconnect();
	}

	private void OnVolumetricEntryDirtied(VolumetricEntry ve)
	{
		if (ve.Volume.Registered)
		{
			VolumesDrawn.Add(ve);
		}
		else
		{
			VolumesDrawn.Remove(ve);
		}
	}
}
