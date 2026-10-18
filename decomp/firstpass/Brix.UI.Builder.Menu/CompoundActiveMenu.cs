using System;
using System.Collections.Generic;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public class CompoundActiveMenu : BaseMenu
{
	private CompoundActiveMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static CompoundActiveMenu Build(Func<RectTransform> binGetter)
	{
		CompoundActiveMenu compoundActiveMenu = new CompoundActiveMenu(binGetter);
		BuilderUtility.AssignTree(compoundActiveMenu);
		return compoundActiveMenu;
	}

	public static List<BasePanel> GetCompoundBuilders()
	{
		List<BasePanel> list = new List<BasePanel>();
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panelN)
		{
			panelN._label.text = "Save selected";
		};
		list.Add(LegacyButtonPanelComponent.Build(onReset));
		return list;
	}
}
