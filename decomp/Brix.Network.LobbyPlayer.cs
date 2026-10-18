using System;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Game;
using Brix.Game.Network;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Logging;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class LobbyPlayer : NetworkLobbyPlayer
{
	protected static BrixLogger logger;

	[SyncVar(hook = "OnNameChange")]
	public string playerName;

	[SyncVar(hook = "downloadProgressionChanged")]
	public float downloadProgression = -1f;

	[SyncVar]
	public bool isHost;

	[SyncVar]
	public int ping;

	[SyncVar(hook = "factionNumber_hook")]
	public int factionNumber;

	[SyncVar(hook = "teamNumber_hook")]
	public int TeamNumber;

	[SyncVar(hook = "onSetIsHostReady")]
	public bool IsHostReady;

	[SyncVar(hook = "SteamId_hook")]
	private ulong _steamID;

	private Texture2D smallAvatar;

	public CSteamID SteamID;

	public bool RequestedMapDownload;

	private Texture2D _tmpAvatar;

	private const int TmpBufferSize = 4096;

	private readonly byte[] _tmpTextureBuffer = new byte[4096];

	private static readonly Color[] TmpColor;

	private static int kCmdCmdClaimSteamID;

	private static int kCmdCmdSendChatMessage;

	private static int kRpcRpcNewChatMessage;

	private static int kCmdCmdSetPlayerName;

	private static int kCmdCmdSetDownloadRequest;

	private static int kRpcRpcPlayerIdKickedSignal;

	private static int kRpcRpcPlayerKickedSignal;

	private static int kRpcRpcServerNewChatMessage;

	private static int kCmdCmdSetFaction;

	private static int kCmdCmdSetTeam;

	public bool IsDownloading => UNetManager.instance.FileTransfer.IsDownloading;

	public string NetworkplayerName
	{
		get
		{
			return playerName;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref playerName;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnNameChange(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public float NetworkdownloadProgression
	{
		get
		{
			return downloadProgression;
		}
		[param: In]
		set
		{
			ref float fieldValue = ref downloadProgression;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				downloadProgressionChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	public bool NetworkisHost
	{
		get
		{
			return isHost;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isHost, 4u);
		}
	}

	public int Networkping
	{
		get
		{
			return ping;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref ping, 8u);
		}
	}

	public int NetworkfactionNumber
	{
		get
		{
			return factionNumber;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref factionNumber;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				factionNumber_hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 16u);
		}
	}

	public int NetworkTeamNumber
	{
		get
		{
			return TeamNumber;
		}
		[param: In]
		set
		{
			ref int teamNumber = ref TeamNumber;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				teamNumber_hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref teamNumber, 32u);
		}
	}

	public bool NetworkIsHostReady
	{
		get
		{
			return IsHostReady;
		}
		[param: In]
		set
		{
			ref bool isHostReady = ref IsHostReady;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				onSetIsHostReady(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref isHostReady, 64u);
		}
	}

	public ulong Network_steamID
	{
		get
		{
			return _steamID;
		}
		[param: In]
		set
		{
			ref ulong steamID = ref _steamID;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SteamId_hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref steamID, 128u);
		}
	}

	public Texture2D GetSmallSteamAvatar()
	{
		return smallAvatar;
	}

	public string GetSteamName()
	{
		if (SteamManager.Initialized)
		{
			return SteamFriends.GetFriendPersonaName(SteamID);
		}
		return string.Empty;
	}

	public void OpenSteamProfile()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayToUser("steamid", SteamID);
		}
	}

	public static void OpenInviteWindow()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(SteamManager.Instance.LobbyId);
		}
	}

	public void Start()
	{
		_tmpAvatar = new Texture2D(32, 32, TextureFormat.RGBA32, mipmap: false, linear: false);
		if (NetworkServer.active)
		{
			NetworkfactionNumber = Faction.GetFirstAvalaibleFaction();
			if (GameParam.Preset != null && GameParam.Preset.settings.ContainsKey("playerRelations") && (long)GameParam.Preset.settings["playerRelations"] == 2)
			{
				NetworkTeamNumber = Faction.GetFirstAvalaibleTeam();
			}
		}
		UNetManager.instance.LobbyPlayerList.Add(this);
		GameParam.instance.ReadAndApplyNetworkParams();
		if (!Neo.multiplayer)
		{
			SendReadyToBeginMessage();
		}
		if (SteamManager.Initialized && SteamID.IsValid())
		{
			SteamFriends.SetPlayedWith(SteamID);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		SteamID = new CSteamID(_steamID);
		RefreshSteamAvatar();
	}

	private void OnDestroy()
	{
		if (NetworkServer.active && !string.IsNullOrEmpty(playerName))
		{
			string message = string.Format("<color=#{0}>{1}</color>", BaseMenu.HexCastleYellow, I2Helper.TryGet("##mainmenu_multiplayer_playerleft").Replace("#x", playerName));
			UNetManager.instance.LocalLobbyPlayer.CallRpcServerNewChatMessage(message);
		}
	}

	public void OnNameChange(string dstName)
	{
		string text = playerName;
		NetworkplayerName = dstName;
		base.gameObject.name = dstName;
		GameSignals.Invoke(UNetManager.LobbyPlayerJoined, this);
		GameSignals.Invoke(UNetManager.PlayerListChanged);
		if (!string.IsNullOrEmpty(text) && text != dstName)
		{
			string message = string.Format("<color=#{0}>{1}</color>", BaseMenu.HexCastleYellow, I2Helper.TryGet("##mainmenu_multiplayer_playerrenamed").Replace("#x2", text).Replace("#x1", dstName));
			UNetManager.instance.LocalLobbyPlayer.CallRpcServerNewChatMessage(message);
		}
	}

	private void onSetIsHostReady(bool value)
	{
		NetworkIsHostReady = value;
		if (NetworkServer.active && SteamManager.Initialized)
		{
			SteamMatchmaking.SetLobbyType(SteamManager.Instance.LobbyId, (!value) ? UNetManager.instance.GameVisibility : ELobbyType.k_ELobbyTypePrivate);
		}
		GameSignals.Invoke(UNetManager.OnSetIsHostReady);
	}

	private void factionNumber_hook(int value)
	{
		NetworkfactionNumber = value;
		GameSignals.Invoke(UNetManager.PlayerListChanged);
	}

	private void teamNumber_hook(int value)
	{
		NetworkTeamNumber = value;
		GameSignals.Invoke(UNetManager.PlayerListChanged);
	}

	private void SteamId_hook(ulong steamid)
	{
		SteamID = new CSteamID(steamid);
		RefreshSteamAvatar();
	}

	private void RefreshSteamAvatar()
	{
		if (SteamManager.Initialized && SteamID.IsValid())
		{
			if (_tmpAvatar == null)
			{
				_tmpAvatar = new Texture2D(32, 32, TextureFormat.RGBA32, mipmap: false, linear: false);
			}
			SteamUtils.GetImageRGBA(SteamFriends.GetSmallFriendAvatar(SteamID), _tmpTextureBuffer, 4096);
			_tmpAvatar.LoadRawTextureData(_tmpTextureBuffer);
			_tmpAvatar.Apply();
			Flip(_tmpAvatar);
			smallAvatar = _tmpAvatar;
			GameSignals.Invoke(UNetManager.LobbyRefresh);
		}
	}

	private static void Flip(Texture2D tex)
	{
		Color[] pixels = tex.GetPixels();
		Color[] tmpColor = TmpColor;
		for (int i = 0; i < tex.height; i++)
		{
			Array.Copy(pixels, i * tex.width, tmpColor, (tex.height - 1 - i) * tex.width, tex.width);
		}
		tex.SetPixels(tmpColor);
		tex.Apply();
	}

	private void downloadProgressionChanged(float value)
	{
		bool isDownloading = IsDownloading;
		NetworkdownloadProgression = value;
		if (base.hasAuthority && !IsDownloading)
		{
			GameParam.instance.ReadAndApplyNetworkParams();
		}
		if (isDownloading != IsDownloading)
		{
			if (IsDownloading)
			{
				GameSignals.Invoke(UNetManager.OnPlayerDownloadStart);
			}
			else
			{
				GameSignals.Invoke(UNetManager.OnPlayerDownloadEnd);
			}
		}
	}

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();
		UNetManager.instance.LocalLobbyPlayer = this;
		if (string.IsNullOrEmpty(UNetManager.instance.LocalPlayerName))
		{
			UNetManager.instance.LocalPlayerName = ((!string.IsNullOrEmpty(Neo.multiplayerName)) ? Neo.multiplayerName : I2Helper.TryGet("##gamemenu_player"));
		}
		CallCmdSetPlayerName(UNetManager.instance.LocalPlayerName);
		if (SteamManager.Initialized)
		{
			CallCmdClaimSteamID(SteamUser.GetSteamID().m_SteamID);
		}
		NetworkisHost = NetworkServer.active;
		if (!NetworkServer.active)
		{
			GameParam.RefreshFactionsAndTeams();
		}
		GameSignals.Invoke(UNetManager.OnLocalPlayerReadySignal);
	}

	public override void OnClientReady(bool readyState)
	{
		base.OnClientReady(readyState);
		GameSignals.Invoke(UNetManager.OnAnyToggleReadySignal);
	}

	[Command]
	public void CmdClaimSteamID(ulong id)
	{
		Network_steamID = id;
	}

	[Command]
	public void CmdSendChatMessage(string message)
	{
		CallRpcNewChatMessage(base.gameObject, message);
	}

	[ClientRpc]
	public void RpcNewChatMessage(GameObject lobbyPlayerSource, string message)
	{
		string arg = $"<color=#{BaseMenu.HexCastleBrown}>[{$"{DateTime.Now:HH:mm:ss}"}]</color>";
		string arg2 = $"<color=#{BaseMenu.HexCastleBrown}>{lobbyPlayerSource.GetComponent<LobbyPlayer>().playerName}:</color>";
		string t = $"{arg} {arg2} {message}";
		GameSignals.Invoke(UNetManager.LobbyNewChatMessage, t);
	}

	[Command]
	public void CmdSetPlayerName(string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			if (string.IsNullOrEmpty(playerName))
			{
				string message = string.Format("<color=#{0}>{1}</color>", BaseMenu.HexCastleYellow, I2Helper.TryGet("##mainmenu_multiplayer_playerjoined").Replace("#x", name));
				UNetManager.instance.LocalLobbyPlayer.CallRpcServerNewChatMessage(message);
			}
			int length = ((name.Length < 30) ? name.Length : 30);
			name = name.Substring(0, length);
			NetworkplayerName = name;
			base.gameObject.name = name;
			if (string.IsNullOrEmpty(playerName))
			{
				string message2 = string.Format("<color=#{0}>{1}</color>", BaseMenu.HexCastleYellow, I2Helper.TryGet("##mainmenu_multiplayer_playerjoined").Replace("#x", name));
				UNetManager.instance.LocalLobbyPlayer.CallRpcServerNewChatMessage(message2);
			}
			if (UNetManager.instance.networkDiscovery.running && NetworkServer.active)
			{
				UNetManager.instance.networkDiscovery.broadcastData = UNetManager.instance.LocalPlayerName;
			}
		}
	}

	[Command]
	public void CmdSetDownloadRequest(bool value)
	{
		RequestedMapDownload = value;
	}

	[ClientRpc]
	public void RpcPlayerIdKickedSignal(int id)
	{
		if (id == UNetManager.instance.LocalLobbyPlayer.netId.Value)
		{
			UNetManager.SetBrixNetworkError(UNetManager.BrixNetworkError.PlayerKicked);
		}
	}

	[ClientRpc]
	public void RpcPlayerKickedSignal()
	{
		GameSignals.Invoke(UNetManager.ServerPlayerKicked, this);
	}

	[ClientRpc]
	public void RpcServerNewChatMessage(string message)
	{
		GameSignals.Invoke(UNetManager.LobbyNewChatMessage, message);
	}

	[Command]
	public void CmdSetFaction(int faction)
	{
		if (GameParam.Map.IsNewGame())
		{
			int maxFactionCount = GameParam.MaxFactionCount;
			if (UNetManager.instance.Lobby_AssignedFaction().Count < maxFactionCount || UNetManager.instance.Lobby_AssignedFaction().Contains(faction) || Faction.LobbyPlayerCountForFaction(factionNumber) == 1)
			{
				NetworkfactionNumber = faction;
			}
		}
		else
		{
			NetworkfactionNumber = faction;
		}
	}

	[Command]
	public void CmdSetTeam(int team)
	{
		NetworkTeamNumber = team;
	}

	public void OnDisable()
	{
		UNetManager.instance.LobbyPlayerList.Remove(this);
		GameSignals.Invoke(UNetManager.LobbyPlayerLeft, this);
		GameSignals.Invoke(UNetManager.PlayerListChanged);
		UnityEngine.Object.Destroy(_tmpAvatar);
		UnityEngine.Object.Destroy(smallAvatar);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	static LobbyPlayer()
	{
		logger = new BrixLogger(typeof(LobbyPlayer).FullName);
		TmpColor = new Color[1024];
		kCmdCmdClaimSteamID = 117193894;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdClaimSteamID, InvokeCmdCmdClaimSteamID);
		kCmdCmdSendChatMessage = -833841296;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSendChatMessage, InvokeCmdCmdSendChatMessage);
		kCmdCmdSetPlayerName = -535136297;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetPlayerName, InvokeCmdCmdSetPlayerName);
		kCmdCmdSetDownloadRequest = 435017980;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetDownloadRequest, InvokeCmdCmdSetDownloadRequest);
		kCmdCmdSetFaction = 2085961521;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetFaction, InvokeCmdCmdSetFaction);
		kCmdCmdSetTeam = 1483423080;
		NetworkBehaviour.RegisterCommandDelegate(typeof(LobbyPlayer), kCmdCmdSetTeam, InvokeCmdCmdSetTeam);
		kRpcRpcNewChatMessage = 169367932;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcNewChatMessage, InvokeRpcRpcNewChatMessage);
		kRpcRpcPlayerIdKickedSignal = -1663272362;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcPlayerIdKickedSignal, InvokeRpcRpcPlayerIdKickedSignal);
		kRpcRpcPlayerKickedSignal = -46310117;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcPlayerKickedSignal, InvokeRpcRpcPlayerKickedSignal);
		kRpcRpcServerNewChatMessage = -808964641;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LobbyPlayer), kRpcRpcServerNewChatMessage, InvokeRpcRpcServerNewChatMessage);
		NetworkCRC.RegisterBehaviour("LobbyPlayer", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdClaimSteamID(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClaimSteamID called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdClaimSteamID(reader.ReadPackedUInt64());
		}
	}

	protected static void InvokeCmdCmdSendChatMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChatMessage called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSendChatMessage(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetPlayerName(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetPlayerName called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetPlayerName(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetDownloadRequest(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDownloadRequest called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetDownloadRequest(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetFaction(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetFaction called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetFaction((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSetTeam(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTeam called on client.");
		}
		else
		{
			((LobbyPlayer)obj).CmdSetTeam((int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdClaimSteamID(ulong id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdClaimSteamID called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdClaimSteamID(id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdClaimSteamID);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt64(id);
		SendCommandInternal(networkWriter, 0, "CmdClaimSteamID");
	}

	public void CallCmdSendChatMessage(string message)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendChatMessage called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendChatMessage(message);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendChatMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(message);
		SendCommandInternal(networkWriter, 0, "CmdSendChatMessage");
	}

	public void CallCmdSetPlayerName(string name)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetPlayerName called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetPlayerName(name);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetPlayerName);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(name);
		SendCommandInternal(networkWriter, 0, "CmdSetPlayerName");
	}

	public void CallCmdSetDownloadRequest(bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetDownloadRequest called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetDownloadRequest(value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetDownloadRequest);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetDownloadRequest");
	}

	public void CallCmdSetFaction(int faction)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetFaction called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetFaction(faction);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetFaction);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)faction);
		SendCommandInternal(networkWriter, 0, "CmdSetFaction");
	}

	public void CallCmdSetTeam(int team)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTeam called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTeam(team);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTeam);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)team);
		SendCommandInternal(networkWriter, 0, "CmdSetTeam");
	}

	protected static void InvokeRpcRpcNewChatMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNewChatMessage called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcNewChatMessage(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcPlayerIdKickedSignal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayerIdKickedSignal called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcPlayerIdKickedSignal((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcPlayerKickedSignal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayerKickedSignal called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcPlayerKickedSignal();
		}
	}

	protected static void InvokeRpcRpcServerNewChatMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcServerNewChatMessage called on server.");
		}
		else
		{
			((LobbyPlayer)obj).RpcServerNewChatMessage(reader.ReadString());
		}
	}

	public void CallRpcNewChatMessage(GameObject lobbyPlayerSource, string message)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcNewChatMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcNewChatMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(lobbyPlayerSource);
		networkWriter.Write(message);
		SendRPCInternal(networkWriter, 0, "RpcNewChatMessage");
	}

	public void CallRpcPlayerIdKickedSignal(int id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayerIdKickedSignal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayerIdKickedSignal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)id);
		SendRPCInternal(networkWriter, 0, "RpcPlayerIdKickedSignal");
	}

	public void CallRpcPlayerKickedSignal()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayerKickedSignal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayerKickedSignal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcPlayerKickedSignal");
	}

	public void CallRpcServerNewChatMessage(string message)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcServerNewChatMessage called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcServerNewChatMessage);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(message);
		SendRPCInternal(networkWriter, 0, "RpcServerNewChatMessage");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(playerName);
			writer.Write(downloadProgression);
			writer.Write(isHost);
			writer.WritePackedUInt32((uint)ping);
			writer.WritePackedUInt32((uint)factionNumber);
			writer.WritePackedUInt32((uint)TeamNumber);
			writer.Write(IsHostReady);
			writer.WritePackedUInt64(_steamID);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(playerName);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(downloadProgression);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(isHost);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)ping);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)factionNumber);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)TeamNumber);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(IsHostReady);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt64(_steamID);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			playerName = reader.ReadString();
			downloadProgression = reader.ReadSingle();
			isHost = reader.ReadBoolean();
			ping = (int)reader.ReadPackedUInt32();
			factionNumber = (int)reader.ReadPackedUInt32();
			TeamNumber = (int)reader.ReadPackedUInt32();
			IsHostReady = reader.ReadBoolean();
			_steamID = reader.ReadPackedUInt64();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnNameChange(reader.ReadString());
		}
		if ((num & 2) != 0)
		{
			downloadProgressionChanged(reader.ReadSingle());
		}
		if ((num & 4) != 0)
		{
			isHost = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			ping = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x10) != 0)
		{
			factionNumber_hook((int)reader.ReadPackedUInt32());
		}
		if ((num & 0x20) != 0)
		{
			teamNumber_hook((int)reader.ReadPackedUInt32());
		}
		if ((num & 0x40) != 0)
		{
			onSetIsHostReady(reader.ReadBoolean());
		}
		if ((num & 0x80) != 0)
		{
			SteamId_hook(reader.ReadPackedUInt64());
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
