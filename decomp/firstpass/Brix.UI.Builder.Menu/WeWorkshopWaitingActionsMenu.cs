using System;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class WeWorkshopWaitingActionsMenu : BaseMenu
{
	public static Signal RefreshSignal = new Signal();

	private WeWorkshopWaitingActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static WeWorkshopWaitingActionsMenu Build(Func<RectTransform> getParent)
	{
		return new WeWorkshopWaitingActionsMenu(getParent);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(GetBuilders());
	}

	public PanelList GetBuilders()
	{
		PanelList panelList = PanelList.Request();
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			panel._button.interactable = !NetworkWaitingModalMenu.WaitingToClose;
		};
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Abort";
			panel._label.fontSize = 20;
			panel._layoutElement.flexibleWidth = 1f;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				OnAbort();
			});
		};
		LegacyButtonPanelComponent.BasePanel item = LegacyButtonPanelComponent.Build(onReset, onRefresh);
		panelList.Add(item);
		return panelList;
	}

	private static void OnAbort()
	{
		NetworkWaitingModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
