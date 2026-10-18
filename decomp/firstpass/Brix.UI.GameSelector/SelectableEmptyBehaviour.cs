using System.Collections.Generic;

namespace Brix.UI.GameSelector;

public class SelectableEmptyBehaviour : SelectableBehaviour
{
	public SelectableEmptyBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override void OnSelect()
	{
	}

	public override void OnUnselect()
	{
	}

	public override void OnHover()
	{
	}

	public override void OnUnhover()
	{
	}

	public override bool IsSelectable()
	{
		return Selectable;
	}

	public override bool IsHoverable()
	{
		return Hoverable;
	}

	public override void OnMultiSelect(List<UISelectable> selectables)
	{
	}
}
