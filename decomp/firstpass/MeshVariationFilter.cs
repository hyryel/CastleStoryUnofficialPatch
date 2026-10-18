using System.Collections.Generic;
using Brix.External.Factories;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[DisallowMultipleComponent]
public class MeshVariationFilter : MonoBehaviour, IInitializable
{
	public List<Mesh> meshVariations;

	public bool RandomVariationOnSpawn = true;

	public void ApplyRandomVariation()
	{
		if (meshVariations != null && meshVariations.Count > 0)
		{
			GetComponent<MeshFilter>().sharedMesh = meshVariations[Random.Range(0, meshVariations.Count)];
		}
	}

	public void Initialize()
	{
		if (RandomVariationOnSpawn)
		{
			ApplyRandomVariation();
		}
	}
}
