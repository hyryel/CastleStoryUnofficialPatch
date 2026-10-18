using UnityEngine;

public abstract class MeshAugmentable : MeshModifiable
{
	public int Sommet(Vector3 p, Vector3 n, Vector2 uv)
	{
		return Sommet(p, n, uv, Vector2.zero);
	}

	public abstract int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2);

	public abstract int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2, Vector4 tangent);

	public abstract int Triangle(int a, int b, int c);

	public abstract void AddVertices(Vector3[] other);

	public abstract void AddNormals(Vector3[] other);

	public abstract void AddUv(Vector2[] other);

	public abstract void AddUv2(Vector2[] other);

	public abstract void AddTangents(Vector4[] other);

	public abstract void AddTriangles(int[] other);

	public virtual void InitTangents(int want)
	{
		Vector4[] tangents = GetTangents();
		int num = ((tangents != null) ? tangents.Length : 0);
		AddTangents(new Vector4[want - num]);
	}

	public int DoubleTriangle(int a, int b, int c)
	{
		Triangle(a, b, c);
		return Triangle(b, a, c);
	}

	public int Quad(int a, int b, int c, int d)
	{
		Triangle(a, b, c);
		return Triangle(a, c, d);
	}

	public int DoubleQuad(int a, int b, int c, int d)
	{
		Quad(a, b, c, d);
		return Quad(d, c, b, a);
	}

	public virtual void AddMesh(MeshModifiable mesh)
	{
		if (mesh == null)
		{
			return;
		}
		int vertexCount = GetVertexCount();
		int[] triangles = mesh.GetTriangles();
		if (triangles != null && triangles.Length != 0)
		{
			if (vertexCount > 0)
			{
				for (int i = 0; i < triangles.Length; i++)
				{
					triangles[i] += vertexCount;
				}
			}
			AddTriangles(triangles);
			if (vertexCount > 0)
			{
				for (int j = 0; j < triangles.Length; j++)
				{
					triangles[j] -= vertexCount;
				}
			}
		}
		Vector3[] vertices = mesh.GetVertices();
		if (vertices != null && vertices.Length != 0)
		{
			AddVertices(vertices);
		}
		Vector3[] normals = mesh.GetNormals();
		if (normals != null && normals.Length != 0)
		{
			AddNormals(normals);
		}
		Vector2[] uv = mesh.GetUv();
		if (uv != null && uv.Length != 0)
		{
			AddUv(uv);
		}
		Vector2[] uv2 = mesh.GetUv2();
		if (uv2 != null && uv2.Length != 0)
		{
			AddUv2(uv2);
		}
		Vector4[] tangents = mesh.GetTangents();
		if (MeshModifiable.HasTangents(tangents))
		{
			InitTangents(vertexCount);
			AddTangents(tangents);
		}
		else if (HasTangents())
		{
			InitTangents(GetVertexCount());
		}
	}
}
