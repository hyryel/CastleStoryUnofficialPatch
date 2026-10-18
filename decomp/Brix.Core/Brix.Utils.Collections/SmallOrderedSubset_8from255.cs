using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallOrderedSubset_8from255 : ISmallOrderedSubset, ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IList<int>, IComparable<SmallOrderedSubset_8from255>, IEquatable<SmallOrderedSubset_8from255>
{
	public struct Enumerator : IEnumerator<int>, IDisposable, IEnumerator
	{
		private int x;

		private ulong next;

		object IEnumerator.Current => (x & 0xFF) - 1;

		public int Current => (x & 0xFF) - 1;

		public Enumerator(ulong packedIndices)
		{
			x = 0;
			next = packedIndices;
		}

		public bool MoveNext()
		{
			x = (int)next;
			if (x == 0)
			{
				return false;
			}
			next >>= 8;
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

	public const int GroupSize = 8;

	public const int GroupMask = 255;

	public const int StorageSize = 64;

	public const ulong MinusOne = ulong.MaxValue;

	private const ulong MinusMax = 18446744073709551361uL;

	private const ulong SpacedOnesMask = 72340172838076673uL;

	private ulong packedIndices;

	public int Count => CountIndices();

	public int Capacity => 8;

	public bool Empty => packedIndices == 0;

	public bool Full => packedIndices >= 72057594037927936L;

	public int First => (int)(packedIndices & 0xFF) - 1;

	public int this[int i]
	{
		get
		{
			return (int)((packedIndices >> i * 8) & 0xFF) - 1;
		}
		set
		{
			uint num = (uint)(value + 1);
			int num2 = i * 8;
			packedIndices = (packedIndices & (ulong)(~(long)(255 << num2))) | (num << num2);
		}
	}

	public ulong Bits => packedIndices;

	public int[] Array
	{
		get
		{
			int[] array = new int[Count];
			Enumerator enumerator = GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num] = enumerator.Current;
				num++;
			}
			return array;
		}
	}

	bool ICollection<int>.IsReadOnly => false;

	public void Clear()
	{
		packedIndices = 0uL;
	}

	public int CountIndices()
	{
		ulong num = packedIndices;
		ulong num2 = num | (num >> 1);
		ulong num3 = num2 | (num2 >> 2);
		ulong num4 = num3 | (num3 >> 4);
		return (int)((num4 & 0x101010101010101L) % 255);
	}

	public bool Contains(int x)
	{
		uint enc = (uint)(x + 1);
		return GetExistingWeight(enc) != 0;
	}

	public int IndexOf(int x)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = GetExistingWeight(enc);
		return (int)(((existingWeight - 1) & 0x101010101010101L) % 255);
	}

	public int GetOrAddIndexOf(int x)
	{
		uint num = (uint)(x + 1);
		ulong existingOrFreeGroupWeight = GetExistingOrFreeGroupWeight(num);
		packedIndices |= existingOrFreeGroupWeight * num;
		return (int)(((existingOrFreeGroupWeight - 1) & 0x101010101010101L) % 255);
	}

	public bool Add(int x)
	{
		uint num = (uint)(x + 1);
		if (GetExistingWeight(num) != 0)
		{
			return false;
		}
		ulong freeGroupWeight = GetFreeGroupWeight();
		if (freeGroupWeight == 0)
		{
			throw new InvalidOperationException($"Add group overflow: i={x}, Bits={packedIndices}");
		}
		packedIndices |= freeGroupWeight * num;
		return true;
	}

	public bool Remove(int x)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = GetExistingWeight(enc);
		ulong num = existingWeight * ulong.MaxValue;
		ulong num2 = ~num;
		packedIndices = (packedIndices & num2) | ((packedIndices & (num << 8)) >> 8);
		return existingWeight != 0;
	}

	public void Append(int x)
	{
		uint num = (uint)(x + 1);
		ulong freeGroupWeight = GetFreeGroupWeight();
		packedIndices |= freeGroupWeight * num;
	}

	public int Pop()
	{
		ulong freeGroupWeight = GetFreeGroupWeight();
		ulong num = 0L - freeGroupWeight;
		ulong num2 = ~num >> 8;
		int num3 = (int)(packedIndices / (num2 + 1));
		packedIndices &= num2;
		return num3 - 1;
	}

	public void AddOrIgnore(int x)
	{
		uint num = (uint)(x + 1);
		ulong existingOrFreeGroupWeight = GetExistingOrFreeGroupWeight(num);
		packedIndices |= existingOrFreeGroupWeight * num;
	}

	public void RemoveOrIgnore(int x)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = GetExistingWeight(enc);
		ulong num = existingWeight * ulong.MaxValue;
		ulong num2 = ~num;
		packedIndices = (packedIndices & num2) | ((packedIndices & (num << 8)) >> 8);
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		ulong num = packedIndices;
		int num2 = arrayIndex;
		while (num != 0)
		{
			int num3 = (int)(num & 0xFF) - 1;
			array[num2++] = num3;
			num >>= 8;
		}
	}

	void ICollection<int>.Add(int i)
	{
		AddOrIgnore(i);
	}

	public void Insert(int index, int item)
	{
		uint num = (uint)(item + 1);
		int num2 = index * 8;
		uint num3 = (uint)(-1 << num2);
		uint num4 = ~num3;
		packedIndices = (packedIndices & num4) | ((packedIndices & num3) << 8);
		packedIndices |= num << num2;
	}

	public void RemoveAt(int index)
	{
		uint enc = (uint)((int)(packedIndices >> index * 8) & 0xFF);
		ulong existingWeight = GetExistingWeight(enc);
		ulong num = 0L - existingWeight;
		ulong num2 = ~num;
		packedIndices = (packedIndices & num2) | ((packedIndices & (num << 8)) >> 8);
	}

	internal void SetFreeBits(uint enc, ulong weight)
	{
		packedIndices |= weight * enc;
	}

	internal void RemoveBits(ulong high, ulong low)
	{
		packedIndices = (packedIndices & low) | ((packedIndices & (high << 8)) >> 8);
	}

	public ulong GetExistingWeight(uint enc)
	{
		ulong enc2 = ~packedIndices ^ (ulong)((long)enc * 72340172838076673L);
		return GetFullBits(enc2);
	}

	public ulong GetFreeGroupWeight()
	{
		return GetFullBits(~packedIndices) * 18446744073709551361uL;
	}

	public ulong GetFullBits(ulong enc)
	{
		ulong num = enc & (enc >> 1);
		ulong num2 = num & (num >> 2);
		ulong num3 = num2 & (num2 >> 4);
		return num3 & 0x101010101010101L;
	}

	public ulong GetExistingOrFreeGroupWeight(uint enc)
	{
		ulong existingWeight = GetExistingWeight(enc);
		ulong freeGroupWeight = GetFreeGroupWeight();
		ulong num = existingWeight + freeGroupWeight;
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

	public int CompareTo(SmallOrderedSubset_8from255 other)
	{
		return packedIndices.CompareTo(other.packedIndices);
	}

	public bool Equals(SmallOrderedSubset_8from255 other)
	{
		return packedIndices.Equals(other.packedIndices);
	}

	public static bool operator ==(SmallOrderedSubset_8from255 a, SmallOrderedSubset_8from255 b)
	{
		return a.packedIndices == b.packedIndices;
	}

	public static bool operator !=(SmallOrderedSubset_8from255 a, SmallOrderedSubset_8from255 b)
	{
		return a.packedIndices != b.packedIndices;
	}
}
