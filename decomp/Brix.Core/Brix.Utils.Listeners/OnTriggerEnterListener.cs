using UnityEngine;

namespace Brix.Utils.Listeners;

public class OnTriggerEnterListener : GameObjectListener<Collider>
{
	private void OnTriggerEnter(Collider c)
	{
		Invoke(c);
	}
}
