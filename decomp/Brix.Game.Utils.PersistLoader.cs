using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Game.Utils;

public class PersistLoader : MonoBehaviour
{
	public static bool PersistsLoaded;

	private void Awake()
	{
		if (!PersistsLoaded)
		{
			SceneManager.LoadScene("PersistentScene", LoadSceneMode.Additive);
			PersistsLoaded = true;
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
