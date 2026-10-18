namespace Brix.Utils.Listeners;

public class OnDisableListener : GameObjectListener
{
	private void OnDisable()
	{
		Invoke();
	}
}
