using System.Collections.Generic;
using Brix.External.Signals;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Drawers;

public class VolumeDrivenDebugDrawer : IVolumetricSettingsProvider
{
	private SignalConnection _volumetricEntryCreatedSignalConnection;

	private SignalConnection _volumetricEntryDirtiedSignalConnection;

	private SignalConnection _volumetricEntryDestroyedSignalConnection;

	public HashSet<VolumetricEntry> VolumesUnregistered = new HashSet<VolumetricEntry>();

	public HashSet<VolumetricEntry> VolumesInRegistry = new HashSet<VolumetricEntry>();

	private bool _enabled = true;

	public bool DrawUnregisteredVolumesGUI { get; set; }

	public bool DrawVolumesInRegistryGUI { get; set; }

	public HashSet<VolumetricEntry> VolumesDrawn { get; private set; }

	public bool DrawVolumeDrawnListGUI { get; set; }

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
			if (_enabled != value)
			{
				_enabled = value;
				Redraw();
			}
		}
	}

	public VolumeDrivenDebugDrawer()
	{
		_volumetricEntryCreatedSignalConnection = VolumetricEntry.VolumetricEntryCreatedSignal.Connect(OnVolumetricEntryCreated);
		_volumetricEntryDirtiedSignalConnection = VolumetricEntry.VolumetricEntryDirtiedSignal.Connect(OnVolumetricEntryDirtied);
		_volumetricEntryDestroyedSignalConnection = VolumetricEntry.VolumetricEntryDestroyedSignal.Connect(RemoveEntry);
		VolumesDrawn = new HashSet<VolumetricEntry>();
		VPropertiesToDraw = ~VPropertyMask.All;
	}

	public void Clear()
	{
		_volumetricEntryCreatedSignalConnection.Disconnect();
		_volumetricEntryDirtiedSignalConnection.Disconnect();
		_volumetricEntryDestroyedSignalConnection.Disconnect();
		VolumesUnregistered.Clear();
		VolumesInRegistry.Clear();
		VolumesDrawn.Clear();
	}

	public void VSettingsForVolumetricEntry(ref VPropertyMask mask, VolumetricEntry ve)
	{
		if (_enabled && VolumesDrawn.Contains(ve))
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

	private void OnVolumetricEntryCreated(VolumetricEntry ve)
	{
		VolumesUnregistered.Add(ve);
	}

	private void OnVolumetricEntryDirtied(VolumetricEntry ve)
	{
		if (ve.Volume.Registered)
		{
			if (ve.DebugDrawing)
			{
				AddToDrawingList(ve);
			}
			else
			{
				AddToRegisteredVolumeList(ve);
			}
		}
		else
		{
			AddToUnregisteredVolumeList(ve);
		}
	}

	private void AddToDrawingList(VolumetricEntry volumetricEntry)
	{
		if (VolumesInRegistry.Remove(volumetricEntry) || VolumesUnregistered.Remove(volumetricEntry))
		{
			VolumesDrawn.Add(volumetricEntry);
		}
	}

	private void AddToRegisteredVolumeList(VolumetricEntry volumetricEntry)
	{
		if (VolumesDrawn.Remove(volumetricEntry) || VolumesUnregistered.Remove(volumetricEntry))
		{
			VolumesInRegistry.Add(volumetricEntry);
		}
	}

	private void AddToUnregisteredVolumeList(VolumetricEntry volumetricEntry)
	{
		if (VolumesDrawn.Remove(volumetricEntry) || VolumesInRegistry.Remove(volumetricEntry))
		{
			VolumesUnregistered.Add(volumetricEntry);
		}
	}

	private void RemoveEntry(VolumetricEntry volumetricEntry)
	{
		VolumesDrawn.Remove(volumetricEntry);
		VolumesInRegistry.Remove(volumetricEntry);
		VolumesUnregistered.Remove(volumetricEntry);
	}
}
