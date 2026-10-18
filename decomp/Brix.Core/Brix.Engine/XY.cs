using System;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public struct XY : IComparable<XY>, IEquatable<XY>
{
	public int x;

	public int y;

	public static XY zero = new XY
	{
		x = 0,
		y = 0
	};

	public static XY right = new XY
	{
		x = 1,
		y = 0
	};

	public static XY up = new XY
	{
		x = 0,
		y = 1
	};

	public static XY one = new XY
	{
		x = 1,
		y = 1
	};

	public bool Equals(XY other)
	{
		return x == other.x && y == other.y;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is XY && Equals((XY)obj);
	}

	public override int GetHashCode()
	{
		int num = x;
		return (num * 397) ^ y;
	}

	public XY(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static XY FromXYZ(XYZ xyz)
	{
		return new XY(xyz.x, xyz.y);
	}

	public XYZ ToXYZ()
	{
		return new XYZ(x, y, 0);
	}

	public static XY FromVector2(Vector2 v)
	{
		return Round(v.x, v.y);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public static XY Round(float x, float y)
	{
		return new XY(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
	}

	public static XY Floor(float x, float y)
	{
		return new XY(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
	}

	public static XY Ceil(float x, float y)
	{
		return new XY(Mathf.CeilToInt(x), Mathf.CeilToInt(y));
	}

	public XY Plus(int x, int y)
	{
		this.x += x;
		this.y += y;
		return this;
	}

	public static XY Min(XY a, XY b)
	{
		a.x = Mathf.Min(a.x, b.x);
		a.y = Mathf.Min(a.y, b.y);
		return a;
	}

	public static XY Max(XY a, XY b)
	{
		a.x = Mathf.Max(a.x, b.x);
		a.y = Mathf.Max(a.y, b.y);
		return a;
	}

	public static XY Min(XY a, XY b, XY c)
	{
		a.x = Mathf.Min(a.x, b.x, c.x);
		a.y = Mathf.Min(a.y, b.y, c.y);
		return a;
	}

	public static XY Max(XY a, XY b, XY c)
	{
		a.x = Mathf.Max(a.x, b.x, c.x);
		a.y = Mathf.Max(a.y, b.y, c.y);
		return a;
	}

	public static XY Clamp(XY a, XY b, XY c)
	{
		a.x = Mathf.Clamp(a.x, b.x, c.x);
		a.y = Mathf.Clamp(a.y, b.y, c.y);
		return a;
	}

	public static XY Abs(XY a)
	{
		a.x = Mathf.Abs(a.x);
		a.y = Mathf.Abs(a.y);
		return a;
	}

	public static XY Sqr(XY a)
	{
		a.x *= a.x;
		a.y *= a.y;
		return a;
	}

	public static XY MinAbs(XY a, XY b)
	{
		a.x = Mathf.Min(Mathf.Abs(a.x), Mathf.Abs(b.x));
		a.y = Mathf.Min(Mathf.Abs(a.y), Mathf.Abs(b.y));
		return a;
	}

	public static XY MinTowardsZero(XY a, XY b)
	{
		a.x = Calcule.MinTowardsZero(a.x, b.x);
		a.y = Calcule.MinTowardsZero(a.y, b.y);
		return a;
	}

	public static XY Mul(XY a, XY b)
	{
		a.x *= b.x;
		a.y *= b.y;
		return a;
	}

	public static XY Div(XY a, XY b)
	{
		a.x /= b.x;
		a.y /= b.y;
		return a;
	}

	public static XY Mod(XY a, XY b)
	{
		a.x %= b.x;
		a.y %= b.y;
		return a;
	}

	public static int Dot(XY a, XY b)
	{
		return a.x * b.x + a.y * b.y;
	}

	public static int SqrMagnitude(XY a)
	{
		return Dot(a, a);
	}

	public static int ManhattanMagnitude(XY a)
	{
		return Mathf.Abs(a.x) + Mathf.Abs(a.y);
	}

	public static int MaxMagnitude(XY a)
	{
		return Mathf.Max(Mathf.Abs(a.x), Mathf.Abs(a.y));
	}

	public static bool InHalfInterval(XY p, XY a, XY b)
	{
		return p.x >= a.x && p.y >= a.y && p.x < b.x && p.y < b.y;
	}

	public static XY Step(XY a, XY b)
	{
		return new XY((b.x >= a.x) ? 1 : 0, (b.y >= a.y) ? 1 : 0);
	}

	public static XY operator +(XY a, XY b)
	{
		a.x += b.x;
		a.y += b.y;
		return a;
	}

	public static XY operator -(XY a, XY b)
	{
		a.x -= b.x;
		a.y -= b.y;
		return a;
	}

	public static XY operator *(XY a, int f)
	{
		a.x *= f;
		a.y *= f;
		return a;
	}

	public static XY operator /(XY a, int f)
	{
		a.x /= f;
		a.y /= f;
		return a;
	}

	public static XY operator %(XY a, int f)
	{
		a.x %= f;
		a.y %= f;
		return a;
	}

	public static XY operator >>(XY a, int d)
	{
		a.x >>= d;
		a.y >>= d;
		return a;
	}

	public static XY operator <<(XY a, int d)
	{
		a.x <<= d;
		a.y <<= d;
		return a;
	}

	public static XY operator -(XY a)
	{
		a.x = -a.x;
		a.y = -a.y;
		return a;
	}

	public int CompareTo(XY other)
	{
		return (y != other.y) ? y.CompareTo(other.y) : x.CompareTo(other.x);
	}

	public override string ToString()
	{
		return x + ", " + y;
	}

	public static bool operator ==(XY a, XY b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(XY a, XY b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public static bool operator <(XY a, XY b)
	{
		return a.x < b.x && a.y < b.y;
	}

	public static bool operator >(XY a, XY b)
	{
		return a.x > b.x && a.y > b.y;
	}

	public static bool operator <=(XY a, XY b)
	{
		return a.x <= b.x && a.y <= b.y;
	}

	public static bool operator >=(XY a, XY b)
	{
		return a.x >= b.x && a.y >= b.y;
	}
}
