using System;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkWaitingActionsMenu : BaseMenu
{
	public static Signal RefreshSignal = new Signal();

	public PanelList ContentPanels
	{
		get
		{
			PanelList panelList = PanelList.Request();
			Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
			{
				panel._button.interactable = !NetworkWaitingModalMenu.WaitingToClose;
			};
			Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
			{
				panel._label.color = Colors.Black;
				panel._label.text = I2Helper.TryGet("##mainmenu_multiplayer_abort").ToUpper();
				panel._label.fontSize = 20;
				panel._layoutElement.flexibleWidth = 1f;
				panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					OnAbort();
				});
			};
			panelList.Add(LegacyButtonPanelComponent.Build(onReset, onRefresh).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon));
			return panelList;
		}
	}

	private NetworkWaitingActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkWaitingActionsMenu Build(Func<RectTransform> getParent)
	{
		return new NetworkWaitingActionsMenu(getParent);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(ContentPanels);
	}

	private static void OnAbort()
	{
		UNetManager.instance.Disconnect();
		SceneManager.LoadScene(UNetManager.instance.offlineScene);
		NetworkWaitingModalMenu.CloseSignal.Invoke();
		NetworkCustomServerModalMenu.CloseSignal.Invoke();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
