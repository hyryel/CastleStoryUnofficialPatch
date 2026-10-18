using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class DictionaryExtender
{
	public static PooledDictionaryEnumerator<TKey, TValue> GetPooledDictionaryEnumerator<TKey, TValue>(this Dictionary<TKey, TValue> dict)
	{
		return new PooledDictionaryEnumerator<TKey, TValue>(dict);
	}

	public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dico, Func<KeyValuePair<TKey, TValue>, bool> predicate)
	{
		List<TKey> list = CollectionPool.RequestList<TKey>();
		foreach (KeyValuePair<TKey, TValue> item in dico)
		{
			if (predicate(item))
			{
				list.Add(item.Key);
			}
		}
		foreach (TKey item2 in list)
		{
			dico.Remove(item2);
		}
		list.Dispose();
	}

	public static void RemoveAll<TKey, TValue, TParam>(this Dictionary<TKey, TValue> dico, Func<KeyValuePair<TKey, TValue>, TParam, bool> predicate, TParam p)
	{
		List<TKey> list = CollectionPool.RequestList<TKey>();
		foreach (KeyValuePair<TKey, TValue> item in dico)
		{
			if (predicate(item, p))
			{
				list.Add(item.Key);
			}
		}
		foreach (TKey item2 in list)
		{
			dico.Remove(item2);
		}
		list.Dispose();
	}

	public static KeyValuePair<TKey, TValue> GetAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> dico, TKey key)
	{
		if (dico.TryGetValue(key, out var value))
		{
			dico.Remove(key);
		}
		return new KeyValuePair<TKey, TValue>(key, value);
	}

	public static TValue GetValueOrDefaultAndRemove<TKey, TValue>(this IDictionary<TKey, TValue> dico, TKey key, TValue defaultValue)
	{
		if (dico.TryGetValue(key, out var value))
		{
			dico.Remove(key);
			return value;
		}
		return defaultValue;
	}

	public static TValue GetValueOrDefaultThenUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dico, TKey key, TValue newValue, TValue defaultValue = default(TValue))
	{
		if (dico.TryGetValue(key, out var value))
		{
			dico[key] = newValue;
			return value;
		}
		dico.Add(key, newValue);
		return defaultValue;
	}

	public static KeyValuePair<TKey, TValue> AddAndGet<TKey, TValue, TDicoKey, TDicoValue>(this IDictionary<TDicoKey, TDicoValue> dico, TKey key, TValue value) where TKey : TDicoKey where TValue : TDicoValue
	{
		dico.Add((TDicoKey)(object)key, (TDicoValue)(object)value);
		return new KeyValuePair<TKey, TValue>(key, value);
	}

	public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return default(TValue);
	}

	public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = new TValue();
			dictionary.Add(key, value);
		}
		return value;
	}

	public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (!dictionary.TryGetValue(key, out var value2))
		{
			value2 = value;
			dictionary.Add(key, value2);
		}
		return value2;
	}

	public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> instantiator)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = instantiator();
			dictionary.Add(key, value);
		}
		return value;
	}

	public static TValue GetValueOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> instantiator)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = instantiator(key);
			dictionary.Add(key, value);
		}
		return value;
	}

	public static TValue GetValueOrCreate<TKey, TValue, TContext>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TContext, TValue> instantiator, TContext context)
	{
		if (!dictionary.TryGetValue(key, out var value))
		{
			value = instantiator(key, context);
			dictionary.Add(key, value);
		}
		return value;
	}

	public static TCollection GetValueOrCreateThenAdd<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = dico.GetValueOrCreate(key);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static TCollection GetValueOrCreateThenAdd<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value, Func<TCollection> instantiator) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = dico.GetValueOrCreate(key, instantiator);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static TCollection GetValueOrCreateThenAdd<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value, Func<TKey, TCollection> instantiator) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = dico.GetValueOrCreate(key, instantiator);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static TCollection GetValueOrCreateThenAdd<TKey, TCollection, TValue, TContext>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value, Func<TKey, TContext, TCollection> instantiator, TContext context) where TCollection : ICollection<TValue>, new()
	{
		TCollection valueOrCreate = dico.GetValueOrCreate(key, instantiator, context);
		valueOrCreate.Add(value);
		return valueOrCreate;
	}

	public static bool GetValueOrDefaultThenRemove<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value) where TCollection : class, ICollection<TValue>
	{
		TCollection valueOrDefault = dico.GetValueOrDefault(key);
		if (valueOrDefault == null)
		{
			return false;
		}
		if (!valueOrDefault.Remove(value))
		{
			return false;
		}
		if (valueOrDefault.Count == 0)
		{
			dico.Remove(key);
		}
		return true;
	}

	public static bool GetValueOrDefaultThenRemove<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value, Action<TCollection> disposer) where TCollection : class, ICollection<TValue>
	{
		TCollection valueOrDefault = dico.GetValueOrDefault(key);
		if (valueOrDefault == null)
		{
			return false;
		}
		if (!valueOrDefault.Remove(value))
		{
			return false;
		}
		if (valueOrDefault.Count == 0)
		{
			dico.Remove(key);
			disposer(valueOrDefault);
		}
		return true;
	}

	public static bool GetValueOrDefaultThenRemove<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dico, TKey key, TValue value, Action<TKey, TCollection> disposer) where TCollection : class, ICollection<TValue>
	{
		TCollection valueOrDefault = dico.GetValueOrDefault(key);
		if (valueOrDefault == null)
		{
			return false;
		}
		if (!valueOrDefault.Remove(value))
		{
			return false;
		}
		if (valueOrDefault.Count == 0)
		{
			dico.Remove(key);
			disposer(key, valueOrDefault);
		}
		return true;
	}
}
