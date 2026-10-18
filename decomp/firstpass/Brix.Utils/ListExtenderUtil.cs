using System;
using System.Collections.Generic;

namespace Brix.Utils;

public class ListExtenderUtil<T, T1>
{
	public static int RemoveAll(List<T> list, Func<T, T1, bool> predicate, T1 p1)
	{
		int count = list.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (num != i)
			{
				list[num] = list[i];
			}
			if (!predicate(list[i], p1))
			{
				num++;
			}
		}
		if (num != count)
		{
			list.RemoveRange(num, count - num);
		}
		return count - num;
	}
}
