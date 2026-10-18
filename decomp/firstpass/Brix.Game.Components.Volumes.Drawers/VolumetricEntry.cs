using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.Components.Volumes.Drawers;

public class VolumetricEntry : IEquatable<VolumetricEntry>
{
	public static Signal<VolumetricEntry> VolumetricEntryCreatedSignal = new Signal<VolumetricEntry>();

	public static Signal<VolumetricEntry> VolumetricEntryDirtiedSignal = new Signal<VolumetricEntry>();

	public static Signal<VolumetricEntry> VolumetricEntryDestroyedSignal = new Signal<VolumetricEntry>();

	private SignalConnection _onVolumeRegistrationConnectionSignal = SignalConnection.Empty;

	private SignalConnection _onVolumeUnregistrationConnectionSignal = SignalConnection.Empty;

	private SignalConnection _onReleaseOrDestroySignal = SignalConnection.Empty;

	private Signal<GameObject> _stopDrawingSignal = new Signal<GameObject>();

	private IVolume _volume;

	[SerializeField]
	private bool _debugDrawing;

	public Signal<GameObject> StopDrawingSignal => _stopDrawingSignal;

	public IVolume Volume => _volume;

	public bool DebugDrawing
	{
		get
		{
			return _debugDrawing;
		}
		set
		{
			_debugDrawing = value;
			if (_volume.Registered)
			{
				VolumetricEntryDirtiedSignal.Invoke(this);
			}
		}
	}

	private VolumetricEntry(IVolume v)
	{
		_volume = v;
		_debugDrawing = false;
		_onVolumeRegistrationConnectionSignal = v.RegisteredSignal.Connect(delegate
		{
			OnVolumetricEntryDirtied();
		});
		_onVolumeUnregistrationConnectionSignal = v.UnregisteredSignal.Connect(delegate
		{
			OnVolumetricEntryDirtied();
		});
		v.OnReleaseOrDestroySignal.ConnectOnce(delegate
		{
			DestroyVolumetricEntry(this);
		});
	}

	public static void Create(IVolume volume)
	{
		VolumetricEntryCreatedSignal.Invoke(new VolumetricEntry(volume));
	}

	private static void DestroyVolumetricEntry(VolumetricEntry ve)
	{
		VolumetricEntryDestroyedSignal.Invoke(ve);
		DisconnectSignals(ve);
		ve.Undraw();
	}

	private static void DisconnectSignals(VolumetricEntry ve)
	{
		ve._onVolumeRegistrationConnectionSignal.Disconnect();
		ve._onVolumeUnregistrationConnectionSignal.Disconnect();
		ve._onReleaseOrDestroySignal.Disconnect();
	}

	~VolumetricEntry()
	{
		DestroyVolumetricEntry(this);
	}

	private void OnVolumetricEntryDirtied()
	{
		VolumetricEntryDirtiedSignal.Invoke(this);
	}

	public void Draw()
	{
		Undraw();
		DrawVolumetricEntry();
	}

	public void Undraw()
	{
		StopDrawingSignal.Invoke(null);
	}

	private void DrawVolumetricEntry()
	{
		if (!_volume.Registered)
		{
			return;
		}
		VPropertyMask vPropertyMask = GenerateDrawnVProperties();
		foreach (KeyValuePair<XYZ, VPropertyMask> item in _volume)
		{
			int num = 0;
			VPropertyMask vPropertyMask2 = vPropertyMask & item.Value;
			if (vPropertyMask2 == ~VPropertyMask.All)
			{
				continue;
			}
			Color color = Color.black;
			foreach (int item2 in vPropertyMask2.Bits())
			{
				if (VPropertyExtender.VPropertyColors.TryGetValue((VProperty)item2, out var value))
				{
					color = Color.Lerp(color, value, 1f / (float)(++num));
				}
			}
			if (num > 0)
			{
				Highlighter.highlight.Terrain(item.Key, color, StopDrawingSignal);
			}
		}
	}

	private VPropertyMask GenerateDrawnVProperties()
	{
		VPropertyMask mask = ~VPropertyMask.All;
		for (int i = 0; i < VolumeRegistryDrawer.vSettingsProvider.Count; i++)
		{
			VolumeRegistryDrawer.vSettingsProvider[i].VSettingsForVolumetricEntry(ref mask, this);
		}
		return mask;
	}

	public bool Equals(VolumetricEntry other)
	{
		return Volume.Equals(other._volume);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		return obj is VolumetricEntry && Equals((VolumetricEntry)obj);
	}

	public override int GetHashCode()
	{
		return _volume.GetHashCode();
	}

	public static bool operator ==(VolumetricEntry left, VolumetricEntry right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(VolumetricEntry left, VolumetricEntry right)
	{
		return !left.Equals(right);
	}
}
