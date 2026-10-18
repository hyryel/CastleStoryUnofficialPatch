using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallSubset_64from64 : ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IComparable<SmallSubset_64from64>, IEquatable<SmallSubset_64from64>
{
	public struct Enumerator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private ulong x;

		private ulong remaining;

		object IEnumerator.Current => Current;

		public int Current
		{
			get
			{
				ulong num = x - remaining;
				int num2 = (int)(((num >> 4) & 0x808080808080808L) % 2040);
				int num3 = (int)((uint)((int)(num >> num2 + 1) * 138547332) % 31u);
				return num2 + num3;
			}
		}

		public Enumerator(ulong bits)
		{
			x = 0uL;
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

	private const ulong GroupsOf2 = 6148914691236517205uL;

	private const ulong GroupsOf4 = 1229782938247303441uL;

	private const ulong GroupsOf4x3 = 3689348814741910323uL;

	private const ulong GroupsOf8 = 72340172838076673uL;

	private const ulong GroupsOf8L3 = 578721382704613384uL;

	private const ulong GroupsOf8x15 = 1085102592571150095uL;

	private const uint MaskOf8 = 255u;

	private const uint MaskOf8L3 = 2040u;

	private const ulong GroupsOf9 = 36099303471055874uL;

	private const ulong GroupsOf7 = 145249953336295682uL;

	private const ulong GroupsOf5 = 595056260442243600uL;

	private ulong bits;

	public int Count
	{
		get
		{
			ulong num = bits - ((bits >> 1) & 0x5555555555555555L);
			ulong num2 = (num & 0x3333333333333333L) + ((num >> 2) & 0x3333333333333333L);
			ulong num3 = (num2 + (num2 >> 4)) & 0xF0F0F0F0F0F0F0FL;
			return (int)(num3 * 72340172838076673L >> 56);
		}
	}

	public int Capacity => 64;

	public bool Empty => bits == 0;

	public bool Full => bits == ulong.MaxValue;

	bool ICollection<int>.IsReadOnly => false;

	public static SmallSubset_64from64 FromBits(long bits)
	{
		return new SmallSubset_64from64
		{
			bits = (ulong)bits
		};
	}

	public void Clear()
	{
		bits = 0uL;
	}

	public bool Contains(int i)
	{
		return ((bits >> i) & 1) != 0;
	}

	public bool Add(int i)
	{
		ulong num = (ulong)(1L << i);
		if ((bits & num) != 0)
		{
			return false;
		}
		bits |= num;
		return true;
	}

	public bool Remove(int i)
	{
		ulong num = (ulong)(1L << i);
		if ((bits & num) == 0)
		{
			return false;
		}
		bits -= num;
		return true;
	}

	public void AddOrIgnore(int i)
	{
		ulong num = (ulong)(1L << i);
		bits |= num;
	}

	public void RemoveOrIgnore(int i)
	{
		ulong num = (ulong)(1L << i);
		bits &= ~num;
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		ulong num = bits;
		int num2 = arrayIndex;
		while (num != 0)
		{
			ulong num3 = num - 1;
			num &= num3;
			ulong num4 = num3 - num;
			int num5 = (int)(((num4 >> 4) & 0x808080808080808L) % 2040);
			int num6 = (int)((uint)((int)(num4 >> num5 + 1) * 138547332) % 31u);
			int num7 = num5 + num6;
			array[num2++] = num7;
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

	public int CompareTo(SmallSubset_64from64 other)
	{
		return bits.CompareTo(other.bits);
	}

	public bool Equals(SmallSubset_64from64 other)
	{
		return bits.Equals(other.bits);
	}

	public static bool operator ==(SmallSubset_64from64 a, SmallSubset_64from64 b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(SmallSubset_64from64 a, SmallSubset_64from64 b)
	{
		return a.bits != b.bits;
	}
}
