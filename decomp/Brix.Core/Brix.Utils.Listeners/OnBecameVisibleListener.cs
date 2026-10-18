namespace Brix.Utils.Listeners;

public class OnBecameVisibleListener : GameObjectListener
{
	private void OnBecameVisible()
	{
		Invoke();
	}
}
