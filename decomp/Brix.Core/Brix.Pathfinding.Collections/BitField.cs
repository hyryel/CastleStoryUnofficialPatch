using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Pathfinding.Collections;

public struct BitField : IEnumerable<int>, IEnumerable
{
	private uint bits;

	public int Bits => (int)bits;

	public long Bits64 => bits;

	public bool this[int i]
	{
		get
		{
			return BitFieldHelper.GetValue(bits, i);
		}
		set
		{
			bits = (uint)BitFieldHelper.SetValue(bits, i, value);
		}
	}

	public void Add(int i)
	{
		bits |= (uint)(1 << i);
	}

	public void AddBits(long valueBits)
	{
		bits |= (uint)(int)valueBits;
	}

	public void Clear()
	{
		bits = 0u;
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
public struct BitField<T> : IEnumerable<T>, IEnumerable where T : IConvertible
{
	private uint bits;

	public int Bits => (int)bits;

	public long Bits64 => bits;

	public T this[int i]
	{
		get
		{
			return BitFieldHelper<T>.GetValue(bits, i);
		}
		set
		{
			bits = (uint)BitFieldHelper<T>.SetValue(bits, i, value);
		}
	}

	public void Add(int i, T value)
	{
		AddBits(BitFieldHelper<T>.GetBits(i, value));
	}

	public void AddBits(long valueBits)
	{
		bits |= (uint)(int)valueBits;
	}

	public void Clear()
	{
		bits = 0u;
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
