using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Utils;

public static class ApplicationQuitter
{
	public static string DontDestroyOnLoadObject = "GA_Controller";

	public static void Quit()
	{
		DestroyEverything();
		new GameObject("CloseMainWindow").AddComponent<CloseMainWindow>();
	}

	public static void DestroyEverything()
	{
		Scene[] array = GetAllScenes().ToArray();
		foreach (Scene scene in array)
		{
			DestroyScene(scene);
		}
		GameObject gameObject = GameObject.Find(DontDestroyOnLoadObject);
		if (gameObject != null)
		{
			DestroyScene(gameObject.scene);
		}
	}

	private static void DestroyScene(Scene scene)
	{
		if (scene.IsValid())
		{
			GameObject[] rootGameObjects = scene.GetRootGameObjects();
			GameObject[] array = rootGameObjects;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(value: false);
			}
			GameObject[] array2 = rootGameObjects;
			foreach (GameObject obj in array2)
			{
				Object.Destroy(obj);
			}
		}
	}

	private static IEnumerable<Scene> GetAllScenes()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			yield return SceneManager.GetSceneAt(i);
		}
	}
}
