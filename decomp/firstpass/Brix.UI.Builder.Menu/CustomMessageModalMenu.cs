using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class CustomMessageModalMenu : BaseModalMenu
{
	public struct CustomMessage
	{
		public string TitleLabel;

		public string ContentLabel;

		public PanelList ExtraPanels;

		public List<KeyValuePair<string, Action>> Buttons;

		public CustomMessage(string titleLabel, string contentLabel, PanelList extraPanels, List<KeyValuePair<string, Action>> buttons)
		{
			TitleLabel = titleLabel;
			ContentLabel = contentLabel;
			ExtraPanels = extraPanels;
			Buttons = buttons;
		}
	}

	private BaseMenu _titleMenu;

	private string _titleLabel = string.Empty;

	private string _contentLabel = string.Empty;

	private PanelList _extraContentPanels;

	private PanelList _extraFooterPanels;

	private List<KeyValuePair<string, Action>> _buttons = new List<KeyValuePair<string, Action>>();

	private static readonly Action<CustomMessage, CustomMessageModalMenu> OnOpenWithContent = delegate(CustomMessage customMessage, CustomMessageModalMenu menu)
	{
		menu._titleLabel = customMessage.TitleLabel;
		menu._contentLabel = customMessage.ContentLabel;
		menu._extraContentPanels = customMessage.ExtraPanels;
		menu._extraFooterPanels = null;
		menu._buttons = customMessage.Buttons;
		menu.Open();
	};

	private static readonly Action<CustomMessage, CustomMessageModalMenu> OnOpenWithFooter = delegate(CustomMessage customMessage, CustomMessageModalMenu menu)
	{
		menu._titleLabel = customMessage.TitleLabel;
		menu._contentLabel = customMessage.ContentLabel;
		menu._extraContentPanels = null;
		menu._extraFooterPanels = customMessage.ExtraPanels;
		menu._buttons = customMessage.Buttons;
		menu.Open();
	};

	public static Signal<CustomMessage> OpenWithContentSignal = new Signal<CustomMessage>();

	public static Signal<CustomMessage> OpenWithFooterSignal = new Signal<CustomMessage>();

	public static Signal CloseSignal = new Signal();

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
	};

	protected override bool ReloadOnOpen => true;

	private CustomMessageModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static CustomMessageModalMenu Build(Func<RectTransform> binGetter)
	{
		return new CustomMessageModalMenu(binGetter);
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_titleMenu);
	}

	protected override PanelList GetHeaderBuilders()
	{
		PanelList headerBuilders = base.GetHeaderBuilders();
		Action<HorizontalLayoutPanelComponent> onReset = delegate(HorizontalLayoutPanelComponent panel)
		{
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 0f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
		};
		HorizontalLayoutPanel panelN = HorizontalLayoutPanel.New(onReset);
		SidebarTitleMenu titleMenu = SidebarTitleMenu.Build(() => panelN.GetSocket_2(), _titleLabel, hasCloseButton: false);
		_titleMenu = titleMenu;
		headerBuilders.Add(panelN);
		return headerBuilders;
	}

	protected override PanelList GetContentBuilders()
	{
		PanelList contentBuilders = base.GetContentBuilders();
		Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
		{
			panel._label.color = Colors.Black;
			panel._label.text = _contentLabel;
			panel._label.alignment = TextAnchor.MiddleCenter;
			panel._layoutElement.minHeight = panel._label.preferredHeight;
			panel._layoutElement.preferredHeight = panel._label.preferredHeight;
			panel._layoutElement.flexibleWidth = 1f;
			panel._layoutElement.flexibleHeight = 1f;
		};
		TitleTextPanelComponent.BasePanel item = TitleTextPanelComponent.Build(action, action).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
		contentBuilders.Add(item);
		if (_extraContentPanels != null)
		{
			foreach (IPanel extraContentPanel in _extraContentPanels)
			{
				contentBuilders.Add(extraContentPanel);
			}
		}
		return contentBuilders;
	}

	protected override PanelList GetFooterBuilders()
	{
		PanelList footerBuilders = base.GetFooterBuilders();
		if (_extraFooterPanels != null)
		{
			foreach (IPanel extraFooterPanel in _extraFooterPanels)
			{
				footerBuilders.Add(extraFooterPanel);
			}
		}
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 2f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
			panel._fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		};
		ILayoutPanel item = VerticalLayoutPanel.New(onReset).AddPanels(GetActionBuilders());
		footerBuilders.Add(item);
		return footerBuilders;
	}

	private PanelList GetActionBuilders()
	{
		PanelList panelList = PanelList.Request();
		for (int i = 0; i < _buttons.Count; i++)
		{
			int index = i;
			KeyValuePair<string, Action> buttonN = _buttons[index];
			Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
			{
				panel._layoutElement.minHeight = 48f;
				panel._layoutElement.preferredHeight = 48f;
				panel._layoutElement.flexibleWidth = 1f;
				panel._label.text = buttonN.Key;
				panel._selectionImage.transform.parent.gameObject.SetActive(value: false);
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					buttonN.Value();
				});
			};
			LegacyButtonPanelComponent.BasePanel basePanel = LegacyButtonPanelComponent.Build(onReset);
			basePanel.As(LegacyButtonPanelComponent.BasePanel.Variant.LegacyReskin_SglTxt_LeftIcon);
			basePanel.IgnoreWidgets();
			panelList.Add(basePanel);
		}
		return panelList;
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenWithContentSignal, OnOpenWithContent, this).AddSignalConnection(MenuState.Loaded, OpenWithFooterSignal, OnOpenWithFooter, this)
			.AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this);
	}
}
