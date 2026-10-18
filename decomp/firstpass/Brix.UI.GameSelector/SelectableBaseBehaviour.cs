namespace Brix.UI.GameSelector;

public class SelectableBaseBehaviour : SelectableEmptyBehaviour
{
	public SelectableBaseBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override bool IsSelectable()
	{
		return Selectable && base.Visible;
	}

	public override bool IsHoverable()
	{
		return Hoverable && base.Visible;
	}
}
