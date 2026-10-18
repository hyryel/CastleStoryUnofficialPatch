using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game;

[RequireComponent(typeof(Collider))]
public class PhysicsCollisionSignaler : MonoBehaviour
{
	public Signal<Collision> CollisionEnterSignal = new Signal<Collision>();

	public Signal<Collision> CollisionExitSignal = new Signal<Collision>();

	private void OnCollisionEnter(Collision c)
	{
		CollisionEnterSignal.Invoke(c);
	}

	private void OnCollisionExit(Collision c)
	{
		CollisionExitSignal.Invoke(c);
	}
}
