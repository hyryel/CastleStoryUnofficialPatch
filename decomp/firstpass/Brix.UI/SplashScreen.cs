using Brix.Utils.Factories;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.UI;

public class SplashScreen : MonoBehaviour
{
	public string nextScene;

	public bool skip;

	public GameObject firstScreen;

	public void Start()
	{
		FactorySignalInvoker.OnFactoriesReadySig.ConnectOnce(delegate(SplashScreen _this)
		{
			if (_this.skip)
			{
				_this.LoadNextScene();
			}
			else
			{
				_this.firstScreen.SetActive(value: true);
			}
		}, this);
	}

	public void LoadNextScene()
	{
		if (!string.IsNullOrEmpty(nextScene))
		{
			SceneManager.LoadScene(nextScene);
		}
	}
}
