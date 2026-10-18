using System;
using Brix.Game.Components;
using Brix.Input;
using UnityEngine;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectableBlueprintBehaviour : SelectableBaseBehaviour
{
	[SerializeField]
	private Blueprint blueprint;

	public SelectableBlueprintBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override bool IsSelectable()
	{
		return Selectable && BlueprintPlaceInSelectedProject();
	}

	public override bool IsHoverable()
	{
		return Hoverable;
	}

	private bool BlueprintPlaceInSelectedProject()
	{
		if (UIGameObserver.projects.CurrentSelected != blueprint.Project && InputModeController.IsInEditMode())
		{
			return false;
		}
		if (!blueprint.State.HasFlag(PlacementFlags.placed))
		{
			return false;
		}
		return true;
	}
}
