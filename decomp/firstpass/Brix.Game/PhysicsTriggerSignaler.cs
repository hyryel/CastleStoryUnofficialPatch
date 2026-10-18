using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game;

[RequireComponent(typeof(Collider))]
public class PhysicsTriggerSignaler : MonoBehaviour
{
	public Signal<Collider> TriggerEnterSignal = new Signal<Collider>();

	public Signal<Collider> TriggerExitSignal = new Signal<Collider>();

	private void OnTriggerEnter(Collider c)
	{
		TriggerEnterSignal.Invoke(c);
	}

	private void OnTriggerExit(Collider c)
	{
		TriggerExitSignal.Invoke(c);
	}
}
