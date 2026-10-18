using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using Brix.Utils;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class PlantBrush : BrushComponent
{
	[SerializeField]
	private AnimationCurve _falloff = new AnimationCurve();

	private bool _lastFrameGrowth;

	public new PlantBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as PlantBrushSettings;
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
		_brushSettings = new PlantBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			bool flag = GrowPlants(_voxelRayHit.WorldHitVector, brushSettings.PlantType, brushSettings.ConvertedSize, brushSettings.ConvertedIntensity, _unlimitedBrushHeight, brushSettings.Validate, _falloff);
			if (flag && !_lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Plant_Growth_Start");
			}
			else if (!flag && _lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Plant_Growth_Stop");
			}
			_lastFrameGrowth = flag;
		}
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Plant_Growth_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Plant_Growth_Stop");
		_lastFrameGrowth = false;
	}

	protected override void UpdateBrushHighlight()
	{
		base.UpdateBrushHighlight();
		if (base.Active && !base.StandBy)
		{
			Picking.Instance.BrushHighlight.BrushColor = Picking.Instance.BrushHighlight.DefaultBrushColor;
		}
	}

	public static bool GrowPlants(Vector3 center, int plantType, int areaSize, float intensity, bool unlimitedInY, bool useGrassValidation, AnimationCurve falloff = null)
	{
		BrushUtils.BeginChanges(VoxelDirtyProperties.Plants);
		bool result = false;
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		int num = Mathf.CeilToInt((float)areaSize / 2f);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ item = XYZ.FromVector3(center) + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(item.x, item.z);
				if (BrushUtils.IsInsideRange(crustInColumn.ToVector3(), center, areaSize, unlimitedInY) && !TreeRegistry.Instance.HasTree(crustInColumn) && (!useGrassValidation || BrushUtils.ValidateGrass(crustInColumn)))
				{
					int columnType = (int)VoxelEngine.GetColumnType(item.x, item.z);
					switch (BlockInfo.GetFromID(columnType).AcceptNature)
					{
					case BlockInfo.AcceptNatureType.RemovesOverlay:
						list.Add(item);
						break;
					case BlockInfo.AcceptNatureType.Accepts:
					{
						float growth = intensity * BrushUtils.GetFalloffFactorAt(item.ToVector3(), falloff, center, areaSize);
						result = PlantRegistry.AddOrGrow(crustInColumn, plantType, growth);
						break;
					}
					}
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Plants);
		if (list.Count > 0)
		{
			BrushUtils.BeginChanges();
			foreach (XYZ item2 in list)
			{
				VoxelEngine.SetColumnType(item2.x, item2.z, VoxelTerrainType.Sand);
			}
			BrushUtils.ApplyChanges();
		}
		list.Dispose();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		return result;
	}
}
