using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Components;

public class Highlight : MonoBehaviour
{
	[SerializeField]
	private MeshFilter meshFilter;

	public GameObject TargetGO;

	public XYZ TargetPosition = XYZ.zero;

	public BlockData TargetBlock;

	private CombineInstance[] combineBuffer = new CombineInstance[0];

	public Color color
	{
		get
		{
			return GetComponent<Renderer>().material.color;
		}
		set
		{
			GetComponent<Renderer>().material.color = value;
		}
	}

	public static GameObject FromFactory(Color color, Transform parent)
	{
		ObjectPoolSingleton.Request(out var go, Test.ObjectHighlight);
		go.transform.parent = parent;
		go.GetComponent<Highlight>().color = color;
		return go;
	}

	public static Highlight FromGameObject(GameObject go, Color color, Transform parent)
	{
		GameObject gameObject = FromFactory(color, parent);
		gameObject.name = go.name + "-Highlight";
		return gameObject.GetComponent<Highlight>();
	}

	public static Highlight FromPosition(XYZ pos, Color color, Transform parent)
	{
		GameObject gameObject = FromFactory(color, parent);
		gameObject.name = pos.ToString() + "-Highlight";
		gameObject.transform.position = pos.ToVector3();
		return gameObject.GetComponent<Highlight>();
	}

	public void HighlightBlock(BlockData block)
	{
		GameObject gameObject = Factory.Peek(block.BlockInfo.Key);
		CombinedMeshFromChildren(meshFilter, gameObject);
		base.transform.position = block.AbsolutePivotPosition;
		base.transform.rotation = block.Rotation;
		TargetBlock = block;
		SetNormalsFromModel(gameObject);
	}

	public void HighlightPosition(XYZ pos)
	{
		base.transform.position = pos.ToVector3();
		TargetPosition = pos;
	}

	public void HighlightObject(GameObject go)
	{
		CombinedMeshFromChildren(meshFilter, go);
		base.transform.position = go.transform.position;
		base.transform.rotation = go.transform.rotation;
		TargetGO = go;
		SetNormalsFromModel(go);
	}

	private void CombinedMeshFromChildren(MeshFilter filter, GameObject go)
	{
		CollectionPool.Request<MeshFilter>(out List<MeshFilter> list);
		if (!go.GetComponent<WardController>())
		{
			go.GetComponentsInChildren(list);
		}
		MeshFilter component = go.GetComponent<MeshFilter>();
		if ((bool)component)
		{
			list.Add(component);
		}
		list.RemoveAll((MeshFilter m) => m.GetComponent<CircleHighlight>() != null || m.GetComponent<SelectionCircle>() != null || m.GetComponent<Garnotte>() != null);
		int num = 0;
		Array.Resize(ref combineBuffer, list.Count);
		for (; num < list.Count; num++)
		{
			Mesh sharedMesh = list[num].sharedMesh;
			if (sharedMesh != null)
			{
				combineBuffer[num].mesh = sharedMesh;
				combineBuffer[num].transform = go.transform.worldToLocalMatrix * list[num].transform.localToWorldMatrix;
			}
		}
		filter.mesh = new Mesh();
		filter.mesh.CombineMeshes(combineBuffer);
		list.Dispose();
	}

	private void SetNormalsFromModel(GameObject original)
	{
		StateRenderer component = original.GetComponent<StateRenderer>();
		if ((bool)component)
		{
			GetComponent<Renderer>().material.SetTexture("_Normals", component.NormalMap);
		}
	}
}
