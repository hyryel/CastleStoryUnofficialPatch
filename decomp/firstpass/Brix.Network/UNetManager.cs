using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brix.Engine;
using Brix.Game;
using Brix.Game.Network;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Logging;
using Network;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Brix.Network;

public class UNetManager : NetworkLobbyManager
{
	public enum Mode
	{
		Host,
		Client
	}

	public enum BrixNetworkError
	{
		None,
		BadMessage,
		CRCMismatch,
		DNSFailure,
		MessageToLong,
		NoResources,
		Timeout,
		UsageError,
		VersionMismatch,
		WrongChannel,
		WrongConnection,
		WrongHost,
		WrongOperation,
		PlayerKicked,
		UseMessage
	}

	public enum CustomMsgType : short
	{
		FileDataMessageStart = 48,
		FileDataMessage,
		FileDataMessageComplete,
		DeblocBatchBegin,
		DeblocBatchData,
		DeblocBatchEnd,
		AssignUserObject,
		LuaDataMessage,
		AnimFloat,
		AnimTrigger,
		AnimBool,
		AnimReset,
		AnimInt,
		AnimPlay,
		AnimResetExit,
		AbortDownload
	}

	private static BrixLogger logger = new BrixLogger(typeof(UNetManager).FullName);

	public List<UNetAbstractMessageHandler> CustomHandlerList = new List<UNetAbstractMessageHandler>();

	public UNetLuaMessageHandler LuaMessageHandler = new UNetLuaMessageHandler();

	public UNetFileTransferMessageHandler FileTransfer = new UNetFileTransferMessageHandler();

	public UnetDeblocMessageHandler DeblocMessageHandler = new UnetDeblocMessageHandler();

	public UNetNetworkDiscovery networkDiscovery;

	public MatchList matchList = new MatchList();

	public List<NetworkServerSimple> NatServers = new List<NetworkServerSimple>();

	public List<User> UserList;

	public List<LobbyPlayer> LobbyPlayerList;

	public GameParam gameParam;

	private string _localPlayerName;

	public NetworkClient LocalNetworkClient;

	public bool debugLoader;

	private string _currentSceneName;

	private Scene _currentScene;

	private static string _lastBrixNetworkErrorMessage;

	private static BrixNetworkError _lastBrixNetworkError;

	public static Dictionary<NetworkError, BrixNetworkError> ToBrixNetworkErrorMap = new Dictionary<NetworkError, BrixNetworkError>(FastEnumIntEqualityComparer<NetworkError>.Instance)
	{
		{
			NetworkError.Ok,
			BrixNetworkError.None
		},
		{
			NetworkError.BadMessage,
			BrixNetworkError.BadMessage
		},
		{
			NetworkError.CRCMismatch,
			BrixNetworkError.CRCMismatch
		},
		{
			NetworkError.DNSFailure,
			BrixNetworkError.DNSFailure
		},
		{
			NetworkError.MessageToLong,
			BrixNetworkError.MessageToLong
		},
		{
			NetworkError.NoResources,
			BrixNetworkError.NoResources
		},
		{
			NetworkError.Timeout,
			BrixNetworkError.Timeout
		},
		{
			NetworkError.UsageError,
			BrixNetworkError.UsageError
		},
		{
			NetworkError.VersionMismatch,
			BrixNetworkError.VersionMismatch
		},
		{
			NetworkError.WrongChannel,
			BrixNetworkError.WrongChannel
		},
		{
			NetworkError.WrongConnection,
			BrixNetworkError.WrongConnection
		},
		{
			NetworkError.WrongHost,
			BrixNetworkError.WrongHost
		},
		{
			NetworkError.WrongOperation,
			BrixNetworkError.WrongOperation
		}
	};

	public static Dictionary<BrixNetworkError, string> BrixNetworkErrorNameMap = new Dictionary<BrixNetworkError, string>(FastEnumIntEqualityComparer<BrixNetworkError>.Instance)
	{
		{
			BrixNetworkError.BadMessage,
			"##mainmenu_multiplayer_error_badmessage"
		},
		{
			BrixNetworkError.CRCMismatch,
			"##mainmenu_multiplayer_error_crcmismatch"
		},
		{
			BrixNetworkError.DNSFailure,
			"##mainmenu_multiplayer_error_dnsfailure"
		},
		{
			BrixNetworkError.MessageToLong,
			"##mainmenu_multiplayer_error_messagetoolong"
		},
		{
			BrixNetworkError.NoResources,
			"##mainmenu_multiplayer_error_noresources"
		},
		{
			BrixNetworkError.Timeout,
			"##mainmenu_multiplayer_error_timeout"
		},
		{
			BrixNetworkError.UsageError,
			"##mainmenu_multiplayer_error_usageerror"
		},
		{
			BrixNetworkError.VersionMismatch,
			"##mainmenu_multiplayer_error_versionmismatch"
		},
		{
			BrixNetworkError.WrongChannel,
			"##mainmenu_multiplayer_error_wrongchannel"
		},
		{
			BrixNetworkError.WrongConnection,
			"##mainmenu_multiplayer_error_wrongconnection"
		},
		{
			BrixNetworkError.WrongHost,
			"##mainmenu_multiplayer_error_wronghost"
		},
		{
			BrixNetworkError.WrongOperation,
			"##mainmenu_multiplayer_error_wrongoperation"
		},
		{
			BrixNetworkError.PlayerKicked,
			"##mainmenu_multiplayer_error_playerkicked"
		}
	};

	private int _hostingPort;

	private ELobbyType _gameVisibility;

	public static Dictionary<ELobbyType, string> GameVisibilityNameMap = new Dictionary<ELobbyType, string>(FastEnumIntEqualityComparer<ELobbyType>.Instance)
	{
		{
			ELobbyType.k_ELobbyTypeFriendsOnly,
			"##mainmenu_multiplayer_privacy_friendsonly"
		},
		{
			ELobbyType.k_ELobbyTypePrivate,
			"##mainmenu_multiplayer_privacy_private"
		},
		{
			ELobbyType.k_ELobbyTypePublic,
			"##mainmenu_multiplayer_privacy_public"
		}
	};

	public static List<ELobbyType> GameVisibilityList = new List<ELobbyType>
	{
		ELobbyType.k_ELobbyTypeFriendsOnly,
		ELobbyType.k_ELobbyTypePrivate,
		ELobbyType.k_ELobbyTypePublic
	};

	public NATHelper natHelper;

	private FieldInfo clientIDField;

	private bool calledDisconnect;

	private float lastSend;

	public static GameSignals.GameSignal OnLocalPlayerReadySignal = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnAnyToggleReadySignal = new GameSignals.GameSignal();

	public static GameSignals.GameSignal ClientNewPlayerConnectFailed = new GameSignals.GameSignal();

	public static GameSignals.GameSignal<NetworkConnection> ClientNewPlayerConnected = new GameSignals.GameSignal<NetworkConnection>();

	public static GameSignals.GameSignal ClientNewPlayerLobbyObjReady = new GameSignals.GameSignal();

	public static GameSignals.GameSignal<NetworkConnection> ClientNewPlayerDisconnected = new GameSignals.GameSignal<NetworkConnection>();

	public static GameSignals.GameSignal ClientConnectedSelf = new GameSignals.GameSignal();

	public static GameSignals.GameSignal ClientConnectFailedSelf = new GameSignals.GameSignal();

	public static GameSignals.GameSignal Disconnected = new GameSignals.GameSignal();

	public static GameSignals.GameSignal LateDisconnected = new GameSignals.GameSignal();

	public static GameSignals.GameSignal ServerNewPlayerConnectFailed = new GameSignals.GameSignal();

	public static GameSignals.GameSignal<NetworkConnection> ServerNewPlayerConnected = new GameSignals.GameSignal<NetworkConnection>();

	public static GameSignals.GameSignal<NetworkConnection> ServerPlayerDisconnected = new GameSignals.GameSignal<NetworkConnection>();

	public static GameSignals.GameSignal<LobbyPlayer> ServerPlayerKicked = new GameSignals.GameSignal<LobbyPlayer>();

	public static GameSignals.GameSignal ServerPlayersReady = new GameSignals.GameSignal();

	public static GameSignals.GameSignal ServerStopped = new GameSignals.GameSignal();

	public static GameSignals.GameSignal ServerStarted = new GameSignals.GameSignal();

	public static GameSignals.GameSignal LobbyRefresh = new GameSignals.GameSignal();

	public static GameSignals.GameSignal<string> LobbyNewChatMessage = new GameSignals.GameSignal<string>();

	public static GameSignals.GameSignal PlayerListChanged = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnSetIsHostReady = new GameSignals.GameSignal();

	public static GameSignals.GameSignal<LobbyPlayer> LobbyPlayerJoined = new GameSignals.GameSignal<LobbyPlayer>();

	public static GameSignals.GameSignal<LobbyPlayer> LobbyPlayerLeft = new GameSignals.GameSignal<LobbyPlayer>();

	public static GameSignals.GameSignal<User> InGamePlayerJoined = new GameSignals.GameSignal<User>();

	public static GameSignals.GameSignal<User> InGamePlayerLeft = new GameSignals.GameSignal<User>();

	public static GameSignals.GameSignal OnPlayerDownloadStart = new GameSignals.GameSignal();

	public static GameSignals.GameSignal OnPlayerDownloadEnd = new GameSignals.GameSignal();

	public static UNetManager instance => (UNetManager)NetworkManager.singleton;

	public Mode mode { get; set; }

	public string LocalPlayerName
	{
		get
		{
			return _localPlayerName;
		}
		set
		{
			int length = ((value.Length < 30) ? value.Length : 30);
			_localPlayerName = value.Substring(0, length);
		}
	}

	public LobbyPlayer LocalLobbyPlayer { get; set; }

	public bool LocalPlayerIsHost => mode == Mode.Host;

	private static bool IsConnected => NetworkClient.active || NetworkServer.active;

	public static int HostingMaxConnections => 4;

	public static string HostingIP => "127.0.0.1";

	public static int HostingPort
	{
		get
		{
			return instance._hostingPort;
		}
		private set
		{
			instance._hostingPort = value;
		}
	}

	public ELobbyType GameVisibility
	{
		get
		{
			return _gameVisibility;
		}
		set
		{
			_gameVisibility = value;
			if (SteamManager.Initialized)
			{
				SteamMatchmaking.SetLobbyType(SteamManager.Instance.LobbyId, GameVisibility);
			}
		}
	}

	private float SendRate { get; set; }

	public void Initialize()
	{
		calledDisconnect = false;
		LobbyPlayerList = new List<LobbyPlayer>();
		UserList = new List<User>();
		SendRate = 0.1f;
		lastSend = Time.fixedTime;
		base.maxConnections = 4;
		GameParam.instance.NetworkHostLoaded = false;
		CustomHandlerList.Add(DeblocMessageHandler);
		CustomHandlerList.Add(LuaMessageHandler);
		CustomHandlerList.Add(FileTransfer);
		CustomHandlerList.Slinq().ForEach(delegate(UNetAbstractMessageHandler handler)
		{
			GameSignals.Connect(ClientConnectedSelf, delegate(UNetAbstractMessageHandler h)
			{
				h.Initialize();
			}, handler);
		});
	}

	private void ManuallyInitTransport()
	{
		NetworkTransport.Init(base.globalConfig);
		base.connectionConfig.Channels.Clear();
		base.connectionConfig.AcksType = ConnectionAcksType.Acks64;
		base.connectionConfig.MaxSentMessageQueueSize = 2048;
		if (base.customConfig)
		{
			foreach (QosType channel in base.channels)
			{
				base.connectionConfig.AddChannel(channel);
			}
			return;
		}
		base.connectionConfig.AddChannel(QosType.ReliableSequenced);
		base.connectionConfig.AddChannel(QosType.Unreliable);
	}

	private void Start()
	{
		base.useGUILayout = false;
		natHelper = GetComponent<NATHelper>();
		clientIDField = typeof(NetworkClient).GetField("m_ClientId", BindingFlags.Instance | BindingFlags.NonPublic);
		networkDiscovery = GetComponent<UNetNetworkDiscovery>();
		if (SceneManager.GetActiveScene().buildIndex != 2)
		{
			ResetPersistantNetworkSceneObjects();
		}
	}

	private void Update()
	{
		NatServers.ForEach(delegate(NetworkServerSimple server)
		{
			server.Update();
		});
	}

	private void FixedUpdate()
	{
		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene != _currentScene)
		{
			_currentScene = activeScene;
			_currentSceneName = _currentScene.name;
		}
		if (_currentSceneName == instance.lobbyScene)
		{
			if (LocalPlayerIsHost)
			{
				UpdateLobbyPlayersInfos();
			}
			GameSignals.Invoke(LobbyRefresh);
		}
	}

	public void LoadDebug()
	{
		logger.Info("The main scene was loaded using debug method");
		mode = Mode.Host;
		base.autoCreatePlayer = true;
		LocalNetworkClient = StartHost();
		GameObject gameObject = UnityEngine.Object.Instantiate(base.gamePlayerPrefab);
		gameObject.SetActive(value: true);
		NetworkServer.SpawnWithClientAuthority(gameObject, NetworkServer.localConnections.Slinq().First());
		User.LocalUser = gameObject.GetComponent<User>();
		debugLoader = true;
	}

	public static string PopLastBrixNetworkError()
	{
		BrixNetworkError lastBrixNetworkError = _lastBrixNetworkError;
		_lastBrixNetworkError = BrixNetworkError.None;
		if (lastBrixNetworkError == BrixNetworkError.UseMessage)
		{
			if (string.IsNullOrEmpty(_lastBrixNetworkErrorMessage))
			{
				return "Could not resolve error message.";
			}
			string lastBrixNetworkErrorMessage = _lastBrixNetworkErrorMessage;
			_lastBrixNetworkErrorMessage = null;
			return lastBrixNetworkErrorMessage;
		}
		return I2Helper.TryGet(BrixNetworkErrorNameMap[lastBrixNetworkError]);
	}

	public static bool HasLastBrixNetworkError()
	{
		return _lastBrixNetworkError != BrixNetworkError.None;
	}

	public static void SetBrixNetworkError(BrixNetworkError error, bool doReboot = true)
	{
		_lastBrixNetworkError = BrixNetworkError.UseMessage;
		if (error == BrixNetworkError.UseMessage)
		{
			SetNetworkError(string.Empty, doReboot);
			return;
		}
		_lastBrixNetworkError = error;
		if (doReboot)
		{
			LoadOfflineScene();
		}
	}

	public static void SetNetworkError(NetworkError error, bool doReboot = true)
	{
		if (_lastBrixNetworkError == BrixNetworkError.None)
		{
			_lastBrixNetworkError = ToBrixNetworkErrorMap[error];
			if (doReboot)
			{
				LoadOfflineScene();
			}
		}
	}

	public static void SetNetworkError(string errorMsg, bool doReboot = true)
	{
		if (_lastBrixNetworkError == BrixNetworkError.None)
		{
			_lastBrixNetworkError = BrixNetworkError.UseMessage;
			_lastBrixNetworkErrorMessage = errorMsg;
			if (doReboot)
			{
				LoadOfflineScene();
			}
		}
	}

	private static void LoadOfflineScene()
	{
		if (IsConnected)
		{
			instance.Disconnect();
		}
		else
		{
			SceneManager.LoadScene(instance.offlineScene);
		}
	}

	public static void SetHostingPort(int port)
	{
		HostingPort = port;
	}

	private void OnHolePunchedServer(int natListenPort)
	{
		if (base.maxConnections > LobbyPlayerList.Count)
		{
			NATServer nATServer = new NATServer();
			if (nATServer.Listen(natListenPort, NetworkServer.hostTopology))
			{
				logger.InfoFormat("NatTraversal : New punch from client. Listening on port {0}.", natListenPort);
				NatServers.Add(nATServer);
			}
		}
	}

	public void ConnectToServerViaRelay(string ip, int hostPort, int listenPort)
	{
		try
		{
			instance.ResetPersistantNetworkSceneObjects();
			base.networkAddress = ip;
			base.networkPort = hostPort;
			Neo.multiplayer = true;
			mode = Mode.Client;
			logger.InfoFormat("NatTraversal : Attempting to connect to server {0} : {1}", base.networkAddress, base.networkPort);
			ManuallyInitTransport();
			Initialize();
			HostTopology topology = new HostTopology(base.connectionConfig, base.maxPlayers);
			int num = NetworkTransport.AddHost(topology, listenPort);
			client = new NetworkClient();
			client.Configure(topology);
			client.Connect(base.networkAddress, base.networkPort);
			clientIDField.SetValue(client, num);
			UseExternalClient(client);
			LocalNetworkClient = client;
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "NatTraversal : Could not connect to host {0}:{1} via steam", base.networkAddress, base.networkPort);
			PersistentCoroutineRunner.Relais(delegate(UNetManager self)
			{
				if (self != null)
				{
					self.OnClientError(null, 1);
				}
			}, this);
		}
	}

	public static void TryUPnPForwarding(int ServerPort)
	{
		try
		{
			UPnP.connect(ServerPort);
		}
		catch (Exception ex)
		{
			logger.WarnFormat("UPnP : forwarding failed '{0}'", ex.Message);
		}
	}

	public void StartHostLocalOnly()
	{
		logger.Info("UNet : Started local only game.");
		instance.maxConnections = 1;
		instance.networkPort = 0;
		StartHost();
	}

	public NetworkClient StartHostMultiplayer(string address, int port)
	{
		logger.InfoFormat("UNet : Launching multiplayer game on address {0}:{1}.", address, port);
		NetworkClient networkClient = null;
		try
		{
			Neo.multiplayer = true;
			instance.Initialize();
			base.networkAddress = address;
			base.networkPort = port;
			try
			{
				networkClient = StartHost();
			}
			catch (Exception t)
			{
				Disconnect();
				logger.ErrorFormat("Could not start server: {0}", t);
				SetNetworkError(NetworkError.WrongOperation);
				return null;
			}
			if (networkClient == null)
			{
				Disconnect();
				logger.Error("Could not start server.");
				SetNetworkError(NetworkError.WrongOperation);
				return null;
			}
			natHelper.connectToNATFacilitator();
			if (base.networkPort == 3000)
			{
				networkDiscovery.BroadcastLanGame();
			}
			if (SteamManager.Initialized)
			{
				SteamMatchmaking.LeaveLobby(SteamManager.Instance.LobbyId);
				SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
			}
			natHelper.ListenForPunchthrough(OnHolePunchedServer);
			natHelper.ListenForUdpProxyRequests(OnHolePunchedServer);
			TryUPnPForwarding(base.networkPort);
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "UNet : Could not start server on address {0}:{1}.", address, port);
		}
		return networkClient;
	}

	public override NetworkClient StartHost()
	{
		instance.mode = Mode.Host;
		ManuallyInitTransport();
		try
		{
			LocalNetworkClient = base.StartHost();
		}
		catch (Exception t)
		{
			logger.ErrorFormat("Could not start server: {0}", t);
			SetNetworkError(NetworkError.WrongOperation);
			return null;
		}
		return LocalNetworkClient;
	}

	public void ConnectToServerWithMatchData(MatchList.MatchData matchData)
	{
		string text = ((!SteamManager.Initialized) ? string.Empty : SteamMatchmaking.GetLobbyData(matchData.LobbySteamID, "guid"));
		if (matchData.IsLanGame || string.IsNullOrEmpty(text))
		{
			ConnectToServer(matchData.Address.ToString(writePort: false), matchData.Address.GetPort());
			return;
		}
		try
		{
			natHelper.connectToNATFacilitator();
			if (!NetworkManager.singleton.IsClientConnected())
			{
				logger.Info("NATPunch : Trying to NAT punch to the server.");
				natHelper.TryNatTraversalToGuid(text);
			}
			else
			{
				logger.Warn("NatTraversal : Aborted, the client is already connected.");
			}
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "NatTraversal : Could not punchthrough to host {0}:{1} via steam", base.networkAddress, base.networkPort);
			PersistentCoroutineRunner.Relais(delegate(UNetManager self)
			{
				if (self != null)
				{
					self.OnClientError(null, 1);
				}
			}, this);
		}
	}

	public NetworkClient ConnectToServer(string address, int port)
	{
		logger.InfoFormat("UNet : Attempting direct connection to server {0}:{1}.", address, port);
		try
		{
			instance.ResetPersistantNetworkSceneObjects();
			ManuallyInitTransport();
			Initialize();
			mode = Mode.Client;
			base.networkAddress = address;
			base.networkPort = port;
			Neo.multiplayer = true;
			LocalNetworkClient = StartClient();
		}
		catch (Exception ex)
		{
			logger.ErrorFormat(ex, "UNet : Direct Connection failed on address : {0}:{1}.", address, port);
			SetNetworkError(string.Format("{0}\n\n<i>{1}</i>", I2Helper.TryGet("####mainmenu_multiplayer_error_join_failure"), ex.Message));
			return null;
		}
		return LocalNetworkClient;
	}

	public void Disconnect()
	{
		calledDisconnect = true;
		logger.Info("UNet : Disconnecting.");
		if (!(networkDiscovery == null))
		{
			if (networkDiscovery.running)
			{
				networkDiscovery.StopBroadcast();
			}
			if (NetworkServer.active)
			{
				StopServer();
			}
			if (SteamManager.Initialized)
			{
				GameVisibility = ELobbyType.k_ELobbyTypePublic;
				SteamMatchmaking.LeaveLobby(SteamManager.Instance.LobbyId);
			}
			NatServers.Slinq().ForEach(delegate(NetworkServerSimple nat)
			{
				nat.Stop();
			});
			NatServers.Clear();
			if (NetworkClient.active)
			{
				StopClient();
			}
			if (LocalNetworkClient != null)
			{
				LocalNetworkClient.Disconnect();
				LocalNetworkClient.Shutdown();
			}
			NetworkServer.ClearHandlers();
			NetworkTransport.Shutdown();
			ManuallyInitTransport();
			NetworkManager.networkSceneName = string.Empty;
		}
	}

	public void SafeDisconnectConnection(NetworkConnection connection)
	{
		NetworkServerSimple networkServerSimple = NatServers.Slinq().Where((NetworkServerSimple s, NetworkConnection connn) => s.connections.Slinq().Any((NetworkConnection c, NetworkConnection conn) => c != null && c.connectionId == conn.connectionId, connn), connection).FirstOrDefault();
		if (networkServerSimple != null)
		{
			networkServerSimple.Stop();
			NatServers.Remove(networkServerSimple);
			logger.InfoFormat("NatTraversal : Closed nattraversal on port {0}", networkServerSimple.listenPort);
		}
		else
		{
			connection.Disconnect();
		}
		NetworkServer.DestroyPlayersForConnection(connection);
		NetworkServer.RemoveExternalConnection(connection.connectionId);
		NatServers.RemoveAll((NetworkServerSimple s) => s.connections.Slinq().All((NetworkConnection c) => c == null));
	}

	public void ResetPersistantNetworkSceneObjects()
	{
		if (GameParam.instance != null)
		{
			gameParam = GameParam.instance.GetComponent<DontDestroyOnLoad>().ResetObject().GetComponent<GameParam>();
		}
		gameParam.gameObject.SetActive(value: false);
	}

	public void ResetToDefault()
	{
		ResetPersistantNetworkSceneObjects();
		CustomHandlerList.Clear();
		LobbyPlayerList = new List<LobbyPlayer>();
		UserList = new List<User>();
		LocalNetworkClient = null;
		LocalPlayerName = string.Empty;
		Neo.multiplayer = false;
	}

	public void SendMapToClients()
	{
		(from p in LobbyPlayerList.Slinq()
			where p.RequestedMapDownload
			select p).ForEach(delegate(LobbyPlayer p, UNetManager self)
		{
			self.SendFiles(p.connectionToClient);
		}, this);
	}

	public void AbortUploads()
	{
		FileTransfer.AbortUploads();
	}

	public void SendFiles(NetworkConnection connection)
	{
		if (connection != null && NetworkServer.connections.Count > 1)
		{
			logger.Info("UNet : Sending missing files to client.");
			string text = Path.GetTempPath() + "CastleStory/";
			string text2 = $"{text}{GameParam.instance.mapChecksum}";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!File.Exists(text2))
			{
				DirectoryCopy.Copy(GameParam.Map.path, text + "/compressdir", copySubDirs: false);
				lzip.compressDir(text + "/compressdir", 10, text2);
			}
			if (File.Exists(text2))
			{
				FileTransfer.SendFileToClient(connection, text2);
			}
		}
	}

	public void RequestMapDownload(bool value)
	{
		if (LocalLobbyPlayer != null)
		{
			LocalLobbyPlayer.CallCmdSetDownloadRequest(value);
		}
	}

	private void UpdateLobbyPlayersInfos()
	{
		if (!(Time.fixedTime > lastSend + SendRate))
		{
			return;
		}
		foreach (LobbyPlayer lobbyPlayer in LobbyPlayerList)
		{
			if (lobbyPlayer != null && !lobbyPlayer.isHost)
			{
				lobbyPlayer.Networkping = NetworkTransport.GetCurrentRTT(lobbyPlayer.connectionToClient.hostId, lobbyPlayer.connectionToClient.connectionId, out var _);
				lobbyPlayer.NetworkdownloadProgression = FileTransfer.GetDownloadProgressionFromConnection(lobbyPlayer.connectionToClient);
			}
		}
		lastSend = Time.fixedTime;
		if (SteamManager.Initialized && SteamManager.Instance.LobbyId.IsValid())
		{
			SteamMatchmaking.SetLobbyData(SteamManager.Instance.LobbyId, "playercount", LobbyPlayerList.Count.ToString());
		}
	}

	public bool IsFactionAssignedInLobby(int factionNumber)
	{
		return LobbyPlayerList.Slinq().Any((LobbyPlayer p, int i) => p.factionNumber == i, factionNumber);
	}

	public bool IsTeamAssignedInLobby(int teamNumber)
	{
		return LobbyPlayerList.Slinq().Any((LobbyPlayer p, int i) => p.TeamNumber == i, teamNumber);
	}

	public List<int> Lobby_AssignedFaction()
	{
		return (from p in LobbyPlayerList.Slinq()
			select p.factionNumber).Distinct().ToList();
	}

	public List<int> Lobby_AssignedTeam()
	{
		return (from p in LobbyPlayerList.Slinq()
			select p.TeamNumber).Distinct().ToList();
	}

	public bool CanLobbyPlayerSetReady(LobbyPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		return player.downloadProgression == -1f;
	}

	public override void OnLobbyClientAddPlayerFailed()
	{
		base.OnLobbyClientAddPlayerFailed();
		GameSignals.Invoke(ClientNewPlayerConnectFailed);
	}

	public override void OnLobbyClientConnect(NetworkConnection conn)
	{
		base.OnLobbyClientConnect(conn);
		GameSignals.Invoke(ClientNewPlayerConnected, conn);
	}

	public void OnFailedToConnect()
	{
		GameSignals.Invoke(ClientConnectFailedSelf);
	}

	public override void OnLobbyClientDisconnect(NetworkConnection conn)
	{
		base.OnLobbyClientDisconnect(conn);
		GameSignals.Invoke(ClientNewPlayerDisconnected, conn);
	}

	public override void OnLobbyClientEnter()
	{
		base.OnLobbyClientEnter();
		if (NetworkServer.active)
		{
			GameSignals.Invoke(ServerStarted);
		}
		GameSignals.Invoke(ClientConnectedSelf);
	}

	public override void OnLobbyClientExit()
	{
		base.OnLobbyClientExit();
	}

	public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
	{
		base.OnLobbyServerPlayerRemoved(conn, playerControllerId);
	}

	public override void OnLobbyStopHost()
	{
		base.OnLobbyStopHost();
		GameSignals.Invoke(ServerStopped);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);
		if (conn.lastError == NetworkError.Ok && !calledDisconnect)
		{
			SetNetworkError(NetworkError.Timeout);
		}
		else
		{
			SetNetworkError(conn.lastError);
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		SafeDisconnectConnection(conn);
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		StatsTracker.StopRecording();
		GameSignals.Invoke(Disconnected);
		GameSignals.Invoke(LateDisconnected);
	}

	public override void OnLobbyServerPlayersReady()
	{
		logger.Info("UNet : All players ready. Changing scene.");
		if (!LobbyPlayerList.TrueForAll((LobbyPlayer player) => player.readyToBegin))
		{
			return;
		}
		if (networkDiscovery.running)
		{
			networkDiscovery.StopBroadcast();
		}
		if (SteamManager.Initialized)
		{
			SteamMatchmaking.SetLobbyType(SteamManager.Instance.LobbyId, ELobbyType.k_ELobbyTypePrivate);
			SteamMatchmaking.LeaveLobby(SteamManager.Instance.LobbyId);
		}
		NetworkServer.SetAllClientsNotReady();
		ServerChangeScene(base.playScene);
		foreach (LobbyPlayer lobbyPlayer in LobbyPlayerList)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(base.playerPrefab);
			gameObject.GetComponent<User>().factionNumber = lobbyPlayer.factionNumber;
			gameObject.GetComponent<User>().NetworkTeamNumber = lobbyPlayer.TeamNumber;
			NetworkServer.ReplacePlayerForConnection(lobbyPlayer.connectionToClient, gameObject, 0);
			NetworkServer.SpawnWithClientAuthority(gameObject, lobbyPlayer.connectionToClient);
		}
		GameSignals.Invoke(ServerPlayersReady);
	}

	public override void OnClientSceneChanged(NetworkConnection conn)
	{
		if (!Neo.multiplayer)
		{
			base.OnClientSceneChanged(conn);
		}
		if (SceneManager.GetActiveScene().buildIndex == 1)
		{
			Disconnect();
		}
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		if (sceneName != base.playScene)
		{
			base.OnServerSceneChanged(sceneName);
		}
	}

	public override void OnLobbyStopClient()
	{
		base.OnLobbyStopClient();
		ResetToDefault();
	}

	public override void OnLobbyServerConnect(NetworkConnection conn)
	{
		base.OnLobbyServerConnect(conn);
		GameSignals.Invoke(ServerNewPlayerConnected, conn);
		if (base.maxConnections <= LobbyPlayerList.Count)
		{
			SafeDisconnectConnection(conn);
			logger.Warn("UNet : Someone tried to join but the maximum number of player has been reached.");
		}
		if (base.maxConnections == LobbyPlayerList.Count + 1 && SteamManager.Initialized)
		{
			SteamMatchmaking.SetLobbyType(SteamManager.Instance.LobbyId, ELobbyType.k_ELobbyTypePrivate);
		}
	}

	public override void OnLobbyServerDisconnect(NetworkConnection conn)
	{
		base.OnLobbyServerDisconnect(conn);
		GameSignals.Invoke(ServerPlayerDisconnected, conn);
		if (base.maxConnections > LobbyPlayerList.Count - 1 && SteamManager.Initialized)
		{
			SteamMatchmaking.SetLobbyType(SteamManager.Instance.LobbyId, GameVisibility);
		}
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		logger.Info("UNet : Server Started.");
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer(conn, playerControllerId);
		logger.InfoFormat("UNet : New player connected! Connection id : {0} ", conn.connectionId);
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
	{
		base.OnServerRemovePlayer(conn, player);
		logger.InfoFormat("UNet : Removed player connection id : {0} ", conn.connectionId);
	}

	public override void OnServerReady(NetworkConnection conn)
	{
		base.OnServerReady(conn);
		if (SceneManager.GetActiveScene().buildIndex == 2 && (bool)GetUserFromConnection(conn))
		{
			GetUserFromConnection(conn).NetworkReady = true;
		}
	}

	public override void OnServerError(NetworkConnection conn, int errorCode)
	{
		int t = conn?.connectionId ?? (-1);
		string t2 = Enum.GetName(typeof(NetworkError), errorCode);
		logger.ErrorFormat("UNet : Server error code({0}) for connectionId: {1}", t2, t);
		SetNetworkError((NetworkError)errorCode);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		string t = Enum.GetName(typeof(NetworkError), errorCode);
		logger.ErrorFormat("UNet : Client error code : {1}({0})", errorCode, t);
		SetNetworkError((NetworkError)errorCode);
	}

	public override GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
	{
		return UnityEngine.Object.Instantiate(base.gamePlayerPrefab);
	}

	public string GetLobbyPlayerNameFromConnection(NetworkConnection connection)
	{
		if (!LobbyPlayerList.Slinq().Any())
		{
			return "Player";
		}
		return LobbyPlayerList.Find((LobbyPlayer x) => x.connectionToClient.connectionId.Equals(connection.connectionId)).playerName;
	}

	public User GetUserFromConnection(NetworkConnection connection)
	{
		if (debugLoader)
		{
			return User.LocalUser;
		}
		return UserList.Find((User x) => x.connectionToClient.connectionId.Equals(connection.connectionId));
	}

	public void SetAllClientsAsNotReady()
	{
		LobbyPlayerList.Slinq().ForEach(delegate(LobbyPlayer c)
		{
			c.readyToBegin = false;
		});
	}

	public void KickPlayer(LobbyPlayer player)
	{
		logger.InfoFormat("UNet : Kicked player {0}.", player.playerName);
		string message = string.Format("<color=#{0}>{1}</color>", BaseMenu.HexCastleYellow, I2Helper.TryGet("##mainmenu_lobby_playerwaskicked").Replace("#x", player.playerName));
		LocalLobbyPlayer.CallRpcServerNewChatMessage(message);
		LocalLobbyPlayer.CallRpcPlayerIdKickedSignal((int)player.netId.Value);
	}

	private void OnApplicationQuit()
	{
		if (LocalNetworkClient != null)
		{
			LocalNetworkClient.Disconnect();
		}
		NetworkTransport.Shutdown();
	}
}
