using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Brix.External.Signals;
using Brix.Game;
using Brix.Input;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public class NetworkChatMenu : BaseMenu
{
	private static List<string> _outputLines = new List<string>();

	private static bool _showsChat = true;

	private static readonly Action<User, NetworkChatMenu> OnPlayerLeft = delegate(User player, NetworkChatMenu menu)
	{
		OnReceiveChatMessage(string.Format("{0}{1}", player.ScreenName, I2Helper.TryGet("str_chatbox_player_left")), menu);
	};

	private static readonly Action<string, NetworkChatMenu> OnReceiveChatMessage = delegate(string line, NetworkChatMenu menu)
	{
		AddOutputLine(line);
		menu.Refresh();
	};

	private static readonly Action<NetworkChatMenu> OnShow = delegate(NetworkChatMenu menu)
	{
		_showsChat = true;
		menu.Refresh();
	};

	private static readonly Action<NetworkChatMenu> OnHide = delegate(NetworkChatMenu menu)
	{
		_showsChat = false;
		menu.Refresh();
	};

	public static Signal ShowSignal = new Signal();

	public static Signal HideSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	public static Signal OpenSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	protected override bool ReloadOnReopen => true;

	private static string OutputText => string.Join("\n", _outputLines.ToArray());

	private NetworkChatMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkChatMenu Build(Func<RectTransform> binGetter)
	{
		return new NetworkChatMenu(binGetter);
	}

	protected override PanelHandle AddPanels()
	{
		return base.AddPanels().AddPanels(GetBuilders());
	}

	private static void AddOutputLine(string line)
	{
		_outputLines.Add(line);
		_outputLines.Reverse();
		_outputLines = _outputLines.Slinq().Take(20).ToList();
		_outputLines.Reverse();
	}

	private static void ClearOutput()
	{
		_outputLines.Clear();
	}

	public static PanelList GetBuilders()
	{
		PanelList panelList = PanelList.Request();
		ControlContext.CreateLink("UI Input Field: Network Chat Input", "Camera");
		ControlContext.CreateLink("UI Input Field: Network Chat Input", "General");
		ControlContext.CreateLink("UI Input Field: Network Chat Input", "Bricktron");
		Action<ChatPanelComponent> onRefreshN = delegate(ChatPanelComponent panel)
		{
			panel.gameObject.SetActive(_showsChat);
			panel._outputText.text = OutputText;
			RectTransform rectTransform = panel.transform as RectTransform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
		};
		Action<ChatPanelComponent> onReset = delegate(ChatPanelComponent panel)
		{
			onRefreshN(panel);
			panel._outputText.fontStyle = FontStyle.Normal;
			panel._inputField.textComponent.fontStyle = FontStyle.Normal;
			panel._inputField.textComponent.alignment = TextAnchor.MiddleLeft;
			panel._inputFieldPlaceholderText.alignment = TextAnchor.MiddleLeft;
			panel._inputField.onEndEdit.RemoveAllListeners();
			panel._inputField.text = string.Empty;
			panel._inputField.onEndEdit.AddListener(delegate(string value)
			{
				if (UnityEngine.Input.GetKey(KeyCode.Return))
				{
					InputField.SubmitEvent onEndEdit = panel._inputField.onEndEdit;
					panel._inputField.onEndEdit = new InputField.SubmitEvent();
					panel._inputField.text = string.Empty;
					panel._inputField.onEndEdit = onEndEdit;
					SendChatMessage(value);
					panel._inputField.ActivateInputField();
					panel._inputField.Select();
				}
				else
				{
					panel._inputField.DeactivateInputField();
				}
			});
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerEnter,
				callback = new EventTrigger.TriggerEvent()
			};
			entry.callback.AddListener(delegate
			{
				ControlContext.SetContext("UI Input Field: Network Chat Input", focus: false);
				ControlContext.SetContext("Chat", focus: true);
			});
			panel._inputFieldEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerExit,
				callback = new EventTrigger.TriggerEvent()
			};
			entry2.callback.AddListener(delegate
			{
				ControlContext.SetContext("UI Input Field: Network Chat Input", focus: true);
				ControlContext.SetContext("Chat", focus: false);
			});
			panel._inputFieldEventTrigger.triggers.Add(entry2);
		};
		ChatPanelComponent.BasePanel basePanel = ChatPanelComponent.Build(onReset, onRefreshN);
		basePanel.IgnoreWidgets();
		panelList.Add(basePanel);
		return panelList;
	}

	private static void SendChatMessage(string line)
	{
		if (!string.IsNullOrEmpty(line))
		{
			line = Regex.Replace(line, "<[^>]*>", string.Empty);
			if (!string.IsNullOrEmpty(line))
			{
				UNetManager.instance.LocalLobbyPlayer.CallCmdSendChatMessage(line);
			}
		}
	}

	protected override void OpenSelf()
	{
		ClearOutput();
		base.OpenSelf();
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenSignal, BaseMenu.OnOpen, this).AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.InGamePlayerLeft, OnPlayerLeft, this)
			.AddSignalConnection(MenuState.Opened, UNetManager.LobbyNewChatMessage, OnReceiveChatMessage, this)
			.AddSignalConnection(MenuState.Opened, ShowSignal, OnShow, this)
			.AddSignalConnection(MenuState.Opened, HideSignal, OnHide, this);
	}
}
