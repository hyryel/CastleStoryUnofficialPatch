using System;
using System.Collections.Generic;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class ModeActionsMenu : BaseMenu
{
	private ModeActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static ModeActionsMenu Build(Func<RectTransform> binGetter)
	{
		ModeActionsMenu modeActionsMenu = new ModeActionsMenu(binGetter);
		BuilderUtility.AssignTree(modeActionsMenu);
		return modeActionsMenu;
	}

	public static List<BasePanel> GetBuilders()
	{
		List<BasePanel> list = new List<BasePanel>();
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panelN)
		{
			panelN._label.text = "Refresh";
			panelN._button.onClick.RemoveAllListeners();
			panelN._button.onClick.AddListener(delegate
			{
				WeModesValidationMenu.ReOpenSignal.Invoke();
			});
		};
		LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(onReset);
		BaseLayoutPanel<VerticalLayoutPanelComponent> item = VerticalLayoutPanel.New().AddPanels_2(basePanel);
		list.Add(item);
		return list;
	}
}
