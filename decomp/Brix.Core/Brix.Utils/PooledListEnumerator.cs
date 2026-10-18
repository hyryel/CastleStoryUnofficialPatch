using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public struct PooledListEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
{
	private List<T> _pooledList;

	private List<T>.Enumerator _enumerator;

	public T Current => _enumerator.Current;

	object IEnumerator.Current => Current;

	public PooledListEnumerator(List<T> pooledList)
	{
		_pooledList = pooledList;
		_enumerator = _pooledList.GetEnumerator();
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
		_pooledList.Dispose();
		_enumerator.Dispose();
	}
}
