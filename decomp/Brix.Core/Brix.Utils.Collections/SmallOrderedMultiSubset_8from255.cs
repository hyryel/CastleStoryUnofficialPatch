using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public struct SmallOrderedMultiSubset_8from255 : IEnumerable<KeyValuePair<int, int>>, IEnumerable
{
	public struct Enumerator : IEnumerator<KeyValuePair<int, int>>, IDisposable, IEnumerator
	{
		private SmallOrderedSubset_8from255.Enumerator keys;

		private ulong counts;

		private int current;

		object IEnumerator.Current => current;

		public KeyValuePair<int, int> Current => new KeyValuePair<int, int>(keys.Current, current);

		public Enumerator(SmallOrderedSubset_8from255 keys, ulong counts)
		{
			this.keys = keys.GetEnumerator();
			this.counts = counts;
			current = 0;
		}

		public bool MoveNext()
		{
			current = (int)counts & 0xFF;
			if (!keys.MoveNext())
			{
				return false;
			}
			counts >>= 8;
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

	public const ulong MinusOne = ulong.MaxValue;

	public SmallOrderedSubset_8from255 Keys;

	public ulong Counts;

	public int Count => Keys.Count;

	public int Capacity => 8;

	public bool Empty => Keys.Empty;

	public bool Full => Keys.Full;

	public int this[int x]
	{
		get
		{
			uint enc = (uint)(x + 1);
			ulong existingWeight = Keys.GetExistingWeight(enc);
			return (int)((Counts & (existingWeight * 255)) / existingWeight);
		}
		set
		{
			uint enc = (uint)(x + 1);
			ulong num = Keys.GetExistingWeight(enc);
			if (num == 0)
			{
				num = Keys.GetFreeGroupWeight();
				Keys.SetFreeBits(enc, num);
			}
			int num2 = (int)(Counts / num);
			Counts ^= (uint)((value ^ num2) & 0xFF) * num;
		}
	}

	public int[] CountArray
	{
		get
		{
			int[] array = new int[Count];
			Enumerator enumerator = GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext())
			{
				array[num] = enumerator.Current.Value;
				num++;
			}
			return array;
		}
	}

	public int GetValueOrDefault(int x)
	{
		return Keys.Contains(x) ? this[x] : 0;
	}

	public void Increment(int x, int value)
	{
		uint enc = (uint)(x + 1);
		ulong num = Keys.GetExistingWeight(enc);
		if (num == 0)
		{
			num = Keys.GetFreeGroupWeight();
			Keys.SetFreeBits(enc, num);
		}
		int num2 = (int)(Counts / num);
		int num3 = num2 + value;
		num3 |= -((num3 ^ num2) >> 8) >> 31;
		Counts += (uint)((num3 & 0xFF) - num2) * num;
	}

	public void Increment_Unsafe(int x, int value)
	{
		uint enc = (uint)(x + 1);
		ulong num = Keys.GetExistingWeight(enc);
		if (num == 0)
		{
			num = Keys.GetFreeGroupWeight();
			Keys.SetFreeBits(enc, num);
		}
		Counts += (ulong)((long)value * (long)num);
	}

	public void Decrement(int x, int value)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = Keys.GetExistingWeight(enc);
		if (existingWeight != 0)
		{
			int num = (int)(Counts / existingWeight);
			int num2 = num - value;
			num2 &= ~(num2 >> 31);
			Counts += (uint)((num2 & 0xFF) - num) * existingWeight;
			if (Counts % existingWeight == 0)
			{
				ulong num3 = existingWeight * ulong.MaxValue;
				ulong num4 = ~num3;
				Keys.RemoveBits(num3, num4);
				Counts = (Counts & num4) | ((Counts & (num3 << 8)) >> 8);
			}
		}
	}

	public void Decrement_Unsafe(int x, int value)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = Keys.GetExistingWeight(enc);
		if (existingWeight != 0)
		{
			Counts -= (ulong)((long)value * (long)existingWeight);
			if (Counts % existingWeight == 0)
			{
				ulong num = existingWeight * ulong.MaxValue;
				ulong num2 = ~num;
				Keys.RemoveBits(num, num2);
				Counts = (Counts & num2) | ((Counts & (num << 8)) >> 8);
			}
		}
	}

	public bool Add(int x)
	{
		uint enc = (uint)(x + 1);
		ulong num = Keys.GetExistingWeight(enc);
		if (num != 0)
		{
			ulong num2 = num * 255;
			if ((Counts & num2) == num2)
			{
				return false;
			}
			Counts += num;
		}
		else
		{
			num = Keys.GetFreeGroupWeight();
			Keys.SetFreeBits(enc, num);
			Counts |= num;
		}
		return num != 0;
	}

	public void Add_Unsafe(int x)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = Keys.GetExistingWeight(enc);
		if (existingWeight != 0)
		{
			Counts += existingWeight;
			return;
		}
		existingWeight = Keys.GetFreeGroupWeight();
		Keys.SetFreeBits(enc, existingWeight);
		Counts |= existingWeight;
	}

	public bool Remove(int x)
	{
		uint enc = (uint)(x + 1);
		ulong existingWeight = Keys.GetExistingWeight(enc);
		Counts -= existingWeight;
		ulong num = existingWeight * 255;
		if ((Counts & num) == 0)
		{
			ulong num2 = existingWeight * ulong.MaxValue;
			ulong num3 = ~num2;
			Keys.RemoveBits(num2, num3);
			Counts = (Counts & num3) | ((Counts & (num2 << 8)) >> 8);
		}
		return existingWeight != 0;
	}

	public void Clear()
	{
		Keys.Clear();
		Counts = 0uL;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(Keys, Counts);
	}

	IEnumerator<KeyValuePair<int, int>> IEnumerable<KeyValuePair<int, int>>.GetEnumerator()
	{
		return new Enumerator(Keys, Counts);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(Keys, Counts);
	}
}
