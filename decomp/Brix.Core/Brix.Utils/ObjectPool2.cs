using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class ObjectPool2
{
	public static List<object> AllPools = new List<object>();

	public static List<Action> AllPoolsClear = new List<Action>();

	public static ObjectPool2<T> CreatePool<T>() where T : class, IResetable, new()
	{
		ObjectPool2<T> objectPool = new ObjectPool2<T>();
		AllPools.Add(objectPool);
		AllPoolsClear.Add(objectPool.Clear);
		return objectPool;
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
public class ObjectPool2<T> where T : class, IResetable, new()
{
	private readonly List<T> _objects = new List<T>();

	public T Reserve()
	{
		lock (_objects)
		{
			if (_objects.Count != 0)
			{
				return _objects.Pop();
			}
		}
		return new T();
	}

	public void Release(T value)
	{
		value.ResetObject();
		lock (_objects)
		{
			_objects.Add(value);
		}
	}

	public void Clear()
	{
		lock (_objects)
		{
			_objects.Clear();
		}
	}
}
