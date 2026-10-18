using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Lua;
using Brix.Network;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game;

public class User : NetworkBehaviour
{
	[LuaDrivable("localUser")]
	public static User LocalUser;

	[SyncVar]
	[LuaDrivable("name")]
	private string _screenName;

	public bool NetworkReady;

	public bool TerrainLoaded;

	public bool FactionReady;

	public bool LevelReady;

	[SyncVar]
	public string twitchName = string.Empty;

	[SyncVar]
	public int TeamNumber;

	[SyncVar(hook = "factionSet")]
	public GameObject DefaultAlliedFactionGO;

	public int factionNumber;

	private NetworkInstanceId ___DefaultAlliedFactionGONetId;

	private static int kCmdCmdSetTwitchName;

	private static int kCmdCmdSendChatMessage;

	private static int kRpcRpcNewChatMessage;

	private static int kCmdCmdSetTerrainLoaded;

	private static int kCmdCmdJoinSpectator;

	private static int kCmdCmdSetFactionReady;

	private static int kCmdCmdSetLevelReady;

	public string ScreenName => _screenName;

	[LuaDrivable("alliedFaction")]
	public Faction faction
	{
		get
		{
			if (DefaultAlliedFactionGO != null)
			{
				return DefaultAlliedFactionGO.GetComponent<Faction>();
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				NetworkDefaultAlliedFactionGO = value.gameObject;
			}
			else
			{
				NetworkDefaultAlliedFactionGO = null;
			}
		}
	}

	public string Network_screenName
	{
		get
		{
			return _screenName;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _screenName, 1u);
		}
	}

	public string NetworktwitchName
	{
		get
		{
			return twitchName;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref twitchName, 2u);
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
			SetSyncVar(value, ref TeamNumber, 4u);
		}
	}

	public GameObject NetworkDefaultAlliedFactionGO
	{
		get
		{
			return DefaultAlliedFactionGO;
		}
		[param: In]
		set
		{
			ref GameObject defaultAlliedFactionGO = ref DefaultAlliedFactionGO;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				factionSet(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref defaultAlliedFactionGO, 8u, ref ___DefaultAlliedFactionGONetId);
		}
	}

	[Command]
	public void CmdSetTwitchName(string dstTwitchName)
	{
		NetworktwitchName = dstTwitchName;
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		if (NetworkServer.active)
		{
			Network_screenName = UNetManager.instance.GetLobbyPlayerNameFromConnection(base.connectionToClient);
			_screenName.Normalize();
		}
		UNetManager.instance.UserList.Add(this);
	}

	public void OnDisable()
	{
		GameSignals.Invoke(UNetManager.InGamePlayerLeft, this);
		UNetManager.instance.UserList.Remove(this);
		Object.Destroy(base.gameObject);
	}

	public override void OnStartAuthority()
	{
		LocalUser = this;
	}

	[Command]
	public void CmdSendChatMessage(string message)
	{
		CallRpcNewChatMessage(base.gameObject, message);
	}

	[ClientRpc]
	public void RpcNewChatMessage(GameObject lobbyPlayerSource, string message)
	{
		bool flag = !string.IsNullOrEmpty(LocalUser.twitchName);
		bool flag2 = !string.IsNullOrEmpty(twitchName);
		if (!flag || !flag2)
		{
			User component = lobbyPlayerSource.GetComponent<User>();
			Color factionColor = component.faction.FactionColor;
			string text = $"#{(int)(factionColor.r * 255f):X2}{(int)(factionColor.g * 255f):X2}{(int)(factionColor.b * 255f):X2}";
			string text2 = "<color=" + text + ">" + component.ScreenName + "</color>";
			text2 = ((!string.IsNullOrEmpty(component.twitchName)) ? (twitchName + "(" + text2 + ")") : text2);
			GameSignals.Invoke(UNetManager.LobbyNewChatMessage, text2 + " : " + message);
		}
	}

	[Command]
	public void CmdSetTerrainLoaded()
	{
		TerrainLoaded = true;
	}

	[LuaDrivable("JoinSpectators")]
	public static DynValue JoinSpectator(ScriptExecutionContext context, CallbackArguments args)
	{
		User user = args.AsComponent<User>(0, "JoinSpectators");
		user.CallCmdJoinSpectator();
		return DynValue.Void;
	}

	[Command]
	public void CmdJoinSpectator()
	{
		faction = Faction.Spectator;
	}

	private void factionSet(GameObject faction)
	{
		NetworkDefaultAlliedFactionGO = faction;
		if (base.hasAuthority)
		{
			CallCmdSetFactionReady();
		}
		if (NetworkServer.active && GameSignals.Received(GameSignals.LevelReady))
		{
			Neo.Relais(delegate
			{
				LocalFactionVision.RefreshAllObservers();
			}, 1f, "RefreshAllObservers");
		}
	}

	[Command]
	public void CmdSetFactionReady()
	{
		FactionReady = true;
	}

	[Command]
	public void CmdSetLevelReady()
	{
		LevelReady = true;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetTwitchName(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTwitchName called on client.");
		}
		else
		{
			((User)obj).CmdSetTwitchName(reader.ReadString());
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
			((User)obj).CmdSendChatMessage(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetTerrainLoaded(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTerrainLoaded called on client.");
		}
		else
		{
			((User)obj).CmdSetTerrainLoaded();
		}
	}

	protected static void InvokeCmdCmdJoinSpectator(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdJoinSpectator called on client.");
		}
		else
		{
			((User)obj).CmdJoinSpectator();
		}
	}

	protected static void InvokeCmdCmdSetFactionReady(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetFactionReady called on client.");
		}
		else
		{
			((User)obj).CmdSetFactionReady();
		}
	}

	protected static void InvokeCmdCmdSetLevelReady(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLevelReady called on client.");
		}
		else
		{
			((User)obj).CmdSetLevelReady();
		}
	}

	public void CallCmdSetTwitchName(string dstTwitchName)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTwitchName called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTwitchName(dstTwitchName);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTwitchName);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(dstTwitchName);
		SendCommandInternal(networkWriter, 0, "CmdSetTwitchName");
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

	public void CallCmdSetTerrainLoaded()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTerrainLoaded called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTerrainLoaded();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTerrainLoaded);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSetTerrainLoaded");
	}

	public void CallCmdJoinSpectator()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdJoinSpectator called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdJoinSpectator();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdJoinSpectator);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdJoinSpectator");
	}

	public void CallCmdSetFactionReady()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetFactionReady called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetFactionReady();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetFactionReady);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSetFactionReady");
	}

	public void CallCmdSetLevelReady()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetLevelReady called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetLevelReady();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetLevelReady);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSetLevelReady");
	}

	protected static void InvokeRpcRpcNewChatMessage(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNewChatMessage called on server.");
		}
		else
		{
			((User)obj).RpcNewChatMessage(reader.ReadGameObject(), reader.ReadString());
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

	static User()
	{
		kCmdCmdSetTwitchName = 313912981;
		NetworkBehaviour.RegisterCommandDelegate(typeof(User), kCmdCmdSetTwitchName, InvokeCmdCmdSetTwitchName);
		kCmdCmdSendChatMessage = 1729519420;
		NetworkBehaviour.RegisterCommandDelegate(typeof(User), kCmdCmdSendChatMessage, InvokeCmdCmdSendChatMessage);
		kCmdCmdSetTerrainLoaded = -486627677;
		NetworkBehaviour.RegisterCommandDelegate(typeof(User), kCmdCmdSetTerrainLoaded, InvokeCmdCmdSetTerrainLoaded);
		kCmdCmdJoinSpectator = 1265246842;
		NetworkBehaviour.RegisterCommandDelegate(typeof(User), kCmdCmdJoinSpectator, InvokeCmdCmdJoinSpectator);
		kCmdCmdSetFactionReady = 1664370878;
		NetworkBehaviour.RegisterCommandDelegate(typeof(User), kCmdCmdSetFactionReady, InvokeCmdCmdSetFactionReady);
		kCmdCmdSetLevelReady = 1052537366;
		NetworkBehaviour.RegisterCommandDelegate(typeof(User), kCmdCmdSetLevelReady, InvokeCmdCmdSetLevelReady);
		kRpcRpcNewChatMessage = 944793648;
		NetworkBehaviour.RegisterRpcDelegate(typeof(User), kRpcRpcNewChatMessage, InvokeRpcRpcNewChatMessage);
		NetworkCRC.RegisterBehaviour("User", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_screenName);
			writer.Write(twitchName);
			writer.WritePackedUInt32((uint)TeamNumber);
			writer.Write(DefaultAlliedFactionGO);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_screenName);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(twitchName);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)TeamNumber);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(DefaultAlliedFactionGO);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			_screenName = reader.ReadString();
			twitchName = reader.ReadString();
			TeamNumber = (int)reader.ReadPackedUInt32();
			___DefaultAlliedFactionGONetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			_screenName = reader.ReadString();
		}
		if ((num & 2) != 0)
		{
			twitchName = reader.ReadString();
		}
		if ((num & 4) != 0)
		{
			TeamNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
		{
			factionSet(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		if (!___DefaultAlliedFactionGONetId.IsEmpty())
		{
			NetworkDefaultAlliedFactionGO = ClientScene.FindLocalObject(___DefaultAlliedFactionGONetId);
		}
	}
}
