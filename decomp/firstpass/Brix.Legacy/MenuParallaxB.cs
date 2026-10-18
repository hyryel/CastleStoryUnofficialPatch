using Brix.Input;
using UnityEngine;

namespace Brix.Legacy;

public class MenuParallaxB : MonoBehaviour
{
	public float multiplicateur = 1f;

	public Vector2 rel;

	public GameObject[] objs;

	private void Update()
	{
		if (Curseur.IsInGameWindow())
		{
			Vector2 screenPosition = PickingUtility.Player.controllers.Mouse.screenPosition;
			rel = new Vector2(screenPosition.x / (float)Screen.width, screenPosition.y / (float)Screen.height) - Vector2.one / 2f;
			for (int i = 0; i < objs.Length; i++)
			{
				float num = (float)i * multiplicateur;
				(objs[i].transform as RectTransform).localPosition = new Vector3(rel.x * num, rel.y * num, (objs[i].transform as RectTransform).localPosition.z);
			}
		}
	}
}
