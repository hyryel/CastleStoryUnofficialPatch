using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public struct PooledDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator
{
	private Dictionary<TKey, TValue> _pooledDictionary;

	private Dictionary<TKey, TValue>.Enumerator _enumerator;

	public KeyValuePair<TKey, TValue> Current => _enumerator.Current;

	object IEnumerator.Current => Current;

	public PooledDictionaryEnumerator(Dictionary<TKey, TValue> pooledDictionary)
	{
		_pooledDictionary = pooledDictionary;
		_enumerator = _pooledDictionary.GetEnumerator();
	}

	public bool MoveNext()
	{
		return _enumerator.MoveNext();
	}

	public void Reset()
	{
	}

	public void Dispose()
	{
		_pooledDictionary.Dispose();
		_enumerator.Dispose();
	}
}
