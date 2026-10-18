using System;
using System.Collections.Generic;

namespace Brix.Utils;

internal static class InternalPool<T, TElem> where T : ICollection<TElem>, new()
{
	private static Stack<T> collections = CollectionPool.CreateCollection<T>();

	public static T Request()
	{
		lock (collections)
		{
			if (collections.Count == 0)
			{
				return new T();
			}
			return collections.Pop();
		}
	}

	public static T Request(Func<T> factoryMethod)
	{
		lock (collections)
		{
			if (collections.Count != 0)
			{
				return collections.Pop();
			}
		}
		return factoryMethod();
	}

	public static void Release(T t)
	{
		t.Clear();
		lock (collections)
		{
			collections.Push(t);
		}
	}
}
