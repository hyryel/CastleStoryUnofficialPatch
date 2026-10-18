using System;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkServersActionsMenu : BaseMenu
{
	public static Signal RefreshSignal = new Signal();

	public static bool CanJoin => NetworkServersMenu.CanJoin;

	private NetworkServersActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkServersActionsMenu Build(Func<RectTransform> getParent)
	{
		return new NetworkServersActionsMenu(getParent);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(GetBuilders());
	}

	public static PanelList GetBuilders()
	{
		PanelList panelList = PanelList.Request();
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Custom Address...";
			panel._label.fontSize = 14;
			panel._layoutElement.flexibleWidth = 1f;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				OnCustomServer();
			});
		};
		LegacyButtonPanelComponent.BasePanel item = LegacyButtonPanelComponent.Build(onReset);
		panelList.Add(item);
		Action<SidebarDividerPanelComponent> onReset2 = delegate(SidebarDividerPanelComponent panel)
		{
			panel._layoutElement.flexibleWidth = 1f;
			panel._dividerImage.color = new Color(0.5019608f, 0.5019608f, 0.5019608f, 0.101960786f);
		};
		SidebarDividerPanelComponent.BasePanel item2 = SidebarDividerPanelComponent.Build(onReset2);
		panelList.Add(item2);
		Action<SidebarDividerPanelComponent> onReset3 = delegate(SidebarDividerPanelComponent panel)
		{
			panel._layoutElement.flexibleWidth = 1f;
			panel._dividerImage.color = new Color(0.5019608f, 0.5019608f, 0.5019608f, 0.101960786f);
		};
		SidebarDividerPanelComponent.BasePanel item3 = SidebarDividerPanelComponent.Build(onReset3);
		panelList.Add(item3);
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			panel._button.interactable = CanJoin;
		};
		Action<LegacyButtonPanelComponent> onReset4 = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Join";
			panel._label.fontSize = 20;
			panel._layoutElement.flexibleWidth = 1f;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				OnJoin();
			});
		};
		LegacyButtonPanelComponent.BasePanel item4 = LegacyButtonPanelComponent.Build(onReset4, onRefresh);
		panelList.Add(item4);
		Action<LegacyButtonPanelComponent> onReset5 = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Cancel";
			panel._label.fontSize = 20;
			panel._layoutElement.flexibleWidth = 1f;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				OnCancel();
			});
		};
		LegacyButtonPanelComponent.BasePanel item5 = LegacyButtonPanelComponent.Build(onReset5);
		panelList.Add(item5);
		return panelList;
	}

	private static void OnCustomServer()
	{
		NetworkWaitingModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.OpenSignal.Invoke();
	}

	private static void OnJoin()
	{
		NetworkWaitingModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
		NetworkWaitingModalMenu.OpenAsClientSignal.Invoke();
	}

	private static void OnCancel()
	{
		NetworkWaitingModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, UNetManager.instance.matchList.MatchListChangedSignal, BaseMenu.OnRefresh, this).AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
