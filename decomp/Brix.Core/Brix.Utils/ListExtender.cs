using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class ListExtender
{
	private static class KeyComparer<T>
	{
		public static readonly Comparison<KeyValuePair<float, T>> Comp = (KeyValuePair<float, T> a, KeyValuePair<float, T> b) => a.Key.CompareTo(b.Key);
	}

	public static PooledListEnumerator<T> GetPooledListEnumerator<T>(this List<T> list)
	{
		return new PooledListEnumerator<T>(list);
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

	public static void SortBy<T>(this List<T> list, Func<T, float> scoreFunc)
	{
		List<KeyValuePair<float, T>> list2 = CollectionPool.RequestList<KeyValuePair<float, T>>();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list2.Add(new KeyValuePair<float, T>(scoreFunc(list[i]), list[i]));
		}
		list2.Sort(KeyComparer<T>.Comp);
		for (int j = 0; j < count; j++)
		{
			list[j] = list2[j].Value;
		}
		list2.Dispose();
	}

	public static void SortBy<T, T1>(this List<T> list, Func<T, T1, float> scoreFunc, T1 t1)
	{
		List<KeyValuePair<float, T>> list2 = CollectionPool.RequestList<KeyValuePair<float, T>>();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list2.Add(new KeyValuePair<float, T>(scoreFunc(list[i], t1), list[i]));
		}
		list2.Sort(KeyComparer<T>.Comp);
		for (int j = 0; j < count; j++)
		{
			list[j] = list2[j].Value;
		}
		list2.Dispose();
	}
}
