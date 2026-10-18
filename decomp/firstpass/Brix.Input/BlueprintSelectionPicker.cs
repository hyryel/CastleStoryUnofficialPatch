using Brix.Engine;
using Brix.UI.GameSelector;

namespace Brix.Input;

public class BlueprintSelectionPicker : PickerComponent
{
	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			if (_voxelRayHit.HitGameObject == null)
			{
				UIGameSelector.UnselectAllInTrack(SelectionTrackID.Blueprints);
				GameSignals.Invoke(PickingObserver.lua_BlueprintSelectionActionTaken);
			}
			SelectionBox.BeginSelection(clickInfo.mousePosition, SelectionTrackID.Blueprints);
		}
	}

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode() && clickInfo.pickingActiveOnLastMouseDown)
		{
			SelectionBox.UpdateAction(clickInfo.mousePosition);
		}
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && SelectionBox.active && !PickingUtility.isInViewMode())
		{
			SelectionBox.EndSelection();
			GameSignals.Invoke(PickingObserver.lua_BlueprintSelectionActionTaken);
		}
	}

	protected override void OnActiveChange()
	{
		if (SelectionBox.active)
		{
			SelectionBox.CancelSelection();
		}
	}

	protected override void OnStandByChange()
	{
		if (SelectionBox.active)
		{
			SelectionBox.CancelSelection();
		}
	}
}
