using System;
using System.Collections.Generic;
using Brix.Engine.Abstract;
using Brix.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public struct XYZ : IComparable<XYZ>, IEquatable<XYZ>
{
	public int x;

	public int y;

	public int z;

	public static XYZ zero = new XYZ
	{
		x = 0,
		y = 0,
		z = 0
	};

	public static XYZ right = new XYZ
	{
		x = 1,
		y = 0,
		z = 0
	};

	public static XYZ up = new XYZ
	{
		x = 0,
		y = 1,
		z = 0
	};

	public static XYZ forward = new XYZ
	{
		x = 0,
		y = 0,
		z = 1
	};

	public static XYZ one = new XYZ
	{
		x = 1,
		y = 1,
		z = 1
	};

	public bool Equals(XYZ other)
	{
		return x == other.x && y == other.y && z == other.z;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is XYZ && Equals((XYZ)obj);
	}

	public override int GetHashCode()
	{
		int num = x;
		num = (num * 397) ^ y;
		return (num * 397) ^ z;
	}

	public XYZ(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static XYZ FromVector3(Vector3 v)
	{
		return Round(v.x, v.y, v.z);
	}

	public static XYZ FromScalaire(int scalaire)
	{
		return new XYZ(Voxel.GetX(scalaire), Voxel.GetY(scalaire), Voxel.GetZ(scalaire));
	}

	public static XYZ FromScalaireSafe(int signedScalaire)
	{
		XYZ result = FromScalaire(signedScalaire);
		if (result.x >= 512)
		{
			result.x -= 1024;
			result.z++;
		}
		if (result.z >= 512)
		{
			result.z -= 1024;
			result.y++;
		}
		if (result.y >= 512)
		{
			result.y -= 1024;
		}
		return result;
	}

	[Pure]
	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public int ToScalaire()
	{
		return Voxel.GetScalaire(x, y, z);
	}

	public static XYZ Floor(float x, float y, float z)
	{
		return new XYZ(Mathf.FloorToInt(x), Mathf.FloorToInt(y), Mathf.FloorToInt(z));
	}

	public static XYZ Ceil(float x, float y, float z)
	{
		return new XYZ(Mathf.CeilToInt(x), Mathf.CeilToInt(y), Mathf.CeilToInt(z));
	}

	public static XYZ Round(float x, float y, float z)
	{
		return new XYZ(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
	}

	public static XYZ Floor(Vector3 v)
	{
		return new XYZ(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
	}

	public static XYZ Ceil(Vector3 v)
	{
		return new XYZ(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
	}

	public static XYZ Round(Vector3 v)
	{
		return new XYZ(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	}

	public static XYZ Floor(XYZ a, XYZ roundFactor)
	{
		return a - Mod(a, roundFactor);
	}

	public static XYZ Ceil(XYZ a, XYZ roundFactor)
	{
		return Floor(a + roundFactor - one, roundFactor);
	}

	public static XYZ Round(XYZ a, XYZ roundFactor)
	{
		return Floor(a + (roundFactor >> 1), roundFactor);
	}

	public static XYZ Normalize(XYZ a)
	{
		int num = Mathf.Abs(a.x);
		int num2 = Mathf.Abs(a.x);
		int num3 = Mathf.Abs(a.x);
		if (num >= num2 && num >= num3)
		{
			return (a.x > 0) ? right : (-right);
		}
		if (num3 >= num2)
		{
			return (a.z > 0) ? forward : (-forward);
		}
		return (a.y > 0) ? up : (-up);
	}

	public XYZ Flatten()
	{
		return new XYZ(x, 0, z);
	}

	public XYZ Plus(int x, int y, int z)
	{
		this.x += x;
		this.y += y;
		this.z += z;
		return this;
	}

	public static XYZ Min(XYZ a, XYZ b)
	{
		a.x = Min(a.x, b.x);
		a.y = Min(a.y, b.y);
		a.z = Min(a.z, b.z);
		return a;
	}

	public static XYZ Max(XYZ a, XYZ b)
	{
		a.x = Max(a.x, b.x);
		a.y = Max(a.y, b.y);
		a.z = Max(a.z, b.z);
		return a;
	}

	public static XYZ Min(XYZ a, XYZ b, XYZ c)
	{
		a.x = Min(a.x, b.x, c.x);
		a.y = Min(a.y, b.y, c.y);
		a.z = Min(a.z, b.z, c.z);
		return a;
	}

	public static XYZ Max(XYZ a, XYZ b, XYZ c)
	{
		a.x = Max(a.x, b.x, c.x);
		a.y = Max(a.y, b.y, c.y);
		a.z = Max(a.z, b.z, c.z);
		return a;
	}

	public static XYZ Clamp(XYZ a, XYZ b, XYZ c)
	{
		a.x = Clamp(a.x, b.x, c.x);
		a.y = Clamp(a.y, b.y, c.y);
		a.z = Clamp(a.z, b.z, c.z);
		return a;
	}

	public static XYZ Abs(XYZ a)
	{
		a.x = Abs(a.x);
		a.y = Abs(a.y);
		a.z = Abs(a.z);
		return a;
	}

	public static XYZ Sqr(XYZ a)
	{
		a.x *= a.x;
		a.y *= a.y;
		a.z *= a.z;
		return a;
	}

	public static XYZ Min(IList<XYZ> list)
	{
		int count = list.Count;
		if (count == 0)
		{
			throw new ArgumentException("XYZ.Min has no default value for an empty list", "list");
		}
		XYZ xYZ = list[0];
		for (int i = 1; i < count; i++)
		{
			xYZ = Min(xYZ, list[i]);
		}
		return xYZ;
	}

	public static XYZ Max(IList<XYZ> list)
	{
		int count = list.Count;
		if (count == 0)
		{
			throw new ArgumentException("XYZ.Min has no default value for an empty list", "list");
		}
		XYZ xYZ = list[0];
		for (int i = 1; i < count; i++)
		{
			xYZ = Max(xYZ, list[i]);
		}
		return xYZ;
	}

	public static XYZ Min(IEnumerable<XYZ> list)
	{
		IEnumerator<XYZ> enumerator = list.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new ArgumentException("XYZ.Min has no default value for an empty list", "list");
		}
		XYZ xYZ = enumerator.Current;
		while (enumerator.MoveNext())
		{
			xYZ = Min(xYZ, enumerator.Current);
		}
		return xYZ;
	}

	public static XYZ Max(IEnumerable<XYZ> list)
	{
		IEnumerator<XYZ> enumerator = list.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new ArgumentException("XYZ.Max has no default value for an empty list", "list");
		}
		XYZ xYZ = enumerator.Current;
		while (enumerator.MoveNext())
		{
			xYZ = Max(xYZ, enumerator.Current);
		}
		return xYZ;
	}

	public static XYZ MinAbs(XYZ a, XYZ b)
	{
		a.x = Min(Abs(a.x), Abs(b.x));
		a.y = Min(Abs(a.y), Abs(b.y));
		a.z = Min(Abs(a.z), Abs(b.z));
		return a;
	}

	public static XYZ MinTowardsZero(XYZ a, XYZ b)
	{
		a.x = Calcule.MinTowardsZero(a.x, b.x);
		a.y = Calcule.MinTowardsZero(a.y, b.y);
		a.z = Calcule.MinTowardsZero(a.z, b.z);
		return a;
	}

	public static XYZ Mul(XYZ a, XYZ b)
	{
		a.x *= b.x;
		a.y *= b.y;
		a.z *= b.z;
		return a;
	}

	public static XYZ Div(XYZ a, XYZ b)
	{
		a.x /= b.x;
		a.y /= b.y;
		a.z /= b.z;
		return a;
	}

	public static XYZ Mod(XYZ a, XYZ b)
	{
		a.x %= b.x;
		a.y %= b.y;
		a.z %= b.z;
		return a;
	}

	public static XYZ DivCeil(XYZ a, XYZ b)
	{
		a += b - one;
		a.x /= b.x;
		a.y /= b.y;
		a.z /= b.z;
		return a;
	}

	public static XYZ DivRound(XYZ a, XYZ b)
	{
		a += b >> 1;
		a.x /= b.x;
		a.y /= b.y;
		a.z /= b.z;
		return a;
	}

	public static int Dot(XYZ a, XYZ b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}

	public static XYZ Cross(XYZ a, XYZ b)
	{
		return new XYZ(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
	}

	public static int SqrMagnitude(XYZ a)
	{
		return Dot(a, a);
	}

	public static int ManhattanMagnitude(XYZ a)
	{
		return Abs(a.x) + Abs(a.y) + Abs(a.z);
	}

	public static int MaxMagnitude(XYZ a)
	{
		return Max(Abs(a.x), Abs(a.y), Abs(a.z));
	}

	public static bool InHalfInterval(XYZ p, XYZ a, XYZ b)
	{
		return p.x >= a.x && p.y >= a.y && p.z >= a.z && p.x < b.x && p.y < b.y && p.z < b.z;
	}

	public static XYZ Step(XYZ a, XYZ b)
	{
		return new XYZ((b.x >= a.x) ? 1 : 0, (b.y >= a.y) ? 1 : 0, (b.z >= a.z) ? 1 : 0);
	}

	public static XYZ operator +(XYZ a, XYZ b)
	{
		a.x += b.x;
		a.y += b.y;
		a.z += b.z;
		return a;
	}

	public static XYZ operator -(XYZ a, XYZ b)
	{
		a.x -= b.x;
		a.y -= b.y;
		a.z -= b.z;
		return a;
	}

	public static XYZ operator *(XYZ a, int f)
	{
		a.x *= f;
		a.y *= f;
		a.z *= f;
		return a;
	}

	public static XYZ operator /(XYZ a, int f)
	{
		a.x /= f;
		a.y /= f;
		a.z /= f;
		return a;
	}

	public static XYZ operator %(XYZ a, int f)
	{
		a.x %= f;
		a.y %= f;
		a.z %= f;
		return a;
	}

	public static XYZ operator >>(XYZ a, int d)
	{
		a.x >>= d;
		a.y >>= d;
		a.z >>= d;
		return a;
	}

	public static XYZ operator <<(XYZ a, int d)
	{
		a.x <<= d;
		a.y <<= d;
		a.z <<= d;
		return a;
	}

	public static XYZ operator -(XYZ a)
	{
		a.x = -a.x;
		a.y = -a.y;
		a.z = -a.z;
		return a;
	}

	public int CompareTo(XYZ other)
	{
		return (y != other.y) ? y.CompareTo(other.y) : ((z != other.z) ? z.CompareTo(other.z) : x.CompareTo(other.x));
	}

	public override string ToString()
	{
		return x + ", " + y + ", " + z;
	}

	public static bool operator ==(XYZ a, XYZ b)
	{
		return a.x == b.x && a.y == b.y && a.z == b.z;
	}

	public static bool operator !=(XYZ a, XYZ b)
	{
		return a.x != b.x || a.y != b.y || a.z != b.z;
	}

	public static bool operator <(XYZ a, XYZ b)
	{
		return a.x < b.x && a.y < b.y && a.z < b.z;
	}

	public static bool operator >(XYZ a, XYZ b)
	{
		return a.x > b.x && a.y > b.y && a.z > b.z;
	}

	public static bool operator <=(XYZ a, XYZ b)
	{
		return a.x <= b.x && a.y <= b.y && a.z <= b.z;
	}

	public static bool operator >=(XYZ a, XYZ b)
	{
		return a.x >= b.x && a.y >= b.y && a.z >= b.z;
	}

	public static bool Less(XYZ a, XYZ b)
	{
		return a.x < b.x && a.y < b.y && a.z < b.z;
	}

	public static bool LEqual(XYZ a, XYZ b)
	{
		return a.x <= b.x && a.y <= b.y && a.z <= b.z;
	}

	public static bool Greater(XYZ a, XYZ b)
	{
		return a.x < b.x && a.y < b.y && a.z < b.z;
	}

	public static bool GEqual(XYZ a, XYZ b)
	{
		return a.x <= b.x && a.y <= b.y && a.z <= b.z;
	}

	private static int Min(int a, int b)
	{
		return Mathf.Min(a, b);
	}

	private static int Min(int a, int b, int c)
	{
		return Mathf.Min(a, Mathf.Min(b, c));
	}

	private static int Max(int a, int b)
	{
		return Mathf.Max(a, b);
	}

	private static int Max(int a, int b, int c)
	{
		return Mathf.Max(a, Mathf.Max(b, c));
	}

	private static int Abs(int a)
	{
		return Mathf.Abs(a);
	}

	private static int Clamp(int a, int b, int c)
	{
		return Mathf.Clamp(a, b, c);
	}
}
