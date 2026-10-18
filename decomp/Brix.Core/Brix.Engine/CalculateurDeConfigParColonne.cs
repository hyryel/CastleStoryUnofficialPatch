using System;
using System.Collections.Generic;
using System.Linq;

namespace Brix.Engine;

internal class CalculateurDeConfigParColonne
{
	public const bool EliminerVoxelsSolitaires = false;

	public Func<int, IntRange> GetColonne = null;

	private static readonly int[] NoConfigs = new int[0];

	public int[] GetConfigBasEnHaut(int xz)
	{
		IntRange intRange = GetColonne(xz);
		if (Vide(intRange))
		{
			return NoConfigs;
		}
		int[] array = new int[intRange.haut - intRange.bas];
		array[0] |= 32;
		array[intRange.haut - intRange.bas - 1] |= 1;
		PopulateConfigs(intRange, GetColonne(xz - 1), 16, array);
		PopulateConfigs(intRange, GetColonne(xz + 1), 8, array);
		PopulateConfigs(intRange, GetColonne(xz + 1024), 2, array);
		PopulateConfigs(intRange, GetColonne(xz - 1024), 4, array);
		return array;
	}

	public IEnumerable<KeyValuePair<int, int>> GetConfigs(int xz)
	{
		int scalaire = xz + GetColonne(xz).bas * 1048576;
		int[] configBasEnHaut = GetConfigBasEnHaut(xz);
		foreach (int config in configBasEnHaut)
		{
			if (config != 0)
			{
				yield return new KeyValuePair<int, int>(scalaire, config);
			}
			scalaire += 1048576;
		}
	}

	private static bool Vide(IntRange c)
	{
		if (c == null)
		{
			return true;
		}
		int bas = c.bas;
		int haut = c.haut;
		if (haut <= 0 || bas <= 0 || bas >= haut)
		{
			return true;
		}
		return false;
	}

	private static IEnumerable<int> RangeTotal(IntRange c)
	{
		return Enumerable.Range(0, c.haut - c.bas);
	}

	private static IEnumerable<int> RangeDouble(IntRange c, IntRange d)
	{
		for (int y = c.bas; y < d.bas; y++)
		{
			yield return y - c.bas;
		}
		for (int i = d.haut; i < c.haut; i++)
		{
			yield return i - c.bas;
		}
	}

	private static IEnumerable<int> Range(IntRange c, IntRange d)
	{
		if (Vide(d) || d.bas >= c.haut || d.haut <= c.bas)
		{
			return RangeTotal(c);
		}
		return RangeDouble(c, d);
	}

	private static void PopulateConfigs(IntRange c, IntRange d, int config, int[] configs)
	{
		IEnumerable<int> enumerable = Range(c, d);
		foreach (int item in enumerable)
		{
			configs[item] |= config;
		}
	}
}
