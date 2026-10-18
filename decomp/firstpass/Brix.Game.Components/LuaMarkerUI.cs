using Brix.Input;
using Brix.Legacy;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Signals;
using Brix.WorldEditor;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class LuaMarkerUI : GameComponent
{
	private LuaMarker luaMarker;

	[SerializeField]
	private Beacon beacon;

	private bool placed;

	protected override void Start()
	{
		base.Start();
		luaMarker = GetComponentInParent<LuaMarker>();
		if ((bool)luaMarker)
		{
			luaMarker.MarkerChanged.Connect(delegate
			{
				GenerateMarkerName(incrementCount: true);
			}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
		if (!string.IsNullOrEmpty(luaMarker.DisplayName) && !placed)
		{
			SetDisplayName(base.name);
			placed = true;
		}
		else if (GetComponentInParent<Picker>() == null && !placed)
		{
			GenerateMarkerName(incrementCount: true);
			placed = true;
		}
		else
		{
			GenerateMarkerName(incrementCount: false);
		}
	}

	public void GenerateMarkerName(bool incrementCount)
	{
		string empty = string.Empty;
		string m = luaMarker.markerGroups.Slinq().FirstOrDefault();
		string text = empty;
		empty = text + PresetExceptions(m) + " ( " + MarkerSuffix(m, incrementCount) + " )";
		SetDisplayName(empty);
	}

	public void SetDisplayName(string name)
	{
		beacon.SetName(name);
		luaMarker.DisplayName = name;
	}

	private string PresetExceptions(string m)
	{
		return m switch
		{
			"Player Starting Point" => I2Helper.TryGet("##mode_worldeditor_markers_player_marker"), 
			"Crystal" => I2Helper.TryGet("##mode_worldeditor_markers_crystal_marker"), 
			"Corruptron Spawn Point" => I2Helper.TryGet("##mode_worldeditor_markers_enemy_marker"), 
			_ => m, 
		};
	}

	private string MarkerSuffix(string m, bool incrementCount)
	{
		LuaDriver componentInParent = GetComponentInParent<LuaDriver>();
		if ((bool)componentInParent)
		{
			return GetMarkerCount(m, incrementCount).ToString();
		}
		return string.Empty;
	}

	private int GetMarkerCount(string m, bool incrementCount)
	{
		WorldEditorSerializableSettings.Instance.TotalMarkerCreatedCount.TryGetValue(m, out var value);
		if (value == 0)
		{
			WorldEditorSerializableSettings.Instance.TotalMarkerCreatedCount[m] = LuaMarker.GetGroupCount(m) - 1;
		}
		value++;
		if (incrementCount)
		{
			WorldEditorSerializableSettings.Instance.TotalMarkerCreatedCount[m] = value;
		}
		return value;
	}

	private void OnTransformParentChanged()
	{
		if ((bool)luaMarker && GetComponentInParent<Picker>() == null && !placed)
		{
			placed = true;
			GenerateMarkerName(incrementCount: true);
		}
	}
}
