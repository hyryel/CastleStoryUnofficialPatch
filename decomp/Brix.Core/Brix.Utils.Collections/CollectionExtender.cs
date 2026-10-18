using System;
using System.Collections.Generic;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Utils.Collections;

public static class CollectionExtender
{
	private static class NotUnityNullWrapper<T, C>
	{
		public static Slinq<T, PredicateContext<T, C>> NotUnityNull(Slinq<T, C> source)
		{
			return source.Where((T item) => item != null && !item.Equals(null));
		}
	}

	public static Slinq<T, PredicateContext<T, C>> NotUnityNull<T, C>(this Slinq<T, C> source)
	{
		return NotUnityNullWrapper<T, C>.NotUnityNull(source);
	}

	public static List<T> ToPooledList<T, C>(this Slinq<T, C> slinq)
	{
		CollectionPool.Request<T>(out List<T> list);
		return slinq.AddTo(list);
	}

	public static List<T> ToPooledList<T>(this ICollection<T> oldCollection)
	{
		CollectionPool.Request<T>(out List<T> list);
		return oldCollection.Slinq().AddTo(list);
	}

	public static HashSet<T> ToPooledHashSet<T>(this ICollection<T> oldCollection)
	{
		CollectionPool.Request<T>(out HashSet<T> hashset);
		return oldCollection.Slinq().AddTo(hashset);
	}

	public static HashSet<T> ToPooledHashSet<T, C>(this Slinq<T, C> slinq)
	{
		CollectionPool.Request<T>(out HashSet<T> hashset);
		return slinq.AddTo(hashset);
	}

	public static HashSet<T> ToHashSet<T, C>(this Slinq<T, C> slinq)
	{
		return slinq.AddTo(new HashSet<T>());
	}

	public static Dictionary<TKey, List<TValue>> ToPooledLookup<TKey, TValue>(this IList<TValue> values, Func<TValue, TKey> keyFunc)
	{
		Dictionary<TKey, List<TValue>> dictionary = CollectionPool.RequestDictionary<TKey, List<TValue>>();
		int count = values.Count;
		for (int i = 0; i < count; i++)
		{
			TValue val = values[i];
			TKey key = keyFunc(val);
			if (!dictionary.TryGetValue(key, out var value))
			{
				dictionary.Add(key, value = CollectionPool.RequestList<TValue>());
			}
			value.Add(val);
		}
		return dictionary;
	}

	public static Dictionary<TKey, List<TValue>> ToPooledLookup<TKey, TValue, TSlinq>(this Slinq<TValue, TSlinq> values, Func<TValue, TKey> keyFunc)
	{
		Dictionary<TKey, List<TValue>> dictionary = CollectionPool.RequestDictionary<TKey, List<TValue>>();
		while (values.current.isSome)
		{
			TValue value = values.current.value;
			TKey key = keyFunc(value);
			if (!dictionary.TryGetValue(key, out var value2))
			{
				dictionary.Add(key, value2 = CollectionPool.RequestList<TValue>());
			}
			value2.Add(value);
			values.Skip();
		}
		return dictionary;
	}
}
