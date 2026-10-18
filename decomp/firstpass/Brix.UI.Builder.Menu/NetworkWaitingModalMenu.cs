using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class NetworkWaitingModalMenu : BaseModalMenu
{
	public enum PlayerType
	{
		Host,
		Client
	}

	private PlayerType _playerType;

	private BaseMenu _titleMenu;

	private BaseMenu _actionsMenu;

	private SignalConnection _clientConnectedSelfConnection = SignalConnection.Empty;

	private SignalConnection _clientConnectFailedSelfConnection = SignalConnection.Empty;

	private SignalConnection _clientDisconnectedSelfConnection = SignalConnection.Empty;

	private static bool _waitingToClose;

	private static readonly Action<NetworkWaitingModalMenu> OnReopenAsHost = delegate(NetworkWaitingModalMenu menu)
	{
		menu.ReopenAs(PlayerType.Host);
	};

	private static readonly Action<NetworkWaitingModalMenu> OnReopenAsClient = delegate(NetworkWaitingModalMenu menu)
	{
		menu.ReopenAs(PlayerType.Client);
	};

	public static Signal OnHostFailureSignal = new Signal();

	public static Signal OnJoinFailureSignal = new Signal();

	public static Signal OnHostSuccessSignal = new Signal();

	public static Signal OnJoinSuccessSignal = new Signal();

	public static Signal OpenAsHostSignal = new Signal();

	public static Signal OpenAsClientSignal = new Signal();

	public static Signal CloseSignal = new Signal();

	private static readonly Signal RefreshSignal = new Signal();

	protected override Action<ModalPanelComponent> OnResetModal => delegate(ModalPanelComponent panel)
	{
		panel._contentFitterPlus._sizeOffset = new Vector2(0f, 32f);
	};

	private string Title => (_playerType != PlayerType.Client) ? "Hosting Game" : "Joining Game";

	private static string Message => "Please wait...";

	protected override bool ReloadOnOpen => true;

	public static bool WaitingToClose
	{
		get
		{
			return _waitingToClose;
		}
		set
		{
			_waitingToClose = value;
			RefreshSignal.Invoke();
		}
	}

	private NetworkWaitingModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static NetworkWaitingModalMenu Build(Func<RectTransform> binGetter, PlayerType playerType)
	{
		NetworkWaitingModalMenu networkWaitingModalMenu = new NetworkWaitingModalMenu(binGetter);
		networkWaitingModalMenu._playerType = playerType;
		return networkWaitingModalMenu;
	}

	public static string GetValidPlayerName(string srcValue)
	{
		if (string.IsNullOrEmpty(srcValue))
		{
			srcValue = "Player";
		}
		return srcValue;
	}

	protected override PanelList GetHeaderBuilders()
	{
		PanelList headerBuilders = base.GetHeaderBuilders();
		Action<HorizontalLayoutPanelComponent> onReset = delegate(HorizontalLayoutPanelComponent panel)
		{
			RectOffset padding = panel._layoutGroup.padding;
			padding.bottom = 0;
			padding.top = 0;
			padding.left = 0;
			padding.right = 0;
			panel._layoutGroup.padding = padding;
			panel._layoutGroup.spacing = 0f;
			panel._image.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.preferredWidth = 270f;
		};
		HorizontalLayoutPanel panelN = HorizontalLayoutPanel.New(onReset);
		SidebarTitleMenu titleMenu = SidebarTitleMenu.Build(() => panelN.GetSocket_2(), Title, hasCloseButton: false);
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
		VerticalLayoutPanel panelN = VerticalLayoutPanel.New(onReset);
		NetworkWaitingActionsMenu actionsMenu = NetworkWaitingActionsMenu.Build(() => panelN.GetSocket_2());
		_actionsMenu = actionsMenu;
		footerBuilders.Add(panelN);
		return footerBuilders;
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_titleMenu).AddMenu(_actionsMenu);
	}

	protected override void OpenSelf()
	{
		base.OpenSelf();
		UNetManager.instance.LocalPlayerName = GetValidPlayerName(Neo.multiplayerName);
		switch (_playerType)
		{
		case PlayerType.Client:
			ConnectAsClient();
			break;
		case PlayerType.Host:
			ConnectAsHost();
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void ConnectAsClient()
	{
		_clientConnectedSelfConnection = GameSignals.Connect(UNetManager.OnLocalPlayerReadySignal, OnJoinSuccess);
		_clientConnectFailedSelfConnection = GameSignals.Connect(UNetManager.ClientConnectFailedSelf, OnFailure_ClientConnectFailedSelf);
		_clientDisconnectedSelfConnection = GameSignals.Connect(UNetManager.Disconnected, OnFailure_ClientDisconnectedSelf);
		if (Neo.matchData.IsValid)
		{
			UNetManager.instance.ConnectToServerWithMatchData(Neo.matchData);
		}
		else if (NetworkCustomServerActionsMenu.CanJoin)
		{
			UNetManager.instance.ConnectToServer(NetworkCustomServerModalMenu.Address, int.Parse(NetworkCustomServerModalMenu.Port));
		}
	}

	private void ConnectAsHost()
	{
		_clientConnectedSelfConnection = GameSignals.Connect(UNetManager.OnLocalPlayerReadySignal, OnHostSuccess);
		_clientConnectFailedSelfConnection = GameSignals.Connect(UNetManager.Disconnected, OnFailure_ClientDisconnectedSelf);
		_clientDisconnectedSelfConnection = GameSignals.Connect(UNetManager.ServerStopped, OnFailure_ServerStopped);
		UNetManager.instance.maxConnections = UNetManager.HostingMaxConnections;
		NetworkClient networkClient;
		try
		{
			networkClient = UNetManager.instance.StartHostMultiplayer(UNetManager.HostingIP, UNetManager.HostingPort);
		}
		catch
		{
			PersistentCoroutineRunner.Relais(delegate(NetworkWaitingModalMenu self)
			{
				self?.Close();
				LobbyMenu.CloseSignal.Invoke();
				OnHostFailureSignal.Invoke();
				NetworkMessageModalMenu.Open2Signal.Invoke(I2Helper.TryGet("##mainmenu_multiplayer_error").ToUpper(), I2Helper.TryGet("##mainmenu_multiplayer_error_host_failure").Replace("#x", UNetManager.HostingPort.ToString()));
			}, this);
			return;
		}
		if (networkClient == null || UNetManager.instance.LocalNetworkClient == null)
		{
			PersistentCoroutineRunner.Relais(delegate(NetworkWaitingModalMenu self)
			{
				self?.Close();
				LobbyMenu.CloseSignal.Invoke();
				OnHostFailureSignal.Invoke();
				NetworkMessageModalMenu.Open2Signal.Invoke(I2Helper.TryGet("##mainmenu_multiplayer_error").ToUpper(), I2Helper.TryGet("##mainmenu_multiplayer_error_host_failure").Replace("#x", UNetManager.HostingPort.ToString()));
			}, this);
		}
	}

	protected override void CloseSelf()
	{
		if (!WaitingToClose)
		{
			WaitingToClose = false;
			_clientConnectedSelfConnection.Disconnect();
			_clientConnectFailedSelfConnection.Disconnect();
			_clientDisconnectedSelfConnection.Disconnect();
			base.CloseSelf();
			OnJoinSuccessSignal.Clear();
		}
	}

	private void ReopenAs(PlayerType playerType)
	{
		Close();
		_playerType = playerType;
		Open();
	}

	private void OnJoinSuccess()
	{
		OnJoinSuccessSignal.Invoke();
		OnSuccess();
	}

	private void OnHostSuccess()
	{
		OnHostSuccessSignal.Invoke();
		OnSuccess();
	}

	private void OnSuccess()
	{
		if (!WaitingToClose)
		{
			CloseSignal.Invoke();
			NetworkCustomServerModalMenu.CloseSignal.Invoke();
			LobbyMenu.OpenSignal.Invoke();
		}
	}

	private void OnFailure_ClientConnectFailedSelf()
	{
		OnFailure();
		NetworkMessageModalMenu.Open2Signal.Invoke(I2Helper.TryGet("##mainmenu_multiplayer_error").ToUpper(), I2Helper.TryGet("##mainmenu_multiplayer_error_join_failure"));
	}

	private void OnFailure_ClientDisconnectedSelf()
	{
		OnFailure();
	}

	private void OnFailure_ServerStopped()
	{
		OnFailure();
		NetworkMessageModalMenu.Open2Signal.Invoke(I2Helper.TryGet("##mainmenu_multiplayer_error").ToUpper(), I2Helper.TryGet("##mainmenu_multiplayer_error_client_failure"));
	}

	private void OnFailure()
	{
		if (!WaitingToClose)
		{
			WaitingToClose = true;
			PersistentCoroutineRunner.Relais(delegate(NetworkWaitingModalMenu self)
			{
				WaitingToClose = false;
				self?.Close();
				UNetManager.instance.ResetPersistantNetworkSceneObjects();
				UNetManager.instance.Disconnect();
				NetworkCustomServerModalMenu.CloseSignal.Invoke();
			}, this);
		}
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Loaded, OpenAsClientSignal, OnReopenAsClient, this).AddSignalConnection(MenuState.Loaded, OpenAsHostSignal, OnReopenAsHost, this)
			.AddSignalConnection(MenuState.Loaded, CloseSignal, BaseMenu.OnClose, this)
			.AddSignalConnection(MenuState.Opened, RefreshSignal, BaseMenu.OnRefresh, this);
	}
}
