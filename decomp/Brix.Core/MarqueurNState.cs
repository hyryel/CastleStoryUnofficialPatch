using System;
using System.Collections.Generic;
using UnityEngine;

public class MarqueurNState
{
	private readonly uint[] marque;

	private readonly uint[] factors;

	private readonly uint factorSum;

	private readonly byte b;

	private readonly int k;

	private readonly int n;

	private readonly int[] counts;

	public MarqueurNState(int max, byte nStates)
	{
		n = max;
		b = nStates;
		List<uint> list = new List<uint>();
		uint num = uint.MaxValue / (uint)nStates + 1;
		factorSum = 0u;
		uint num2 = 1u;
		while (num2 != 0 && num2 <= num)
		{
			list.Add(num2);
			factorSum += num2;
			num2 *= b;
		}
		factors = list.ToArray();
		k = factors.Length;
		marque = new uint[Index(n - 1) + 1];
		counts = new int[b];
		counts[0] = n;
	}

	private int Index(int i)
	{
		return i / k;
	}

	private uint Factor(int i)
	{
		return factors[i % k];
	}

	public void SetNeutre(int i)
	{
		Set(i, 0);
	}

	public void Set(int i, byte x)
	{
		Set(x, Factor(i), ref marque[Index(i)]);
	}

	private void Set(byte x, uint factor, ref uint res)
	{
		uint num = res / factor % b;
		if (num != x)
		{
			res += (x - num) * factor;
			counts[num]--;
			counts[x]++;
		}
	}

	public byte Get(int i)
	{
		return Get(marque[Index(i)], Factor(i));
	}

	private byte Get(uint y, uint factor)
	{
		return (byte)(y / factor % b);
	}

	public void Clear()
	{
		Array.Clear(marque, 0, marque.Length);
	}

	public void SetAll(byte x)
	{
		uint num = x * factorSum;
		for (int i = 0; i < marque.Length; i++)
		{
			marque[i] = num;
		}
		Array.Clear(counts, 0, counts.Length);
		counts[x] = n;
	}

	public IEnumerable<int> AvecEtat(byte etat)
	{
		for (int index = 0; index < marque.Length; index++)
		{
			uint m = marque[index];
			int i = index * this.k;
			int ki = Mathf.Min(this.k, n - i);
			if (m == 0)
			{
				if (etat == 0)
				{
					for (int j = 0; j < ki; j++)
					{
						yield return i + j;
					}
				}
				continue;
			}
			for (int k = 0; k < ki; k++)
			{
				if (m % b == etat)
				{
					yield return i + k;
				}
				m /= b;
			}
		}
	}

	public IEnumerable<int> SansEtat(byte etat)
	{
		for (int index = 0; index < marque.Length; index++)
		{
			uint m = marque[index];
			int i = index * this.k;
			int ki = Mathf.Min(this.k, n - i);
			if (m == 0)
			{
				if (etat != 0)
				{
					for (int j = 0; j < ki; j++)
					{
						yield return i + j;
					}
				}
				continue;
			}
			for (int k = 0; k < ki; k++)
			{
				if (m % b != etat)
				{
					yield return i + k;
				}
				m /= b;
			}
		}
	}

	public IEnumerable<int> Neutres()
	{
		return AvecEtat(0);
	}

	public IEnumerable<int> Avec(Func<byte, bool> predicat)
	{
		for (int index = 0; index < marque.Length; index++)
		{
			uint m = marque[index];
			int i = index * k;
			int ki = Mathf.Min(k, n - i);
			for (int j = 0; j < ki; j++)
			{
				if (predicat((byte)(m % b)))
				{
					yield return i + j;
				}
				m /= b;
			}
		}
	}

	public int CountAvecEtat(byte etat)
	{
		return counts[etat];
	}

	public int CountSansEtat(byte etat)
	{
		return n - counts[etat];
	}

	public int CountAvec(Func<byte, bool> predicat)
	{
		int num = 0;
		for (byte b = 0; b < this.b; b++)
		{
			if (predicat(b))
			{
				num += counts[b];
			}
		}
		return num;
	}
}
