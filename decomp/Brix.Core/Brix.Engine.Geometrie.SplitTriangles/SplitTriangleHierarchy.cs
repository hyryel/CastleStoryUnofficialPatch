using System.Collections.Generic;
using System.Linq;
using Brix.Engine.FlatWorld;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.Geometrie.SplitTriangles;

public class SplitTriangleHierarchy : MonoBehaviour
{
	public MeshFilter filter;

	public bool spawn;

	public bool read;

	public bool export;

	public string exportPath;

	public string exportName;

	public bool autoDetectVoxels;

	public bool overrideVoxelOffset;

	public Vector3 voxelOffset;

	public GameObject target;

	public GameObject prefab;

	private Material material;

	private Mesh mesh;

	private int[] triangles;

	private Vector3[] vertices;

	private Vector3[] normals;

	private Vector2[] uv;

	private Vector2[] uv2;

	private Vector4[] tangents;

	private Dictionary<XYZ, VoxelConfigurator> dicoVoxels = new Dictionary<XYZ, VoxelConfigurator>();

	private void Start()
	{
		if (filter == null)
		{
			filter = base.gameObject.GetOrAddComponent<MeshFilter>();
		}
	}

	private void Update()
	{
		if (spawn)
		{
			spawn = false;
			Spawn();
		}
		if (read)
		{
			read = false;
			Read();
		}
		if (export)
		{
			export = false;
			Export();
		}
	}

	private void Spawn()
	{
		if (target != null)
		{
			Object.Destroy(target);
		}
		target = new GameObject(filter.name + " (Split)");
		Transform transform = new GameObject("Triangles").transform;
		transform.SetParent(target.transform);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		Transform transform2 = new GameObject("Voxels").transform;
		transform2.SetParent(target.transform);
		transform2.localPosition = Vector3.zero;
		transform2.localRotation = Quaternion.identity;
		material = filter.GetComponent<MeshRenderer>().sharedMaterial;
		mesh = filter.sharedMesh;
		triangles = mesh.triangles;
		vertices = mesh.vertices;
		normals = mesh.normals;
		uv = mesh.uv;
		uv2 = mesh.uv2;
		tangents = mesh.tangents;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int a = triangles[i];
			int b = triangles[i + 1];
			int c = triangles[i + 2];
			TriangleConfigurator triangle = CreateTriangle(transform, i / 3, a, b, c);
			if (autoDetectVoxels)
			{
				Vector3 v = Calcule.AverageVector(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
				Vector3 vector = Calcule.AverageVector(normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]]);
				v -= voxelOffset;
				v -= vector * 0.01f;
				AddTriangleToVoxel(transform2, XYZ.FromVector3(v), triangle);
			}
		}
	}

	private void AddTriangleToVoxel(Transform parent, XYZ pVoxel, TriangleConfigurator triangle)
	{
		if (!dicoVoxels.TryGetValue(pVoxel, out var value))
		{
			value = new GameObject("(" + pVoxel.ToString() + ")").AddComponent<VoxelConfigurator>();
			dicoVoxels.Add(pVoxel, value);
			value.transform.SetParent(parent);
			value.transform.localPosition = pVoxel.ToVector3() + voxelOffset;
			value.transform.localRotation = Quaternion.identity;
			value.position = pVoxel;
		}
		int num = triangle.triangleIndex * 3;
		Vector3 vector = vertices[triangles[num]];
		Vector3 vector2 = vertices[triangles[num + 1]];
		Vector3 vector3 = vertices[triangles[num + 2]];
		int i;
		for (i = 0; i < 6; i++)
		{
			Vector3 vector4 = CubeModele.Dirs[i];
			Vector3 vector5 = vector4 / 2f + value.transform.position;
			if (Mathf.Abs(Vector3.Dot(vector5 - vector, vector4)) < 0.01f && Mathf.Abs(Vector3.Dot(vector5 - vector2, vector4)) < 0.01f && Mathf.Abs(Vector3.Dot(vector5 - vector3, vector4)) < 0.01f)
			{
				break;
			}
		}
		FaceConfigurator face = value.GetFace(i);
		if (i < 6)
		{
			face.area += Vector3.Cross(vector - vector2, vector - vector3).magnitude / 2f;
			face.ReevaluateVisibility();
		}
		triangle.transform.SetParent(face.transform);
	}

	private TriangleConfigurator CreateTriangle(Transform parent, int k, int a, int b, int c)
	{
		Vector3 center = (vertices[a] + vertices[b] + vertices[c]) / 3f;
		Vector3 vector = (normals[a] + normals[b] + normals[c]) / 3f;
		Vector3 right = ((tangents != null) ? ((Vector3)(tangents[a] + tangents[b] + tangents[c]) / 3f) : Calcule.PerpendicularVector(vector));
		Vector3 up = Vector3.Cross(vector, right);
		Quaternion quaternion = Quaternion.LookRotation(vector, up);
		GameObject gameObject = new GameObject("Triangle " + (k + 1), typeof(MeshFilter), typeof(MeshRenderer));
		gameObject.transform.SetParent(parent);
		gameObject.transform.localPosition = center;
		gameObject.transform.localRotation = quaternion;
		gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
		MeshAugmentableSimple meshAugmentableSimple = new MeshAugmentableSimple();
		if (vertices != null && vertices.Length != 0)
		{
			meshAugmentableSimple.vertices = new Vector3[3]
			{
				vertices[a],
				vertices[b],
				vertices[c]
			};
		}
		if (normals != null && normals.Length != 0)
		{
			meshAugmentableSimple.normals = new Vector3[3]
			{
				normals[a],
				normals[b],
				normals[c]
			};
		}
		if (uv != null && uv.Length != 0)
		{
			meshAugmentableSimple.uv = new Vector2[3]
			{
				uv[a],
				uv[b],
				uv[c]
			};
		}
		if (uv2 != null && uv2.Length != 0)
		{
			meshAugmentableSimple.uv2 = new Vector2[3]
			{
				uv2[a],
				uv2[b],
				uv2[c]
			};
		}
		if (tangents != null && tangents.Length != 0)
		{
			meshAugmentableSimple.tangents = new Vector4[3]
			{
				tangents[a],
				tangents[b],
				tangents[c]
			};
		}
		meshAugmentableSimple.triangles = new int[3] { 0, 1, 2 };
		string text = $"{k + 1}";
		if (text.Length == 1)
		{
			text = "  " + text + " ";
		}
		else if (text.Length == 2)
		{
			text = " " + text + " ";
		}
		else if (text.Length == 3)
		{
			text = " " + text;
		}
		Vector2 asciiUv = Brix.Engine.FlatWorld.Display.GetAsciiUv(text);
		Vector2[] source = meshAugmentableSimple.vertices.Select((Vector3 v) => new Vector2(Vector3.Dot(v - center, right), Vector3.Dot(v - center, up))).ToArray();
		Vector2 pmin = new Vector2(source.Min((Vector2 v) => v.x), source.Min((Vector2 v) => v.y));
		Vector2 pmax = new Vector2(source.Max((Vector2 v) => v.x), source.Max((Vector2 v) => v.y));
		if (0f - pmin.x < pmax.x)
		{
			pmin.x = 0f - pmax.x;
		}
		else
		{
			pmax.x = 0f - pmin.x;
		}
		if (0f - pmin.y < pmax.y)
		{
			pmin.y = 0f - pmax.y;
		}
		else
		{
			pmax.y = 0f - pmin.y;
		}
		Vector2[] array = source.Select((Vector2 v) => new Vector2(Mathf.InverseLerp(pmin.x, pmax.x, v.x), Mathf.InverseLerp(pmin.y, pmax.y, v.y))).ToArray();
		meshAugmentableSimple.uv = array;
		meshAugmentableSimple.uv2 = new Vector2[3] { asciiUv, asciiUv, asciiUv };
		Matrix4x4 matrix4x = Matrix4x4.TRS(-center, Quaternion.identity, Vector3.one);
		matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(quaternion), Vector3.one) * matrix4x;
		meshAugmentableSimple.Transform(matrix4x);
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		Mesh mesh = (component.sharedMesh = new Mesh());
		Mesh meshOut = mesh;
		meshAugmentableSimple.Rendre(meshOut);
		TriangleConfigurator triangleConfigurator = gameObject.AddComponent<TriangleConfigurator>();
		triangleConfigurator.TriangleIndex = k;
		return triangleConfigurator;
	}

	private void Read()
	{
		VoxelizedMesh voxelMesh = ReadVoxelMesh();
		base.gameObject.GetOrAddComponent<VoxelizedMeshDescriptor>().voxelMesh = voxelMesh;
	}

	private VoxelizedMesh ReadVoxelMesh()
	{
		VoxelizedMesh voxelizedMesh = new VoxelizedMesh();
		voxelizedMesh.originalMesh = filter.sharedMesh;
		List<VoxelizedMesh.VoxelData> list = new List<VoxelizedMesh.VoxelData>();
		foreach (Transform item in target.transform.Find("Voxels"))
		{
			VoxelizedMesh.VoxelData voxelData = ReadVoxel(item);
			list.Add(voxelData);
			voxelizedMesh.pivot = voxelData.position.ToVector3() - item.localPosition;
		}
		voxelizedMesh.voxels = list.ToArray();
		return voxelizedMesh;
	}

	private VoxelizedMesh.VoxelData ReadVoxel(Transform transform)
	{
		VoxelConfigurator component = transform.GetComponent<VoxelConfigurator>();
		List<VoxelizedMesh.FaceData> list = new List<VoxelizedMesh.FaceData>();
		foreach (Transform item in transform)
		{
			list.Add(ReadFace(item));
		}
		VoxelizedMesh.VoxelData voxelData = new VoxelizedMesh.VoxelData();
		voxelData.position = component.position;
		voxelData.faces = list.ToArray();
		Mesh[] array = new Mesh[64];
		MeshAugmentableSimple meshAugmentableSimple = new MeshAugmentableSimple();
		MeshAugmentableSimple meshAugmentableSimple2 = new MeshAugmentableSimple();
		meshAugmentableSimple.Lire(filter.sharedMesh);
		for (int i = 0; i < 64; i++)
		{
			meshAugmentableSimple2.Clear();
			InitPartialMesh(meshAugmentableSimple, meshAugmentableSimple2, transform.localPosition);
			foreach (VoxelizedMesh.FaceData item2 in list)
			{
				int direction = (int)item2.direction;
				if (direction < 0 || direction >= 6 || (i & (1 << direction)) != 0)
				{
					AddPartialMeshTriangles(meshAugmentableSimple, meshAugmentableSimple2, item2.direction, item2.triangles);
				}
			}
			FinishPartialMesh(meshAugmentableSimple2);
			array[i] = meshAugmentableSimple2.Rendre(array[i]);
			array[i].name = filter.name + "." + transform.GetSiblingIndex() + "." + i;
		}
		voxelData.partialMeshes = array;
		return voxelData;
	}

	private VoxelizedMesh.FaceData ReadFace(Transform transform)
	{
		FaceConfigurator component = transform.GetComponent<FaceConfigurator>();
		VoxelizedMesh.FaceData faceData = new VoxelizedMesh.FaceData();
		faceData.direction = component.direction;
		faceData.type = component.configType;
		List<int> list = new List<int>();
		foreach (Transform item2 in transform)
		{
			string text = item2.name;
			int item = int.Parse(text.Substring("Triangle ".Length));
			list.Add(item);
		}
		faceData.triangles = list.ToArray();
		return faceData;
	}

	private void InitPartialMesh(MeshAbstrait originalMesh, MeshAugmentable resultMesh, Vector3 translation)
	{
		originalMesh.Transcrire(resultMesh);
		resultMesh.SetVertices((from v in resultMesh.GetVertices()
			select v - translation).ToArray());
		resultMesh.SetTriangles(new int[0]);
	}

	private void AddPartialMeshTriangles(MeshAbstrait originalMesh, MeshAugmentable resultMesh, VoxelizedMesh.FaceDirection direction, int[] triangleIndices)
	{
		int[] array = originalMesh.GetModifiable().GetTriangles();
		foreach (int num in triangleIndices)
		{
			int num2 = (num - 1) * 3;
			resultMesh.Triangle(array[num2], array[num2 + 1], array[num2 + 2]);
		}
	}

	private void FinishPartialMesh(MeshAugmentable resultMesh)
	{
		int[] trianglesOut = resultMesh.GetTriangles();
		int[] simplifiedVertexIndices = Config.GetSimplifiedVertexIndices(trianglesOut, out trianglesOut);
		Vector3[] vertices = resultMesh.GetVertices();
		Vector3[] normals = resultMesh.GetNormals();
		Vector2[] uv = resultMesh.GetUv();
		Vector2[] uv2 = resultMesh.GetUv2();
		Vector4[] tangents = resultMesh.GetTangents();
		resultMesh.SetVertices(simplifiedVertexIndices.Select((int i) => vertices[i]).ToArray());
		if (normals != null && normals.Length != 0)
		{
			resultMesh.SetNormals(simplifiedVertexIndices.Select((int i) => normals[i]).ToArray());
		}
		if (uv != null && uv.Length != 0)
		{
			resultMesh.SetUv(simplifiedVertexIndices.Select((int i) => uv[i]).ToArray());
		}
		if (uv2 != null && uv2.Length != 0)
		{
			resultMesh.SetUv2(simplifiedVertexIndices.Select((int i) => uv2[i]).ToArray());
		}
		if (tangents != null && tangents.Length != 0)
		{
			resultMesh.SetTangents(simplifiedVertexIndices.Select((int i) => tangents[i]).ToArray());
		}
		resultMesh.SetTriangles(trianglesOut);
	}

	private void Export()
	{
	}
}
