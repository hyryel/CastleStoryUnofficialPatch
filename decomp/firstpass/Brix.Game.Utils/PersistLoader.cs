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
