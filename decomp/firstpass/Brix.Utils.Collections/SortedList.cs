using System;
using UnityEngine;

namespace Brix.Utils.Collections;

public class SortedList<TKey, TValue>
{
	private TKey[] _keys = Empty.Array<TKey>();

	private TValue[] _values = Empty.Array<TValue>();

	private int _count;

	public int Count => _count;

	public ArraySegment<TKey> Keys => new ArraySegment<TKey>(_keys, 0, _count);

	public ArraySegment<TValue> Values => new ArraySegment<TValue>(_values, 0, _count);

	public void Clear()
	{
		Array.Resize(ref _keys, 0);
		Array.Resize(ref _values, 0);
		_count = 0;
	}

	public int IndexOfKey(TKey key)
	{
		return Array.BinarySearch(_keys, 0, _count, key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		int num = Array.BinarySearch(_keys, 0, _count, key);
		if (num < 0)
		{
			value = default(TValue);
			return false;
		}
		value = _values[num];
		return true;
	}

	public TKey GetKeyAtIndex(int index)
	{
		return _keys[index];
	}

	public TValue GetValueAtIndex(int index)
	{
		return _values[index];
	}

	public void RemoveAtIndex(int index)
	{
		_count--;
		if (index < _count)
		{
			Array.Copy(_keys, index + 1, _keys, index, _count);
			Array.Copy(_values, index + 1, _values, index, _count);
		}
		_keys[_count] = default(TKey);
		_values[_count] = default(TValue);
	}

	public void InsertAtIndex(int index, TKey key, TValue value)
	{
		if (_keys.Length <= _count)
		{
			int newSize = Mathf.NextPowerOfTwo(_count + 1);
			Array.Resize(ref _keys, newSize);
			Array.Resize(ref _values, newSize);
		}
		if (index < _count)
		{
			Array.Copy(_keys, index, _keys, index + 1, _count - index);
			Array.Copy(_values, index, _values, index + 1, _count - index);
		}
		_keys[index] = key;
		_values[index] = value;
		_count++;
	}
}
