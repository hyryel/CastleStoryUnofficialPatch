using System;
using Brix.Audio;
using Brix.Game.Components.CatapultSpace;
using UnityEngine;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectableSiegeBehaviour : SelectableBaseBehaviour
{
	[SerializeField]
	private CatapultBody body;

	public SelectableSiegeBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override void OnSelect()
	{
		SoundEngine.Play("Catapult_Select", selectableComponent.gameObject);
		body.Selected();
	}

	public override void OnUnselect()
	{
		body.Unselected();
	}
}
