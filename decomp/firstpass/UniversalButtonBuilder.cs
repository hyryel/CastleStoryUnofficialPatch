using System;
using Brix.NewUI.Tooltips;
using UnityEngine;

public class UniversalButtonBuilder
{
	public TooltipCallDelegate tooltipCallback;

	public Action destroyTooltipCallback;

	public bool showBackgroundByDefault;

	public Transform parent;

	public UniversalButtonBuilder(Transform parent)
	{
		this.parent = parent;
	}

	private void SetBuilderValues(UniversalButtonItem item)
	{
		item.tooltipCallback = tooltipCallback;
		item.destroyTooltipCallback = destroyTooltipCallback;
		item.showBackgroundByDefault = showBackgroundByDefault;
		item.parent = parent;
	}

	public UniversalButtonItem NewButton(string name, Sprite icon, Action onClick, Color color, bool colorIcon, string tooltip)
	{
		UniversalButtonItem universalButtonItem = UniversalButtonItem.NewButton(name, icon, onClick, color, colorIcon, tooltip, string.Empty);
		SetBuilderValues(universalButtonItem);
		return universalButtonItem;
	}

	public UniversalButtonItem NewLabel(string name, Sprite icon, Color color, bool colorIcon, string tooltip)
	{
		UniversalButtonItem universalButtonItem = UniversalButtonItem.NewLabel(name, icon, color, colorIcon, tooltip);
		SetBuilderValues(universalButtonItem);
		return universalButtonItem;
	}

	public UniversalButtonItem NewCounter(string name, Sprite icon, Color color, string tooltip, string count)
	{
		UniversalButtonItem universalButtonItem = UniversalButtonItem.NewCounter(name, icon, color, colorIcon: false, tooltip, count);
		SetBuilderValues(universalButtonItem);
		return universalButtonItem;
	}
}
