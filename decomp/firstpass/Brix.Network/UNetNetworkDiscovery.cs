using System.Collections;
using Brix.External.Signals;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class UNetNetworkDiscovery : NetworkDiscovery
{
	public Signal<string, string> ReceivedBroadcastSignal = new Signal<string, string>();

	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		string a = fromAddress.Split(':').Slinq().LastOrDefault();
		ReceivedBroadcastSignal.Invoke(a, data);
		base.OnReceivedBroadcast(fromAddress, data);
	}

	private void Start()
	{
		base.useGUILayout = false;
	}

	private IEnumerator ClearGhost()
	{
		while (base.running)
		{
			base.broadcastsReceived.Clear();
			yield return new WaitForSeconds(5f);
		}
	}

	public void FindLanGames()
	{
		if (!base.running)
		{
			UNetManager.instance.networkDiscovery.Initialize();
			UNetManager.instance.networkDiscovery.useNetworkManager = false;
			UNetManager.instance.networkDiscovery.broadcastPort = 3100;
			UNetManager.instance.networkDiscovery.StartAsClient();
			StartCoroutine("ClearGhost");
		}
	}

	public void BroadcastLanGame()
	{
		if (!base.running)
		{
			UNetManager.instance.networkDiscovery.useNetworkManager = false;
			UNetManager.instance.networkDiscovery.broadcastData = UNetManager.instance.LocalPlayerName;
			base.broadcastPort = 3100;
			Initialize();
			StartAsServer();
		}
	}
}
