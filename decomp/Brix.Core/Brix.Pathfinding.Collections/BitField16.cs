using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Pathfinding.Collections;

public struct BitField16 : IEnumerable<int>, IEnumerable
{
	private ushort bits;

	public short Bits => (short)bits;

	public long Bits64 => bits;

	public bool this[int i]
	{
		get
		{
			return BitFieldHelper.GetValue(bits, i);
		}
		set
		{
			bits = (ushort)BitFieldHelper.SetValue(bits, i, value);
		}
	}

	public void Add(int i)
	{
		bits |= (ushort)(1 << i);
	}

	public void AddBits(long valueBits)
	{
		bits |= (ushort)valueBits;
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
public struct BitField16<T> : IEnumerable<T>, IEnumerable where T : IConvertible
{
	private ushort bits;

	public short Bits => (short)bits;

	public long Bits64 => bits;

	public T this[int i]
	{
		get
		{
			return BitFieldHelper<T>.GetValue(bits, i);
		}
		set
		{
			bits = (ushort)BitFieldHelper<T>.SetValue(bits, i, value);
		}
	}

	public void Add(int i, T value)
	{
		AddBits(BitFieldHelper<T>.GetBits(i, value));
	}

	public void AddBits(long valueBits)
	{
		bits |= (ushort)valueBits;
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
