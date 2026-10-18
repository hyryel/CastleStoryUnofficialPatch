using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallSubset_16from16_Packed : ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IComparable<SmallSubset_16from16_Packed>, IEquatable<SmallSubset_16from16_Packed>
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
				uint num = (x | (x >> 1)) & 0x5555;
				uint num2 = num * num * 286331153 % 15;
				return (int)(15 + x % 3 - num2 * 2);
			}
		}

		public Enumerator(uint bits)
		{
			x = 0u;
			remaining = bits;
		}

		public bool MoveNext()
		{
			x = remaining & ~(remaining - 1);
			if (x == 0)
			{
				return false;
			}
			remaining -= x;
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

	private const uint GroupsOf3 = 37449u;

	private const uint GroupsOf3L1 = 74898u;

	private const uint GroupsOf3L2 = 149796u;

	private const uint GroupsOf4 = 286331153u;

	private const uint GroupsOf8 = 16843009u;

	private const uint GroupsOf9 = 134480385u;

	private const uint DoubleGroupsOf9 = 2286166545u;

	private uint bits;

	public int Count
	{
		get
		{
			uint num = (bits & 0x9249) % 7;
			uint num2 = (bits & 0x12492) % 14 >> 1;
			uint num3 = (bits & 0x24924) % 28 >> 2;
			return (int)(num + num2 + num3);
		}
	}

	public int Capacity => 16;

	public bool Empty => bits == 0;

	public bool Full => bits == 286331153;

	bool ICollection<int>.IsReadOnly => false;

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
			uint num3 = num;
			num &= num - 1;
			uint num4 = num3 - num;
			uint num5 = (num4 | (num4 >> 1)) * 21845;
			uint num6 = num5 * num5 * 286331153 % 15;
			int num7 = (int)(15 + num3 % 3 - num6 * 2);
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

	public int CompareTo(SmallSubset_16from16_Packed other)
	{
		return bits.CompareTo(other.bits);
	}

	public bool Equals(SmallSubset_16from16_Packed other)
	{
		return bits.Equals(other.bits);
	}

	public static bool operator ==(SmallSubset_16from16_Packed a, SmallSubset_16from16_Packed b)
	{
		return a.bits == b.bits;
	}

	public static bool operator !=(SmallSubset_16from16_Packed a, SmallSubset_16from16_Packed b)
	{
		return a.bits != b.bits;
	}
}
