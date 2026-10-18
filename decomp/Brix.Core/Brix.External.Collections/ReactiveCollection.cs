using System;
using System.Collections;
using System.Collections.Generic;
using Brix.External.Signals;

namespace Brix.External.Collections;

public class ReactiveCollection<T> : ReactiveCollection<T, ICollection<T>>
{
	public ReactiveCollection(ICollection<T> collection)
		: base(collection)
	{
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _collection.GetEnumerator();
	}
}
public class ReactiveCollection<T, TCollection> : ICollection<T>, IEnumerable<T>, IEnumerable where TCollection : ICollection<T>
{
	public Signal<T> Added = new Signal<T>();

	public Signal<T> Removed = new Signal<T>();

	public Signal BecameEmpty = new Signal();

	public Signal NoLongerEmpty = new Signal();

	protected TCollection _collection;

	public TCollection Collection => _collection;

	public int Count => _collection.Count;

	public bool IsReadOnly => _collection.IsReadOnly;

	public ReactiveCollection(TCollection collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("Reactive Collection requires a non null collection");
		}
		_collection = collection;
		foreach (T item in collection)
		{
			Added.Invoke(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _collection.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _collection.GetEnumerator();
	}

	public virtual void Add(T item)
	{
		_collection.Add(item);
		if (Count == 1)
		{
			NoLongerEmpty.Invoke();
		}
		Added.Invoke(item);
	}

	public virtual void Clear()
	{
		foreach (T item in _collection)
		{
			Removed.Invoke(item);
		}
		_collection.Clear();
		BecameEmpty.Invoke();
	}

	public bool Contains(T item)
	{
		return _collection.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_collection.CopyTo(array, arrayIndex);
	}

	protected virtual void _Remove(T item)
	{
		Remove(item);
	}

	public bool Remove(T item)
	{
		if (_collection.Remove(item))
		{
			Removed.Invoke(item);
			if (Count == 0)
			{
				BecameEmpty.Invoke();
			}
			return true;
		}
		return false;
	}
}
