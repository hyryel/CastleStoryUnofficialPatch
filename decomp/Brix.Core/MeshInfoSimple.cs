using UnityEngine;

public class MeshInfoSimple : MeshModifiable
{
	public Vector3[] vertices;

	public Vector3[] normals;

	public Vector2[] uv;

	public Vector2[] uv2;

	public Vector4[] tangents;

	public int[] triangles;

	public override int GetVertexCount()
	{
		return vertices.Length;
	}

	public override int GetTriangleCount()
	{
		return triangles.Length;
	}

	public override Vector3[] GetVertices()
	{
		return vertices;
	}

	public override Vector3[] GetNormals()
	{
		return normals;
	}

	public override Vector2[] GetUv()
	{
		return uv;
	}

	public override Vector2[] GetUv2()
	{
		return uv2;
	}

	public override Vector4[] GetTangents()
	{
		return tangents;
	}

	public override int[] GetTriangles()
	{
		return triangles;
	}

	public override void SetVertices(Vector3[] other)
	{
		vertices = other;
	}

	public override void SetNormals(Vector3[] other)
	{
		normals = other;
	}

	public override void SetUv(Vector2[] other)
	{
		uv = other;
	}

	public override void SetUv2(Vector2[] other)
	{
		uv2 = other;
	}

	public override void SetTangents(Vector4[] other)
	{
		tangents = other;
	}

	public override void SetTriangles(int[] other)
	{
		triangles = other;
	}
}
