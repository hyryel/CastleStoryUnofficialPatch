using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.Core;

[Serializable]
public abstract class SerializableDictionary<TKey, TValue> : DrawableDictionary, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ISerializationCallbackReceiver
{
	[NonSerialized]
	protected Dictionary<TKey, TValue> _dictionary;

	[SerializeField]
	protected TKey[] _keys;

	[SerializeField]
	protected TValue[] _values;

	public Dictionary<TKey, TValue> UnderlyingDictionary => _dictionary;

	public int Count => _dictionary.Count;

	public ICollection<TKey> Keys => _dictionary.Keys;

	public ICollection<TValue> Values => _dictionary.Values;

	public TValue this[TKey key]
	{
		get
		{
			return _dictionary[key];
		}
		set
		{
			_dictionary[key] = value;
		}
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> v)
	{
		return v?.UnderlyingDictionary;
	}

	protected SerializableDictionary()
	{
		_dictionary = CollectionPool.RequestDictionary<TKey, TValue>();
	}

	public void Add(TKey key, TValue value)
	{
		_dictionary.Add(key, value);
	}

	public bool ContainsKey(TKey key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		return _dictionary.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public void Clear()
	{
		_dictionary.Clear();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
	}

	public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	public virtual void OnAfterDeserialize()
	{
		if (_keys != null && _values != null)
		{
			if (_dictionary == null)
			{
				_dictionary = new Dictionary<TKey, TValue>(_keys.Length);
			}
			else
			{
				_dictionary.Clear();
			}
			lock (_dictionary)
			{
				int num = Mathf.Min(_keys.Length, _values.Length);
				for (int i = 0; i < num; i++)
				{
					_dictionary[_keys[i]] = _values[i];
				}
			}
		}
		_keys = null;
		_values = null;
	}

	public virtual void OnBeforeSerialize()
	{
		if (_dictionary == null || _dictionary.Count == 0)
		{
			_keys = null;
			_values = null;
			return;
		}
		lock (_dictionary)
		{
			int count = _dictionary.Count;
			_keys = new TKey[count];
			_values = new TValue[count];
			int num = 0;
			Dictionary<TKey, TValue>.Enumerator enumerator = _dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				_keys[num] = enumerator.Current.Key;
				_values[num++] = enumerator.Current.Value;
			}
		}
	}
}
