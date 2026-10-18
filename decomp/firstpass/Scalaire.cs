using UnityEngine;

public struct Scalaire
{
	internal const int pl = 4;

	internal const int gl = 6;

	internal const int pgl = 10;

	internal const int pl2 = 8;

	internal const int gl2 = 12;

	internal const int pgl2 = 20;

	internal const int pm = 15;

	internal const int gm = 63;

	internal const int pgm = 1023;

	public static readonly Scalaire zero = new Scalaire(0);

	private int indice;

	public int x
	{
		get
		{
			return indice & 0x3FF;
		}
		set
		{
			indice &= -1024;
			indice |= value & 0x3FF;
		}
	}

	public int y
	{
		get
		{
			return (indice >> 20) & 0x3FF;
		}
		set
		{
			indice &= -1073741824;
			indice |= (value & 0x3FF) << 20;
		}
	}

	public int z
	{
		get
		{
			return (indice >> 10) & 0x3FF;
		}
		set
		{
			indice &= -1048576;
			indice |= (value & 0x3FF) << 10;
		}
	}

	public int p
	{
		get
		{
			return (x & 0xF) | ((y & 0xF) << 8) | ((z & 0xF) << 4);
		}
		set
		{
			indice &= -15744016;
			indice |= (value & 0xF) | (((value >> 8) & 0xF) << 20) | (((value >> 4) & 0xF) << 10);
		}
	}

	public int g
	{
		get
		{
			return (x & 0x3F) | ((y & 0x3F) << 12) | ((z & 0x3F) << 6);
		}
		set
		{
			indice &= -1057997809;
			indice |= ((value & 0x3F) | (((value >> 12) & 0xF) << 20) | (((value >> 6) & 0xF) << 10)) << 4;
		}
	}

	public Scalaire(int x, int y, int z)
	{
		indice = x | (y << 20) | (z << 10);
	}

	public Scalaire(int i)
	{
		indice = i;
	}

	public Scalaire(Vector3 v)
	{
		int num = (int)v.x;
		int num2 = (int)v.y;
		int num3 = (int)v.z;
		indice = (num & 0x3FF) | ((num2 & 0x3FF) << 20) | ((num3 & 0x3FF) << 10);
	}

	public static implicit operator Scalaire(int i)
	{
		return new Scalaire(i);
	}

	public static implicit operator int(Scalaire s)
	{
		return s.indice;
	}

	public static implicit operator Scalaire(Vector3 v)
	{
		return new Scalaire(v);
	}

	public static implicit operator Vector3(Scalaire s)
	{
		return new Vector3(s.x, s.y, s.z);
	}

	public static Scalaire operator +(Scalaire i, Scalaire j)
	{
		return new Scalaire(i.indice + j.indice);
	}

	public void PlusEgal(Scalaire s)
	{
		indice += s.indice;
	}
}
