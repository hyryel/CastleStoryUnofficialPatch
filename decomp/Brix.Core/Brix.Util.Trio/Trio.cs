using System;
using UnityEngine;

namespace Brix.Util.Trio;

public class Trio : IComparable<Trio>
{
	public int a;

	public int b;

	public int c;

	public Trio(int a, int b, int c)
	{
		this.a = a;
		this.b = b;
		this.c = c;
	}

	public Trio(Vector3 vecteur)
	{
		a = Mathf.RoundToInt(vecteur.x);
		b = Mathf.RoundToInt(vecteur.y);
		c = Mathf.RoundToInt(vecteur.z);
	}

	public int GetHashcode()
	{
		return a + b + c;
	}

	public int CompareTo(Trio other)
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
		return c.CompareTo(other.c);
	}
}
