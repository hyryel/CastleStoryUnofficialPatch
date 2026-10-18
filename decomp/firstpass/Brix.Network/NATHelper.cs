using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using Brix.Engine;
using Brix.Game;
using Brix.Game.Network;
using Brix.Utils.Logging;
using RakNet;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class NATHelper : MonoBehaviour
{
	public string facilitatorIP = "facilitator.castlestory.net";

	public ushort facilitatorPort = 61111;

	protected static BrixLogger logger = new BrixLogger(typeof(NATHelper).FullName);

	[NonSerialized]
	public string externalIP;

	[NonSerialized]
	public bool isReady;

	[NonSerialized]
	public string guid;

	private NatPunchthroughClient natPunchthroughClient;

	private SystemAddress facilitatorSystemAddress;

	private RakPeerInterface rakPeer;

	private UdpProxyPlugin udpProxy;

	private bool firstTimeConnect = true;

	private ushort listenPort;

	public string TargetGUID;

	private ushort natListenPort;

	private ushort natConnectPort;

	private string natConnectIp;

	public bool IsConnectedToFacilitator()
	{
		return rakPeer.GetConnectionState(facilitatorSystemAddress) == ConnectionState.IS_CONNECTED;
	}

	public void connectToNATFacilitator()
	{
		logger.Info("NatTraversal : Attempting connection to the facilitator.");
		StartCoroutine(connectToNATFacilitatorCoroutine());
	}

	private static ushort FreeTcpPort()
	{
		TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
		tcpListener.Start();
		ushort result = (ushort)((IPEndPoint)tcpListener.LocalEndpoint).Port;
		tcpListener.Stop();
		return result;
	}

	private IEnumerator connectToNATFacilitatorCoroutine()
	{
		isReady = false;
		if (rakPeer != null)
		{
			rakPeer.Shutdown(0u);
			RakPeerInterface.DestroyInstance(rakPeer);
		}
		rakPeer = RakPeerInterface.GetInstance();
		listenPort = FreeTcpPort();
		StartupResult startResult = rakPeer.Startup(2u, new SocketDescriptor(listenPort, string.Empty), 1u);
		rakPeer.SetMaximumIncomingConnections(2);
		if (startResult != StartupResult.RAKNET_STARTED)
		{
			logger.Warn("NatTraversal : Failed to initialize network interface: " + startResult);
			yield break;
		}
		ConnectionAttemptResult connectResult = rakPeer.Connect(facilitatorIP, facilitatorPort, null, 0);
		if (connectResult != ConnectionAttemptResult.CONNECTION_ATTEMPT_STARTED)
		{
			logger.Warn("NatTraversal : Failed to initialize connection to NAT Facilitator: " + connectResult);
			yield break;
		}
		Packet packet;
		while (true)
		{
			Packet packet2;
			packet = (packet2 = rakPeer.Receive());
			if (packet2 != null)
			{
				break;
			}
			yield return GameClock.WaitForEndOfFrame;
		}
		if (packet.data[0] != 16)
		{
			logger.Warn("NatTraversal : Failed to connect to NAT Facilitator: " + (DefaultMessageIDTypes)packet.data[0]);
			yield break;
		}
		guid = rakPeer.GetMyGUID().g.ToString();
		StartCoroutine(SetupSteamLobbyWhenReady());
		logger.Info("NatTraversal : Connected to facilitator: " + guid);
		facilitatorSystemAddress = new SystemAddress(packet.systemAddress.ToString());
		externalIP = rakPeer.GetExternalID(packet.systemAddress).ToString(writePort: false);
		if (firstTimeConnect)
		{
			firstTimeConnect = false;
		}
		natPunchthroughClient = new NatPunchthroughClient();
		rakPeer.AttachPlugin(natPunchthroughClient);
		natPunchthroughClient.FindRouterPortStride(facilitatorSystemAddress);
		isReady = true;
	}

	private IEnumerator SetupSteamLobbyWhenReady()
	{
		if (SteamManager.Initialized && NetworkServer.active)
		{
			while (!isReady)
			{
				yield return new WaitForSeconds(1f);
			}
			SteamMatchmaking.SetLobbyData(SteamManager.Instance.LobbyId, "hostExternalIP", UNetManager.instance.natHelper.externalIP);
			SteamMatchmaking.SetLobbyData(SteamManager.Instance.LobbyId, "hostInternalIP", UnityEngine.Network.player.ipAddress);
			SteamMatchmaking.SetLobbyData(SteamManager.Instance.LobbyId, "guid", UNetManager.instance.natHelper.guid);
			SteamMatchmaking.SetLobbyData(SteamManager.Instance.LobbyId, "name", UNetManager.instance.LocalPlayerName);
			SteamMatchmaking.SetLobbyData(SteamManager.Instance.LobbyId, "version", Neo.SemanticVersion);
		}
	}

	public void ListenForUdpProxyRequests(Action<int> onHolePunched)
	{
		udpProxy = new UdpProxyPlugin();
		udpProxy.ListenForRequests(rakPeer, onHolePunched);
	}

	public void ListenForPunchthrough(Action<int> onHolePunched)
	{
		rakPeer.SetMaximumIncomingConnections(2);
		StartCoroutine(waitForIncomingNATPunchThroughOnServer(onHolePunched));
	}

	private IEnumerator waitForIncomingNATPunchThroughOnServer(Action<int> onHolePunched)
	{
		ushort natListenPort = 0;
		while (NetworkServer.active)
		{
			yield return new WaitForEndOfFrame();
			while (!isReady)
			{
				yield return GameClock.WaitForEndOfFrame;
			}
			Packet packet = rakPeer.Receive();
			if (packet == null)
			{
				continue;
			}
			switch ((DefaultMessageIDTypes)packet.data[0])
			{
			case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_SUCCEEDED:
				if (packet.data[1] != 1)
				{
					logger.InfoFormat("NATPunch : Received punch through from {0} {1}", rakPeer.GetMyGUID(), packet.systemAddress);
				}
				break;
			case DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION:
				logger.Info("NatTraversal : Received raknet connection");
				natListenPort = listenPort;
				rakPeer.CloseConnection(packet.guid, sendDisconnectionNotification: true);
				rakPeer.CloseConnection(facilitatorSystemAddress, sendDisconnectionNotification: true);
				rakPeer.Shutdown(1000u);
				onHolePunched(natListenPort);
				StartCoroutine(connectToNATFacilitatorCoroutine());
				StartCoroutine(waitForIncomingNATPunchThroughOnServer(onHolePunched));
				yield break;
			default:
			{
				BrixLogger brixLogger = logger;
				DefaultMessageIDTypes defaultMessageIDTypes = (DefaultMessageIDTypes)packet.data[0];
				brixLogger.Info(defaultMessageIDTypes.ToString());
				break;
			}
			}
		}
	}

	public void TryNatTraversalToGuid(string hostGUIDString)
	{
		RakNetGUID hostGUID = new RakNetGUID(ulong.Parse(hostGUIDString));
		StartCoroutine(waitForResponseFromServer(hostGUID));
	}

	private IEnumerator waitForResponseFromServer(RakNetGUID hostGUID)
	{
		while (!isReady)
		{
			yield return GameClock.WaitForEndOfFrame;
		}
		udpProxy = new UdpProxyPlugin();
		natPunchthroughClient.OpenNAT(hostGUID, facilitatorSystemAddress);
		while (true)
		{
			Packet packet = rakPeer.Receive();
			if (packet != null)
			{
				switch ((DefaultMessageIDTypes)packet.data[0])
				{
				case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_SUCCEEDED:
					natConnectPort = packet.systemAddress.GetPort();
					rakPeer.Connect(packet.systemAddress.ToString(writePort: false), natConnectPort, string.Empty, 0);
					logger.InfoFormat("NatTraversal : Server confirmed punchthrough on port {0}", natConnectPort);
					break;
				case DefaultMessageIDTypes.ID_CONNECTION_REQUEST_ACCEPTED:
					natConnectPort = packet.systemAddress.GetPort();
					natListenPort = listenPort;
					natConnectIp = packet.systemAddress.ToString(writePort: false);
					logger.InfoFormat("NatTraversal : Server confirmed listen port {0}", natListenPort);
					break;
				case DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
				case DefaultMessageIDTypes.ID_CONNECTION_LOST:
					logger.Info("NatTraversal : Disconnected from raknet.");
					rakPeer.CloseConnection(packet.systemAddress, sendDisconnectionNotification: false);
					rakPeer.CloseConnection(facilitatorSystemAddress, sendDisconnectionNotification: false);
					rakPeer.Shutdown(1000u);
					isReady = false;
					UNetManager.instance.ConnectToServerViaRelay(natConnectIp, natConnectPort, natListenPort);
					yield break;
				case DefaultMessageIDTypes.ID_NAT_PUNCHTHROUGH_FAILED:
					udpProxy = new UdpProxyPlugin();
					udpProxy.RequestForwarding(rakPeer, facilitatorSystemAddress, hostGUID);
					break;
				default:
					logger.WarnFormat("NatTraversal : RakNet Client received unexpected message type: {0}", (DefaultMessageIDTypes)packet.data[0]);
					break;
				}
			}
			yield return GameClock.WaitForEndOfFrame;
		}
	}

	public void Shutdown()
	{
		if (rakPeer != null)
		{
			rakPeer.Shutdown(0u);
		}
	}

	private void OnApplicationQuit()
	{
		if (rakPeer != null)
		{
			rakPeer.Shutdown(0u);
		}
	}
}
