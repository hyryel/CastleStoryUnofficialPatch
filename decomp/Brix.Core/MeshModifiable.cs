using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MeshModifiable : MeshAbstrait
{
	public abstract Vector3[] GetVertices();

	public abstract Vector3[] GetNormals();

	public abstract Vector2[] GetUv();

	public abstract Vector2[] GetUv2();

	public abstract Vector4[] GetTangents();

	public abstract int[] GetTriangles();

	public abstract void SetVertices(Vector3[] other);

	public abstract void SetNormals(Vector3[] other);

	public abstract void SetUv(Vector2[] other);

	public abstract void SetUv2(Vector2[] other);

	public abstract void SetTangents(Vector4[] other);

	public abstract void SetTriangles(int[] other);

	public virtual void Clear()
	{
		Clear(realloc: true);
	}

	public virtual void Clear(bool realloc)
	{
		SetVertices(null);
		SetNormals(null);
		SetUv(null);
		SetUv2(null);
		SetTriangles(null);
	}

	public bool HasTangents()
	{
		return HasTangents(GetTangents());
	}

	public static bool HasTangents(IEnumerable<Vector4> tangents)
	{
		return tangents?.Any() ?? false;
	}

	public bool HasUv2()
	{
		return HasUv2(GetUv2());
	}

	public static bool HasUv2(IEnumerable<Vector2> uv2)
	{
		return uv2?.Any() ?? false;
	}

	public override MeshModifiable Transcrire(MeshModifiable meshDst)
	{
		meshDst.Clear();
		meshDst.SetVertices(GetVertices());
		meshDst.SetNormals(GetNormals());
		meshDst.SetUv(GetUv());
		meshDst.SetUv2(GetUv2());
		meshDst.SetTangents(GetTangents());
		meshDst.SetTriangles(GetTriangles());
		return meshDst;
	}

	public virtual MeshModifiable Lire(Mesh meshIn)
	{
		Clear();
		SetVertices(meshIn.vertices);
		SetNormals(meshIn.normals);
		SetUv(meshIn.uv);
		SetUv2(meshIn.uv2);
		SetTangents(meshIn.tangents);
		SetTriangles(meshIn.triangles);
		return this;
	}

	public virtual MeshModifiable Lire(Primitive meshIn)
	{
		Clear();
		SetVertices(meshIn.vertices);
		SetNormals(meshIn.normals);
		SetUv(meshIn.uv);
		SetUv2(meshIn.uv2);
		SetTangents(meshIn.tangents);
		SetTriangles(meshIn.triangles);
		Clone().Transcrire(this);
		return this;
	}

	public override Mesh Rendre(Mesh meshOut)
	{
		if (meshOut == null)
		{
			meshOut = new Mesh();
		}
		else
		{
			meshOut.Clear();
		}
		meshOut.vertices = GetVertices();
		meshOut.normals = GetNormals();
		meshOut.uv = GetUv();
		meshOut.uv2 = GetUv2();
		meshOut.tangents = GetTangents();
		meshOut.triangles = GetTriangles();
		return meshOut;
	}

	public override Mesh Rendre(Mesh meshOut, Matrix4x4 matrix, MeshRendreOptions option = MeshRendreOptions.Fast)
	{
		MeshModifiable meshModifiable = ((option != MeshRendreOptions.Fast) ? this : Clone());
		meshModifiable.Transform(matrix);
		meshOut = meshModifiable.Rendre(meshOut);
		if (option == MeshRendreOptions.LowMemory)
		{
			meshModifiable.Transform(matrix.inverse);
		}
		return meshOut;
	}

	public MeshModifiable Clone()
	{
		MeshInfoSimple meshInfoSimple = new MeshInfoSimple();
		meshInfoSimple.SetVertices((Vector3[])GetVertices().Clone());
		meshInfoSimple.SetNormals((Vector3[])GetNormals().Clone());
		meshInfoSimple.SetUv((Vector2[])GetUv().Clone());
		meshInfoSimple.SetUv2((Vector2[])GetUv2().Clone());
		meshInfoSimple.SetTriangles((int[])GetTriangles().Clone());
		Vector4[] tangents = GetTangents();
		if (HasTangents(tangents))
		{
			meshInfoSimple.SetTangents((Vector4[])tangents.Clone());
		}
		return meshInfoSimple;
	}

	public virtual void Transform(Matrix4x4 matrix)
	{
		Transform(matrix, matrix);
	}

	public virtual void Transform(Matrix4x4 matrix, Vector3 normalCorrectionFactor)
	{
		Matrix4x4 normalMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, normalCorrectionFactor) * matrix;
		Transform(matrix, normalMatrix);
	}

	public virtual void Transform(Matrix4x4 matrix, Matrix4x4 normalMatrix)
	{
		Matrix4x4 matrix4x = normalMatrix;
		Vector3[] vertices = GetVertices();
		if (vertices != null && vertices.Length != 0)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = matrix.MultiplyPoint(vertices[i]);
			}
			SetVertices(vertices);
		}
		Vector3[] normals = GetNormals();
		if (normals != null && normals.Length != 0)
		{
			for (int j = 0; j < normals.Length; j++)
			{
				normals[j] = normalMatrix.MultiplyVector(normals[j]);
			}
			SetNormals(normals);
		}
		Vector4[] tangents = GetTangents();
		if (tangents != null && tangents.Length != 0)
		{
			for (int k = 0; k < tangents.Length; k++)
			{
				Vector4 vector = tangents[k];
				Vector4 vector2 = matrix4x.MultiplyVector(tangents[k]);
				vector2.w = vector.w;
				tangents[k] = vector2;
			}
			SetTangents(tangents);
		}
	}
}
