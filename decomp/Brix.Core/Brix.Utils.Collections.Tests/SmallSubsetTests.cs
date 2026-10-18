using System;
using System.Collections.Generic;
using System.Linq;

namespace Brix.Utils.Collections.Tests;

public static class SmallSubsetTests
{
	public static void Test1()
	{
		SmallOrderedSubset_8from15 smallOrderedSubset_8from = default(SmallOrderedSubset_8from15);
		smallOrderedSubset_8from.Add(1);
		smallOrderedSubset_8from.Add(2);
		smallOrderedSubset_8from.Add(3);
		smallOrderedSubset_8from.Add(0);
		smallOrderedSubset_8from.Add(5);
		smallOrderedSubset_8from.Remove(2);
		smallOrderedSubset_8from.Add(5);
		smallOrderedSubset_8from.Add(6);
		smallOrderedSubset_8from.Remove(3);
		if (smallOrderedSubset_8from.Full)
		{
			throw new InvalidOperationException("ss8255 full");
		}
		if (smallOrderedSubset_8from.Count != 4)
		{
			throw new InvalidOperationException("ss8255 count: " + smallOrderedSubset_8from.Count);
		}
		if (!smallOrderedSubset_8from.Contains(1) || !smallOrderedSubset_8from.Contains(0) || !smallOrderedSubset_8from.Contains(5) || !smallOrderedSubset_8from.Contains(6))
		{
			throw new InvalidOperationException("ss8255 added members");
		}
		if (smallOrderedSubset_8from.Contains(2) || smallOrderedSubset_8from.Contains(3))
		{
			throw new InvalidOperationException("ss8255 removed members");
		}
		smallOrderedSubset_8from.Add(7);
		smallOrderedSubset_8from.Add(8);
		smallOrderedSubset_8from.Add(9);
		smallOrderedSubset_8from.Add(10);
		if (!smallOrderedSubset_8from.Full)
		{
			throw new InvalidOperationException("ss8255 full");
		}
		if (smallOrderedSubset_8from.Count != 8)
		{
			throw new InvalidOperationException("ss8255 count: " + smallOrderedSubset_8from.Count);
		}
		if (!smallOrderedSubset_8from.Contains(1) || !smallOrderedSubset_8from.Contains(0) || !smallOrderedSubset_8from.Contains(5) || !smallOrderedSubset_8from.Contains(6) || !smallOrderedSubset_8from.Contains(7) || !smallOrderedSubset_8from.Contains(8) || !smallOrderedSubset_8from.Contains(9) || !smallOrderedSubset_8from.Contains(10))
		{
			throw new InvalidOperationException("ss8255 added members");
		}
		List<int> list = new List<int> { 1, 0, 5, 6, 7, 8, 9, 10 };
		foreach (int item in smallOrderedSubset_8from)
		{
			if (!list.Contains(item))
			{
				throw new InvalidOperationException("ss8255 to list contains: " + string.Join(", ", smallOrderedSubset_8from.Select((int x) => string.Concat(x)).ToArray()));
			}
		}
		foreach (int item2 in list)
		{
			if (!smallOrderedSubset_8from.Contains(item2))
			{
				throw new InvalidOperationException("list to ss8255 contains" + string.Join(", ", smallOrderedSubset_8from.Select((int x) => string.Concat(x)).ToArray()));
			}
		}
	}

	public static void Test2()
	{
		SmallOrderedMultiSubset_8from255 smallOrderedMultiSubset_8from = new SmallOrderedMultiSubset_8from255 { 1, 2 };
		for (int i = 0; i < 5; i++)
		{
			smallOrderedMultiSubset_8from.Add(3);
		}
		smallOrderedMultiSubset_8from.Add(4);
		smallOrderedMultiSubset_8from.Add(5);
		smallOrderedMultiSubset_8from.Remove(2);
		smallOrderedMultiSubset_8from.Add(5);
		for (int j = 0; j < 8; j++)
		{
			smallOrderedMultiSubset_8from.Add(6);
		}
		smallOrderedMultiSubset_8from.Remove(3);
		if (smallOrderedMultiSubset_8from.Full)
		{
			throw new InvalidOperationException("ss8255 full");
		}
		if (smallOrderedMultiSubset_8from.Count != 5)
		{
			throw new InvalidOperationException("ss8255 count");
		}
		if (smallOrderedMultiSubset_8from[1] != 1 || smallOrderedMultiSubset_8from[4] != 1 || smallOrderedMultiSubset_8from[5] != 2 || smallOrderedMultiSubset_8from[6] != 8)
		{
			throw new InvalidOperationException("ss8255 added members");
		}
		if (smallOrderedMultiSubset_8from[3] != 4)
		{
			throw new InvalidOperationException("ss8255 mixed members");
		}
		if (smallOrderedMultiSubset_8from.Keys.Contains(2))
		{
			throw new InvalidOperationException("ss8255 removed members");
		}
		smallOrderedMultiSubset_8from.Add(7);
		smallOrderedMultiSubset_8from.Add(8);
		smallOrderedMultiSubset_8from.Add(9);
		smallOrderedMultiSubset_8from.Remove(4);
		smallOrderedMultiSubset_8from.Add(10);
		smallOrderedMultiSubset_8from.Add(11);
		if (!smallOrderedMultiSubset_8from.Full)
		{
			throw new InvalidOperationException("ss8255 full");
		}
		if (smallOrderedMultiSubset_8from.Count != 8)
		{
			throw new InvalidOperationException("ss8255 count");
		}
		if (smallOrderedMultiSubset_8from[7] != 1 || smallOrderedMultiSubset_8from[8] != 1 || smallOrderedMultiSubset_8from[9] != 1 || smallOrderedMultiSubset_8from[10] != 1)
		{
			throw new InvalidOperationException("ss8255 added members");
		}
		if (smallOrderedMultiSubset_8from.Keys.Contains(4))
		{
			throw new InvalidOperationException("ss8255 removed members");
		}
		if (smallOrderedMultiSubset_8from.Keys.Contains(11))
		{
			throw new InvalidOperationException("ss8255 overflowed members");
		}
	}

	public static void Test3()
	{
		List<int> list = new List<int>();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		HashSet<int> hashSet = new HashSet<int>();
		SmallOrderedSubset_8from15 smallOrderedSubset_8from = default(SmallOrderedSubset_8from15);
		SmallOrderedSubset_8from255 smallOrderedSubset_8from2 = default(SmallOrderedSubset_8from255);
		SmallOrderedMultiSubset_8from255 smallOrderedMultiSubset_8from = default(SmallOrderedMultiSubset_8from255);
		Random random = new Random();
		for (int i = 0; i < 20; i++)
		{
			int item = random.Next(255);
			list.Add(item);
		}
		for (int j = 0; j < 1000; j++)
		{
			int index = random.Next(list.Count);
			int num = list[index];
			int num2 = random.Next(3);
			if (num2 > 0)
			{
				if (dictionary.Count < 8)
				{
					dictionary[num] = dictionary.GetValueOrDefault(num) + 1;
				}
				if (hashSet.Count < 8)
				{
					hashSet.Add(num);
				}
				if (num < 15 && !smallOrderedSubset_8from.Full)
				{
					smallOrderedSubset_8from.Add(num);
				}
				if (!smallOrderedSubset_8from2.Full)
				{
					smallOrderedSubset_8from2.Add(num);
				}
				if (!smallOrderedMultiSubset_8from.Full)
				{
					smallOrderedMultiSubset_8from.Add(num);
				}
			}
			else
			{
				int num3 = (dictionary[num] = dictionary.GetValueOrDefault(num) - 1);
				if (num3 <= 0)
				{
					dictionary.Remove(num);
				}
				hashSet.Remove(num);
				if (num < 15)
				{
					smallOrderedSubset_8from.Remove(num);
				}
				smallOrderedSubset_8from2.Remove(num);
				smallOrderedMultiSubset_8from.Remove(num);
			}
		}
		foreach (int item2 in hashSet)
		{
			if (item2 < 15 && !smallOrderedSubset_8from.Contains(item2))
			{
				throw new InvalidOperationException("subset vs ss815 contains");
			}
			if (!smallOrderedSubset_8from2.Contains(item2))
			{
				throw new InvalidOperationException("subset vs ss8255 contains");
			}
		}
		foreach (int item3 in smallOrderedSubset_8from)
		{
			if (!hashSet.Contains(item3))
			{
				throw new InvalidOperationException("ss815 vs subset contains");
			}
		}
		foreach (int item4 in smallOrderedSubset_8from2)
		{
			if (!hashSet.Contains(item4))
			{
				throw new InvalidOperationException("ss8255 vs subset contains");
			}
		}
		foreach (KeyValuePair<int, int> item5 in dictionary)
		{
			if (!smallOrderedMultiSubset_8from.Keys.Contains(item5.Key))
			{
				throw new InvalidOperationException("list vs sms8255 contains");
			}
			if (item5.Value != smallOrderedMultiSubset_8from[item5.Key])
			{
				throw new InvalidOperationException("list vs sms8255 count");
			}
		}
		foreach (KeyValuePair<int, int> item6 in smallOrderedMultiSubset_8from)
		{
			if (!dictionary.ContainsKey(item6.Key))
			{
				throw new InvalidOperationException("sms8255 vs list contains");
			}
		}
	}
}
