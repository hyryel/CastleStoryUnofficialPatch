using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public static class CollectionPool
{
	public static readonly List<ICollection> AllCollections = new List<ICollection>();

	public static readonly List<Action> AllCollectionsClear = new List<Action>();

	internal static Stack<T> CreateCollection<T>()
	{
		Stack<T> stack = new Stack<T>();
		AllCollections.Add(stack);
		AllCollectionsClear.Add(stack.Clear);
		return stack;
	}

	public static void ClearAllCollections()
	{
		for (int i = 0; i < AllCollectionsClear.Count; i++)
		{
			lock (AllCollections[i])
			{
				Action action = AllCollectionsClear[i];
				action();
			}
		}
	}

	public static void Request<TElem>(out List<TElem> list)
	{
		list = InternalPool<List<TElem>, TElem>.Request();
	}

	public static void Request<TElem>(out HashSet<TElem> hashset)
	{
		hashset = InternalPool<HashSet<TElem>, TElem>.Request();
	}

	public static void Request<TElem, TElem2>(out Dictionary<TElem, TElem2> dictionary)
	{
		dictionary = InternalPool<Dictionary<TElem, TElem2>, KeyValuePair<TElem, TElem2>>.Request();
	}

	public static void Request<T, TElem>(out T collection) where T : ICollection<TElem>, new()
	{
		collection = InternalPool<T, TElem>.Request();
	}

	public static void Request<TElem>(out List<TElem> list, Func<List<TElem>> factoryMethod)
	{
		list = InternalPool<List<TElem>, TElem>.Request(factoryMethod);
	}

	public static void Request<TElem>(out HashSet<TElem> hashset, Func<HashSet<TElem>> factoryMethod)
	{
		hashset = InternalPool<HashSet<TElem>, TElem>.Request(factoryMethod);
	}

	public static void Request<TElem, TElem2>(out Dictionary<TElem, TElem2> dictionary, Func<Dictionary<TElem, TElem2>> factoryMethod)
	{
		dictionary = InternalPool<Dictionary<TElem, TElem2>, KeyValuePair<TElem, TElem2>>.Request(factoryMethod);
	}

	public static void Request<T, TElem>(out T collection, Func<T> factoryMethod) where T : ICollection<TElem>, new()
	{
		collection = InternalPool<T, TElem>.Request(factoryMethod);
	}

	public static List<TElem> RequestList<TElem>()
	{
		return InternalPool<List<TElem>, TElem>.Request();
	}

	public static HashSet<TElem> RequestHashSet<TElem>()
	{
		return InternalPool<HashSet<TElem>, TElem>.Request();
	}

	public static Dictionary<TElem, TElem2> RequestDictionary<TElem, TElem2>()
	{
		return InternalPool<Dictionary<TElem, TElem2>, KeyValuePair<TElem, TElem2>>.Request();
	}

	public static T RequestCollection<T, TElem>() where T : ICollection<TElem>, new()
	{
		return InternalPool<T, TElem>.Request();
	}

	public static List<TElem> RequestList<TElem>(Func<List<TElem>> factoryMethod)
	{
		return InternalPool<List<TElem>, TElem>.Request(factoryMethod);
	}

	public static HashSet<TElem> RequestHashSet<TElem>(Func<HashSet<TElem>> factoryMethod)
	{
		return InternalPool<HashSet<TElem>, TElem>.Request(factoryMethod);
	}

	public static Dictionary<TElem, TElem2> RequestDictionary<TElem, TElem2>(Func<Dictionary<TElem, TElem2>> factoryMethod)
	{
		return InternalPool<Dictionary<TElem, TElem2>, KeyValuePair<TElem, TElem2>>.Request(factoryMethod);
	}

	public static T RequestCollection<T, TElem>(Func<T> factoryMethod) where T : ICollection<TElem>, new()
	{
		return InternalPool<T, TElem>.Request(factoryMethod);
	}

	public static void Release<TElem>(ref List<TElem> list)
	{
		InternalPool<List<TElem>, TElem>.Release(list);
		list = null;
	}

	public static void Release<TElem>(ref HashSet<TElem> hashset)
	{
		InternalPool<HashSet<TElem>, TElem>.Release(hashset);
		hashset = null;
	}

	public static void Release<TKey, TElem>(ref Dictionary<TKey, TElem> dictionary)
	{
		InternalPool<Dictionary<TKey, TElem>, KeyValuePair<TKey, TElem>>.Release(dictionary);
		dictionary = null;
	}

	public static void Release<T, TElem>(ref T collection) where T : ICollection<TElem>, new()
	{
		InternalPool<T, TElem>.Release(collection);
		collection = default(T);
	}

	public static void Dispose<TElem>(this List<TElem> list)
	{
		InternalPool<List<TElem>, TElem>.Release(list);
	}

	public static void Dispose<TElem>(this HashSet<TElem> hashset)
	{
		InternalPool<HashSet<TElem>, TElem>.Release(hashset);
	}

	public static void Dispose<TKey, TElem>(this Dictionary<TKey, TElem> dictionary)
	{
		InternalPool<Dictionary<TKey, TElem>, KeyValuePair<TKey, TElem>>.Release(dictionary);
	}

	public static void Dispose<T, TElem>(this T collection) where T : ICollection<TElem>, new()
	{
		Release<T, TElem>(ref collection);
	}
}
