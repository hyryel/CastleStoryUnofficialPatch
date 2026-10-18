using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

[JsonObject(MemberSerialization.OptIn)]
public class Vector3DTO
{
	[JsonProperty]
	private float x;

	[JsonProperty]
	private float y;

	[JsonProperty]
	private float z;

	public Vector3DTO()
	{
	}

	public Vector3DTO(Vector3 v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public Vector3DTO(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public bool IsDefaultPosition()
	{
		return this == DefaultPositionVector3DTO();
	}

	public bool IsDefaultLocalScale()
	{
		return this == DefaultLocalScaleVector3DTO();
	}

	public static Vector3DTO DefaultPositionVector3DTO()
	{
		return new Vector3DTO(0f, 0f, 0f);
	}

	public static Vector3DTO DefaultLocalScaleVector3DTO()
	{
		return new Vector3DTO(1f, 1f, 1f);
	}

	public static implicit operator Vector3DTO(Vector3 t)
	{
		return new Vector3DTO(t);
	}

	public static implicit operator Vector3(Vector3DTO t)
	{
		return new Vector3(t.x, t.y, t.z);
	}

	protected bool Equals(Vector3DTO other)
	{
		return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((Vector3DTO)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = x.GetHashCode();
		hashCode = (hashCode * 397) ^ y.GetHashCode();
		return (hashCode * 397) ^ z.GetHashCode();
	}

	public static bool operator ==(Vector3DTO left, Vector3DTO right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(Vector3DTO left, Vector3DTO right)
	{
		return !object.Equals(left, right);
	}
}
