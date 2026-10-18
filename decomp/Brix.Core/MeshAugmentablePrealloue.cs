using System;
using UnityEngine;

public class MeshAugmentablePrealloue : MeshAugmentable
{
	public Vector3[] vertices;

	public Vector3[] normals;

	public Vector2[] uv;

	public Vector2[] uv2;

	public Vector4[] tangents;

	public int[] triangles;

	public int nv = 0;

	public int nt = 0;

	public MeshAugmentablePrealloue()
	{
	}

	public MeshAugmentablePrealloue(int nv, bool useTangents = false)
	{
		AllocVertices(nv, useTangents);
	}

	public MeshAugmentablePrealloue(int nv, int nt, bool useTangents = false)
	{
		AllocVertices(nv, useTangents);
		AllocTriangles(nt);
	}

	public void AllocVertices(int nv, bool useTangents = false)
	{
		Array.Resize(ref vertices, nv);
		Array.Resize(ref normals, nv);
		Array.Resize(ref uv, nv);
		Array.Resize(ref uv2, nv);
		if (useTangents)
		{
			Array.Resize(ref tangents, nv);
		}
	}

	public void AllocTriangles(int nt)
	{
		Array.Resize(ref triangles, nt);
	}

	public override int GetVertexCount()
	{
		return nv;
	}

	public override int GetTriangleCount()
	{
		return nt;
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

	public override void AddVertices(Vector3[] other)
	{
		nv += other.Length;
		Array.Copy(other, 0, vertices, nv - other.Length, other.Length);
	}

	public override void AddNormals(Vector3[] other)
	{
		Array.Copy(other, 0, normals, nv - other.Length, other.Length);
	}

	public override void AddUv(Vector2[] other)
	{
		Array.Copy(other, 0, uv, nv - other.Length, other.Length);
	}

	public override void AddUv2(Vector2[] other)
	{
		Array.Copy(other, 0, uv2, nv - other.Length, other.Length);
	}

	public override void AddTangents(Vector4[] other)
	{
		if (tangents != null)
		{
			Array.Copy(other, 0, tangents, nv - other.Length, other.Length);
		}
	}

	public override void AddTriangles(int[] other)
	{
		nt += other.Length;
		Array.Copy(other, 0, triangles, nt - other.Length, other.Length);
	}

	public override void Clear(bool realloc)
	{
		if (realloc)
		{
			vertices = null;
			normals = null;
			uv = null;
			uv2 = null;
			tangents = null;
		}
		else
		{
			Array.Clear(triangles, 0, nt);
		}
		nv = (nt = 0);
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2)
	{
		vertices[nv] = p;
		normals[nv] = n;
		this.uv[nv] = uv;
		this.uv2[nv] = uv2;
		return nv++;
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2, Vector4 tangent)
	{
		tangents[nv] = tangent;
		return Sommet(p, n, uv, uv2);
	}

	public override int Triangle(int a, int b, int c)
	{
		triangles[nt++] = a;
		triangles[nt++] = b;
		triangles[nt++] = c;
		return nt - 3;
	}
}
