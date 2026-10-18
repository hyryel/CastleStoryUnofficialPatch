using System.Collections.Generic;
using System.Linq;
using Smooth.Slinq;
using UnityEngine;

public class GetsAllObjectsInLayer : MonoBehaviour
{
	[SerializeField]
	public string layer;

	[Multiline(50)]
	public string text = string.Empty;

	[ContextMenu("Select Objects in Layer")]
	public void SelectObjectsInLayer()
	{
		GameObject[] sceneObjects = GetSceneObjects();
		GetObjectsInLayer(sceneObjects, LayerMask.NameToLayer(layer));
	}

	private void GetObjectsInLayer(GameObject[] root, int layer)
	{
		text = string.Empty;
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject gameObject in root)
		{
			if (gameObject.layer == layer)
			{
				list.Add(gameObject);
			}
		}
		list.Slinq().ForEach(delegate(GameObject go)
		{
			text = text + go.name + "\n";
		});
	}

	private GameObject[] GetSceneObjects()
	{
		return (from go in Resources.FindObjectsOfTypeAll<GameObject>()
			where go.hideFlags == HideFlags.None
			select go).ToArray();
	}
}
