using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

[JsonObject(MemberSerialization.OptIn)]
public class QuaternionDTO
{
	[JsonProperty]
	private float x;

	[JsonProperty]
	private float y;

	[JsonProperty]
	private float z;

	[JsonProperty]
	private float w;

	public QuaternionDTO()
	{
	}

	public QuaternionDTO(Quaternion q)
	{
		x = q.x;
		y = q.y;
		z = q.z;
		w = q.w;
	}

	public QuaternionDTO(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public bool IsDefaultRotation()
	{
		return this == DefaultRotationQuaternionDTO();
	}

	public static QuaternionDTO DefaultRotationQuaternionDTO()
	{
		return new QuaternionDTO(0f, 0f, 0f, 1f);
	}

	public static implicit operator QuaternionDTO(Quaternion q)
	{
		return new QuaternionDTO(q);
	}

	public static implicit operator Quaternion(QuaternionDTO q)
	{
		return new Quaternion(q.x, q.y, q.z, q.w);
	}

	protected bool Equals(QuaternionDTO other)
	{
		return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
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
		return Equals((QuaternionDTO)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = x.GetHashCode();
		hashCode = (hashCode * 397) ^ y.GetHashCode();
		hashCode = (hashCode * 397) ^ z.GetHashCode();
		return (hashCode * 397) ^ w.GetHashCode();
	}

	public static bool operator ==(QuaternionDTO left, QuaternionDTO right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(QuaternionDTO left, QuaternionDTO right)
	{
		return !object.Equals(left, right);
	}
}
