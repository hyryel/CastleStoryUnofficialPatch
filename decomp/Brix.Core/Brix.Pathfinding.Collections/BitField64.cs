using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Pathfinding.Collections;

public struct BitField64 : IEnumerable<int>, IEnumerable
{
	private ulong bits;

	public long Bits => (long)bits;

	public long Bits64 => (long)bits;

	public bool this[int i]
	{
		get
		{
			return BitFieldHelper.GetValue(bits, i);
		}
		set
		{
			bits = BitFieldHelper.SetValue(bits, i, value);
		}
	}

	public void Add(int i)
	{
		bits |= (uint)(1 << i);
	}

	public void AddBits(long valueBits)
	{
		bits |= (ulong)valueBits;
	}

	public void Clear()
	{
		bits = 0uL;
	}

	public IEnumerator<int> GetEnumerator()
	{
		return BitFieldHelper.GetEnumerator(bits);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
public struct BitField64<T> : IEnumerable<T>, IEnumerable where T : IConvertible
{
	private ulong bits;

	public long Bits => (long)bits;

	public long Bits64 => (long)bits;

	public T this[int i]
	{
		get
		{
			return BitFieldHelper<T>.GetValue(bits, i);
		}
		set
		{
			bits = BitFieldHelper<T>.SetValue(bits, i, value);
		}
	}

	public void Add(int i, T value)
	{
		AddBits(BitFieldHelper<T>.GetBits(i, value));
	}

	public void AddBits(long valueBits)
	{
		bits |= (ulong)valueBits;
	}

	public void Clear()
	{
		bits = 0uL;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return BitFieldHelper<T>.GetEnumerator(bits);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
