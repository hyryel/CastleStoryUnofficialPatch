using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class GrassBrush : BrushComponent
{
	public new GrassBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as GrassBrushSettings;
		}
		set
		{
			if (value != null)
			{
				_brushSettings = value;
			}
			else
			{
				CreateSettings();
			}
		}
	}

	protected override void CreateSettings()
	{
		_brushSettings = new GrassBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		BrushUtils.BeginChanges();
		Dictionary<XZ, int> dictionary = CollectionPool.RequestDictionary<XZ, int>();
		Dictionary<XZ, VoxelTerrainType> dictionary2 = CollectionPool.RequestDictionary<XZ, VoxelTerrainType>();
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = _voxelRayHit.VoxelPosition + new XYZ(i, 0, j);
				XYZ terrainTopInColumn = BrushUtils.GetTerrainTopInColumn(xYZ.x, xYZ.z);
				if (IsInsideBrushRange(terrainTopInColumn.ToVector3()))
				{
					VoxelTerrainType columnType = VoxelEngine.GetColumnType(terrainTopInColumn.x, terrainTopInColumn.z);
					if (columnType != (VoxelTerrainType)brushSettings.TerrainType)
					{
						VoxelEngine.SetColumnType(terrainTopInColumn.x, terrainTopInColumn.z, (VoxelTerrainType)brushSettings.TerrainType);
						dictionary.Add(XZ.FromXYZ(terrainTopInColumn), terrainTopInColumn.y);
						dictionary2.Add(XZ.FromXYZ(terrainTopInColumn), columnType);
					}
				}
			}
		}
		if (BlockInfo.GetFromID(brushSettings.TerrainType).IsTerrainOverlay)
		{
			foreach (KeyValuePair<XZ, int> item in dictionary)
			{
				XYZ at = new XYZ(item.Key.x, item.Value, item.Key.z);
				BrushUtils.FixGrass(at, dictionary, dictionary2[item.Key]);
			}
		}
		if (BlockInfo.GetFromID(brushSettings.TerrainType).AcceptNature != BlockInfo.AcceptNatureType.Accepts)
		{
			foreach (KeyValuePair<XZ, int> item2 in dictionary)
			{
				XYZ p = new XYZ(item2.Key.x, item2.Value, item2.Key.z);
				PlantRegistry.Remove(p);
			}
		}
		if (dictionary.Slinq().Any((KeyValuePair<XZ, int> kvp, Dictionary<XZ, VoxelTerrainType> originals) => VoxelEngine.GetColumnType(kvp.Key.x, kvp.Key.z) != originals[kvp.Key], dictionary2))
		{
			SoundEngine.Play("WorldEditor_Terrain_Grass_Add");
		}
		dictionary.Dispose();
		dictionary2.Dispose();
		BrushUtils.ApplyChanges();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
	}

	protected override void UpdateBrushHighlight()
	{
		base.UpdateBrushHighlight();
		if (base.Active && !base.StandBy)
		{
			Picking.Instance.BrushHighlight.BrushColor = Picking.Instance.BrushHighlight.DefaultBrushColor;
		}
	}
}
