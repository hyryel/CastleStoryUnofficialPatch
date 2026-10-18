using System;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkMessageModalMenu : BaseModalMenu
{
	private string _title = string.Empty;

	private string _message = string.Empty;

	private static readonly Action<string, string, NetworkMessageModalMenu> OnOpen2 = delegate(string title, string message, NetworkMessageModalMenu menu)
	{
		menu._title = title;
		menu._message = message;
		menu.Open();
	};

	private static readonly Action<NetworkMessageModalMenu> TryOpen = delegate(NetworkMessageModalMenu menu)
	{
		if (!menu.Opened && UNetManager.HasLastBrixNetworkError())
		{
			string message = UNetManager.PopLastBrixNetworkError();
			menu._title = I2Helper.TryGet("##mainmenu_multiplayer_error").ToUpper();
			menu._message = message;
			menu.Open();
		}
	};

	public static Signal<string, string> Open2Signal = new Signal<string, string>();

	public static Signal CloseSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	public static readonly Signal TryOpenSignal = new Signal();

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
	};

	private NetworkMessageModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkMessageModalMenu Build(Func<RectTransform> binGetter)
	{
		return new NetworkMessageModalMenu(binGetter);
	}

	protected override PanelList GetHeaderBuilders()
	{
		PanelList headerBuilders = base.GetHeaderBuilders();
		Action<HorizontalLayoutPanelComponent> onReset = delegate(HorizontalLayoutPanelComponent panel)
		{
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 0f;
			panel._image.enabled = false;
		};
		ILayoutPanel item = HorizontalLayoutPanel.New(onReset).AddPanel(GetTitle(this));
		headerBuilders.Add(item);
		return headerBuilders;
	}

	public static IPanel GetTitle(NetworkMessageModalMenu m)
	{
		Action<SidebarTitlePanelComponent> onReset = delegate(SidebarTitlePanelComponent panel)
		{
			panel._label.text = m._title;
			panel._label.color = Colors.Black;
		};
		return SidebarTitlePanel.New(onReset);
	}

	protected override PanelList GetContentBuilders()
	{
		PanelList contentBuilders = base.GetContentBuilders();
		Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
		{
			panel._label.text = _message;
			panel._label.color = Colors.Black;
			panel._label.alignment = TextAnchor.MiddleCenter;
			panel._layoutElement.minHeight = panel._label.preferredHeight;
			panel._layoutElement.preferredHeight = panel._label.preferredHeight;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = 1f;
		};
		TitleTextPanelComponent.BasePanel item = TitleTextPanelComponent.Build(action, action).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
		contentBuilders.Add(item);
		return contentBuilders;
	}

	protected override PanelList GetFooterBuilders()
	{
		PanelList footerBuilders = base.GetFooterBuilders();
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 0f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
			panel._fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		};
		ILayoutPanel item = VerticalLayoutPanel.New(onReset).AddPanels(GetActionBuilders());
		footerBuilders.Add(item);
		return footerBuilders;
	}

	private static PanelList GetActionBuilders()
	{
		PanelList panelList = PanelList.Request();
		Action<LegacyButtonPanelComponent> action = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = I2Helper.TryGet("##mainmenu_multiplayer_error_ok").ToUpper();
			panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				OnOk();
			});
		};
		IPanel item = LegacyButtonPanelComponent.Build(action, action).As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon).IgnoreWidgets();
		panelList.Add(item);
		return panelList;
	}

	private static void OnOk()
	{
		CloseSignal.Invoke();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, Open2Signal, OnOpen2, this).AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Loaded, TryOpenSignal, TryOpen, this);
	}
}
