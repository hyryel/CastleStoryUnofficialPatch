using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallSubset_8from8 : ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IComparable<SmallSubset_8from8>, IEquatable<SmallSubset_8from8>
{
	public struct Enumerator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private int x;

		private int remaining;

		object IEnumerator.Current => Current;

		public int Current
		{
			get
			{
				int num = x - remaining;
				return (num & 0x11111111) % 15;
			}
		}

		public Enumerator(int bits)
		{
			x = 0;
			remaining = bits;
		}

		public bool MoveNext()
		{
			if (remaining == 0)
			{
				return false;
			}
			x = remaining - 1;
			remaining &= x;
			return true;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const int GroupsOf4 = 286331153;

	private int bits;

	public int Count => bits % 15;

	public int Capacity => 8;

	public bool Empty => bits == 0;

	public bool Full => bits == 286331153;

	bool ICollection<int>.IsReadOnly => false;

	public void Clear()
	{
		bits = 0;
	}

	public bool Contains(int i)
	{
		return ((bits >> (i << 2)) & 1) != 0;
	}

	public bool Add(int i)
	{
		int num = 1 << (i << 2);
		if ((bits & num) != 0)
		{
			return false;
		}
		bits |= num;
		return true;
	}

	public bool Remove(int i)
	{
		int num = 1 << (i << 2);
		if ((bits & num) == 0)
		{
			return false;
		}
		bits -= num;
		return true;
	}

	public void AddOrIgnore(int i)
	{
		int num = 1 << (i << 2);
		bits |= num;
	}

	public void RemoveOrIgnore(int i)
	{
		int num = 1 << (i << 2);
		bits &= ~num;
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		int num = bits;
		int num2 = arrayIndex;
		while (num != 0)
		{
			int num3 = num - 1;
			num &= num3;
			int num4 = num3 - num;
			int num5 = (num4 & 0x11111111) % 15;
			array[num2++] = num5;
		}
	}

	void ICollection<int>.Add(int i)
	{
		AddOrIgnore(i);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(bits);
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return new Enumerator(bits);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(bits);
	}

	public int CompareTo(SmallSubset_8from8 other)
	{
		return bits.CompareTo(other.bits);
	}

	public bool Equals(SmallSubset_8from8 other)
	{
		return bits.Equals(other.bits);
	}

	public static bool operator ==(SmallSubset_8from8 a, SmallSubset_8from8 b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(SmallSubset_8from8 a, SmallSubset_8from8 b)
	{
		return a.bits != b.bits;
	}
}
