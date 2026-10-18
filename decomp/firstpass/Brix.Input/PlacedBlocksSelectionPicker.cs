using Brix.UI.GameSelector;

namespace Brix.Input;

public class PlacedBlocksSelectionPicker : PickerComponent
{
	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			if (_voxelRayHit.HitGameObject == null && !PickingUtility.isInToggleSelectMode())
			{
				UIGameSelector.UnselectAll(SelectionType.Blocks);
			}
			SelectionBox.BeginSelection(clickInfo.mousePosition, SelectionTrackID.Blocks);
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
		}
	}
}
