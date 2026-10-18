using System.Collections.Generic;
using UnityEngine;

namespace Brix.Legacy;

public class AutoMergeSameNameSiblings : MonoBehaviour
{
	private void Start()
	{
		MergeSiblings();
	}

	public void MergeSiblings()
	{
		List<Transform> list = FindByName(base.name, base.transform.parent);
		Transform transform = list[0];
		foreach (Transform item in list)
		{
			if (item != transform)
			{
				Merge(item, transform);
			}
		}
	}

	public List<Transform> FindByName(string name, Transform parent)
	{
		List<GameObject> list = new List<GameObject>();
		while (true)
		{
			GameObject gameObject = GameObject.Find(name);
			if (gameObject == null)
			{
				break;
			}
			list.Add(gameObject);
			gameObject.name = string.Empty + Random.value;
		}
		foreach (GameObject item in list)
		{
			item.name = name;
		}
		List<Transform> list2 = new List<Transform>();
		foreach (GameObject item2 in list)
		{
			if (item2.transform.parent == parent)
			{
				list2.Add(item2.transform);
			}
		}
		return list2;
	}

	public void Merge(Transform sibling, Transform master)
	{
		foreach (Transform item in sibling)
		{
			item.parent = master;
		}
		Object.Destroy(sibling.gameObject);
	}
}
