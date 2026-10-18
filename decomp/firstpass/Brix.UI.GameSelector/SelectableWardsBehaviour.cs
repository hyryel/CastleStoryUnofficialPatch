using System;
using System.Collections.Generic;
using Brix.Game.Components.Wards;
using UnityEngine;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectableWardsBehaviour : SelectableBaseBehaviour
{
	[SerializeField]
	private WardLink ward;

	public SelectableWardsBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override bool IsSelectable()
	{
		return base.IsSelectable();
	}

	public override bool IsHoverable()
	{
		return base.IsHoverable();
	}

	public override void OnSelect()
	{
	}

	public override void OnMultiSelect(List<UISelectable> selectables)
	{
	}

	public override void OnHover()
	{
		base.OnHover();
		WardLink.ShowLinks();
	}

	public override void OnUnhover()
	{
		base.OnUnhover();
		WardLink.HideLinks();
	}
}
