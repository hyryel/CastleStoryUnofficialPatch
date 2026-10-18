using Brix.Audio;
using Brix.Engine;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class RemoveGrassBrush : BrushComponent
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
		bool flag = false;
		BrushUtils.BeginChanges();
		int num = Mathf.CeilToInt(brushSettings.ConvertedSize / 2);
		for (int i = -num; i != num + 1; i++)
		{
			for (int j = -num; j != num + 1; j++)
			{
				XYZ xYZ = _voxelRayHit.VoxelPosition + new XYZ(i, 0, j);
				XYZ crustInColumn = BrushUtils.GetCrustInColumn(xYZ.x, xYZ.z);
				if (IsInsideBrushRange(crustInColumn.ToVector3()))
				{
					if (VoxelEngine.GetColumnType(crustInColumn.x, crustInColumn.z) != VoxelTerrainType.Sand)
					{
						flag = true;
					}
					BrushUtils.RemoveGrass(crustInColumn);
				}
			}
		}
		if (flag)
		{
			SoundEngine.Play("WorldEditor_Terrain_Grass_Remove");
		}
		BrushUtils.ApplyChanges();
		GameSignals.Invoke(GameSignals.OnTerrainEdited);
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
