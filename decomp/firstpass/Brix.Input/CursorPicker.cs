using Brix.External.Signals;
using UnityEngine;

namespace Brix.Input;

internal class CursorPicker : PickerComponent
{
	public static Signal RectEnabledChanged = new Signal();

	public static Signal TerrainHoverdChanged = new Signal();

	private static Vector3 _terrainHovered;

	public static bool IsHoveringMinimap;

	public static Vector3 TerrainHovered
	{
		get
		{
			return _terrainHovered;
		}
		set
		{
			if (_terrainHovered != value)
			{
				_terrainHovered = value;
				TerrainHoverdChanged.Invoke();
			}
		}
	}

	public static bool IsHoveringTerrain => _terrainHovered != Vector3.zero || IsHoveringMinimap;

	public override void OnEnterTerrain(Vector3? hoveredVoxel)
	{
		TerrainHovered = ((!hoveredVoxel.HasValue) ? Vector3.zero : hoveredVoxel.Value);
	}

	public override void OnLeaveTerrain(Vector3? hoveredVoxel)
	{
		TerrainHovered = Vector3.zero;
	}
}
