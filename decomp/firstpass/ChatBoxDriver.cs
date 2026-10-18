using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.Input;
using Brix.Lua;
using Brix.Network;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class ChatBoxDriver : MonoBehaviour
{
	private static ChatBoxDriver _instance;

	public ContaineeExtended chatPanel;

	public TwitchChat twitchChat;

	private bool _isActive;

	private InputField chatInputField;

	private Animator _animator;

	private Button _submitButton;

	public Signal FocusChanged = new Signal();

	private bool previousFocus;

	private SignalConnection _inGamePlayerLeftConnection = SignalConnection.Empty;

	private SignalConnection _lobbyNewChatMessage = SignalConnection.Empty;

	private SignalConnection _twitchReconnect = SignalConnection.Empty;

	private int _receivedMessageCount;

	private bool initialized;

	private float _animatorUpdate;

	public static ChatBoxDriver Instance => _instance;

	private bool isActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			_isActive = value;
			ControlContext.SetContext("Chat", _isActive);
			UpdateAnimator();
		}
	}

	public bool IsFocused
	{
		get
		{
			if (!initialized)
			{
				return false;
			}
			return chatInputField.isFocused;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
		GameSignals.Once(GameSignals.LevelReady, Initialize);
	}

	[ContextMenu("Twitch Default Connect")]
	public void TwitchDefaultConnect()
	{
	}

	[ContextMenu("Twitch Default Disconnect")]
	public void TwitchDefaultDisconnect()
	{
		Twitch_Disconnect();
	}

	public void Twitch_Connect(string userName, string passName, string chanName)
	{
		twitchChat.Set(userName, passName, chanName);
		User.LocalUser.CallCmdSetTwitchName(userName);
		twitchChat.messageRecievedEvent.AddListener(Twitch_OnReceiveMsg);
		twitchChat.TryDisconnect();
		twitchChat.TryConnect();
	}

	public void Twitch_TryConnectFromSettings()
	{
		UserSettings userSettings = UserSettingsReader.Read();
		string chanName = userSettings.chat.twitch.chanName;
		string userName = userSettings.chat.twitch.userName;
		string passName = userSettings.chat.twitch.passName;
		if (!string.IsNullOrEmpty(chanName) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passName))
		{
			twitchChat.Set(userName, passName, chanName);
			User.LocalUser.CallCmdSetTwitchName(userName);
			twitchChat.messageRecievedEvent.AddListener(Twitch_OnReceiveMsg);
			twitchChat.TryDisconnect();
			twitchChat.TryConnect();
		}
	}

	public void Twitch_Disconnect()
	{
		twitchChat.messageRecievedEvent.RemoveAllListeners();
		twitchChat.TryDisconnect();
		twitchChat.TryConnect();
	}

	private void Twitch_OnReceiveMsg(string msg)
	{
		int num = msg.IndexOf("PRIVMSG #");
		string message = msg.Substring(num + twitchChat.chanName.Length + 11);
		string twitchName = msg.Substring(1, msg.IndexOf('!') - 1);
		Twitch_ReceiveMsg(twitchName, message);
	}

	private void Twitch_ReceiveMsg(string twitchName, string message)
	{
		User user = UNetManager.instance.UserList.Find((User i) => i.twitchName == twitchName);
		if (user != null)
		{
			Color factionColor = user.faction.FactionColor;
			string text = $"#{(int)(factionColor.r * 255f):X2}{(int)(factionColor.g * 255f):X2}{(int)(factionColor.b * 255f):X2}";
			string text2 = "<color=" + text + ">" + user.ScreenName + "</color>";
			text2 = ((!string.IsNullOrEmpty(user.twitchName)) ? (twitchName + "(" + text2 + ")") : text2);
			GameSignals.Invoke(UNetManager.LobbyNewChatMessage, text2 + ": " + message);
		}
		else
		{
			Color color = ColorFromUsername(twitchName);
			string text3 = $"#{(int)(color.r * 255f):X2}{(int)(color.g * 255f):X2}{(int)(color.b * 255f):X2}";
			string text4 = "<color=" + text3 + ">" + twitchName + "</color>";
			GameSignals.Invoke(UNetManager.LobbyNewChatMessage, text4 + ": " + message);
		}
	}

	public void Twitch_OnSendMsg(string msg)
	{
		if (chatInputField.text.Length > 0)
		{
			twitchChat.SendMsg(msg);
			Color factionColor = User.LocalUser.faction.FactionColor;
			string text = $"#{(int)(factionColor.r * 255f):X2}{(int)(factionColor.g * 255f):X2}{(int)(factionColor.b * 255f):X2}";
			string text2 = "<color=" + text + ">" + User.LocalUser.ScreenName + "</color>";
			text2 = ((!string.IsNullOrEmpty(twitchChat.userName)) ? (twitchChat.userName + "(" + text2 + ")") : text2);
			Twitch_ReceiveMsg(text2, msg);
		}
	}

	private static Color ColorFromUsername(string username)
	{
		return Color.white;
	}

	private void SignalFocusChange()
	{
		if (initialized)
		{
			if (IsFocused != previousFocus)
			{
				FocusChanged.Invoke();
			}
			previousFocus = IsFocused;
		}
	}

	private void Update()
	{
		SignalFocusChange();
		if (Neo.multiplayer && PickingUtility.Player.GetButtonDown("command_OpenChat"))
		{
			isActive = true;
		}
		AutoHide();
		DisableAnimatorAfterUpdate(Time.deltaTime);
	}

	private void Connect()
	{
		Disconnect();
		_inGamePlayerLeftConnection = GameSignals.Connect(UNetManager.InGamePlayerLeft, OnPlayerLeft);
		_lobbyNewChatMessage = GameSignals.Connect(UNetManager.LobbyNewChatMessage, ReceivedChatMessage);
		_twitchReconnect = GameSignals.Connect(GameSignals.TwitchReconnect, Twitch_TryConnectFromSettings);
	}

	private void Disconnect()
	{
		_inGamePlayerLeftConnection.Disconnect();
		_lobbyNewChatMessage.Disconnect();
		_twitchReconnect.Disconnect();
	}

	private void OnPlayerLeft(User player)
	{
		ReceivedChatMessage(player.ScreenName + " " + I2Helper.TryGet("##mainmenu_multiplayer_chatbox_player_left"));
	}

	private void ReceivedChatMessage(string msg)
	{
		MenuSound.PlaySound(MenuSound.Prefix.Main, MenuSound.Suffix.ChatReceive);
		_receivedMessageCount++;
		Text plugComponent = chatPanel.GetPlugComponent<Text>("Output Text");
		plugComponent.supportRichText = true;
		plugComponent.text = plugComponent.text + msg + Environment.NewLine;
		if (_receivedMessageCount > 10)
		{
			plugComponent.text = plugComponent.text.Substring(plugComponent.text.IndexOf(Environment.NewLine) + 1);
		}
	}

	private void SendChatMessage(string msg)
	{
		MenuSound.PlaySound(MenuSound.Prefix.Main, MenuSound.Suffix.ChatSend);
		if (!string.IsNullOrEmpty(msg) && msg.Length > 0 && msg.TrimStart()[0] == '/')
		{
			msg = msg.Replace("/", string.Empty);
			string[] array = msg.Split(' ');
			if (array.Length <= 0)
			{
				return;
			}
			if (array.Length == 3)
			{
				if (array[0].ToLower() == "login")
				{
					string userName = array[1];
					string passName = array[2];
					twitchChat.Set(userName, passName, twitchChat.chanName);
				}
			}
			else
			{
				if (array.Length != 2)
				{
					return;
				}
				string text = array[1];
				if (text.Contains("#"))
				{
					text = text.Replace("#", string.Empty);
					if (array[0].ToLower() == "join")
					{
						Twitch_Connect(twitchChat.userName, twitchChat.passName, text);
					}
					else if (array[1].ToLower() == "leave")
					{
						Twitch_Disconnect();
					}
				}
			}
		}
		else
		{
			if (twitchChat.connected)
			{
				Twitch_OnSendMsg(msg);
			}
			if (Neo.multiplayer)
			{
				User.LocalUser.CallCmdSendChatMessage(msg);
			}
		}
	}

	private void Initialize()
	{
		Connect();
		ControlContext.CreateLink("Chat", "Camera");
		ControlContext.CreateLink("Chat", "General");
		ControlContext.CreateLink("Chat", "Bricktron");
		ControlContext.CreateLink("Chat", "Projects");
		ControlContext.CreateLink("Chat", "Blueprints");
		ControlContext.CreateLink("Chat", "we_menu");
		ControlContext.CreateLink("Chat", "we_terrain_menu");
		ControlContext.CreateLink("Chat", "we_markers_menu");
		ControlContext.CreateLink("Chat", "we_time_menu");
		ControlContext.CreateLink("Chat", "we_map_menu");
		ControlContext.CreateLink("Chat", "we_grid_menu");
		ControlContext.CreateLink("Chat", "we_modes_menu");
		ControlContext.CreateLink("Chat", "we_save_menu");
		ControlContext.CreateLink("Chat", "tb_constr_menu");
		ControlContext.CreateLink("Chat", "tb_constr_blueprints_menu");
		ControlContext.CreateLink("Chat", "tb_constr_storage_menu");
		ControlContext.CreateLink("Chat", "debug");
		chatPanel.ResetPlugCache();
		chatInputField = chatPanel.GetPlugComponent<InputField>("Input Field");
		_animator = chatPanel.GetPlugComponent<Animator>("Animator");
		Text plugComponent = chatPanel.GetPlugComponent<Text>("Output Text");
		if (Neo.multiplayer)
		{
			plugComponent.text = I2Helper.TryGet("##mainmenu_multiplayer_chatbox_intro") + Environment.NewLine;
		}
		else
		{
			plugComponent.text = string.Empty;
		}
		chatInputField.text = string.Empty;
		chatInputField.onEndEdit.RemoveAllListeners();
		chatInputField.onEndEdit.AddListener(delegate(string value)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				if (value != string.Empty)
				{
					SendChatMessage(value);
				}
				chatInputField.text = string.Empty;
			}
			isActive = false;
			chatInputField.DeactivateInputField();
		});
		chatInputField.onValueChanged.RemoveAllListeners();
		chatInputField.onValueChanged.AddListener(delegate
		{
			chatInputField.text = chatInputField.text.Substring(0, Math.Min(chatInputField.text.Length, 120));
		});
		_submitButton = chatPanel.GetPlugComponent<Button>("Submit Button");
		if (_submitButton != null)
		{
			_submitButton.onClick.RemoveAllListeners();
			_submitButton.onClick.AddListener(delegate
			{
				string text = chatInputField.text;
				if (text != string.Empty)
				{
					SendChatMessage(text);
				}
				chatInputField.text = string.Empty;
			});
		}
		Twitch_TryConnectFromSettings();
		UpdateAnimator();
		initialized = true;
	}

	private void AutoHide()
	{
		if (isActive)
		{
			chatInputField.Select();
			chatInputField.ActivateInputField();
		}
	}

	private void UpdateAnimator()
	{
		if (_animator != null)
		{
			_animator.SetBool("Active", isActive);
			_animator.SetBool("Discreet", !isActive);
			_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			_animatorUpdate = 1f;
		}
	}

	private void DisableAnimatorAfterUpdate(float deltaTime)
	{
		if (_animatorUpdate > 0f)
		{
			_animatorUpdate -= deltaTime;
			if (_animatorUpdate <= 0f && _animator != null)
			{
				_animator.cullingMode = AnimatorCullingMode.CullCompletely;
			}
		}
	}

	[LuaDrivable("DisplayMessage")]
	private static DynValue lua_ReceivedChatMessage(ScriptExecutionContext context, CallbackArguments args)
	{
		ChatBoxDriver chatBoxDriver = args.AsComponent<ChatBoxDriver>(0, "DisplayMessage");
		string msg = args.AsString(1, "DisplayMessage");
		chatBoxDriver.ReceivedChatMessage(msg);
		return DynValue.Void;
	}

	[LuaDrivable("SendMessage")]
	private static DynValue lua_SendChatMessage(ScriptExecutionContext context, CallbackArguments args)
	{
		ChatBoxDriver chatBoxDriver = args.AsComponent<ChatBoxDriver>(0, "SendMessage");
		string msg = args.AsString(1, "SendMessage");
		chatBoxDriver.SendChatMessage(msg);
		return DynValue.Void;
	}
}
