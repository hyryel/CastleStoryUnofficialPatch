using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.UI.Login;

public class FakeSplashViewController : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene("SceneFakeMenu");
	}

	private void Update()
	{
	}
}
