using System.Collections.Generic;

namespace System.Linq;

public static class IEnumerableExtender
{
	public static IEnumerable<T> TakeWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool inclusive)
	{
		foreach (T item in source)
		{
			if (predicate(item))
			{
				yield return item;
				continue;
			}
			if (inclusive)
			{
				yield return item;
				continue;
			}
			yield break;
		}
	}

	public static IEnumerable<T> NotNull<T>(this IEnumerable<T> source)
	{
		return source.Where((T item) => item != null && !item.Equals(null));
	}
}
