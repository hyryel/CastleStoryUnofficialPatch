using System;
using System.Collections.Generic;

public class MarqueurTristate
{
	private byte[] marque;

	private const byte b = 3;

	public MarqueurTristate(int max)
	{
		marque = new byte[Index(max - 1) + 1];
	}

	public void SetPositif(int i)
	{
		Set(i, 1);
	}

	public void SetNegatif(int i)
	{
		Set(i, 2);
	}

	public void SetNeutre(int i)
	{
		Set(i, 0);
	}

	public bool Present(int i)
	{
		return Get(i) != 0;
	}

	public bool Positif(int i)
	{
		return Get(i) == 1;
	}

	public bool Negatif(int i)
	{
		return Get(i) == 2;
	}

	public int Signe(int i)
	{
		return (Get(i) + 1) % 3 - 1;
	}

	private int Index(int i)
	{
		return i / 5;
	}

	private byte Factor(int i)
	{
		i %= 5;
		return (byte)((1 + (i & 1) * 2) * (1 + (i & 2) * 4) * (1 + (i & 4) * 20));
	}

	private void Set(int i, byte x)
	{
		Set(x, Factor(i), ref marque[Index(i)]);
	}

	private void Set(byte x, byte factor, ref byte res)
	{
		res = (byte)((res / factor / 3 * 3 + x) * factor + res % factor);
	}

	private byte Get(int i)
	{
		return Get(marque[Index(i)], Factor(i));
	}

	private byte Get(byte y, byte factor)
	{
		return (byte)(y / factor % 3);
	}

	public void Clear()
	{
		Array.Clear(marque, 0, marque.Length);
	}

	private void SetAll(byte x)
	{
		byte b = (byte)(x * 121);
		for (int i = 0; i < marque.Length; i++)
		{
			marque[i] = b;
		}
	}

	public void SetToutPositif()
	{
		SetAll(1);
	}

	public void SetToutNegatif()
	{
		SetAll(2);
	}

	public IEnumerable<int> AvecSigne(int signe)
	{
		for (int index = 0; index < marque.Length; index++)
		{
			int m = marque[index];
			if (m != 0)
			{
				int i = index * 5;
				if (m % 3 == signe)
				{
					yield return i;
				}
				m /= 3;
				if (m % 3 == signe)
				{
					yield return i + 1;
				}
				m /= 3;
				if (m % 3 == signe)
				{
					yield return i + 2;
				}
				m /= 3;
				if (m % 3 == signe)
				{
					yield return i + 3;
				}
				m /= 3;
				if (m == signe)
				{
					yield return i + 4;
				}
			}
		}
	}

	public IEnumerable<int> SansSigne(int signe)
	{
		for (int index = 0; index < marque.Length; index++)
		{
			int m = marque[index];
			int i = index * 5;
			if (m == 0)
			{
				yield return i;
				yield return i + 1;
				yield return i + 2;
				yield return i + 3;
				yield return i + 4;
				continue;
			}
			if (m % 3 != signe)
			{
				yield return i;
			}
			m /= 3;
			if (m % 3 != signe)
			{
				yield return i + 1;
			}
			m /= 3;
			if (m % 3 != signe)
			{
				yield return i + 2;
			}
			m /= 3;
			if (m % 3 != signe)
			{
				yield return i + 3;
			}
			m /= 3;
			if (m != signe)
			{
				yield return i + 4;
			}
		}
	}

	public IEnumerable<int> Neutres()
	{
		for (int index = 0; index < marque.Length; index++)
		{
			int m = marque[index];
			int i = index * 5;
			if (m == 0)
			{
				yield return i;
				yield return i + 1;
				yield return i + 2;
				yield return i + 3;
				yield return i + 4;
				continue;
			}
			if (m % 3 == 0)
			{
				yield return i;
			}
			m /= 3;
			if (m % 3 == 0)
			{
				yield return i + 1;
			}
			m /= 3;
			if (m % 3 == 0)
			{
				yield return i + 2;
			}
			m /= 3;
			if (m % 3 == 0)
			{
				yield return i + 3;
			}
			if (m / 3 == 0)
			{
				yield return i + 4;
			}
		}
	}

	public IEnumerable<int> Positifs()
	{
		return AvecSigne(1);
	}

	public IEnumerable<int> Negatifs()
	{
		return AvecSigne(2);
	}

	public IEnumerable<int> NonPositifs()
	{
		return SansSigne(1);
	}

	public IEnumerable<int> NonNegatifs()
	{
		return SansSigne(2);
	}
}
