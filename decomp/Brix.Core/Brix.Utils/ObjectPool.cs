using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public static class ObjectPool
{
	public static List<ICollection> AllPools = new List<ICollection>();

	public static List<Action> AllPoolsClear = new List<Action>();

	public static Queue<T> CreatePool<T>()
	{
		Queue<T> queue = new Queue<T>();
		AllPools.Add(queue);
		AllPoolsClear.Add(queue.Clear);
		return queue;
	}

	public static void ClearAllPools()
	{
		for (int i = 0; i < AllPools.Count; i++)
		{
			lock (AllPools[i])
			{
				Action action = AllPoolsClear[i];
				action();
			}
		}
	}
}
public static class ObjectPool<T> where T : class, IPoolable, new()
{
	private static Queue<T> _queue = ObjectPool.CreatePool<T>();

	public static bool TryReserve(out T value)
	{
		value = null;
		lock (_queue)
		{
			if (_queue.Count != 0)
			{
				value = _queue.Dequeue();
			}
		}
		return value != null;
	}

	public static T Reserve()
	{
		lock (_queue)
		{
			return (_queue.Count != 0) ? _queue.Dequeue() : new T();
		}
	}

	public static void Release(T freedObject)
	{
		freedObject.ResetObject();
		lock (_queue)
		{
			_queue.Enqueue(freedObject);
		}
	}
}
