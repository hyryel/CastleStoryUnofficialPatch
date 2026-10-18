using System.Collections.Generic;

namespace Brix.Common.Collections;

public static class SingleGenericWrapperEnumerable<T>
{
	public static EnumerableBoxer<List<T>, List<T>.Enumerator, T> WrapList(List<T> enumerable)
	{
		return new EnumerableBoxer<List<T>, List<T>.Enumerator, T>
		{
			Enumerable = enumerable,
			GetEnumeratorFunc = (List<T> list) => list.GetEnumerator()
		};
	}

	public static EnumerableBoxer<HashSet<T>, HashSet<T>.Enumerator, T> WrapHashSet(HashSet<T> enumerable)
	{
		return new EnumerableBoxer<HashSet<T>, HashSet<T>.Enumerator, T>
		{
			Enumerable = enumerable,
			GetEnumeratorFunc = (HashSet<T> hashSet) => hashSet.GetEnumerator()
		};
	}
}
