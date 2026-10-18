using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class ListExtender
{
	public static int RemoveAll<T, T1>(this List<T> list, Func<T, T1, bool> predicate, T1 p1)
	{
		return ListExtenderUtil<T, T1>.RemoveAll(list, predicate, p1);
	}
}
