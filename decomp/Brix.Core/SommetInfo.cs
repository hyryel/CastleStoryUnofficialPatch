using System;
using UnityEngine;

public struct SommetInfo : IComparable<SommetInfo>
{
	public Vector3 vertex;

	public Vector3 normal;

	public Vector2 uv;

	public Vector2 uv2;

	public Vector4 tangent;

	public SommetInfo(Vector3 vertex, Vector3 normal, Vector2 uv, Vector2 uv2, Vector4 tangent)
	{
		this.vertex = vertex;
		this.normal = normal;
		this.uv = uv;
		this.uv2 = uv2;
		this.tangent = tangent;
	}

	public int CompareTo(SommetInfo other)
	{
		if (vertex != other.vertex)
		{
			return vertex.CompareTo(other.vertex);
		}
		if (normal != other.normal)
		{
			return normal.CompareTo(other.normal);
		}
		if (uv != other.uv)
		{
			return uv.CompareTo(other.uv);
		}
		if (uv2 != other.uv2)
		{
			return uv2.CompareTo(other.uv2);
		}
		return tangent.CompareTo(other.tangent);
	}

	public bool Equals(SommetInfo other)
	{
		return vertex == other.vertex && normal == other.normal && uv == other.uv && uv2 == other.uv2 && tangent == other.tangent;
	}
}
