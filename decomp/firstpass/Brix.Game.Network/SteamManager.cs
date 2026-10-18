using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Brix.External.Signals;
using Brix.Network;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using Network;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Brix.Game.Network;

public class SteamManager : MonoBehaviour
{
	public Signal<IEnumerable<MatchList.MatchData>> LobbyMatchListReceivedSignal = new Signal<IEnumerable<MatchList.MatchData>>();

	private static BrixLogger logger = new BrixLogger(typeof(SteamManager).FullName);

	private CSteamID lobbyID;

	private static SteamManager s_instance;

	private bool m_bInitialized;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	protected Callback<LobbyEnter_t> m_GameLobbyEntered;

	protected Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;

	protected Callback<LobbyGameCreated_t> m_GameCreated;

	protected Callback<ClientGameServerDeny_t> m_GameDenied;

	protected Callback<LobbyCreated_t> m_LobbyCreated;

	protected Callback<LobbyMatchList_t> m_LobbyMatchListReceived;

	public CSteamID LobbyId => lobbyID;

	public static SteamManager Instance => s_instance;

	public static bool Initialized
	{
		get
		{
			if (Instance == null)
			{
				return false;
			}
			return Instance.m_bInitialized;
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		logger.Warn(pchDebugText);
	}

	private void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (!Packsize.Test())
		{
			logger.ErrorFormat("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform. {0}", this);
		}
		if (!DllCheck.Test())
		{
			logger.ErrorFormat("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version. {0}", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException t)
		{
			logger.ErrorFormat("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n {0} \n {1}", t, this);
			Application.Quit();
			return;
		}
		if (!SteamAPI.IsSteamRunning())
		{
			logger.InfoFormat("[Steamworks.NET] Steam is not running. The steamworks API won't be used.\n({0})", this);
		}
		else if (!(m_bInitialized = SteamAPI.Init()))
		{
			logger.ErrorFormat("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information. {0}", this);
		}
	}

	private void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized)
		{
			if (Initialized)
			{
				m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
				m_GameLobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
				m_LobbyCreated = Callback<LobbyCreated_t>.Create(LobbyCreated);
				m_LobbyMatchListReceived = Callback<LobbyMatchList_t>.Create(LobbyMatchListReceived);
			}
			if (m_SteamAPIWarningMessageHook == null)
			{
				m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
				SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
			}
		}
	}

	private void LobbyMatchListReceived(LobbyMatchList_t param)
	{
		List<MatchList.MatchData> list = new List<MatchList.MatchData>();
		for (int i = 0; i < param.m_nLobbiesMatching; i++)
		{
			MatchList.MatchData item = default(MatchList.MatchData);
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
			item.FacilitatorGUID = SteamMatchmaking.GetLobbyData(lobbyByIndex, "guid");
			item.Address = new MatchAddress(SteamMatchmaking.GetLobbyData(lobbyByIndex, "hostInternalIP"), 3000);
			item.IsLanGame = false;
			item.LastUpdateTimestamp = Time.realtimeSinceStartup;
			item.LobbySteamID = lobbyByIndex;
			int.TryParse(SteamMatchmaking.GetLobbyData(lobbyByIndex, "playercount"), out item.PlayerCount);
			item.MatchName = SteamMatchmaking.GetLobbyData(lobbyByIndex, "name");
			item.Version = SteamMatchmaking.GetLobbyData(lobbyByIndex, "version");
			list.Add(item);
		}
		LobbyMatchListReceivedSignal.Invoke(list);
	}

	private void LobbyCreated(LobbyCreated_t param)
	{
		lobbyID.m_SteamID = param.m_ulSteamIDLobby;
	}

	private void OnLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
	{
		if (!NetworkClient.active)
		{
			SteamMatchmaking.LeaveLobby(Instance.LobbyId);
			SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
			SceneManager.LoadScene(UNetManager.instance.offlineScene);
		}
		else
		{
			logger.Info("Join game request received while already in a game, ignoring.");
		}
	}

	public static uint IpToUInt(string ipString)
	{
		IPAddress iPAddress = IPAddress.Parse(ipString);
		byte[] addressBytes = iPAddress.GetAddressBytes();
		uint num = (uint)(addressBytes[3] << 24);
		num += (uint)(addressBytes[2] << 16);
		num += (uint)(addressBytes[1] << 8);
		return num + addressBytes[0];
	}

	private void OnLobbyEntered(LobbyEnter_t pCallback)
	{
		lobbyID.m_SteamID = pCallback.m_ulSteamIDLobby;
		if (SteamMatchmaking.GetLobbyOwner(lobbyID) != SteamUser.GetSteamID() && !NetworkClient.active)
		{
			MatchList.MatchData matchData = new MatchList.MatchData
			{
				LobbySteamID = lobbyID
			};
			UNetManager.instance.ConnectToServerWithMatchData(matchData);
			NetworkWaitingModalMenu.OpenAsClientSignal.Invoke();
		}
	}

	private void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				SteamAPI.Shutdown();
			}
		}
	}

	private void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}
}
