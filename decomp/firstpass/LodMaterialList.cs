using System;
using UnityEngine;

public class LodMaterialList : MonoBehaviour
{
	public Material lod0Mat;

	public Material lod1Mat;

	public Material lod2Mat;

	public Material lod3Mat;

	private void Awake()
	{
		GetComponent<MeshRenderer>().material = lod0Mat;
	}

	public Material[] ToArray(int count)
	{
		Material[] array = new Material[count];
		if (count > 0)
		{
			array[0] = lod0Mat;
		}
		if (count > 1)
		{
			array[1] = lod1Mat;
		}
		if (count > 2)
		{
			array[2] = lod2Mat;
		}
		if (count > 3)
		{
			array[3] = lod3Mat;
		}
		if (count > 4)
		{
			array[4] = lod3Mat;
		}
		if (count > 5)
		{
			array[5] = lod3Mat;
		}
		if (count > 6)
		{
			array[6] = lod3Mat;
		}
		if (count > 7)
		{
			throw new ArgumentOutOfRangeException();
		}
		return array;
	}
}
