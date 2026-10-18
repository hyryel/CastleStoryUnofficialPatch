using System;
using Brix.External.Factories;
using Brix.Lua;
using UnityEngine;

namespace Brix.Input;

public static class UIMarkersInterface
{
	public enum SpawnerObject
	{
		Unknown,
		PlayerSpawn,
		Crystal,
		CorruptronSpawn,
		Tutorial
	}

	public static void SendToPicker(SpawnerObject key)
	{
		switch (key)
		{
		case SpawnerObject.PlayerSpawn:
			Picking.Instance.SendPicker(Pickers.ObjectPlacement, Balises.WorldEditorMarker, new ObjectPlacementPicker.MarkerPlacementOptions(ToTagName(SpawnerObject.PlayerSpawn)));
			break;
		case SpawnerObject.Crystal:
			Picking.Instance.SendPicker(Pickers.ObjectPlacement, Balises.WorldEditorMarker, new ObjectPlacementPicker.MarkerPlacementOptions(ToTagName(SpawnerObject.Crystal)));
			break;
		case SpawnerObject.CorruptronSpawn:
			Picking.Instance.SendPicker(Pickers.ObjectPlacement, Balises.WorldEditorMarker, new ObjectPlacementPicker.MarkerPlacementOptions(ToTagName(SpawnerObject.CorruptronSpawn)));
			break;
		case SpawnerObject.Tutorial:
			Picking.Instance.SendPicker(Pickers.ObjectPlacement, Balises.WorldEditorMarker, new ObjectPlacementPicker.MarkerPlacementOptions(ToTagName(SpawnerObject.Tutorial)));
			break;
		}
	}

	public static SpawnerObject FromPicker(Picker p)
	{
		if (p.LastHeldKey == Balises.WorldEditorMarker)
		{
			if (!(p.LastOptions is ObjectPlacementPicker.MarkerPlacementOptions markerPlacementOptions))
			{
				return SpawnerObject.Unknown;
			}
			if (markerPlacementOptions.name == ToTagName(SpawnerObject.PlayerSpawn))
			{
				return SpawnerObject.PlayerSpawn;
			}
			if (markerPlacementOptions.name == ToTagName(SpawnerObject.Crystal))
			{
				return SpawnerObject.Crystal;
			}
			if (markerPlacementOptions.name == ToTagName(SpawnerObject.CorruptronSpawn))
			{
				return SpawnerObject.CorruptronSpawn;
			}
			if (markerPlacementOptions.name == ToTagName(SpawnerObject.Tutorial))
			{
				return SpawnerObject.Tutorial;
			}
		}
		return SpawnerObject.Unknown;
	}

	public static SpawnerObject FromGameObject(GameObject go)
	{
		AssetKeyHolder component = go.GetComponent<AssetKeyHolder>();
		if (component == null)
		{
			return SpawnerObject.Unknown;
		}
		if (component.AssetKey == Balises.WorldEditorMarker)
		{
			LuaMarker component2 = go.GetComponent<LuaMarker>();
			if (component2 == null)
			{
				return SpawnerObject.Unknown;
			}
			if (component2.markerGroups.Contains(ToTagName(SpawnerObject.PlayerSpawn)))
			{
				return SpawnerObject.PlayerSpawn;
			}
			if (component2.markerGroups.Contains(ToTagName(SpawnerObject.Crystal)))
			{
				return SpawnerObject.Crystal;
			}
			if (component2.markerGroups.Contains(ToTagName(SpawnerObject.CorruptronSpawn)))
			{
				return SpawnerObject.CorruptronSpawn;
			}
			if (component2.markerGroups.Contains(ToTagName(SpawnerObject.Tutorial)))
			{
				return SpawnerObject.Tutorial;
			}
		}
		return SpawnerObject.Unknown;
	}

	private static string ToTagName(SpawnerObject spawnerObject)
	{
		return spawnerObject switch
		{
			SpawnerObject.PlayerSpawn => "Player Starting Point", 
			SpawnerObject.Crystal => "Crystal", 
			SpawnerObject.CorruptronSpawn => "Corruptron Spawn Point", 
			SpawnerObject.Tutorial => "Tutorial Marker", 
			_ => throw new NotImplementedException(), 
		};
	}

	public static string ToDescriptionName(SpawnerObject spawnerObject)
	{
		return spawnerObject switch
		{
			SpawnerObject.PlayerSpawn => "Crystal Spawning Point Marker", 
			SpawnerObject.Crystal => "Shard Spawning Point Marker", 
			SpawnerObject.CorruptronSpawn => "Enemy Spawning Point Marker", 
			SpawnerObject.Tutorial => "Tutorial Marker", 
			_ => throw new NotImplementedException(), 
		};
	}

	public static string ToDisplayName(SpawnerObject spawnerObject)
	{
		return spawnerObject switch
		{
			SpawnerObject.PlayerSpawn => "Crystal Spawn", 
			SpawnerObject.Crystal => "Shard Spawn", 
			SpawnerObject.CorruptronSpawn => "Enemy Spawn", 
			SpawnerObject.Tutorial => "Tutorial Marker", 
			_ => throw new NotImplementedException(), 
		};
	}
}
