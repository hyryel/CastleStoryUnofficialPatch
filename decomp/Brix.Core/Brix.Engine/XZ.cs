using System;
using Brix.Engine.Abstract;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public struct XZ : IComparable<XZ>, IEquatable<XZ>
{
	public int x;

	public int z;

	public static XZ zero = new XZ
	{
		x = 0,
		z = 0
	};

	public static XZ right = new XZ
	{
		x = 1,
		z = 0
	};

	public static XZ forward = new XZ
	{
		x = 0,
		z = 1
	};

	public static XZ one = new XZ
	{
		x = 1,
		z = 1
	};

	public bool Equals(XZ other)
	{
		return x == other.x && z == other.z;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is XZ && Equals((XZ)obj);
	}

	public override int GetHashCode()
	{
		int num = x;
		return (num * 397) ^ z;
	}

	public XZ(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public static XZ FromXYZ(XYZ xyz)
	{
		return new XZ(xyz.x, xyz.z);
	}

	public static XZ FromVector2(Vector2 v)
	{
		return Round(v.x, v.y);
	}

	public static XZ FromVector3(Vector3 v)
	{
		return Round(v.x, v.z);
	}

	public static XZ FromScalaire(int scalaire)
	{
		return new XZ(Voxel.GetX(scalaire), Voxel.GetZ(scalaire));
	}

	public XYZ ToXYZ()
	{
		return new XYZ(x, 0, z);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, z);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, 0f, z);
	}

	public int ToScalaire()
	{
		return Voxel.GetScalaire(x, 0, z);
	}

	public static XZ Round(float x, float z)
	{
		return new XZ(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
	}

	public static XZ Floor(float x, float z)
	{
		return new XZ(Mathf.FloorToInt(x), Mathf.FloorToInt(z));
	}

	public static XZ Ceil(float x, float z)
	{
		return new XZ(Mathf.CeilToInt(x), Mathf.CeilToInt(z));
	}

	public XZ Plus(int x, int z)
	{
		this.x += x;
		this.z += z;
		return this;
	}

	public static XZ Min(XZ a, XZ b)
	{
		a.x = Mathf.Min(a.x, b.x);
		a.z = Mathf.Min(a.z, b.z);
		return a;
	}

	public static XZ Max(XZ a, XZ b)
	{
		a.x = Mathf.Max(a.x, b.x);
		a.z = Mathf.Max(a.z, b.z);
		return a;
	}

	public static XZ Min(XZ a, XZ b, XZ c)
	{
		a.x = Mathf.Min(a.x, b.x, c.x);
		a.z = Mathf.Min(a.z, b.z, c.z);
		return a;
	}

	public static XZ Max(XZ a, XZ b, XZ c)
	{
		a.x = Mathf.Max(a.x, b.x, c.x);
		a.z = Mathf.Max(a.z, b.z, c.z);
		return a;
	}

	public static XZ Clamp(XZ a, XZ b, XZ c)
	{
		a.x = Mathf.Clamp(a.x, b.x, c.x);
		a.z = Mathf.Clamp(a.z, b.z, c.z);
		return a;
	}

	public static XZ Abs(XZ a)
	{
		a.x = Mathf.Abs(a.x);
		a.z = Mathf.Abs(a.z);
		return a;
	}

	public static XZ Sqr(XZ a)
	{
		a.x *= a.x;
		a.z *= a.z;
		return a;
	}

	public static XZ MinAbs(XZ a, XZ b)
	{
		a.x = Mathf.Min(Mathf.Abs(a.x), Mathf.Abs(b.x));
		a.z = Mathf.Min(Mathf.Abs(a.z), Mathf.Abs(b.z));
		return a;
	}

	public static XZ MinTowardsZero(XZ a, XZ b)
	{
		a.x = Calcule.MinTowardsZero(a.x, b.x);
		a.z = Calcule.MinTowardsZero(a.z, b.z);
		return a;
	}

	public static XZ Mul(XZ a, XZ b)
	{
		a.x *= b.x;
		a.z *= b.z;
		return a;
	}

	public static XZ Div(XZ a, XZ b)
	{
		a.x /= b.x;
		a.z /= b.z;
		return a;
	}

	public static XZ Mod(XZ a, XZ b)
	{
		a.x %= b.x;
		a.z %= b.z;
		return a;
	}

	public static int Dot(XZ a, XZ b)
	{
		return a.x * b.x + a.z * b.z;
	}

	public static int SqrMagnitude(XZ a)
	{
		return Dot(a, a);
	}

	public static int ManhattanMagnitude(XZ a)
	{
		return Mathf.Abs(a.x) + Mathf.Abs(a.z);
	}

	public static int MaxMagnitude(XZ a)
	{
		return Mathf.Max(Mathf.Abs(a.x), Mathf.Abs(a.z));
	}

	public static bool InHalfInterval(XZ p, XZ a, XZ b)
	{
		return p.x >= a.x && p.z >= a.z && p.x < b.x && p.z < b.z;
	}

	public static XZ Step(XZ a, XZ b)
	{
		return new XZ((b.x >= a.x) ? 1 : 0, (b.z >= a.z) ? 1 : 0);
	}

	public static XZ operator +(XZ a, XZ b)
	{
		a.x += b.x;
		a.z += b.z;
		return a;
	}

	public static XZ operator -(XZ a, XZ b)
	{
		a.x -= b.x;
		a.z -= b.z;
		return a;
	}

	public static XZ operator *(XZ a, int f)
	{
		a.x *= f;
		a.z *= f;
		return a;
	}

	public static XZ operator /(XZ a, int f)
	{
		a.x /= f;
		a.z /= f;
		return a;
	}

	public static XZ operator %(XZ a, int f)
	{
		a.x %= f;
		a.z %= f;
		return a;
	}

	public static XZ operator >>(XZ a, int d)
	{
		a.x >>= d;
		a.z >>= d;
		return a;
	}

	public static XZ operator <<(XZ a, int d)
	{
		a.x <<= d;
		a.z <<= d;
		return a;
	}

	public static XZ operator -(XZ a)
	{
		a.x = -a.x;
		a.z = -a.z;
		return a;
	}

	public int CompareTo(XZ other)
	{
		return (z != other.z) ? z.CompareTo(other.z) : x.CompareTo(other.x);
	}

	public override string ToString()
	{
		return x + ", " + z;
	}

	public static bool operator ==(XZ a, XZ b)
	{
		return a.x == b.x && a.z == b.z;
	}

	public static bool operator !=(XZ a, XZ b)
	{
		return a.x != b.x || a.z != b.z;
	}

	public static bool operator <(XZ a, XZ b)
	{
		return a.x < b.x && a.z < b.z;
	}

	public static bool operator >(XZ a, XZ b)
	{
		return a.x > b.x && a.z > b.z;
	}

	public static bool operator <=(XZ a, XZ b)
	{
		return a.x <= b.x && a.z <= b.z;
	}

	public static bool operator >=(XZ a, XZ b)
	{
		return a.x >= b.x && a.z >= b.z;
	}
}
