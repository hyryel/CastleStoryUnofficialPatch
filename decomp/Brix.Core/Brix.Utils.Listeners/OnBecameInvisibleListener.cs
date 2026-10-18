namespace Brix.Utils.Listeners;

public class OnBecameInvisibleListener : GameObjectListener
{
	private void OnBecameInvisible()
	{
		Invoke();
	}
}
