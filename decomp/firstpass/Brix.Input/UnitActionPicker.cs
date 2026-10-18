using System.Collections.Generic;
using Brix.Game.AI;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

internal class UnitActionPicker : PickerComponent
{
	public override void OnEnterCharacter(Labor hoveredCharacter)
	{
		if (UIGameObserver.fogObjects.IsVisible(hoveredCharacter.vision) && !PickingUtility.Player.GetButton("cam_action1") && !hoveredCharacter.IsPhasedOut && !SelectionBox.active)
		{
			UIGameSelector.Hover(hoveredCharacter.gameObject);
		}
	}

	public override void OnLeaveCharacter(Labor hoveredCharacter)
	{
		if (!PickingUtility.Player.GetButton("cam_action1") && (bool)hoveredCharacter)
		{
			UIGameSelector.Unhover(hoveredCharacter.gameObject);
		}
	}

	public override void OnFirstActionCharacter(Labor clickedCharacter, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1" && !PickingUtility.isInViewMode())
		{
			if (PickingUtility.isInBatchSelectMode(clickInfo.clickType))
			{
				UIGameActor.SelectCoworkersOf(clickedCharacter.GetComponent<Labor>(), visibleOnly: true);
			}
			else if (PickingUtility.isInToggleSelectMode())
			{
				UIGameSelector.ToggleSelection(clickedCharacter.gameObject);
			}
			else
			{
				UIGameSelector.Select(clickedCharacter.gameObject);
			}
		}
	}

	public override void OnLastActionCharacter(Labor clickedCharacter, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action2" && !PickingUtility.isInViewMode() && (clickInfo.clickType == ClickType.shortClick || clickInfo.clickType == ClickType.doubleClick))
		{
			CollectionPool.Request<GameObject>(out List<GameObject> list);
			UIGameSelector.GetSelectedOfType(ref list, SelectionType.Unit);
			list.Slinq().Where((GameObject u, GameObject p) => u == p, clickedCharacter.gameObject).Remove();
			DirectOrder(list, clickedCharacter.gameObject);
			list.Dispose();
		}
	}
}
