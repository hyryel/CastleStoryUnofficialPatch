using UnityEngine.Networking;

namespace Brix.Network;

public abstract class UNetAbstractMessageHandler
{
	protected NetworkClient localClient => UNetManager.instance.LocalNetworkClient;

	protected abstract void RegisterHandlers();

	public abstract void Initialize();

	protected virtual void RegisterHandler(short messageID, NetworkMessageDelegate handler)
	{
		if (NetworkServer.active)
		{
			if (NetworkServer.handlers.ContainsKey(messageID))
			{
				NetworkServer.UnregisterHandler(messageID);
			}
			NetworkServer.RegisterHandler(messageID, handler);
		}
		else
		{
			if (localClient.handlers.ContainsKey(messageID))
			{
				UNetManager.instance.LocalNetworkClient.UnregisterHandler(messageID);
			}
			UNetManager.instance.LocalNetworkClient.RegisterHandler(messageID, handler);
		}
	}
}
