using System;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectableCraftingStationBehaviour : SelectableBaseBehaviour
{
	[SerializeField]
	private CraftingStation station;

	public SelectableCraftingStationBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override void OnSelect()
	{
		UIGameSelector.UnselectEverything(station.gameObject);
	}
}
