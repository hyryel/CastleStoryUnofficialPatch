using Rewired;
using UnityEngine;

namespace Brix.Input;

public class ClickInfo
{
	public string actionName;

	public Vector3 mousePositionDown;

	public Vector3 mousePosition;

	public ClickState clickState;

	public float clickDownTime;

	public float clickUpTime;

	public bool pickingActive;

	public bool pickingActiveOnLastMouseDown;

	public ClickType clickType
	{
		get
		{
			if (PickingUtility.Player.GetButtonDoublePressDown(actionName))
			{
				return ClickType.doubleClick;
			}
			if ((double)PickingUtility.Player.GetButtonTimePressed(actionName) > 0.4)
			{
				return ClickType.longClick;
			}
			return ClickType.shortClick;
		}
	}

	public ClickInfo(string action)
	{
		actionName = action;
		mousePositionDown = default(Vector3);
		mousePosition = default(Vector3);
		clickState = ClickState.noClick;
		clickDownTime = 0f;
		clickUpTime = 0f;
		pickingActive = false;
		pickingActiveOnLastMouseDown = false;
	}

	public void SetClickStatus()
	{
		if (PickingUtility.Player.GetButtonDown(actionName))
		{
			clickState = ClickState.down;
			clickDownTime = ReInput.time.unscaledTime;
			mousePositionDown = PickingUtility.Player.controllers.Mouse.screenPosition;
			mousePosition = PickingUtility.Player.controllers.Mouse.screenPosition;
			pickingActiveOnLastMouseDown = Picking.actif && !Picking.Instance.UILocked;
		}
		else if (PickingUtility.Player.GetButtonUp(actionName))
		{
			clickState = ClickState.up;
			clickUpTime = ReInput.time.unscaledTime;
			mousePosition = PickingUtility.Player.controllers.Mouse.screenPosition;
		}
		else if (PickingUtility.Player.GetButton(actionName))
		{
			clickState = ClickState.holdDown;
			mousePosition = PickingUtility.Player.controllers.Mouse.screenPosition;
		}
		else
		{
			clickState = ClickState.noClick;
			mousePosition = PickingUtility.Player.controllers.Mouse.screenPosition;
		}
		pickingActive = Picking.actif && !Picking.Instance.UILocked;
	}

	public void ResetStatus()
	{
		mousePositionDown = default(Vector3);
		mousePosition = default(Vector3);
		clickState = ClickState.noClick;
		clickDownTime = 0f;
		clickUpTime = 0f;
		pickingActive = false;
		pickingActiveOnLastMouseDown = false;
	}
}
