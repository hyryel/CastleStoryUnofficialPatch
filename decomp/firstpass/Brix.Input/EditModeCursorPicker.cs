using Brix.Engine;
using UnityEngine;

namespace Brix.Input;

internal class EditModeCursorPicker : PickerComponent
{
	private bool _rectEnabled;

	private bool _feedBackShowing;

	private bool _draggingLegitDraggable;

	private bool IsPaused => InputModeController.GetCurrentMode() == InputMode.pause;

	public override void OnHoverAny(bool hovering)
	{
		if (!_draggingLegitDraggable)
		{
			ToggleDragFeedback(_rectEnabled);
			_feedBackShowing = _rectEnabled;
		}
	}

	public override void OnFirstActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		if (!IsPaused && clickInfo.actionName == "cam_action1" && clickInfo.clickState == ClickState.down && clickedGameObject.GetComponent<DragHandle>() != null)
		{
			_draggingLegitDraggable = true;
		}
	}

	public override void OnActionAny(bool actioning, ClickInfo clickInfo)
	{
		if (!IsPaused && clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode() && clickInfo.pickingActiveOnLastMouseDown && !_rectEnabled && (Mathf.Abs(clickInfo.mousePositionDown.x - clickInfo.mousePosition.x) > 5f || Mathf.Abs(clickInfo.mousePositionDown.y - clickInfo.mousePosition.y) > 5f))
		{
			_rectEnabled = true;
		}
	}

	public override void OnLastActionAny(bool actioning, ClickInfo clickInfo)
	{
		_rectEnabled = false;
		_draggingLegitDraggable = false;
	}

	protected override void OnStandByChange()
	{
		Reset();
	}

	protected override void OnActiveChange()
	{
		Reset();
	}

	private void Reset()
	{
		_rectEnabled = false;
		_draggingLegitDraggable = false;
		ToggleDragFeedback(show: false);
	}

	public void ToggleDragFeedback(bool show)
	{
		if (!IsPaused)
		{
			if (show && !_feedBackShowing)
			{
				GameSignals.Invoke(GameSignals.OnAutoFadeDialog, "Cannot use box selection in Edit mode", -1f);
			}
			else if (!show && _feedBackShowing)
			{
				GameSignals.Invoke(GameSignals.OnAutoFadeDialog, "Cannot use box selection in Edit mode", 1f);
			}
		}
	}
}
