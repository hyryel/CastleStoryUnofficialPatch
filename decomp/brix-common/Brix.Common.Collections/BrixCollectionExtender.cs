using System.Collections.Generic;
using Smooth.Slinq;
using Smooth.Slinq.Collections;

namespace Brix.Common.Collections;

public static class BrixCollectionExtender
{
	public static EnumerableBoxer<List<T>, List<T>.Enumerator, T> NoLeak<T>(this List<T> enumerable)
	{
		return SingleGenericWrapperEnumerable<T>.WrapList(enumerable);
	}

	public static EnumerableBoxer<HashSet<T>, HashSet<T>.Enumerator, T> NoLeak<T>(this HashSet<T> enumerable)
	{
		return SingleGenericWrapperEnumerable<T>.WrapHashSet(enumerable);
	}

	public static EnumerableBoxer<Dictionary<TKey, TValue>, Dictionary<TKey, TValue>.Enumerator, KeyValuePair<TKey, TValue>> NoLeak<TKey, TValue>(this Dictionary<TKey, TValue> enumerable)
	{
		return DualGenericWrapperEnumerable<TKey, TValue>.WrapDictionary(enumerable);
	}

	public static EnumerableBoxer<SingleEnumerationSlinqEnumerable<T, C>, SlinqEnumerator<T, C>, T> NoLeak<T, C>(this Slinq<T, C> slinq)
	{
		return DualGenericWrapperEnumerable<T, C>.WrapSlinq(slinq);
	}
}
