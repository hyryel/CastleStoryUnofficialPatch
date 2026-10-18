using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class BoulderBrush : BrushComponent
{
	private bool _lastFrameGrowth;

	private static List<XYZ> _boulderSpace = new List<XYZ>();

	public new BoulderBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as BoulderBrushSettings;
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
		_brushSettings = new BoulderBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode() && Voxel.IsCrust(_voxelRayHit.VoxelPosition))
		{
			bool flag = BoulderRegistry.HasBoulders(_voxelRayHit.VoxelPosition);
			bool flag2 = GrowBoulders(_voxelRayHit.WorldHitVector, brushSettings.ConvertedSize, brushSettings.ConvertedIntensity, _unlimitedBrushHeight);
			if (flag2 && !flag)
			{
				SoundEngine.Play("WorldEditor_Terrain_Boulder_Spawn");
			}
			if (flag2 && !_lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Boulder_Growth_Start");
			}
			else if (!flag2 && _lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Boulder_Growth_Stop");
			}
			_lastFrameGrowth = flag2;
		}
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Boulder_Growth_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Boulder_Growth_Stop");
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

	private static bool GrowBoulder(List<XYZ> at, float growth, Vector3 offset, Vector3 centerPos)
	{
		foreach (XYZ item in at)
		{
			if (TreeRegistry.Instance.HasTree(item))
			{
				return false;
			}
		}
		if (ValidateBoulder(at, XYZ.FromVector3(centerPos + offset)))
		{
			return BoulderRegistry.AddOrGrow(centerPos, growth, offset, (BoulderSize)at.Count);
		}
		return false;
	}

	private static bool ValidateBoulder(List<XYZ> positions, XYZ rootPosition)
	{
		if (!positions.Contains(rootPosition))
		{
			return false;
		}
		foreach (XYZ position in positions)
		{
			if (position.y - rootPosition.y < -1)
			{
				return false;
			}
		}
		return true;
	}

	public static bool GrowBoulders(Vector3 center, int areaSize, float intensity, bool unlimitedInY)
	{
		BrushUtils.BeginChanges(VoxelDirtyProperties.Minerals);
		bool flag = false;
		_boulderSpace.Clear();
		int num = Mathf.CeilToInt((float)areaSize / 2f);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = XYZ.FromVector3(center) + new XYZ(i, 0, j);
				if (BrushUtils.IsInsideRange(xYZ.ToVector3(), center, areaSize, unlimitedInY))
				{
					XYZ item = (Voxel.IsEmpty(xYZ) ? BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z) : ((!Voxel.IsCrust(xYZ + XYZ.up)) ? xYZ : (xYZ + XYZ.up)));
					_boulderSpace.Add(item);
				}
			}
		}
		Vector3 snappedCenterPosition = BrushUtils.GetSnappedCenterPosition(center, areaSize);
		Vector3 offset = center - snappedCenterPosition;
		if (!BoulderRegistry.HasBoulders(_boulderSpace))
		{
			foreach (XYZ item2 in _boulderSpace)
			{
				PlantRegistry.Remove(item2);
			}
		}
		flag = GrowBoulder(_boulderSpace, intensity, offset, snappedCenterPosition);
		foreach (XYZ item3 in _boulderSpace)
		{
			BrushUtils.AddOrFixGrass(item3);
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Minerals);
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		return flag;
	}
}
