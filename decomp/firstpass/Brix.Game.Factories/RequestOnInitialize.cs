using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Game.Factories;

internal class RequestOnInitialize : MonoBehaviour
{
	[SerializeField]
	private Factory.AssetKey key;

	[SerializeField]
	private Material overrideMaterial;

	[SerializeField]
	private bool useOverrideMaterial;

	[SerializeField]
	private List<Transform> at;

	public void Request()
	{
		foreach (Transform item in at)
		{
			ObjectPoolSingleton.Request(out var go, key);
			go.transform.SetParent(item, worldPositionStays: false);
			go.name = "Visual";
			if (useOverrideMaterial)
			{
				MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
				foreach (MeshRenderer meshRenderer in componentsInChildren)
				{
					meshRenderer.sharedMaterial = overrideMaterial;
				}
			}
		}
	}
}
