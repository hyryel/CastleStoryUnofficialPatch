using System;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Legacy;

public class MeshInfo : MeshAugmentable
{
	public Vector3[] vertices;

	public Vector3[] normals;

	public Vector2[] uv;

	public Vector2[] uv2;

	public Vector4[] tangents;

	public int[] triangles;

	public int index;

	public MeshInfo(Vector3[] _verts, Vector3[] _norm, Vector2[] _uvs, Vector2[] _uv2s, int[] _tri)
	{
		vertices = _verts;
		normals = _norm;
		uv = _uvs;
		uv2 = _uv2s;
		triangles = _tri;
	}

	public MeshInfo(Array _verts, Array _norm, Array _uvs, Array _uv2s, Array _tri)
	{
		vertices = (Vector3[])_verts;
		normals = (Vector3[])_norm;
		uv = (Vector2[])_uvs;
		uv2 = (Vector2[])_uv2s;
		triangles = (int[])_tri;
	}

	public MeshInfo(Mesh mesh)
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

	public MeshInfo(GameObject obj)
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

	public MeshInfo()
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

	public override void AddVertices(Vector3[] other)
	{
		if (vertices.Length > 0)
		{
			vertices.Slinq().Concat(other.Slinq());
		}
		else
		{
			SetVertices(other);
		}
	}

	public override void AddNormals(Vector3[] other)
	{
		if (normals.Length > 0)
		{
			normals.Slinq().Concat(other.Slinq());
		}
		else
		{
			SetNormals(other);
		}
	}

	public override void AddUv(Vector2[] other)
	{
		if (uv.Length > 0)
		{
			uv.Slinq().Concat(other.Slinq());
		}
		else
		{
			SetUv(other);
		}
	}

	public override void AddUv2(Vector2[] other)
	{
		if (uv2.Length > 0)
		{
			uv2.Slinq().Concat(other.Slinq());
		}
		else
		{
			SetUv2(other);
		}
	}

	public override void AddTangents(Vector4[] other)
	{
		if (tangents != null && tangents.Length > 0)
		{
			tangents.Slinq().Concat(other.Slinq());
		}
		else
		{
			SetTangents(other);
		}
	}

	public override void AddTriangles(int[] other)
	{
		if (triangles.Length > 0)
		{
			triangles.Slinq().Concat(other.Slinq());
		}
		else
		{
			SetTriangles(other);
		}
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2)
	{
		vertices = vertices.Add(p);
		normals = normals.Add(n);
		this.uv = this.uv.Add(uv);
		this.uv2 = this.uv2.Add(uv2);
		return vertices.Length - 1;
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2, Vector4 tangent)
	{
		tangents = tangents.Add(tangent);
		return Sommet(p, n, uv, uv2);
	}

	public override int Triangle(int a, int b, int c)
	{
		triangles = triangles.Add(a);
		triangles = triangles.Add(b);
		triangles = triangles.Add(c);
		return triangles.Length - 3;
	}

	public new void Rendre(Mesh mesh)
	{
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.uv2 = uv2;
		if (tangents != null && tangents.Length != 0)
		{
			mesh.tangents = tangents;
		}
		mesh.triangles = triangles;
	}

	public void Add(Mesh mesh)
	{
		Vector3[] array = mesh.vertices;
		foreach (Vector3 item in array)
		{
			vertices = vertices.Add(item);
		}
		Vector3[] array2 = mesh.normals;
		foreach (Vector3 item2 in array2)
		{
			normals = normals.Add(item2);
		}
		Vector2[] array3 = mesh.uv;
		foreach (Vector2 item3 in array3)
		{
			uv = uv.Add(item3);
		}
		Vector2[] array4 = mesh.uv2;
		foreach (Vector2 item4 in array4)
		{
			uv2 = uv2.Add(item4);
		}
		int num = triangles.Length;
		int[] array5 = mesh.triangles;
		foreach (int num2 in array5)
		{
			triangles = triangles.Add(num2 + num);
		}
		Vector4[] array6 = mesh.tangents;
		if (array6 != null)
		{
			if (tangents != null)
			{
				tangents = new Vector4[0];
			}
			Vector4[] array7 = array6;
			foreach (Vector4 item5 in array7)
			{
				tangents = tangents.Add(item5);
			}
		}
	}

	public static void TranscrireMesh(Mesh meshSrc, Mesh meshDst)
	{
		if (!(meshDst == meshSrc))
		{
			meshDst.Clear();
			meshDst.vertices = meshSrc.vertices;
			meshDst.normals = meshSrc.normals;
			meshDst.uv = meshSrc.uv;
			meshDst.uv2 = meshSrc.uv2;
			meshDst.triangles = meshSrc.triangles;
			Vector4[] array = meshSrc.tangents;
			if (array != null && array.Length == 0)
			{
				array = null;
			}
			meshDst.tangents = array;
		}
	}

	public static void AssertCount<T>(T[] aOut, int start, ref int count)
	{
		if (count > aOut.Length - start)
		{
			count = aOut.Length - start;
		}
	}

	public static void CopyVertices(Vector3[] vIn, Vector3[] vOut, int start, int count)
	{
		AssertCount(vOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref vOut[start + i];
			reference = vIn[i];
		}
	}

	public static void CopyVertices(Vector3[] vIn, Vector3[] vOut, Vector3 translation, int start, int count)
	{
		AssertCount(vOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref vOut[start + i];
			reference = vIn[i] + translation;
		}
	}

	public static void CopyVertices(Vector3[] vIn, Vector3[] vOut, Vector3 translation, float scale, int start, int count)
	{
		AssertCount(vOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref vOut[start + i];
			reference = vIn[i] * scale + translation;
		}
	}

	public static void CopyVertices(Vector3[] vIn, Vector3[] vOut, Vector3 translation, Quaternion rotation, int start, int count)
	{
		AssertCount(vOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref vOut[start + i];
			reference = rotation * vIn[i] + translation;
		}
	}

	public static void CopyVertices(Vector3[] vIn, Vector3[] vOut, Matrix4x4 transform, int start, int count)
	{
		AssertCount(vOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref vOut[start + i];
			reference = transform.MultiplyPoint(vIn[i]);
		}
	}

	public static void CopyNormals(Vector3[] nIn, Vector3[] nOut, int start, int count)
	{
		AssertCount(nOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref nOut[start + i];
			reference = nIn[i];
		}
	}

	public static void CopyNormals(Vector3[] nIn, Vector3[] nOut, Quaternion rotation, int start, int count)
	{
		AssertCount(nOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref nOut[start + i];
			reference = rotation * nIn[i];
		}
	}

	public static void CopyNormals(Vector3[] nIn, Vector3[] nOut, Matrix4x4 transform, int start, int count)
	{
		AssertCount(nOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector3 reference = ref nOut[start + i];
			reference = transform.MultiplyVector(nIn[i]);
		}
	}

	public static void CopyTangents(Vector4[] tIn, Vector4[] tOut, int start, int count)
	{
		AssertCount(tOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector4 reference = ref tOut[start + i];
			reference = tIn[i];
		}
	}

	public static void CopyTangents(Vector4[] tIn, Vector4[] tOut, Quaternion rotation, int start, int count)
	{
		AssertCount(tOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			Vector4 vector = tIn[i];
			Vector3 vector2 = rotation * vector;
			ref Vector4 reference = ref tOut[start + i];
			reference = new Vector4(vector2.x, vector2.y, vector2.z, vector.w);
		}
	}

	public static void CopyTangents(Vector4[] tIn, Vector4[] tOut, Matrix4x4 transform, int start, int count)
	{
		AssertCount(tOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			Vector4 vector = tIn[i];
			Vector3 vector2 = transform.MultiplyVector(vector);
			ref Vector4 reference = ref tOut[start + i];
			reference = new Vector4(vector2.x, vector2.y, vector2.z, vector.w);
		}
	}

	public static void CopyUv(Vector2[] uvIn, Vector2[] uvOut, int start, int count)
	{
		AssertCount(uvOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			ref Vector2 reference = ref uvOut[start + i];
			reference = uvIn[i];
		}
	}

	public static void SetUv(Vector2 uvIn, Vector2[] uvOut, int start, int count)
	{
		AssertCount(uvOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			uvOut[start + i] = uvIn;
		}
	}

	public static void CopyTriangles(int[] tIn, int[] tOut, int vertexOffset, int start, int count)
	{
		AssertCount(tOut, start, ref count);
		for (int i = 0; i < count; i++)
		{
			tOut[start + i] = tIn[i] + vertexOffset;
		}
	}
}
