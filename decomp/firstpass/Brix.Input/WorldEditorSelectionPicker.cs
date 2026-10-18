using Brix.UI.GameSelector;

namespace Brix.Input;

public class WorldEditorSelectionPicker : PickerComponent
{
	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode() && _voxelRayHit.HitGameObject == null && !PickingUtility.isInToggleSelectMode())
		{
			UIGameSelector.UnselectAllInTrack(SelectionTrackID.WorldEditor);
		}
	}
}
