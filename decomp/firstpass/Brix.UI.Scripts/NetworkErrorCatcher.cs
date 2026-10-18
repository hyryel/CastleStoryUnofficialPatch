using Brix.Components;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Brix.UI.Scripts;

public class NetworkErrorCatcher : BrixSingleton<NetworkErrorCatcher>
{
	private static bool _connected;

	private static readonly UnityAction<Scene, Scene> ActiveSceneChanged = delegate
	{
		PersistentCoroutineRunner.Relais(delegate
		{
			NetworkMessageModalMenu.TryOpenSignal.Invoke();
		});
	};

	private void OnEnable()
	{
		if (!_connected)
		{
			_connected = true;
			SceneManager.activeSceneChanged += ActiveSceneChanged;
		}
	}
}
