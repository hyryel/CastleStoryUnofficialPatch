using System;
using System.Linq;
using UnityEngine;

namespace Brix.Legacy;

public class BuiltinMeshInfo : MeshModifiable
{
	public Vector3[] vertices;

	public Vector3[] normals;

	public Vector2[] uv;

	public Vector2[] uv2;

	public Vector4[] tangents;

	public int[] triangles;

	public int index;

	public BuiltinMeshInfo(Vector3[] _verts, Vector3[] _norm, Vector2[] _uvs, Vector2[] _uv2s, int[] _tri, Vector4[] _tan)
	{
		vertices = (Vector3[])_verts.Clone();
		normals = (Vector3[])_norm.Clone();
		uv = (Vector2[])_uvs.Clone();
		uv2 = (Vector2[])_uv2s.Clone();
		triangles = (int[])_tri.Clone();
		if (MeshModifiable.HasTangents(_tan))
		{
			tangents = (Vector4[])_tan.Clone();
		}
	}

	public BuiltinMeshInfo(Array _verts, Array _norm, Array _uvs, Array _uv2s, Array _tri)
	{
		vertices = _verts.Cast<Vector3>().ToArray();
		normals = _norm.Cast<Vector3>().ToArray();
		uv = _uvs.Cast<Vector2>().ToArray();
		uv2 = _uv2s.Cast<Vector2>().ToArray();
		triangles = _tri.Cast<int>().ToArray();
	}

	public BuiltinMeshInfo(Mesh mesh)
	{
		vertices = mesh.vertices;
		normals = mesh.normals;
		uv = mesh.uv;
		uv2 = mesh.uv2;
		tangents = mesh.tangents;
		if (tangents != null && tangents.Length == 0)
		{
			tangents = null;
		}
		triangles = mesh.triangles;
	}

	public BuiltinMeshInfo(GameObject obj)
	{
		Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
		vertices = mesh.vertices;
		normals = mesh.normals;
		uv = mesh.uv;
		uv2 = mesh.uv2;
		tangents = mesh.tangents;
		if (tangents != null && tangents.Length == 0)
		{
			tangents = null;
		}
		triangles = mesh.triangles;
	}

	public BuiltinMeshInfo(Mesh mesh, Matrix4x4 transform)
	{
		vertices = mesh.vertices;
		normals = mesh.normals;
		uv = mesh.uv;
		uv2 = mesh.uv2;
		tangents = mesh.tangents;
		if (tangents != null && tangents.Length == 0)
		{
			tangents = null;
		}
		triangles = mesh.triangles;
		ProceduralCube.MeshTransform(this, transform);
	}

	public BuiltinMeshInfo(MeshAbstrait meshIn)
	{
		MeshModifiable modifiable = meshIn.GetModifiable();
		vertices = modifiable.GetVertices();
		normals = modifiable.GetNormals();
		uv = modifiable.GetUv();
		uv2 = modifiable.GetUv2();
		tangents = modifiable.GetTangents();
		triangles = modifiable.GetTriangles();
	}

	public BuiltinMeshInfo(int vn, int tn)
	{
		vertices = new Vector3[vn];
		normals = new Vector3[vn];
		uv = new Vector2[vn];
		uv2 = new Vector2[vn];
		triangles = new int[tn];
	}

	public BuiltinMeshInfo()
	{
		vertices = new Vector3[0];
		normals = new Vector3[0];
		uv = new Vector2[0];
		uv2 = new Vector2[0];
		triangles = new int[0];
	}

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

	public override Mesh Rendre(Mesh mesh)
	{
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.uv2 = uv2;
		if (MeshModifiable.HasTangents(tangents))
		{
			mesh.tangents = tangents;
		}
		mesh.triangles = triangles;
		return mesh;
	}
}
