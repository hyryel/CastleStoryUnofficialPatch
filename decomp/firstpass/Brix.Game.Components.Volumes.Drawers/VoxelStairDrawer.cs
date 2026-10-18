using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using Brix.Util;
using UnityEngine;

namespace Brix.Game.Components.Volumes.Drawers;

public class VoxelStairDrawer : MonoBehaviour
{
	[SerializeField]
	private Transform hidden;

	[SerializeField]
	private Transform displayed;

	private GameObject[] stairsMeshes = new GameObject[15];

	private GameObject[] cubeMeshes = new GameObject[15];

	public bool Showing
	{
		get
		{
			return displayed.gameObject.activeSelf;
		}
		set
		{
			displayed.gameObject.SetActive(value);
		}
	}

	public void Redraw(List<XYZ> positions, List<Faces> directions)
	{
		int index = 0;
		int index2 = 0;
		if (positions.Count > stairsMeshes.Length)
		{
			Array.Resize(ref stairsMeshes, positions.Count);
			Array.Resize(ref cubeMeshes, positions.Count);
		}
		for (int i = 1; i < positions.Count; i++)
		{
			if (directions[i] == Faces.up)
			{
				PlaceBlockAt(positions[i], directions[i - 1], ref index2);
			}
			else
			{
				PlaceStairAt(positions[i], directions[i], ref index);
			}
		}
		HideRemaining(cubeMeshes, index2);
		HideRemaining(stairsMeshes, index);
	}

	private void HideRemaining(GameObject[] array, int from)
	{
		for (int i = from; i < array.Length; i++)
		{
			if (!(array[i] == null))
			{
				array[i].transform.SetParent(hidden);
			}
		}
	}

	private void PlaceBlockAt(XYZ pos, Faces dir, ref int index)
	{
		GameObject meshOrCreate = GetMeshOrCreate(cubeMeshes, index, VolumesUI.Block_Empty);
		meshOrCreate.transform.SetParent(displayed);
		Vector3 position = pos.ToVector3();
		Vector3 vector = Calcule.FaceIndexToVecteur((int)dir);
		position -= vector;
		meshOrCreate.transform.position = position;
		index++;
	}

	private void PlaceStairAt(XYZ pos, Faces dir, ref int index)
	{
		GameObject meshOrCreate = GetMeshOrCreate(stairsMeshes, index, VolumesUI.Staircase_Empty);
		meshOrCreate.transform.SetParent(displayed);
		Vector3 vector = pos.ToVector3();
		Vector3 vector2 = Calcule.FaceIndexToVecteur((int)dir);
		vector -= vector2 * 0.5f;
		meshOrCreate.transform.position = vector;
		meshOrCreate.transform.LookAt(vector + vector2);
		index++;
	}

	private static GameObject GetMeshOrCreate(GameObject[] array, int i, Factory.AssetKey key)
	{
		if (array[i] == null)
		{
			ObjectPoolSingleton.Request(out var go, key);
			array[i] = go;
		}
		return array[i];
	}

	private void ReleaseAll(GameObject[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			ObjectPoolSingleton.Release(array[i]);
		}
	}

	private void OnDestroy()
	{
		ReleaseAll(cubeMeshes);
		ReleaseAll(stairsMeshes);
	}
}
