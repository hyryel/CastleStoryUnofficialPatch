using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Pathfinding.Collections;
using UnityEngine;

namespace Brix.Pathfinding.Map;

public class MapRenderer : MonoBehaviour
{
	private class MeshInfo
	{
		public List<Vector3> vertices = new List<Vector3>();

		public List<Vector3> normals = new List<Vector3>();

		public List<int> triangles = new List<int>();
	}

	public MapSection map;

	public int batchSize = 100;

	public bool rendering;

	private Transform output;

	private IEnumerator<XYZ> voxelEnumerator;

	private MeshInfo meshInfo;

	private static XYZ[] neighbors = new XYZ[6]
	{
		XYZ.right,
		-XYZ.right,
		XYZ.forward,
		-XYZ.forward,
		XYZ.up,
		-XYZ.up
	};

	private static Mesh cubeMesh;

	private static Dictionary<byte, MeshInfo> voxelMeshDico = new Dictionary<byte, MeshInfo>();

	[ContextMenu("Start Rendering")]
	public void StartRendering()
	{
		ClearAll();
		output = new GameObject(map.name).transform;
		output.SetParent(base.transform);
		voxelEnumerator = map.SurfaceVoxels.GetEnumerator();
		meshInfo = new MeshInfo();
	}

	public void ClearAll()
	{
		Transform transform = base.transform.Find(map.name);
		if (transform != null)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	private static BitField8 GetAbsentNeighborIndices(MapSection map, XYZ p)
	{
		BitField8 result = default(BitField8);
		for (int i = 0; i < neighbors.Length; i++)
		{
			if (!map.Contains(p + neighbors[i]))
			{
				result.Add(i);
			}
		}
		return result;
	}

	private static MeshInfo GetVoxelMesh(byte bits)
	{
		if (voxelMeshDico.TryGetValue(bits, out var value))
		{
			return value;
		}
		if (cubeMesh == null)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cubeMesh = gameObject.GetComponent<MeshFilter>().mesh;
			UnityEngine.Object.Destroy(gameObject);
		}
		value = new MeshInfo();
		if (bits != 0)
		{
			Vector3[] array = cubeMesh.vertices;
			Vector3[] array2 = cubeMesh.normals;
			int[] array3 = cubeMesh.triangles;
			HashSet<int> hashSet = new HashSet<int>();
			HashSet<int> hashSet2 = new HashSet<int>();
			int i;
			for (i = 0; i < neighbors.Length; i++)
			{
				if ((bits & (1 << i)) != 0)
				{
					continue;
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (Vector3.Dot(array2[j], neighbors[i].ToVector3()) >= 0.9f)
					{
						hashSet.Add(j);
					}
				}
				for (int k = 0; k < array3.Length; k++)
				{
					int j = array3[k];
					if (Vector3.Dot(array2[j], neighbors[i].ToVector3()) >= 0.9f)
					{
						hashSet2.Add(k);
					}
				}
			}
			int[] array4 = new int[array.Length];
			i = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (!hashSet.Contains(j))
				{
					array[i] = array[j];
					array2[i] = array2[j];
					array4[j] = i++;
				}
			}
			Array.Resize(ref array, i);
			Array.Resize(ref array2, i);
			i = 0;
			for (int k = 0; k < array3.Length; k++)
			{
				if (!hashSet2.Contains(k))
				{
					array3[i] = array4[array3[k]];
					i++;
				}
			}
			Array.Resize(ref array3, i);
			value.vertices = array.ToList();
			value.normals = array2.ToList();
			value.triangles = array3.ToList();
		}
		voxelMeshDico.Add(bits, value);
		return value;
	}

	private static void CopyMeshInfo(MeshInfo src, MeshInfo dst, Vector3 translation)
	{
		int count = dst.vertices.Count;
		foreach (int triangle in src.triangles)
		{
			dst.triangles.Add(triangle + count);
		}
		foreach (Vector3 vertex in src.vertices)
		{
			dst.vertices.Add(vertex + translation);
		}
		foreach (Vector3 normal in src.normals)
		{
			dst.normals.Add(normal);
		}
	}

	private static void RenderMeshInfo(MeshInfo src, Mesh dst)
	{
		if (src.vertices.Count > 65000)
		{
			src.vertices.RemoveRange(65000, src.vertices.Count - 65000);
		}
		if (src.normals.Count > 65000)
		{
			src.normals.RemoveRange(65000, src.normals.Count - 65000);
		}
		if (src.triangles.Count > 64998)
		{
			src.triangles.RemoveRange(64998, src.triangles.Count - 64998);
		}
		dst.vertices = src.vertices.ToArray();
		dst.normals = src.normals.ToArray();
		dst.triangles = src.triangles.ToArray();
	}

	[ContextMenu("Update Mesh")]
	public void UpdateMesh()
	{
		MeshFilter meshFilter = output.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = output.gameObject.AddComponent<MeshFilter>();
		}
		MeshRenderer meshRenderer = output.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = output.gameObject.AddComponent<MeshRenderer>();
		}
		if (meshRenderer.sharedMaterial == null)
		{
			meshRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
		}
		Mesh mesh = meshFilter.mesh;
		if (mesh == null)
		{
			mesh = (meshFilter.mesh = new Mesh());
		}
		RenderMeshInfo(meshInfo, mesh);
	}

	public void RenderSome(int count)
	{
		for (int i = 0; i < count; i++)
		{
			try
			{
				if (!voxelEnumerator.MoveNext())
				{
					voxelEnumerator = null;
					break;
				}
			}
			catch (InvalidOperationException)
			{
				StartRendering();
				continue;
			}
			MeshInfo voxelMesh = GetVoxelMesh(GetAbsentNeighborIndices(map, voxelEnumerator.Current).Bits);
			CopyMeshInfo(voxelMesh, meshInfo, voxelEnumerator.Current.ToVector3());
		}
	}

	private void Start()
	{
		StartRendering();
	}

	private void Update()
	{
		rendering = voxelEnumerator != null;
		if (rendering)
		{
			RenderSome(batchSize);
		}
		bool flag = voxelEnumerator != null;
		if (rendering && !flag)
		{
			UpdateMesh();
		}
	}

	public static Mesh GetRenderedMesh(MapSection map)
	{
		MeshInfo meshInfo = new MeshInfo();
		foreach (XYZ surfaceVoxel in map.SurfaceVoxels)
		{
			MeshInfo voxelMesh = GetVoxelMesh(GetAbsentNeighborIndices(map, surfaceVoxel).Bits);
			CopyMeshInfo(voxelMesh, meshInfo, surfaceVoxel.ToVector3());
		}
		Mesh mesh = new Mesh();
		RenderMeshInfo(meshInfo, mesh);
		return mesh;
	}
}
