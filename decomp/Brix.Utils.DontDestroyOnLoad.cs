using UnityEngine;

namespace Brix.Utils;

public class DontDestroyOnLoad : MonoBehaviour
{
	private bool beingDestroyed;

	private void Awake()
	{
		if (base.transform.parent == null)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public GameObject ResetObject()
	{
		if (!beingDestroyed)
		{
			GameObject gameObject = Object.Instantiate(base.gameObject);
			gameObject.name = base.name;
			gameObject.transform.parent = base.transform.parent;
			Object.Destroy(base.gameObject);
			Object.DestroyImmediate(base.gameObject);
			beingDestroyed = true;
			return gameObject;
		}
		return null;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
