using System;
using UnityEngine;

public class CompCache<T> where T : class
{
	private GameObject _source;

	private T _cache;

	private bool _tried;

	public T Get
	{
		get
		{
			if (_cache == null && _source != null)
			{
				_cache = (T)(object)_source.GetComponent(typeof(T));
			}
			return _cache;
		}
	}

	public T GetOrAdd
	{
		get
		{
			if (_cache == null && _source != null)
			{
				_cache = (T)(object)_source.GetComponent(typeof(T));
				if (_cache == null)
				{
					if (typeof(T).IsInterface)
					{
						throw new InvalidOperationException("CompCache::GetOrAdd -- Trying to add a component from interface type " + typeof(T).Name);
					}
					_cache = (T)(object)_source.AddComponent(typeof(T));
				}
			}
			else if (_source == null)
			{
				throw new InvalidOperationException("CompCache::GetOrAdd -- Trying to add a component of type " + typeof(T).Name + " on a destroyed object");
			}
			return _cache;
		}
	}

	public T GetOrNull
	{
		get
		{
			if (!_tried && _cache == null && _source != null)
			{
				_cache = (T)(object)_source.GetComponent(typeof(T));
				_tried = true;
			}
			return _cache;
		}
	}

	private CompCache()
	{
	}

	public CompCache(GameObject source)
	{
		_source = source;
		_cache = (T)(object)_source.GetComponent(typeof(T));
	}

	public CompCache(T component)
	{
		_cache = component;
	}
}
