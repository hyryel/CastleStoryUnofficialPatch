using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public class CachedEnumerable<T> : IEnumerable<T>, IEnumerable, IDisposable
{
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		public CachedEnumerable<T> CachedEnumerable;

		public int CurrentIndex;

		public T Current { get; private set; }

		object IEnumerator.Current => Current;

		public Enumerator(CachedEnumerable<T> cachedEnumerable)
		{
			this = default(Enumerator);
			CachedEnumerable = cachedEnumerable;
		}

		public bool MoveNext()
		{
			Current = CachedEnumerable.GetNthOrDefault(CurrentIndex);
			return CurrentIndex++ < CachedEnumerable.CurrentLength;
		}

		public void Reset()
		{
			CurrentIndex = 0;
		}

		public void Dispose()
		{
			CachedEnumerable = null;
			CurrentIndex = 0;
		}
	}

	private IEnumerator<T> _enumerator;

	private List<T> _cache;

	public int CurrentLength => _cache.Count;

	public int TotalLength
	{
		get
		{
			CacheAllNow();
			return CurrentLength;
		}
	}

	public T this[int i]
	{
		get
		{
			T nthOrDefault = GetNthOrDefault(i);
			if (i < CurrentLength)
			{
				return nthOrDefault;
			}
			throw new IndexOutOfRangeException("i=" + i + ", count=" + CurrentLength);
		}
	}

	public CachedEnumerable(IEnumerable<T> movements)
	{
		_cache = new List<T>();
		_enumerator = movements.GetEnumerator();
	}

	public CachedEnumerable(IEnumerator<T> movements)
	{
		_cache = new List<T>();
		_enumerator = movements;
	}

	public void Reset(IEnumerable<T> movements)
	{
		_cache.Clear();
		_enumerator = movements.GetEnumerator();
	}

	public void Reset(IEnumerator<T> movements)
	{
		_cache.Clear();
		_enumerator = movements;
	}

	public void Dispose()
	{
		_cache.Dispose();
		_cache = null;
		_enumerator = null;
	}

	public void CacheAllNow()
	{
		if (_enumerator != null)
		{
			while (_enumerator.MoveNext())
			{
				_cache.Add(_enumerator.Current);
			}
		}
	}

	public void CacheOne()
	{
		if (_enumerator != null && _enumerator.MoveNext())
		{
			_cache.Add(_enumerator.Current);
		}
	}

	public T GetNthOrDefault(int i, T defaultValue = default(T))
	{
		if (i < 0)
		{
			return defaultValue;
		}
		if (i < _cache.Count)
		{
			return _cache[i];
		}
		if (_enumerator == null)
		{
			return defaultValue;
		}
		do
		{
			if (!_enumerator.MoveNext())
			{
				_enumerator.Dispose();
				_enumerator = null;
				return defaultValue;
			}
			_cache.Add(_enumerator.Current);
		}
		while (_cache.Count <= i);
		return _cache[i];
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
