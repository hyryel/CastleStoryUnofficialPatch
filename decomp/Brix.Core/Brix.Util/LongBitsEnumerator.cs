using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Util;

public struct LongBitsEnumerator : IEnumerator<int>, IDisposable, IEnumerator
{
	public static readonly int[] MsbTable = BitsEnumerator.MsbTable;

	public static readonly int[] LsbTable = BitsEnumerator.LsbTable;

	public ulong Bits;

	private int _i;

	public int Current => _i;

	object IEnumerator.Current => Current;

	public LongBitsEnumerator(long bits)
	{
		Bits = (ulong)bits;
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

	public void Decrease()
	{
		ulong num = Bits >> 32;
		if (num != 0)
		{
			ulong num2 = num >> 16;
			if (num2 != 0)
			{
				ulong num3 = num2 >> 8;
				_i = ((num3 != 0L) ? (56 + MsbTable[num3]) : (48 + MsbTable[num2]));
			}
			else
			{
				ulong num4 = num >> 8;
				_i = ((num4 != 0L) ? (40 + MsbTable[num4]) : (32 + MsbTable[num]));
			}
		}
		else
		{
			ulong num5 = Bits >> 16;
			if (num5 != 0)
			{
				ulong num6 = num5 >> 8;
				_i = ((num6 != 0L) ? (24 + MsbTable[num6]) : (16 + MsbTable[num5]));
			}
			else
			{
				ulong num7 = Bits >> 8;
				_i = ((num7 != 0L) ? (8 + MsbTable[num7]) : MsbTable[Bits]);
			}
		}
		Bits &= (ulong)(~(1L << _i));
	}

	public void Increase()
	{
		while ((Bits & 0xFF) == 0)
		{
			Bits >>= 8;
			_i += 8;
		}
		int num = LsbTable[(uint)(Bits & 0xFF)];
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
