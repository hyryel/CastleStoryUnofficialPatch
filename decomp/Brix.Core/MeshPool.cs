using System;
using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

public class MeshPool
{
	public bool ClearVertices = true;

	public float VertexOversizeFactor = 1.2f;

	public float TriangleOversizeFactor = 1.2f;

	private Dictionary<int, MeshAugmentablePrealloue> _meshPool = new Dictionary<int, MeshAugmentablePrealloue>();

	private Dictionary<int, Vector4[]> _meshTangentPool = new Dictionary<int, Vector4[]>();

	private Dictionary<int, int[]> _meshTrianglePool = new Dictionary<int, int[]>();

	public MeshAugmentablePrealloue GetPooledMesh(int vertexCount, int triangleCount, bool useTangents)
	{
		MeshAugmentablePrealloue valueOrCreate = _meshPool.GetValueOrCreate(Mathf.CeilToInt(Mathf.Log(vertexCount, VertexOversizeFactor)), (int i) => new MeshAugmentablePrealloue(Mathf.CeilToInt(Mathf.Pow(VertexOversizeFactor, i))));
		valueOrCreate.triangles = _meshTrianglePool.GetValueOrCreate(Mathf.CeilToInt(Mathf.Log(triangleCount, TriangleOversizeFactor)), (int i) => new int[Mathf.CeilToInt(Mathf.Pow(TriangleOversizeFactor, i) / 3f) * 3]);
		valueOrCreate.tangents = ((!useTangents) ? null : _meshTangentPool.GetValueOrCreate(Mathf.CeilToInt(Mathf.Log(vertexCount, VertexOversizeFactor)), (int i) => new Vector4[Mathf.CeilToInt(Mathf.Pow(VertexOversizeFactor, i))]));
		if (ClearVertices)
		{
			Array.Clear(valueOrCreate.vertices, vertexCount, valueOrCreate.vertices.Length - vertexCount);
		}
		Array.Clear(valueOrCreate.triangles, triangleCount, valueOrCreate.triangles.Length - triangleCount);
		valueOrCreate.nv = 0;
		valueOrCreate.nt = 0;
		return valueOrCreate;
	}
}
