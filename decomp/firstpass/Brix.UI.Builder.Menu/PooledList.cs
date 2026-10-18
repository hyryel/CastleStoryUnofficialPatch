using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Utils;

namespace Brix.UI.Builder.Menu;

public abstract class PooledList<T> : IList<T>, IDisposable, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private List<T> _internalList = new List<T>();

	public int Count => _internalList.Count;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			return _internalList[index];
		}
		set
		{
			_internalList[index] = value;
		}
	}

	public void Dispose()
	{
		CollectionPool.Release(ref _internalList);
	}

	public List<T>.Enumerator GetEnumerator()
	{
		return _internalList.GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _internalList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _internalList.GetEnumerator();
	}

	public void Add(T item)
	{
		_internalList.Add(item);
	}

	public void AddRange(PooledList<T> items)
	{
		_internalList.AddRange(items._internalList);
		CollectionPool.Release(ref items._internalList);
	}

	public void Clear()
	{
		_internalList.Clear();
	}

	public bool Contains(T item)
	{
		return _internalList.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_internalList.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		return _internalList.Remove(item);
	}

	public int IndexOf(T item)
	{
		return _internalList.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		_internalList.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		_internalList.RemoveAt(index);
	}
}
