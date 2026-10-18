using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public struct PooledHashSetEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	private HashSet<T> _pooledHashSet;

	private HashSet<T>.Enumerator _enumerator;

	public T Current => _enumerator.Current;

	object IEnumerator.Current => Current;

	public PooledHashSetEnumerator(HashSet<T> pooledHashSet)
	{
		_pooledHashSet = pooledHashSet;
		_enumerator = _pooledHashSet.GetEnumerator();
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
		_pooledHashSet.Dispose();
		_enumerator.Dispose();
	}
}
