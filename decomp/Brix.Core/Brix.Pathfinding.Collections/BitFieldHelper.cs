using System;
using System.Collections.Generic;
using System.Globalization;

namespace Brix.Pathfinding.Collections;

public static class BitFieldHelper
{
	public static IEnumerator<int> GetEnumerator(ulong bits)
	{
		for (int i = 0; bits >> i != 0; i++)
		{
			if (((bits >> i) & 1) != 0)
			{
				yield return i;
			}
		}
	}

	public static bool GetValue(ulong bits, int i)
	{
		bits >>= i;
		ulong num = bits & 1;
		return num != 0;
	}

	public static ulong SetValue(ulong bits, int i, bool value)
	{
		return (bits & (ulong)(~(1L << i))) | (ulong)((long)(value ? 1 : 0) << i);
	}
}
public static class BitFieldHelper<T> where T : IConvertible
{
	public static readonly int ValueSize = (int)Math.Ceiling(Math.Log(Enum.GetValues(typeof(T)).Length, 2.0));

	public static readonly int ValueBits = (1 << ValueSize) - 1;

	public static long GetBits(int i, T value)
	{
		return value.ToInt64(CultureInfo.CurrentCulture) << i * ValueSize;
	}

	public static IEnumerator<T> GetEnumerator(ulong bits)
	{
		int i = 0;
		while (bits != 0)
		{
			ulong valueBits = bits & (ulong)ValueBits;
			if (valueBits != 0)
			{
				yield return (T)Enum.ToObject(typeof(T), valueBits);
			}
			bits >>= ValueSize;
			i++;
		}
	}

	public static T GetValue(ulong bits, int i)
	{
		bits >>= i * ValueSize;
		ulong value = bits & (ulong)ValueBits;
		return (T)Enum.ToObject(typeof(T), value);
	}

	public static ulong SetValue(ulong bits, int i, T value)
	{
		return (bits & (ulong)(~((long)ValueBits << i * ValueSize))) | (ulong)GetBits(i, value);
	}
}
