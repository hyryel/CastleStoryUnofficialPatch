using System;
using Brix.Utils.Logging;
using RakNet;

namespace Brix.Network;

public class UdpProxyPlugin : UDPProxyClientResultHandler
{
	private UDPProxyClient _udpProxyClient;

	private static BrixLogger logger = new BrixLogger(typeof(UdpProxyPlugin).FullName);

	private RakPeerInterface _rakPeer;

	public string ProxyIp;

	public int ProxyPort;

	public void RequestForwarding(RakPeerInterface rakPeer, SystemAddress facilitatorAddress, RakNetGUID targetGuid)
	{
		_rakPeer = rakPeer;
		_udpProxyClient = UDPProxyClient.GetInstance();
		rakPeer.AttachPlugin(_udpProxyClient);
		_udpProxyClient.SetResultHandler(this);
		_udpProxyClient.RequestForwarding(facilitatorAddress, global::RakNet.RakNet.UNASSIGNED_SYSTEM_ADDRESS, targetGuid, 600000u);
	}

	public void ListenForRequests(RakPeerInterface rakPeer, Action<int> holePunched)
	{
		_rakPeer = rakPeer;
		_udpProxyClient = UDPProxyClient.GetInstance();
		rakPeer.AttachPlugin(_udpProxyClient);
		_udpProxyClient.SetResultHandler(this);
	}

	public override void OnAllServersBusy(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
	{
		base.OnAllServersBusy(proxyCoordinator, sourceAddress, targetAddress, targetGuid, proxyClientPlugin);
		logger.Warn("NatTraversal : All proxy servers are busy.");
	}

	public override void OnForwardingInProgress(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
	{
		base.OnForwardingInProgress(proxyIPAddress, proxyPort, proxyCoordinator, sourceAddress, targetAddress, targetGuid, proxyClientPlugin);
		logger.Warn("NatTraversal : UDP forwarding already in progress.");
	}

	public override void OnForwardingNotification(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
	{
		base.OnForwardingNotification(proxyIPAddress, proxyPort, proxyCoordinator, sourceAddress, targetAddress, targetGuid, proxyClientPlugin);
		logger.InfoFormat("NatTraversal : New UDP forwarding incoming from {0}.", sourceAddress);
	}

	public override void OnForwardingSuccess(string proxyIPAddress, ushort proxyPort, SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
	{
		base.OnForwardingSuccess(proxyIPAddress, proxyPort, proxyCoordinator, sourceAddress, targetAddress, targetGuid, proxyClientPlugin);
		logger.InfoFormat("NatTraversal : UDP forwarding successful for targetUID {0}.", targetGuid);
		ProxyIp = proxyIPAddress;
		ProxyPort = proxyPort;
		_rakPeer.Connect(proxyIPAddress, proxyPort, string.Empty, 0);
	}

	public override void OnNoServersOnline(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
	{
		base.OnNoServersOnline(proxyCoordinator, sourceAddress, targetAddress, targetGuid, proxyClientPlugin);
		logger.Warn("NatTraversal : No proxy server online.");
	}

	public override void OnRecipientNotConnected(SystemAddress proxyCoordinator, SystemAddress sourceAddress, SystemAddress targetAddress, RakNetGUID targetGuid, UDPProxyClient proxyClientPlugin)
	{
		base.OnRecipientNotConnected(proxyCoordinator, sourceAddress, targetAddress, targetGuid, proxyClientPlugin);
		logger.Warn("NatTraversal : Target is not connected to the facilitator.");
	}
}
