using Brix.Game.Components;
using Brix.UI.GameSelector;

namespace Brix.Input;

public class CommandSelectionPicker : PickerComponent
{
	public override void OnFirstActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			if (!PickingUtility.isInToggleSelectMode())
			{
				UIGameSelector.UnselectEverything();
			}
			SelectionBox.BeginSelection(clickInfo.mousePosition, SelectionTrackID.Command, SelectionType.Unit);
		}
	}

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (PickingUtility.isInViewMode())
		{
			SelectionBox.CancelSelection();
		}
		else if (clickInfo.actionName == "cam_action1" && clickInfo.pickingActiveOnLastMouseDown)
		{
			SelectionBox.UpdateAction(clickInfo.mousePosition);
		}
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (PickingUtility.isInViewMode())
		{
			SelectionBox.CancelSelection();
		}
		else if (clickInfo.actionName == "cam_action1" && SelectionBox.active)
		{
			SelectionBox.EndSelection();
		}
	}

	protected override void OnActiveChange()
	{
		if (SelectionBox.active)
		{
			SelectionBox.CancelSelection();
		}
		UIGameSelector.UnhoverEverything();
	}

	protected override void OnStandByChange()
	{
		if (SelectionBox.active)
		{
			SelectionBox.CancelSelection();
		}
	}

	public override void OnEnterBlueprint(Blueprint hoveredBlueprint)
	{
		if (!hoveredBlueprint.IsNullOrReleased() && hoveredBlueprint.recepteur != null && !hoveredBlueprint.recepteur.IsEmpty())
		{
			UIGameSelector.Hover(hoveredBlueprint.gameObject);
		}
	}

	public override void OnLeaveBlueprint(Blueprint hoveredBlueprint)
	{
		if (!hoveredBlueprint.IsNullOrReleased())
		{
			UIGameSelector.Unhover(hoveredBlueprint.gameObject);
		}
	}
}
