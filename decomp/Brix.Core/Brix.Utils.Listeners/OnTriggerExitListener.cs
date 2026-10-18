using UnityEngine;

namespace Brix.Utils.Listeners;

public class OnTriggerExitListener : GameObjectListener<Collider>
{
	private void OnTriggerExit(Collider c)
	{
		Invoke(c);
	}
}
