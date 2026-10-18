namespace Brix.UI.GameSelector;

public interface ISelectable
{
	void OnSelect();

	void OnUnselect();

	void OnHover();

	void OnUnhover();

	bool IsSelectable();

	bool IsHoverable();
}
