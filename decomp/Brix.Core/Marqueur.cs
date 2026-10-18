using System;
using System.Collections.Generic;
using System.Linq;

public class Marqueur
{
	private int[] marque;

	public int Length => marque.Length << 5;

	public Marqueur(int max)
	{
		marque = new int[max + 31 >> 5];
	}

	public void Clear()
	{
		Array.Clear(marque, 0, marque.Length);
	}

	public void Marquer(int i)
	{
		marque[i >> 5] |= 1 << i;
	}

	public void Enlever(int i)
	{
		marque[i >> 5] &= ~(1 << i);
	}

	public void Inverser(int i)
	{
		marque[i >> 5] ^= 1 << i;
	}

	public void Modifier(int i, bool val)
	{
		if (val)
		{
			Marquer(i);
		}
		else
		{
			Enlever(i);
		}
	}

	public bool Present(int i)
	{
		return (marque[i >> 5] & (1 << i)) != 0;
	}

	public bool Libre(int i)
	{
		return (marque[i >> 5] & (1 << (i & 0x1F))) == 0;
	}

	public bool MarquerSiLibre(int i)
	{
		return MarquerSiLibre(ref marque[i >> 5], 1 << (i & 0x1F));
	}

	public bool EnleverSiPresent(int i)
	{
		return EnleverSiPresent(ref marque[i >> 5], 1 << (i & 0x1F));
	}

	private static bool MarquerSiLibre(ref int m, int b)
	{
		if ((m & b) != 0)
		{
			return false;
		}
		m |= b;
		return true;
	}

	private static bool EnleverSiPresent(ref int m, int b)
	{
		if ((m & b) == 0)
		{
			return false;
		}
		m &= ~b;
		return true;
	}

	public void Intersect(Marqueur other)
	{
		for (int i = 0; i < marque.Length; i++)
		{
			marque[i] &= other.marque[i];
		}
	}

	public void Union(Marqueur other)
	{
		for (int i = 0; i < marque.Length; i++)
		{
			marque[i] |= other.marque[i];
		}
	}

	public IEnumerable<int> Presents()
	{
		for (int index = 0; index < marque.Length; index++)
		{
			int m = marque[index];
			if (m == 0)
			{
				continue;
			}
			int i = index << 5;
			for (int j = 0; j < 4; j++)
			{
				if ((m & 0xFF) != 0)
				{
					if ((m & 1) != 0)
					{
						yield return i;
					}
					if ((m & 2) != 0)
					{
						yield return i + 1;
					}
					if ((m & 4) != 0)
					{
						yield return i + 2;
					}
					if ((m & 8) != 0)
					{
						yield return i + 3;
					}
					if ((m & 0x10) != 0)
					{
						yield return i + 4;
					}
					if ((m & 0x20) != 0)
					{
						yield return i + 5;
					}
					if ((m & 0x40) != 0)
					{
						yield return i + 6;
					}
					if ((m & 0x80) != 0)
					{
						yield return i + 7;
					}
				}
				m >>= 8;
				i += 8;
			}
		}
	}

	public IEnumerable<int> Libres()
	{
		for (int index = 0; index < marque.Length; index++)
		{
			int m = marque[index];
			if (m == -1)
			{
				continue;
			}
			int i = index << 5;
			for (int j = 0; j < 4; j++)
			{
				if ((m & 0xFF) != 255)
				{
					if ((m & 1) != 0)
					{
						yield return i;
					}
					if ((m & 2) != 0)
					{
						yield return i + 1;
					}
					if ((m & 4) != 0)
					{
						yield return i + 2;
					}
					if ((m & 8) != 0)
					{
						yield return i + 3;
					}
					if ((m & 0x10) != 0)
					{
						yield return i + 4;
					}
					if ((m & 0x20) != 0)
					{
						yield return i + 5;
					}
					if ((m & 0x40) != 0)
					{
						yield return i + 6;
					}
					if ((m & 0x80) != 0)
					{
						yield return i + 7;
					}
				}
				m >>= 8;
				i += 8;
			}
		}
	}

	public bool TousLibres()
	{
		return marque.All((int m) => m == 0);
	}

	public bool TousPresents()
	{
		return marque.All((int m) => m == -1);
	}
}
