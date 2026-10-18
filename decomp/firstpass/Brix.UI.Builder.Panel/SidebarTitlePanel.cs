using System;
using Brix.External.Factories;
using UnityEngine;

namespace Brix.UI.Builder.Panel;

public class SidebarTitlePanel : BaseLayoutPanel<SidebarTitlePanelComponent>
{
	private readonly Factory.AssetKey _assetKey = Panels.SidebarTitle;

	protected override Factory.AssetKey AssetKey => _assetKey;

	public SidebarTitlePanel(bool luaDriven = false)
		: base((Action<SidebarTitlePanelComponent>)null, (Action<SidebarTitlePanelComponent>)null, luaDriven)
	{
	}

	public SidebarTitlePanel(Action<SidebarTitlePanelComponent> onReset, Action<SidebarTitlePanelComponent> onRefresh)
		: base(onReset, onRefresh, luaDriven: false)
	{
	}

	public override RectTransform GetLayoutTrs()
	{
		return base.Instance._buttonBin;
	}

	public static SidebarTitlePanel LuaNew()
	{
		return new SidebarTitlePanel(luaDriven: true);
	}

	public static SidebarTitlePanel New(Action<SidebarTitlePanelComponent> onReset = null, Action<SidebarTitlePanelComponent> onRefresh = null)
	{
		return new SidebarTitlePanel(onReset, onRefresh);
	}
}
