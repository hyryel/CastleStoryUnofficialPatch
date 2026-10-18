using System;
using UnityEngine;

namespace Brix.Util.Quatuor;

public class Quatuor : IComparable<Quatuor>
{
	public int a;

	public int b;

	public int c;

	public int d;

	public Quatuor(int a, int b, int c, int d)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}

	public Quatuor(Vector3 vecteur)
	{
		a = Mathf.RoundToInt(vecteur.x);
		b = Mathf.RoundToInt(vecteur.y);
		c = Mathf.RoundToInt(vecteur.z);
		d = 0;
	}

	public Quatuor(int a, int b, int c)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		d = 0;
	}

	public int GetHashcode()
	{
		return a + b + c + d;
	}

	public int CompareTo(Quatuor other)
	{
		int num = a.CompareTo(other.a);
		if (num != 0)
		{
			return num;
		}
		num = b.CompareTo(other.b);
		if (num != 0)
		{
			return num;
		}
		num = c.CompareTo(other.c);
		if (num != 0)
		{
			return num;
		}
		return d.CompareTo(other.d);
	}

	public bool Equals(Quatuor other)
	{
		return a == other.a && b == other.b && c == other.c && d == other.d;
	}
}
