using System.Collections.Generic;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.Utils;
using UnityEngine;

namespace Brix.Input;

internal class ObjectActionPicker : PickerComponent
{
	public override void OnEnterGameObject(GameObject hoveredGameObject)
	{
		if (!PickingUtility.Player.GetButton("cam_action1") && !SelectionBox.active)
		{
			GameObject obj = PickingUtility.FindSignificantParent(hoveredGameObject);
			UIGameSelector.Hover(obj);
		}
	}

	public override void OnLeaveGameObject(GameObject hoveredGameObject)
	{
		if (!PickingUtility.Player.GetButton("cam_action1") && !hoveredGameObject.IsNullOrReleased())
		{
			GameObject obj = PickingUtility.FindSignificantParent(hoveredGameObject);
			UIGameSelector.Unhover(obj);
		}
	}

	public override void OnFirstActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		if (!(clickInfo.actionName == "cam_action1") || PickingUtility.isInViewMode())
		{
			return;
		}
		GameObject gameObject = PickingUtility.FindSignificantParent(clickedGameObject);
		UISelectable uISelectable = UIGameSelector.FindSelectable(gameObject);
		Labor labor = null;
		if ((bool)uISelectable)
		{
			labor = uISelectable.GetComponent<Labor>();
		}
		if (PickingUtility.isInBatchSelectMode(clickInfo.clickType))
		{
			if ((bool)labor)
			{
				UIGameActor.SelectCoworkersOf(labor, visibleOnly: true);
			}
		}
		else if (PickingUtility.isInToggleSelectMode())
		{
			UIGameSelector.ToggleSelection(gameObject);
		}
		else
		{
			UIGameSelector.Select(gameObject);
		}
	}

	public override void OnLastActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action2" && !PickingUtility.isInViewMode())
		{
			GameObject gameObject = PickingUtility.FindSignificantParent(clickedGameObject);
			if (!(gameObject == null))
			{
				CollectionPool.Request<GameObject>(out List<GameObject> list);
				UIGameSelector.GetSelectedOfType(ref list, SelectionType.Unit);
				DirectOrder(list, gameObject);
				list.Dispose();
			}
		}
	}
}
