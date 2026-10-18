using System;
using System.Collections.Generic;
using System.Net;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Input;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkCustomServerModalMenu : BaseModalMenu
{
	private BaseMenu _titleMenu;

	private BaseMenu _actionsMenu;

	private static string _tmpAddress;

	private static string _tmpPort;

	public static Signal OnSetTmpInfoSignal = new Signal();

	public static Signal OnTrySetInfoSignal = new Signal();

	public static Signal OnSetInfoSignal = new Signal();

	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	protected override bool ReloadOnOpen => true;

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentLayoutGroup.padding = new RectOffset(16, 16, 16, 16);
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
		panel._contentFitterPlus._useBounds = true;
		panel._contentFitterPlus._useAltBounds = true;
		panel._contentFitterPlus._useAllDescendants = false;
	};

	public static string TmpAddress
	{
		get
		{
			return _tmpAddress;
		}
		set
		{
			if (!(_tmpAddress == value))
			{
				_tmpAddress = value;
				OnSetTmpInfoSignal.Invoke();
			}
		}
	}

	public static string Address
	{
		get
		{
			return Neo.multiplayerIP;
		}
		set
		{
			OnTrySetInfoSignal.Invoke();
			if (IsAddressValid(value) && !(Neo.multiplayerIP == value))
			{
				Neo.multiplayerIP = value;
				OnSetInfoSignal.Invoke();
			}
		}
	}

	public static string TmpPort
	{
		get
		{
			return _tmpPort;
		}
		set
		{
			if (!(_tmpPort == value))
			{
				_tmpPort = value;
				OnSetTmpInfoSignal.Invoke();
			}
		}
	}

	public static string Port
	{
		get
		{
			return Neo.multiplayerPort;
		}
		set
		{
			OnTrySetInfoSignal.Invoke();
			if (IsPortValid(value) && !(Neo.multiplayerPort == value))
			{
				Neo.multiplayerPort = value;
				OnSetInfoSignal.Invoke();
			}
		}
	}

	private NetworkCustomServerModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkCustomServerModalMenu Build(Func<RectTransform> binGetter)
	{
		return new NetworkCustomServerModalMenu(binGetter);
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_titleMenu).AddMenu(_actionsMenu);
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
		SidebarTitleMenu titleMenu = SidebarTitleMenu.Build(() => panelN.GetSocket_2(), I2Helper.TryGet("##mainmenu_multiplayer_customaddress").ToUpper(), hasCloseButton: false);
		_titleMenu = titleMenu;
		panelN.IgnoreWidgets();
		headerBuilders.Add(panelN);
		return headerBuilders;
	}

	protected override PanelList GetContentBuilders()
	{
		PanelList contentBuilders = base.GetContentBuilders();
		ControlContext.CreateLink("UI Input Field: Join Custom Game", "Camera");
		ControlContext.CreateLink("UI Input Field: Join Custom Game", "General");
		ControlContext.CreateLink("UI Input Field: Join Custom Game", "Bricktron");
		Action<ServerEntryPanelComponent> onRefreshN = delegate(ServerEntryPanelComponent panel)
		{
			InputField.OnChangeEvent onValueChanged = panel._inputField.onValueChanged;
			panel._inputField.onValueChanged = new InputField.OnChangeEvent();
			TmpAddress = Address;
			string tmpAddress = TmpAddress;
			panel._inputField.text = tmpAddress;
			panel._inputFieldPlaceholderText.text = ((!string.IsNullOrEmpty(tmpAddress)) ? tmpAddress : "<None>");
			panel._inputField.onValueChanged = onValueChanged;
		};
		Action<ServerEntryPanelComponent> onReset = delegate(ServerEntryPanelComponent panel)
		{
			onRefreshN(panel);
			panel._keyLabel.text = I2Helper.TryGet("##mainmenu_multiplayer_customaddress_ip");
			panel._selectButton.gameObject.SetActive(value: false);
			panel._fullSelectButton.gameObject.SetActive(value: false);
			panel._inputFieldPlaceholderText.alignment = TextAnchor.MiddleRight;
			panel._inputField.textComponent.alignment = TextAnchor.MiddleRight;
			panel._inputField.lineType = InputField.LineType.SingleLine;
			panel._layoutElement.minHeight = 32f;
			panel._inputField.onEndEdit.RemoveAllListeners();
			panel._inputField.onEndEdit.AddListener(delegate(string dstValue)
			{
				TmpAddress = dstValue;
				Address = dstValue;
			});
			panel._inputField.onValueChanged.RemoveAllListeners();
			panel._inputField.onValueChanged.AddListener(delegate(string dstValue)
			{
				TmpAddress = dstValue;
			});
			panel._inputFieldEventTrigger.triggers.Clear();
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.Deselect,
				callback = new EventTrigger.TriggerEvent()
			};
			entry.callback.AddListener(delegate
			{
				Address = panel._inputField.text;
			});
			panel._inputFieldEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerEnter,
				callback = new EventTrigger.TriggerEvent()
			};
			entry2.callback.AddListener(delegate
			{
				ControlContext.SetContext("UI Input Field: Join Custom Game", focus: true);
			});
			panel._inputFieldEventTrigger.triggers.Add(entry2);
			EventTrigger.Entry entry3 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerExit,
				callback = new EventTrigger.TriggerEvent()
			};
			entry3.callback.AddListener(delegate
			{
				ControlContext.SetContext("UI Input Field: Join Custom Game", focus: false);
			});
			panel._inputFieldEventTrigger.triggers.Add(entry3);
		};
		ServerEntryPanelComponent.BasePanel basePanel = ServerEntryPanelComponent.Build(onReset, onRefreshN);
		basePanel.As(ServerEntryPanelComponent.BasePanel.Variant.Reskin);
		basePanel.IgnoreWidgets();
		contentBuilders.Add(basePanel);
		Action<ServerEntryPanelComponent> onRefreshN2 = delegate(ServerEntryPanelComponent panel)
		{
			InputField.OnChangeEvent onValueChanged = panel._inputField.onValueChanged;
			panel._inputField.onValueChanged = new InputField.OnChangeEvent();
			TmpPort = Port;
			string tmpPort = TmpPort;
			panel._inputField.text = tmpPort;
			panel._inputFieldPlaceholderText.text = ((!string.IsNullOrEmpty(tmpPort)) ? tmpPort : ("<" + I2Helper.TryGet("##mainmenu_multiplayer_customaddress_none") + ">"));
			panel._inputField.onValueChanged = onValueChanged;
		};
		Action<ServerEntryPanelComponent> onReset2 = delegate(ServerEntryPanelComponent panel)
		{
			onRefreshN2(panel);
			panel._keyLabel.text = I2Helper.TryGet("##mainmenu_multiplayer_customaddress_port");
			panel._selectButton.gameObject.SetActive(value: false);
			panel._fullSelectButton.gameObject.SetActive(value: false);
			panel._inputFieldPlaceholderText.alignment = TextAnchor.MiddleRight;
			panel._inputField.textComponent.alignment = TextAnchor.MiddleRight;
			panel._inputField.lineType = InputField.LineType.SingleLine;
			panel._layoutElement.minHeight = 32f;
			panel._inputField.onEndEdit.RemoveAllListeners();
			panel._inputField.onEndEdit.AddListener(delegate(string dstValue)
			{
				TmpPort = dstValue;
				Port = dstValue;
			});
			panel._inputField.onValueChanged.RemoveAllListeners();
			panel._inputField.onValueChanged.AddListener(delegate(string dstValue)
			{
				TmpPort = dstValue;
			});
			panel._inputFieldEventTrigger.triggers.Clear();
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.Deselect,
				callback = new EventTrigger.TriggerEvent()
			};
			entry.callback.AddListener(delegate
			{
				Port = panel._inputField.text;
			});
			panel._inputFieldEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerEnter,
				callback = new EventTrigger.TriggerEvent()
			};
			entry2.callback.AddListener(delegate
			{
				ControlContext.SetContext("UI Input Field: Join Custom Game", focus: true);
			});
			panel._inputFieldEventTrigger.triggers.Add(entry2);
			EventTrigger.Entry entry3 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerExit,
				callback = new EventTrigger.TriggerEvent()
			};
			entry3.callback.AddListener(delegate
			{
				ControlContext.SetContext("UI Input Field: Join Custom Game", focus: false);
			});
			panel._inputFieldEventTrigger.triggers.Add(entry3);
		};
		ServerEntryPanelComponent.BasePanel basePanel2 = ServerEntryPanelComponent.Build(onReset2, onRefreshN2);
		basePanel2.As(ServerEntryPanelComponent.BasePanel.Variant.Reskin);
		basePanel2.IgnoreWidgets();
		contentBuilders.Add(basePanel2);
		return contentBuilders;
	}

	protected override PanelList GetFooterBuilders()
	{
		PanelList footerBuilders = base.GetFooterBuilders();
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._layoutGroup.padding = new RectOffset();
			panel._layoutGroup.spacing = 2f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
			panel._fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		};
		VerticalLayoutPanel panelN = VerticalLayoutPanel.New(onReset);
		_actionsMenu = NetworkCustomServerActionsMenu.Build(() => panelN.GetSocket_2());
		footerBuilders.Add(panelN);
		return footerBuilders;
	}

	public static bool IsAddressValid(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		IPAddress address;
		return IPAddress.TryParse(value, out address);
	}

	public static bool IsPortValid(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		int result;
		return int.TryParse(value, out result);
	}

	private static string GetValidAddress(string srcValue)
	{
		if (string.IsNullOrEmpty(srcValue))
		{
			srcValue = string.Empty;
		}
		List<string> list = srcValue.Split('.').Slinq().ToList();
		while (list.Count < 4)
		{
			if (list.Count == 0)
			{
				list.Add("127");
			}
			if (list.Count == 1)
			{
				list.Add("0");
			}
			if (list.Count == 2)
			{
				list.Add("0");
			}
			if (list.Count == 3)
			{
				list.Add("1");
			}
		}
		if (list.Count > 4)
		{
			list = list.Slinq().Take(4).ToList();
		}
		return string.Join(".", list.ToArray());
	}

	private static string GetValidPort(string srcValue)
	{
		if (string.IsNullOrEmpty(srcValue))
		{
			srcValue = string.Empty;
		}
		return srcValue;
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Opened, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, OnTrySetInfoSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, OnSetInfoSignal, BaseMenu.OnRefresh, this);
	}
}
