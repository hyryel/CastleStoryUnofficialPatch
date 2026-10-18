using UnityEngine;

namespace Brix.NewUI.LoadingScreen;

public class InstantiatesLoadingScreen : MonoBehaviour
{
	public GameObject loadingScreen;

	public GameObject parent;

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(loadingScreen);
		gameObject.transform.SetParent(parent.transform);
		gameObject.name = loadingScreen.name;
		gameObject.transform.SetSiblingIndex(base.transform.GetSiblingIndex());
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
		gameObject.GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
		Object.Destroy(base.gameObject);
	}
}
