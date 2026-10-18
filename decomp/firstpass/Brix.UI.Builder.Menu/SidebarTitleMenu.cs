using System;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public class SidebarTitleMenu : BaseMenu
{
	public string _label;

	public bool _hasCloseButton;

	private SidebarTitleMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static SidebarTitleMenu Build(Func<RectTransform> binGetter, string label, bool hasCloseButton = true, SidebarTitleMenu srcMenu = null)
	{
		SidebarTitleMenu sidebarTitleMenu = srcMenu ?? new SidebarTitleMenu(binGetter);
		sidebarTitleMenu._label = label;
		sidebarTitleMenu._hasCloseButton = hasCloseButton;
		BuilderUtility.AssignTree(sidebarTitleMenu);
		return sidebarTitleMenu;
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(GetBuilders());
	}

	public PanelList GetBuilders()
	{
		PanelList panelList = PanelList.Request();
		Action<SidebarTitlePanelComponent> onReset = delegate(SidebarTitlePanelComponent panel)
		{
			panel._label.text = _label;
			panel._label.color = Colors.Black;
		};
		SidebarTitlePanel item = SidebarTitlePanel.New(onReset);
		panelList.Add(item);
		return panelList;
	}
}
