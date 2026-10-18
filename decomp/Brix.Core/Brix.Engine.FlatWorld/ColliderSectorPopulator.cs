using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine.FlatWorld.Layers;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class ColliderSectorPopulator : MonoBehaviour
{
	internal TestWorldConfig3 test;

	public ColliderSectorUpdater prefab;

	public bool build;

	public bool initLayers;

	private IEnumerator Start()
	{
		if (test == null)
		{
			test = Object.FindObjectOfType<TestWorldConfig3>();
		}
		if (initLayers)
		{
			yield return new WaitForEndOfFrame();
			initLayers = false;
			test.InitLayers();
		}
	}

	private void Update()
	{
		if (build && !initLayers)
		{
			build = false;
			Clear();
			Build();
		}
	}

	[ContextMenu("Clear")]
	private void Clear()
	{
		foreach (Transform item in base.transform)
		{
			Object.Destroy(item.gameObject);
		}
	}

	[ContextMenu("Build")]
	private void Build()
	{
		test.InitLayers();
		XYZ b = XYZ.Mul(prefab.sectorMvSize, test.MvSize);
		IEnumerable<XYZ> enumerable = (from q in test.drawer.GetOccupied()
			select XYZ.Div(q, prefab.sectorMvSize)).Distinct();
		foreach (XYZ item in enumerable)
		{
			XYZ xYZ = XYZ.Mul(item, b);
			GameObject gameObject = Object.Instantiate(prefab.gameObject);
			gameObject.name = "Collider Sector " + xYZ;
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.position = xYZ.ToVector3();
			gameObject.SetActive(value: true);
		}
	}
}
