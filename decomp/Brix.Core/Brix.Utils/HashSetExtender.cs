using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class HashSetExtender
{
	public static PooledHashSetEnumerator<T> GetPooledHashSetEnumerator<T>(this HashSet<T> list)
	{
		return new PooledHashSetEnumerator<T>(list);
	}

	public static void RemoveWhere<T1, T2>(this List<T1> list, Func<T1, bool> predicate, List<T2> otherList)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (num != i)
			{
				list[num] = list[i];
				otherList[num] = otherList[i];
			}
			if (!predicate(list[i]))
			{
				num++;
			}
		}
		if (num < list.Count)
		{
			list.RemoveRange(num, list.Count - num);
			otherList.RemoveRange(num, otherList.Count - num);
		}
	}
}
