using UnityEngine;

public class MeshAugmentableDebordant : MeshAugmentableSimple
{
	public float facteurDeDebordement = 0.1f;

	public MeshAugmentableDebordant(float facteurDeDebordement)
	{
		this.facteurDeDebordement = facteurDeDebordement;
	}

	private T[] PrepareArray<T>(Classeur<T> c)
	{
		if ((float)c.AvailableSpace > (float)c.Count * facteurDeDebordement)
		{
			c.RemoveInternalArrayPadding();
		}
		return c.GetInternalArray();
	}

	private int[] PrepareTriangles(IntClasse c)
	{
		if ((float)c.AvailableSpace > (float)c.Count * facteurDeDebordement || c.AvailableSpace % 3 != 0)
		{
			c.RemoveInternalArrayPadding();
		}
		else
		{
			c.ClearInternalArrayPadding();
		}
		return c.GetInternalArray();
	}

	public override Vector3[] GetVertices()
	{
		return PrepareArray(vertices);
	}

	public override Vector3[] GetNormals()
	{
		return PrepareArray(normals);
	}

	public override Vector2[] GetUv()
	{
		return PrepareArray(uv);
	}

	public override Vector2[] GetUv2()
	{
		return PrepareArray(uv2);
	}

	public override Vector4[] GetTangents()
	{
		return (tangents != null) ? PrepareArray(tangents) : null;
	}

	public override int[] GetTriangles()
	{
		return PrepareTriangles(triangles);
	}

	public override void Clear()
	{
		Clear(realloc: false);
	}
}
