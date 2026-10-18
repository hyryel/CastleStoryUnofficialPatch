using UnityEngine;

public class MeshAugmentableSimple : MeshAugmentable
{
	public Classeur<Vector3> vertices = new Classeur<Vector3>();

	public Classeur<Vector3> normals = new Classeur<Vector3>();

	public Classeur<Vector2> uv = new Classeur<Vector2>();

	public Classeur<Vector2> uv2 = new Classeur<Vector2>();

	public Classeur<Vector4> tangents;

	public IntClasse triangles = new IntClasse();

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

	public override void Clear(bool realloc)
	{
		vertices.Clear(realloc);
		normals.Clear(realloc);
		uv.Clear(realloc);
		uv2.Clear(realloc);
		if (tangents != null)
		{
			tangents.Clear(realloc);
		}
		triangles.Clear(realloc);
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
}
