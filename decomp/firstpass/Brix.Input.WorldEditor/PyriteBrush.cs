using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class PyriteBrush : BrushComponent
{
	[SerializeField]
	private AnimationCurve _falloff = new AnimationCurve();

	private bool _lastFrameGrowth;

	public new MineralBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as MineralBrushSettings;
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
		_brushSettings = new MineralBrushSettings();
	}

	public override void OnActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			bool flag = GrowPyrite(_voxelRayHit.WorldHitVector, brushSettings.ConvertedSize, brushSettings.ConvertedIntensity, _unlimitedBrushHeight, _falloff);
			if (flag && !_lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Brimstone_Growth_Start");
			}
			else if (!flag && _lastFrameGrowth)
			{
				SoundEngine.Play("WorldEditor_Terrain_Brimstone_Growth_Stop");
			}
			_lastFrameGrowth = flag;
		}
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Brimstone_Growth_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Brimstone_Growth_Stop");
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

	public static bool GrowPyrite(Vector3 center, int areaSize, float intensity, bool unlimitedInY, AnimationCurve falloff = null)
	{
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Minerals);
		bool result = false;
		int num = Mathf.CeilToInt((float)areaSize / 2f);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = XYZ.FromVector3(center) + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
				if (BrushUtils.IsInsideRange(crustInColumn.ToVector3(), center, areaSize, unlimitedInY) && !TreeRegistry.Instance.HasTree(crustInColumn))
				{
					float growth = intensity * BrushUtils.GetFalloffFactorAt(xYZ.ToVector3(), falloff, center, areaSize);
					PlantRegistry.Remove(crustInColumn);
					result = PyriteRegistry.AddOrGrow(crustInColumn, growth);
					BrushUtils.AddOrFixGrass(crustInColumn);
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Minerals);
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		return result;
	}
}
