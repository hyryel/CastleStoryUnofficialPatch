using System;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkWaitingQuitModalMenu : BaseModalMenu
{
	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	private string Title => I2Helper.TryGet("##mainmenu_multiplayer_lobby_leaving").ToUpper();

	private static string Message => I2Helper.TryGet("##mainmenu_multiplayer_lobby_leaving_info");

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
		panel._footerBin.gameObject.SetActive(value: false);
		panel._footerLineImage.color = Colors.Black;
		panel._headerLineImage.color = Colors.Black;
	};

	private NetworkWaitingQuitModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkWaitingQuitModalMenu Build(Func<RectTransform> binGetter)
	{
		return new NetworkWaitingQuitModalMenu(binGetter);
	}

	protected override PanelList GetHeaderBuilders()
	{
		PanelList headerBuilders = base.GetHeaderBuilders();
		Action<HorizontalLayoutPanelComponent> onReset = delegate(HorizontalLayoutPanelComponent horizontalLayoutPanelComponent)
		{
			horizontalLayoutPanelComponent._layoutGroup.padding = new RectOffset();
			horizontalLayoutPanelComponent._layoutGroup.spacing = 0f;
			horizontalLayoutPanelComponent._image.enabled = false;
			horizontalLayoutPanelComponent._layoutElement.enabled = true;
			horizontalLayoutPanelComponent._layoutElement.preferredWidth = 270f;
		};
		HorizontalLayoutPanel horizontalLayoutPanel = HorizontalLayoutPanel.New(onReset);
		Action<SidebarTitlePanelComponent> onReset2 = delegate(SidebarTitlePanelComponent sidebarTitlePanelComponent)
		{
			sidebarTitlePanelComponent._label.text = Title;
			sidebarTitlePanelComponent._label.color = Colors.Black;
		};
		SidebarTitlePanel panel = SidebarTitlePanel.New(onReset2);
		horizontalLayoutPanel.AddPanel(panel);
		headerBuilders.Add(horizontalLayoutPanel);
		return headerBuilders;
	}

	protected override PanelList GetContentBuilders()
	{
		PanelList contentBuilders = base.GetContentBuilders();
		Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
		{
			panel._label.color = Colors.Black;
			panel._label.text = Message;
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

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
