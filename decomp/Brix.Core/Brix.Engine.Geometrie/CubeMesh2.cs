using UnityEngine;

namespace Brix.Engine.Geometrie;

public class CubeMesh2 : ICubeMeshDescriptor
{
	public static readonly Vector3[] _SharedVertices = new Vector3[8]
	{
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f)
	};

	public static readonly Vector2[] _SharedUvs = new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	public static readonly Vector3[] _SharedNormals = new Vector3[6]
	{
		new Vector3(0f, 1f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 0f, -1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0f, -1f, 0f)
	};

	public static readonly Vector4[] _SharedTangents = new Vector4[6]
	{
		new Vector4(0f, 0f, -1f, -1f),
		new Vector4(-1f, 0f, 0f, -1f),
		new Vector4(1f, 0f, 0f, -1f),
		new Vector4(0f, 0f, 1f, -1f),
		new Vector4(0f, 0f, -1f, -1f),
		new Vector4(0f, 0f, 1f, -1f)
	};

	public static readonly Vector4[] _SharedTangents2 = new Vector4[6]
	{
		new Vector4(-1f, 0f, 0f, -1f),
		new Vector4(0f, -1f, 0f, -1f),
		new Vector4(0f, -1f, 0f, -1f),
		new Vector4(0f, -1f, 0f, -1f),
		new Vector4(0f, -1f, 0f, -1f),
		new Vector4(-1f, 0f, 0f, -1f)
	};

	public static readonly int[] _SharedTriangles = new int[6] { 3, 0, 1, 0, 3, 2 };

	public static readonly int[] _FaceTriangles = new int[36]
	{
		3, 0, 1, 0, 3, 2, 1, 2, 0, 2,
		1, 3, 1, 2, 0, 2, 1, 3, 1, 2,
		0, 2, 1, 3, 1, 2, 0, 2, 1, 3,
		2, 1, 3, 1, 2, 0
	};

	public static readonly int[] _FaceVertexIndices = new int[24]
	{
		1, 3, 0, 2, 1, 0, 5, 4, 2, 3,
		6, 7, 3, 1, 7, 5, 0, 2, 4, 6,
		7, 5, 6, 4
	};

	public static readonly int[] _FaceEdgeIndices = new int[24]
	{
		3, 2, 4, 1, 0, 4, 5, 3, 0, 3,
		5, 4, 0, 1, 5, 2, 0, 2, 5, 1,
		3, 1, 4, 2
	};

	public static readonly int[] _VertexFaceIndices = new int[24]
	{
		0, 1, 4, 0, 1, 3, 0, 2, 4, 0,
		2, 3, 1, 4, 5, 1, 3, 5, 2, 4,
		5, 2, 3, 5
	};

	public Vector3[] SharedVertices => _SharedVertices;

	public Vector2[] SharedUvs => _SharedUvs;

	public Vector3[] SharedNormals => _SharedNormals;

	public Vector4[] SharedTangents => _SharedTangents;

	public Vector4[] SharedTangents2 => _SharedTangents2;

	public int[] SharedTriangles => _SharedTriangles;

	public int[] FaceTriangles => _FaceTriangles;

	public int[] FaceVertexIndices => _FaceVertexIndices;

	public int[] FaceEdgeIndices => _FaceEdgeIndices;

	public int[] VertexFaceIndices => _VertexFaceIndices;
}
