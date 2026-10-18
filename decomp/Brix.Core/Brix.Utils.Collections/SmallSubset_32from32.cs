using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallSubset_32from32 : ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IComparable<SmallSubset_32from32>, IEquatable<SmallSubset_32from32>
{
	public struct Enumerator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private uint x;

		private uint remaining;

		object IEnumerator.Current => Current;

		public int Current
		{
			get
			{
				uint num = x - remaining;
				int num2 = (int)((num & 0x44444444) % 60);
				int num3 = (int)(num >> num2);
				return num2 | ((num3 >> 1) % 3 + (num3 & 1));
			}
		}

		public Enumerator(uint bits)
		{
			x = 0u;
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

	private const uint GroupsOf3 = 613566756u;

	private const uint GroupsOf3L1 = 1227133512u;

	private const uint GroupsOf3L2 = 2454267024u;

	private const uint GroupsOf4 = 286331153u;

	private const uint GroupsOf4L2 = 1145324612u;

	private const uint GroupsOf4x3 = 858993459u;

	private const uint GroupsOf5 = 138547332u;

	private const uint GroupsOf2 = 1431655765u;

	private const uint MaskOf4 = 15u;

	private const uint MaskOf4L2 = 60u;

	private const uint GroupsOf8 = 16843009u;

	private const uint GroupsOf8x7 = 117901063u;

	private const uint GroupsOf8x15 = 252645135u;

	private const uint GroupsOf9 = 134480385u;

	private const uint DoubleGroupsOf9 = 2286166545u;

	private uint bits;

	public int Count
	{
		get
		{
			uint num = bits - ((bits >> 1) & 0x55555555);
			uint num2 = (num & 0x33333333) + ((num >> 2) & 0x33333333);
			uint num3 = (num2 + (num2 >> 4)) & 0xF0F0F0F;
			return (int)(num3 * 16843009 >> 24);
		}
	}

	public int Capacity => 32;

	public bool Empty => bits == 0;

	public bool Full => bits == uint.MaxValue;

	bool ICollection<int>.IsReadOnly => false;

	public static SmallSubset_32from32 FromBits(int bits)
	{
		return new SmallSubset_32from32
		{
			bits = (uint)bits
		};
	}

	public void Clear()
	{
		bits = 0u;
	}

	public bool Contains(int i)
	{
		return ((bits >> i) & 1) != 0;
	}

	public bool Add(int i)
	{
		uint num = (uint)(1 << i);
		if ((bits & num) != 0)
		{
			return false;
		}
		bits |= num;
		return true;
	}

	public bool Remove(int i)
	{
		uint num = (uint)(1 << i);
		if ((bits & num) == 0)
		{
			return false;
		}
		bits -= num;
		return true;
	}

	public void AddOrIgnore(int i)
	{
		uint num = (uint)(1 << i);
		bits |= num;
	}

	public void RemoveOrIgnore(int i)
	{
		uint num = (uint)(1 << i);
		bits &= ~num;
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		uint num = bits;
		int num2 = arrayIndex;
		while (num != 0)
		{
			uint num3 = num - 1;
			num &= num3;
			uint num4 = num3 - num;
			int num5 = (int)((num4 & 0x44444444) % 60);
			int num6 = (int)(num4 >> num5);
			int num7 = num5 | ((num6 >> 1) % 3 + (num6 & 1));
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

	public int CompareTo(SmallSubset_32from32 other)
	{
		return bits.CompareTo(other.bits);
	}

	public bool Equals(SmallSubset_32from32 other)
	{
		return bits.Equals(other.bits);
	}

	public static bool operator ==(SmallSubset_32from32 a, SmallSubset_32from32 b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(SmallSubset_32from32 a, SmallSubset_32from32 b)
	{
		return a.bits != b.bits;
	}
}
