using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Brix.Utils.Collections;
using UnityEngine;

namespace Brix.Util;

public struct BitsEnumerator : IEnumerator<int>, IDisposable, IEnumerator
{
	public static readonly int[] MsbTable = (from i in Enumerable.Range(0, 256)
		select FindMSB(i)).ToArray();

	public static readonly int[] LsbTable = (from i in Enumerable.Range(0, 256)
		select FindLSB(i)).ToArray();

	public uint Bits;

	private int _i;

	public int Current => _i;

	object IEnumerator.Current => Current;

	private static int FindMSB(int x)
	{
		for (int num = 7; num >= 0; num--)
		{
			if ((x & (1 << num)) != 0)
			{
				return num;
			}
		}
		return -1;
	}

	private static int FindLSB(int x)
	{
		for (int i = 0; i < 8; i++)
		{
			if ((x & (1 << i)) != 0)
			{
				return i + 1;
			}
		}
		return 0;
	}

	public BitsEnumerator(int bits)
	{
		Bits = (uint)bits;
		_i = -1;
	}

	public bool MoveNext()
	{
		if (Bits == 0)
		{
			return false;
		}
		Increase();
		return true;
	}

	public static void Test()
	{
		int num = 1000000;
		System.Random r = new System.Random();
		int[] array = (from i in Enumerable.Range(0, num)
			select (1 << r.Next(0, 31)) | (1 << r.Next(0, 31)) | (1 << r.Next(0, 31))).ToArray();
		int[] array2 = (from i in Enumerable.Range(0, num)
			select 0).ToArray();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int num2 = 0; num2 < num; num2++)
		{
			LongBitsEnumerator longBitsEnumerator = new LongBitsEnumerator(array[num2]);
			int num3 = 0;
			while (longBitsEnumerator.Bits != 0)
			{
				longBitsEnumerator.Decrease();
				if ((num3 & (1 << longBitsEnumerator.Current)) != 0)
				{
					break;
				}
				num3 |= 1 << longBitsEnumerator.Current;
				array2[num2]++;
			}
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("Decrease: " + stopwatch.Elapsed.TotalMilliseconds);
		stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int num4 = 0; num4 < num; num4++)
		{
			LongBitsEnumerator longBitsEnumerator2 = new LongBitsEnumerator(array[num4]);
			int num5 = 0;
			while (longBitsEnumerator2.Bits != 0)
			{
				longBitsEnumerator2.Increase();
				if ((num5 & (1 << longBitsEnumerator2.Current)) != 0)
				{
					break;
				}
				num5 |= 1 << longBitsEnumerator2.Current;
				array2[num4]--;
			}
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("Increase: " + stopwatch.Elapsed.TotalMilliseconds);
		for (int num6 = 0; num6 < num; num6++)
		{
			if (array2[num6] != 0)
			{
				UnityEngine.Debug.Log("Wrong! " + num6 + ", " + array[num6]);
				break;
			}
		}
		stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int num7 = 0; num7 < num; num7++)
		{
			int num8 = 0;
			BitsEnumerator bitsEnumerator = new BitsEnumerator(array[num7]);
			while (bitsEnumerator.MoveNext())
			{
				num8 += bitsEnumerator.Current;
			}
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("BitsEnumerator Loop: " + stopwatch.Elapsed.TotalMilliseconds);
		stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int num9 = 0; num9 < num; num9++)
		{
			int num10 = 0;
			SmallSubset_32from32.Enumerator enumerator = new SmallSubset_32from32.Enumerator((uint)array[num9]);
			while (enumerator.MoveNext())
			{
				num10 += enumerator.Current;
			}
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("SmallSubset_32from32 Loop: " + stopwatch.Elapsed.TotalMilliseconds);
		stopwatch = new Stopwatch();
		stopwatch.Start();
		int num11 = 0;
		for (int num12 = 0; num12 < num; num12++)
		{
			num11 += array[num12].CountBits();
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("BitsEnumerator Count: " + stopwatch.Elapsed.TotalMilliseconds);
		stopwatch = new Stopwatch();
		stopwatch.Start();
		int num13 = 0;
		for (int num14 = 0; num14 < num; num14++)
		{
			num13 += SmallSubset_32from32.FromBits(array[num14]).Count;
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("SmallSubset_32from32 Count: " + stopwatch.Elapsed.TotalMilliseconds);
	}

	private void Decrease()
	{
		uint num = Bits >> 16;
		if (num != 0)
		{
			uint num2 = num >> 8;
			int num3 = ((num2 != 0) ? (24 + MsbTable[num2]) : (16 + MsbTable[num]));
			Bits &= (uint)(~(1 << num3));
		}
		else
		{
			uint num4 = Bits >> 8;
			int num5 = ((num4 != 0) ? (8 + MsbTable[num4]) : MsbTable[Bits]);
			Bits &= (uint)(~(1 << num5));
		}
	}

	private void Increase()
	{
		while ((Bits & 0xFF) == 0)
		{
			Bits >>= 8;
			_i += 8;
		}
		int num = LsbTable[Bits & 0xFF];
		Bits >>= num;
		_i += num;
	}

	public void Reset()
	{
	}

	public void Dispose()
	{
	}
}
