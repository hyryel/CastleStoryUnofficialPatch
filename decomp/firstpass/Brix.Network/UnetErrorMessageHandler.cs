using UnityEngine.Networking;

namespace Brix.Network;

public class UnetErrorMessageHandler : UNetAbstractMessageHandler
{
	public override void Initialize()
	{
		RegisterHandlers();
	}

	protected override void RegisterHandlers()
	{
	}

	private void OnError(NetworkMessage message)
	{
	}
}
