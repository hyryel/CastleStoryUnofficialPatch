using UnityEngine.Networking;

namespace Brix.Network;

public class NATServer : NetworkServerSimple
{
	public override void Initialize()
	{
		base.Initialize();
		SetNetworkConnectionClass<NATServerNetworkConnection>();
	}

	public override void OnConnected(NetworkConnection conn)
	{
		base.OnConnected(conn);
		NetworkServer.AddExternalConnection(conn);
	}
}
