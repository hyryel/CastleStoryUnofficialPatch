using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class RemoveIronBrush : BrushComponent
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
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		BrushUtils.BeginChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Minerals);
		bool flag = false;
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				XYZ xYZ = _voxelRayHit.VoxelPosition + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
				if (IsInsideBrushRange(crustInColumn.ToVector3()))
				{
					float ungrowth = brushSettings.ConvertedIntensity * GetFalloffFactorAt(xYZ.ToVector3(), _falloff);
					flag = IronRegistry.RemoveOrUngrow(crustInColumn, ungrowth);
					BrushUtils.AddOrFixGrass(crustInColumn);
				}
			}
		}
		BrushUtils.ApplyChanges(VoxelDirtyProperties.Terrain | VoxelDirtyProperties.Minerals);
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
		if (flag && !_lastFrameGrowth)
		{
			SoundEngine.Play("WorldEditor_Terrain_Iron_Decrease_Start");
		}
		else if (!flag && _lastFrameGrowth)
		{
			SoundEngine.Play("WorldEditor_Terrain_Iron_Decrease_Stop");
		}
		_lastFrameGrowth = flag;
	}

	public override void OnLastActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		SoundEngine.Play("WorldEditor_Terrain_Iron_Decrease_Stop");
		_lastFrameGrowth = false;
	}

	protected override void OnActiveChange()
	{
		SoundEngine.Play("WorldEditor_Terrain_Iron_Decrease_Stop");
		_lastFrameGrowth = false;
	}

	protected override void UpdateBrushHighlight()
	{
		base.UpdateBrushHighlight();
		if (base.Active && !base.StandBy)
		{
			Picking.Instance.BrushHighlight.BrushColor = Picking.Instance.BrushHighlight.DefaultRemoverColor;
		}
	}
}
