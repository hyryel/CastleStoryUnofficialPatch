using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Pathfinding.Collections;

public struct BitField8 : IEnumerable<int>, IEnumerable
{
	private byte bits;

	public byte Bits => bits;

	public long Bits64 => bits;

	public bool this[int i]
	{
		get
		{
			return BitFieldHelper.GetValue(bits, i);
		}
		set
		{
			bits = (byte)BitFieldHelper.SetValue(bits, i, value);
		}
	}

	public void Add(int i)
	{
		bits |= (byte)(1 << i);
	}

	public void AddBits(long valueBits)
	{
		bits |= (byte)valueBits;
	}

	public void Clear()
	{
		bits = 0;
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
public struct BitField8<T> : IEnumerable<T>, IEnumerable where T : IConvertible
{
	private byte bits;

	public byte Bits => bits;

	public long Bits64 => bits;

	public T this[int i]
	{
		get
		{
			return BitFieldHelper<T>.GetValue(bits, i);
		}
		set
		{
			bits = (byte)BitFieldHelper<T>.SetValue(bits, i, value);
		}
	}

	public void Add(int i, T value)
	{
		AddBits(BitFieldHelper<T>.GetBits(i, value));
	}

	public void AddBits(long valueBits)
	{
		bits |= (byte)valueBits;
	}

	public void Clear()
	{
		bits = 0;
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
