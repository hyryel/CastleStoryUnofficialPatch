using Brix.Util;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class TerrainHeightBrush : BrushComponent
{
	public new TerrainHeightBrushSettings brushSettings
	{
		get
		{
			if (_brushSettings == null)
			{
				CreateSettings();
			}
			return _brushSettings as TerrainHeightBrushSettings;
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
		_brushSettings = new TerrainHeightBrushSettings();
	}

	public override void OnHoverAny(bool hovering)
	{
		Picking.Instance.WorldEditorGrid.BrushPosition = _voxelRayHit.WorldHitVector;
	}

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			Vector3 hitVoxel = Calcule.RoundVecteur(_voxelRayHit.WorldHitVector + new Vector3(0f, 0.5f, 0f));
			CreateOrRemoveIsland(hitVoxel, brushSettings, removeMode: false);
		}
	}

	protected override void UpdateBrushHighlight()
	{
		if (base.Active)
		{
			if (!base.StandBy)
			{
				Picking.Instance.WorldEditorGrid.BrushColor = Picking.Instance.WorldEditorGrid.DefaultBrushColor;
			}
			else
			{
				Picking.Instance.WorldEditorGrid.BrushPosition = Vector3.zero;
			}
		}
	}
}
