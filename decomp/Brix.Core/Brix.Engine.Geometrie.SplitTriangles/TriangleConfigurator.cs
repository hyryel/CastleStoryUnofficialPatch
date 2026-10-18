using Brix.Util;
using UnityEngine;

namespace Brix.Engine.Geometrie.SplitTriangles;

public class TriangleConfigurator : MonoBehaviour
{
	[HideInInspector]
	public int triangleIndex;

	[HideInInspector]
	public Mesh mesh;

	[HideInInspector]
	public int[] triangles;

	[HideInInspector]
	public Vector3[] vertices;

	[HideInInspector]
	public Vector3[] normals;

	[HideInInspector]
	public Vector2[] uv;

	[HideInInspector]
	public Vector2[] uv2;

	[HideInInspector]
	public Vector4[] tangents;

	[HideInInspector]
	public string label;

	[HideInInspector]
	public Vector3 avgNormal;

	[HideInInspector]
	public Vector3 avgVertex;

	public int TriangleIndex
	{
		set
		{
			triangleIndex = value;
		}
	}

	private void Awake()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		mesh = component.sharedMesh;
		triangles = mesh.triangles;
		vertices = mesh.vertices;
		normals = mesh.normals;
		uv = mesh.uv;
		uv2 = mesh.uv2;
		tangents = mesh.tangents;
		avgNormal = base.transform.TransformVector(Calcule.AverageVector(normals));
		avgVertex = base.transform.TransformPoint(Calcule.AverageVector(vertices));
	}

	private void Start()
	{
		label = string.Concat(triangleIndex + 1);
	}
}
