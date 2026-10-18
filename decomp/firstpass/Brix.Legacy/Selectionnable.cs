namespace Brix.Legacy;

public interface Selectionnable : PieMenuable
{
	object GetSelection();

	bool IsSelected();

	bool IsHovered();
}
