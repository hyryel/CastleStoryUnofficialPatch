using System.Collections.Generic;
using Brix.Game;
using Brix.Game.Components;

namespace Brix.UI.GameSelector;

public abstract class SelectableBehaviour : ISelectable
{
	protected bool Selectable;

	protected bool Hoverable;

	protected SelectionType Type;

	protected UISelectable selectableComponent;

	protected bool Visible => LocalFactionVision.ObjectIsVisibleForFaction(selectableComponent.gameObject, User.LocalUser.faction);

	public SelectableBehaviour(UISelectable selectableComponent, bool selectableGetter, bool hoverableGetter, SelectionType selectionTypeGetter)
	{
		Setup(selectableComponent, selectableGetter, hoverableGetter, selectionTypeGetter);
	}

	public void Setup(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
	{
		this.selectableComponent = selectableComponent;
		Type = type;
		Selectable = selectable;
		Hoverable = hoverable;
	}

	public abstract void OnSelect();

	public abstract void OnUnselect();

	public abstract void OnHover();

	public abstract void OnUnhover();

	public abstract bool IsSelectable();

	public abstract bool IsHoverable();

	public abstract void OnMultiSelect(List<UISelectable> selectables);
}
