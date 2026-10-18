using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.NewUI.Compass;

public abstract class CompassDriver : MonoBehaviour
{
	[SerializeField]
	protected CompassDispenser dispenser;

	protected Signal Disconnected = new Signal();

	protected abstract void ConnectToSignals();

	protected virtual void Start()
	{
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectToSignals).DisconnectWhen(Disconnected);
	}

	protected virtual void OnDestroy()
	{
		Disconnected.Invoke();
	}
}
