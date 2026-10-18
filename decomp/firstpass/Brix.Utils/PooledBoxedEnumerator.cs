using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public static class PooledBoxedEnumerator
{
	public static PooledBoxedEnumerator<T, TEnumerator> Request<T, TEnumerator>(TEnumerator e) where TEnumerator : IEnumerator<T>
	{
		return PooledBoxedEnumerator<T, TEnumerator>.Request(e);
	}
}
public static class PooledBoxedEnumerator<T>
{
	public static PooledBoxedEnumerator<T, TEnumerator> Request<TEnumerator>(TEnumerator e) where TEnumerator : IEnumerator<T>
	{
		return PooledBoxedEnumerator<T, TEnumerator>.Request(e);
	}
}
public class PooledBoxedEnumerator<T, TEnumerator> : IEnumerator<T>, IPoolable, IDisposable, IEnumerator, IResetable where TEnumerator : IEnumerator<T>
{
	private TEnumerator _e;

	private bool _disposed;

	object IEnumerator.Current => Current;

	public T Current => _e.Current;

	public static PooledBoxedEnumerator<T, TEnumerator> Request()
	{
		return Request(default(TEnumerator));
	}

	public static PooledBoxedEnumerator<T, TEnumerator> Request(TEnumerator e)
	{
		PooledBoxedEnumerator<T, TEnumerator> pooledBoxedEnumerator = ObjectPool<PooledBoxedEnumerator<T, TEnumerator>>.Reserve();
		pooledBoxedEnumerator._e = e;
		return pooledBoxedEnumerator;
	}

	public bool MoveNext()
	{
		return _e.MoveNext();
	}

	public void Reset()
	{
		_e.Reset();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			ObjectPool<PooledBoxedEnumerator<T, TEnumerator>>.Release(this);
		}
	}

	public void ResetObject()
	{
		_e.Dispose();
		_disposed = false;
	}
}
