using System;
using UnityEngine;

public class MeshAugmentablePrudent : MeshAugmentableSimple
{
	public const int maxVertices = 64998;

	public const int maxTriangles = 64998;

	private static T[] Bind<T>(T[] array, int max)
	{
		if (array.Length < max)
		{
			return array;
		}
		array = (T[])array.Clone();
		Array.Resize(ref array, max);
		return array;
	}

	public override void AddVertices(Vector3[] other)
	{
		base.AddVertices(Bind(other, 64998 - vertices.Count));
	}

	public override void AddNormals(Vector3[] other)
	{
		base.AddNormals(Bind(other, 64998 - vertices.Count));
	}

	public override void AddUv(Vector2[] other)
	{
		base.AddUv(Bind(other, 64998 - vertices.Count));
	}

	public override void AddUv2(Vector2[] other)
	{
		base.AddUv2(Bind(other, 64998 - vertices.Count));
	}

	public override void AddTangents(Vector4[] other)
	{
	}

	public override void AddTriangles(int[] other)
	{
		base.AddTriangles(Bind(other, 64998 - triangles.Count));
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2, Vector4 tangent)
	{
		if (vertices.Count >= 64998)
		{
			return vertices.Count - 1;
		}
		return base.Sommet(p, n, uv, uv2, tangent);
	}

	public override int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2)
	{
		if (vertices.Count >= 64998)
		{
			return vertices.Count - 1;
		}
		return base.Sommet(p, n, uv, uv2);
	}

	public override int Triangle(int a, int b, int c)
	{
		if (triangles.Count >= 64998)
		{
			return triangles.Count - 3;
		}
		return base.Triangle(a, b, c);
	}
}
