using Brix.Engine;
using Brix.Utils.Logging;
using Mono.Nat;

namespace Brix.Network;

public static class UPnP
{
	private static BrixLogger logger = new BrixLogger(typeof(UPnP).FullName);

	private static int Port { get; set; }

	public static void connect(int _port)
	{
		logger.InfoFormat("UPNp : Attemping UPNp forwarding on TCP/UDP port {0}", _port);
		Port = _port;
		NatUtility.DeviceFound += DeviceFound;
		NatUtility.DeviceLost += DeviceLost;
		NatUtility.StartDiscovery();
	}

	private static void DeviceFound(object sender, DeviceEventArgs args)
	{
		INatDevice device = args.Device;
		device.CreatePortMap(new Mapping(Protocol.Tcp, Port, Port));
		device.CreatePortMap(new Mapping(Protocol.Udp, Port, Port));
		logger.InfoFormat("UPNp : Device Found. Forwarding your port on : {0}", device.GetExternalIP());
		GameSignals.Invoke(GameSignals.UPnPSuceeded);
		logger.Info(device.GetSpecificMapping(Protocol.Udp, Port).PublicPort);
		logger.Info(device.GetSpecificMapping(Protocol.Tcp, Port).PublicPort);
	}

	private static void DeviceLost(object sender, DeviceEventArgs args)
	{
		INatDevice device = args.Device;
		logger.InfoFormat("UPNp : Device lost. Deleting mapping for : {0}", device.GetExternalIP());
		device.DeletePortMap(new Mapping(Protocol.Tcp, Port, Port));
		device.DeletePortMap(new Mapping(Protocol.Udp, Port, Port));
	}
}
