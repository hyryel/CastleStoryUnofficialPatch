using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class ArrayExtender
{
	public static T GetValueOrCreate<T>(this IList<T> array, int index, Func<T> factoryMethod)
	{
		T val = array[index];
		if (!EqualityComparer<T>.Default.Equals(val, default(T)))
		{
			val = (array[index] = factoryMethod());
		}
		return val;
	}

	public static T GetValueOrCreate<T>(this IList<T> array, int index, Func<int, T> factoryMethod)
	{
		T val = array[index];
		if (!EqualityComparer<T>.Default.Equals(val, default(T)))
		{
			val = (array[index] = factoryMethod(index));
		}
		return val;
	}

	public static T GetValueOrCreate<T>(this IList<T> array, int index) where T : new()
	{
		T val = array[index];
		if (!EqualityComparer<T>.Default.Equals(val, default(T)))
		{
			val = (array[index] = new T());
		}
		return val;
	}

	public static TCollection GetValueOrCreateThenAdd<TValue, TCollection>(this IList<TCollection> array, int index, TValue value) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = array.GetValueOrCreate(index);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static TCollection GetValueOrCreateThenAdd<TCollection, TValue>(this IList<TCollection> array, int index, TValue value, Func<TCollection> instantiator) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = array.GetValueOrCreate(index, instantiator);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static TCollection GetValueOrCreateThenAdd<TCollection, TValue>(this IList<TCollection> array, int index, TValue value, Func<int, TCollection> instantiator) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = array.GetValueOrCreate(index, instantiator);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static bool GetValueOrDefaultThenRemove<TCollection, TValue>(this IList<TCollection> array, int index, TValue value) where TCollection : class, ICollection<TValue>
	{
		TCollection val = array[index];
		if (val == null)
		{
			return false;
		}
		if (!val.Remove(value))
		{
			return false;
		}
		if (val.Count == 0)
		{
			array[index] = null;
		}
		return true;
	}

	public static bool GetValueOrDefaultThenRemove<TCollection, TValue>(this IList<TCollection> array, int index, TValue value, Action<TCollection> disposer) where TCollection : class, ICollection<TValue>
	{
		TCollection val = array[index];
		if (val == null)
		{
			return false;
		}
		if (!val.Remove(value))
		{
			return false;
		}
		if (val.Count == 0)
		{
			array[index] = null;
			disposer(val);
		}
		return true;
	}

	public static bool GetValueOrDefaultThenRemove<TCollection, TValue>(this IList<TCollection> array, int index, TValue value, Action<int, TCollection> disposer) where TCollection : class, ICollection<TValue>
	{
		TCollection val = array[index];
		if (val == null)
		{
			return false;
		}
		if (!val.Remove(value))
		{
			return false;
		}
		if (val.Count == 0)
		{
			array[index] = null;
			disposer(index, val);
		}
		return true;
	}
}
