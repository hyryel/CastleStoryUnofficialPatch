using UnityEngine.Networking;

namespace Brix.Network;

public class NATServerNetworkConnection : NetworkConnection
{
	private int offset;

	public override void Initialize(string networkAddress, int networkHostId, int networkConnectionId, HostTopology hostTopology)
	{
		offset = NetworkServer.hostTopology.MaxDefaultConnections + networkHostId;
		base.Initialize(networkAddress, networkHostId, networkConnectionId + offset, hostTopology);
	}

	public override bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error)
	{
		if (!base.isConnected)
		{
			error = 0;
			return false;
		}
		return NetworkTransport.Send(hostId, connectionId - offset, channelId, bytes, numBytes, out error);
	}
}
