using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.UI.GameSelector;

[Serializable]
public class SelectableUnitsBehaviour : SelectableBaseBehaviour
{
	[SerializeField]
	private Labor labor;

	public SelectableUnitsBehaviour(UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
		: base(selectableComponent, selectable, hoverable, type)
	{
	}

	public override bool IsSelectable()
	{
		return base.IsSelectable() && LaborIsValid();
	}

	public override bool IsHoverable()
	{
		return base.IsHoverable() && LaborIsValid();
	}

	public override void OnSelect()
	{
		SoundEngine.Play(labor.Profession.Sounds.Select, selectableComponent.gameObject);
	}

	public override void OnMultiSelect(List<UISelectable> selectables)
	{
		if (selectables.Count > 1)
		{
			PlayBricktronSounds(selectables);
		}
	}

	private bool LaborIsValid()
	{
		return !labor.IsNullOrReleased() && !labor.IsPhasedOut;
	}

	private static void PlayBricktronSounds(List<UISelectable> selectables)
	{
		SoundEngine.SetParam("Bricktron_Multi_Select", selectables.Count);
		SoundEngine.Play("Bricktron_Multi_Select", selectables[0].gameObject);
	}
}
