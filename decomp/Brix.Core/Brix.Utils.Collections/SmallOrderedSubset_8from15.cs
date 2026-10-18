using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallOrderedSubset_8from15 : ISmallOrderedSubset, ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IList<int>, IComparable<SmallOrderedSubset_8from15>, IEquatable<SmallOrderedSubset_8from15>
{
	public struct Enumerator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private uint x;

		private uint next;

		object IEnumerator.Current => (int)((x & 0xF) - 1);

		public int Current => (int)((x & 0xF) - 1);

		public Enumerator(uint packedIndices)
		{
			x = 0u;
			next = packedIndices;
		}

		public bool MoveNext()
		{
			x = next;
			if (x == 0)
			{
				return false;
			}
			next >>= 4;
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

	private const int GroupSize = 4;

	private const uint GroupMask = 15u;

	private const int StorageSize = 32;

	private const uint SpacedOnesMask = 286331153u;

	private const uint MinusMax = 4294967281u;

	private uint packedIndices;

	public int Count => CountIndices();

	public int Capacity => 8;

	public bool Empty => packedIndices == 0;

	public bool Full => packedIndices >= 268435456;

	public int First => (int)((packedIndices & 0xF) - 1);

	public int this[int i]
	{
		get
		{
			return (int)(((packedIndices >> i * 4) & 0xF) - 1);
		}
		set
		{
			uint num = (uint)(value + 1);
			int num2 = i * 4;
			packedIndices = (packedIndices & (uint)(~(15 << num2))) | (num << num2);
		}
	}

	bool ICollection<int>.IsReadOnly => false;

	public void Clear()
	{
		packedIndices = 0u;
	}

	public int CountIndices()
	{
		uint num = packedIndices;
		uint num2 = num | (num >> 1);
		uint num3 = num2 | (num2 >> 2);
		return (int)((num3 & 0x11111111) % 15);
	}

	public bool Contains(int i)
	{
		uint enc = (uint)(i + 1);
		return GetExistingWeight(enc) != 0;
	}

	public int IndexOf(int x)
	{
		uint enc = (uint)(x + 1);
		uint existingWeight = GetExistingWeight(enc);
		return (int)(((existingWeight - 1) & 0x11111111) % 15);
	}

	public int GetOrAddIndexOf(int x)
	{
		uint num = (uint)(x + 1);
		uint existingOrFreeGroupWeight = GetExistingOrFreeGroupWeight(num);
		packedIndices |= existingOrFreeGroupWeight * num;
		return (int)(((existingOrFreeGroupWeight - 1) & 0x11111111) % 15);
	}

	public bool Add(int i)
	{
		uint num = (uint)(i + 1);
		if (GetExistingWeight(num) != 0)
		{
			return false;
		}
		uint freeGroupWeight = GetFreeGroupWeight();
		if (freeGroupWeight == 0)
		{
			throw new InvalidOperationException($"Add group overflow: i={i}, Bits={packedIndices}");
		}
		packedIndices |= freeGroupWeight * num;
		return true;
	}

	public bool Remove(int i)
	{
		uint enc = (uint)(i + 1);
		uint existingWeight = GetExistingWeight(enc);
		uint num = (uint)(0uL - (ulong)existingWeight);
		uint num2 = ~num;
		packedIndices = (packedIndices & num2) | ((packedIndices & (num << 4)) >> 4);
		return existingWeight != 0;
	}

	public void Append(int i)
	{
		uint num = (uint)(i + 1);
		uint freeGroupWeight = GetFreeGroupWeight();
		packedIndices |= freeGroupWeight * num;
	}

	public int Pop()
	{
		uint freeGroupWeight = GetFreeGroupWeight();
		uint num = (uint)(0uL - (ulong)freeGroupWeight);
		uint num2 = ~num >> 4;
		int num3 = (int)(packedIndices / (num2 + 1));
		packedIndices &= num2;
		return num3 - 1;
	}

	public void AddOrIgnore(int i)
	{
		uint num = (uint)(i + 1);
		uint existingOrFreeGroupWeight = GetExistingOrFreeGroupWeight(num);
		packedIndices |= existingOrFreeGroupWeight * num;
	}

	public void RemoveOrIgnore(int i)
	{
		uint enc = (uint)(i + 1);
		uint existingWeight = GetExistingWeight(enc);
		uint num = (uint)(0uL - (ulong)existingWeight);
		uint num2 = ~num;
		packedIndices = (packedIndices & num2) | ((packedIndices & (num << 4)) >> 4);
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		uint num = packedIndices;
		int num2 = arrayIndex;
		while (num != 0)
		{
			int num3 = (int)((num & 0xF) - 1);
			array[num2++] = num3;
			num >>= 4;
		}
	}

	void ICollection<int>.Add(int i)
	{
		AddOrIgnore(i);
	}

	public void Insert(int index, int item)
	{
		uint num = (uint)(item + 1);
		int num2 = index * 4;
		uint num3 = (uint)(-1 << num2);
		uint num4 = ~num3;
		packedIndices = (packedIndices & num4) | ((packedIndices & num3) << 4);
		packedIndices |= num << num2;
	}

	public void RemoveAt(int index)
	{
		uint enc = (packedIndices >> index * 4) & 0xF;
		uint existingWeight = GetExistingWeight(enc);
		uint num = (uint)(0uL - (ulong)existingWeight);
		uint num2 = ~num;
		packedIndices = (packedIndices & num2) | ((packedIndices & (num << 4)) >> 4);
	}

	public uint GetExistingWeight(uint enc)
	{
		uint enc2 = ~packedIndices ^ (enc * 286331153);
		return GetFullBits(enc2);
	}

	public uint GetFreeGroupWeight()
	{
		return GetFullBits(~packedIndices) * 4294967281u;
	}

	public uint GetFullBits(uint enc)
	{
		uint num = enc & (enc >> 1);
		uint num2 = num & (num >> 2);
		return num2 & 0x11111111;
	}

	public uint GetExistingOrFreeGroupWeight(uint enc)
	{
		uint existingWeight = GetExistingWeight(enc);
		uint freeGroupWeight = GetFreeGroupWeight();
		uint num = existingWeight + freeGroupWeight;
		return num & ~(num - 1);
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(packedIndices);
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return new Enumerator(packedIndices);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(packedIndices);
	}

	public int CompareTo(SmallOrderedSubset_8from15 other)
	{
		return packedIndices.CompareTo(other.packedIndices);
	}

	public bool Equals(SmallOrderedSubset_8from15 other)
	{
		return packedIndices.Equals(other.packedIndices);
	}

	public static bool operator ==(SmallOrderedSubset_8from15 a, SmallOrderedSubset_8from15 b)
	{
		return a.packedIndices == b.packedIndices;
	}

	public static bool operator !=(SmallOrderedSubset_8from15 a, SmallOrderedSubset_8from15 b)
	{
		return a.packedIndices != b.packedIndices;
	}
}
