using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Legacy;

public class MeshInfoClasse : MeshAugmentable
{
	public Classeur<Vector3> vertices;

	public Classeur<Vector3> normals;

	public Classeur<Vector2> uv;

	public Classeur<Vector2> uv2;

	public Classeur<Vector4> tangents;

	public IntClasse triangles;

	public int index;

	public MeshInfoClasse(Vector3[] _verts, Vector3[] _norm, Vector2[] _uvs, Vector2[] _uv2s, int[] _tri)
	{
		vertices = new Classeur<Vector3>(_verts);
		normals = new Classeur<Vector3>(_norm);
		uv = new Classeur<Vector2>(_uvs);
		uv2 = new Classeur<Vector2>(_uv2s);
		triangles = new IntClasse(_tri);
	}

	public MeshInfoClasse(Array _verts, Array _norm, Array _uvs, Array _uv2s, Array _tri)
		: this((Vector3[])_verts, (Vector3[])_norm, (Vector2[])_uvs, (Vector2[])_uv2s, (int[])_tri)
	{
	}

	public MeshInfoClasse(Mesh mesh)
		: this(mesh.vertices, mesh.normals, mesh.uv, mesh.uv2, mesh.triangles)
	{
		Vector4[] array = mesh.tangents;
		if (array != null)
		{
			tangents = new Classeur<Vector4>(array);
		}
	}

	public MeshInfoClasse(GameObject obj)
		: this(obj.GetComponent<MeshFilter>().mesh)
	{
	}

	public MeshInfoClasse(MeshInfo meshInfo)
		: this(meshInfo.vertices, meshInfo.normals, meshInfo.uv, meshInfo.uv2, meshInfo.triangles)
	{
		if (meshInfo.tangents != null)
		{
			tangents = new Classeur<Vector4>(meshInfo.tangents);
		}
	}

	public MeshInfoClasse(int vn, int tn)
		: this(new Vector3[vn], new Vector3[vn], new Vector2[vn], new Vector2[vn], new int[tn])
	{
	}

	public MeshInfoClasse()
		: this(0, 0)
	{
	}

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
		return vertices.ToBuiltin();
	}

	public override Vector3[] GetNormals()
	{
		return normals.ToBuiltin();
	}

	public override Vector2[] GetUv()
	{
		return uv.ToBuiltin();
	}

	public override Vector2[] GetUv2()
	{
		return uv2.ToBuiltin();
	}

	public override Vector4[] GetTangents()
	{
		return (tangents == null) ? null : tangents.ToBuiltin();
	}

	public override int[] GetTriangles()
	{
		return triangles.ToBuiltin();
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
		if (vertices.Count > 0)
		{
			vertices.Concat(other);
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
			normals.Concat(other);
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
			uv.Concat(other);
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
			uv2.Concat(other);
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
			tangents.Concat(other);
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
			triangles.Concat(other);
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
			tangents = new Classeur<Vector4>();
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

	public Vector3 ComputeNormal(int a, int b, int c)
	{
		try
		{
			Vector3 lhs = vertices[b] - vertices[a];
			Vector3 rhs = vertices[c] - vertices[a];
			return Vector3.Cross(lhs, rhs).normalized;
		}
		catch
		{
			return Vector3.zero;
		}
	}

	public void RecalculateNormals()
	{
		for (int i = 0; i + 2 < triangles.Count; i += 3)
		{
			int num = triangles[i];
			int num2 = triangles[i + 1];
			int num3 = triangles[i + 2];
			Vector3 value = ComputeNormal(num, num2, num3);
			if (num >= 0 && num < normals.Count)
			{
				normals[num] = value;
			}
			if (num2 >= 0 && num2 < normals.Count)
			{
				normals[num2] = value;
			}
			if (num3 >= 0 && num3 < normals.Count)
			{
				normals[num3] = value;
			}
		}
	}

	public void EliminerRedondance()
	{
		Dictionary<SommetInfo, int> dictionary = new Dictionary<SommetInfo, int>();
		Classeur<SommetInfo> classeur = new Classeur<SommetInfo>();
		IntClasse intClasse = new IntClasse();
		int num = 0;
		int value = 0;
		int num2 = 0;
		for (num = 0; num < vertices.Count; num++)
		{
			SommetInfo sommetInfo = new SommetInfo(vertices[num], normals[num], uv[num], uv2[num], (tangents == null) ? Vector4.zero : tangents[num]);
			if (!dictionary.TryGetValue(sommetInfo, out value))
			{
				value = num2++;
				dictionary.Add(sommetInfo, value);
				classeur.Add(sommetInfo);
			}
			intClasse.Add(value);
		}
		vertices.Alloc(num2);
		normals.Alloc(num2);
		uv.Alloc(num2);
		uv2.Alloc(num2);
		if (tangents != null)
		{
			tangents.Alloc(num2);
		}
		for (value = 0; value < num2; value++)
		{
			SommetInfo sommetInfo = classeur[value];
			vertices[value] = sommetInfo.vertex;
			normals[value] = sommetInfo.normal;
			uv[value] = sommetInfo.uv;
			uv2[value] = sommetInfo.uv2;
			if (tangents != null)
			{
				tangents[value] = sommetInfo.tangent;
			}
		}
		for (num = 0; num < triangles.Count; num++)
		{
			triangles[num] = intClasse[triangles[num]];
		}
	}

	public BuiltinMeshInfo ToBuiltin()
	{
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo(vertices.ToBuiltin(), normals.ToBuiltin(), uv.ToBuiltin(), uv2.ToBuiltin(), triangles.ToBuiltin());
		if (MeshModifiable.HasTangents(tangents))
		{
			builtinMeshInfo.tangents = tangents.ToBuiltin();
		}
		return builtinMeshInfo;
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
		mesh.vertices = vertices.ToBuiltin();
		mesh.normals = normals.ToBuiltin();
		mesh.uv = uv.ToBuiltin();
		mesh.uv2 = uv2.ToBuiltin();
		if (MeshModifiable.HasTangents(tangents) && tangents.Length == uv.Length)
		{
			mesh.tangents = tangents.ToBuiltin();
		}
		mesh.triangles = triangles.ToBuiltin();
		return mesh;
	}

	public static string Encoder(MeshInfoClasse meshInfo)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		BuiltinMeshInfo value = (BuiltinMeshInfo)meshInfo.Transcrire(new BuiltinMeshInfo());
		return JsonConvert.SerializeObject(value, Formatting.None, jsonSerializerSettings);
	}

	public MeshModifiable FromString(string s)
	{
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo();
		JsonConvert.PopulateObject(s, builtinMeshInfo);
		return builtinMeshInfo.Transcrire(this);
	}
}
