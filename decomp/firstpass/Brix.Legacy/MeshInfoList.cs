using System.Collections.Generic;
using UnityEngine;

namespace Brix.Legacy;

public class MeshInfoList : MeshAugmentable
{
	public List<Vector3> vertices = new List<Vector3>();

	public List<Vector3> normals = new List<Vector3>();

	public List<Vector2> uv = new List<Vector2>();

	public List<Vector2> uv2 = new List<Vector2>();

	public List<Vector4> tangents;

	public List<int> triangles = new List<int>();

	public override int GetVertexCount()
	{
		return vertices.Count;
	}

	public override int GetTriangleCount()
	{
		return triangles.Count;
	}

	public override Vector3[] GetVertices()
	{
		return vertices.ToArray();
	}

	public override Vector3[] GetNormals()
	{
		return normals.ToArray();
	}

	public override Vector2[] GetUv()
	{
		return uv.ToArray();
	}

	public override Vector2[] GetUv2()
	{
		return uv2.ToArray();
	}

	public override Vector4[] GetTangents()
	{
		return (tangents == null) ? null : tangents.ToArray();
	}

	public override int[] GetTriangles()
	{
		return triangles.ToArray();
	}

	public override void SetVertices(Vector3[] other)
	{
		vertices = new List<Vector3>(other);
	}

	public override void SetNormals(Vector3[] other)
	{
		normals = new List<Vector3>(other);
	}

	public override void SetUv(Vector2[] other)
	{
		uv = new List<Vector2>(other);
	}

	public override void SetUv2(Vector2[] other)
	{
		uv2 = new List<Vector2>(other);
	}

	public override void SetTangents(Vector4[] other)
	{
		tangents = new List<Vector4>(other);
	}

	public override void SetTriangles(int[] other)
	{
		triangles = new List<int>(other);
	}

	public override void AddVertices(Vector3[] other)
	{
		if (vertices.Count > 0)
		{
			vertices.AddRange(other);
		}
		else
		{
			SetVertices(other);
		}
	}

	public override void AddNormals(Vector3[] other)
	{
		if (normals.Count > 0)
		{
			normals.AddRange(other);
		}
		else
		{
			SetNormals(other);
		}
	}

	public override void AddUv(Vector2[] other)
	{
		if (uv.Count > 0)
		{
			uv.AddRange(other);
		}
		else
		{
			SetUv(other);
		}
	}

	public override void AddUv2(Vector2[] other)
	{
		if (uv2.Count > 0)
		{
			uv2.AddRange(other);
		}
		else
		{
			SetUv2(other);
		}
	}

	public override void AddTangents(Vector4[] other)
	{
		if (tangents != null && tangents.Count > 0)
		{
			tangents.AddRange(other);
		}
		else
		{
			SetTangents(other);
		}
	}

	public override void AddTriangles(int[] other)
	{
		if (triangles.Count > 0)
		{
			triangles.AddRange(other);
		}
		else
		{
			SetTriangles(other);
		}
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2)
	{
		vertices.Add(p);
		normals.Add(n);
		this.uv.Add(uv);
		this.uv2.Add(uv2);
		return vertices.Count - 1;
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2, Vector4 tangent)
	{
		if (tangents == null)
		{
			tangents = new List<Vector4>();
		}
		tangents.Add(tangent);
		return Sommet(p, n, uv, uv2);
	}

	public override int Triangle(int a, int b, int c)
	{
		triangles.Add(a);
		triangles.Add(b);
		triangles.Add(c);
		return triangles.Count - 3;
	}

	public override void Clear()
	{
		vertices.Clear();
		normals.Clear();
		uv.Clear();
		uv2.Clear();
		tangents = null;
		triangles.Clear();
	}

	public override Mesh Rendre(Mesh mesh)
	{
		mesh.Clear();
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.SetUVs(0, uv);
		mesh.SetUVs(1, uv2);
		if (MeshModifiable.HasTangents(tangents) && tangents.Count == uv.Count)
		{
			mesh.SetTangents(tangents);
		}
		mesh.SetTriangles(triangles, 0);
		return mesh;
	}
}
