using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Utils;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class RaiseTerrainBrush : BrushComponent
{
	[SerializeField]
	private AnimationCurve _falloff = new AnimationCurve();

	private bool _isRaising;

	public new ElevationBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as ElevationBrushSettings;
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
		_brushSettings = new ElevationBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		Dictionary<XZ, int> dictionary = CollectionPool.RequestDictionary<XZ, int>();
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = _voxelRayHit.VoxelPosition + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
				if (IsInsideBrushRange(crustInColumn.ToVector3()))
				{
					float amount = GetFalloffFactorAt(xYZ.ToVector3(), _falloff) * brushSettings.Intensity;
					NatureRegistry.RemoveNatureAt(crustInColumn);
					BrushUtils.RemoveTree(crustInColumn);
					VoxelEngine.SetColumnType(xYZ.x, xYZ.z, VoxelTerrainType.Sand);
					int num2 = VoxelEngine.AddColumnHeight(xYZ.x, xYZ.z, amount);
					dictionary.Add(new XZ(xYZ.x, xYZ.z), num2 - 1);
				}
			}
		}
		HashSet<XZ> hashSet = CollectionPool.RequestHashSet<XZ>();
		BrushUtils.PopulateAdjacentColumns(dictionary, hashSet);
		foreach (XZ item in hashSet)
		{
			XYZ at = BrushUtils.ManualGetTerrainTopInColumn(item, dictionary);
			BrushUtils.AddOrFixGrass(at, dictionary);
		}
		hashSet.Dispose();
		dictionary.Dispose();
		if (!_isRaising)
		{
			SoundEngine.Play("WorldEditor_Terrain_Elevation_Start");
			_isRaising = true;
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Trees);
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (_isRaising)
		{
			SoundEngine.Play("WorldEditor_Terrain_Elevation_Stop");
			_isRaising = false;
		}
	}

	protected override void OnActiveChange()
	{
		if (_isRaising)
		{
			SoundEngine.Play("WorldEditor_Terrain_Elevation_Stop");
			_isRaising = false;
		}
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
