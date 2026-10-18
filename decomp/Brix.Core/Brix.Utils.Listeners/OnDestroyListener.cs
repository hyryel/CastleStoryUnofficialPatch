namespace Brix.Utils.Listeners;

public class OnDestroyListener : GameObjectListener
{
	private void OnDestroy()
	{
		Invoke();
	}
}
