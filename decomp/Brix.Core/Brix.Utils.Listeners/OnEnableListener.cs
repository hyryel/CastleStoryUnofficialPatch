namespace Brix.Utils.Listeners;

public class OnEnableListener : GameObjectListener
{
	private void OnEnable()
	{
		Invoke();
	}
}
