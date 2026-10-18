using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Brix.Network.Util;

public class NetworkArray<T> : IList, ICollection, IEnumerable
{
	public delegate void NetworkHandler(object data);

	public NetworkHandler Pushed;

	public NetworkHandler Cleared;

	public NetworkHandler Popped;

	public NetworkHandler Trimmed;

	public NetworkHandler Peeked;

	public NetworkHandler Concatenated;

	public T Invalid;

	private object _syncRoot;

	protected List<T> _innerArray;

	public virtual object this[int index]
	{
		get
		{
			return _innerArray[index];
		}
		set
		{
			_innerArray[index] = (T)value;
		}
	}

	public virtual int length => _innerArray.Count;

	public virtual int Count => _innerArray.Count;

	public virtual bool IsSynchronized => false;

	public virtual bool IsReadOnly => false;

	public virtual bool IsFixedSize => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null)
			{
				Interlocked.CompareExchange(ref _syncRoot, new object(), null);
			}
			return _syncRoot;
		}
	}

	public NetworkArray()
	{
		_innerArray = new List<T>();
	}

	public virtual void Push(T value)
	{
		_innerArray.Add(value);
		if (Pushed != null)
		{
			Pushed(value);
		}
	}

	public virtual int Add(object value)
	{
		_innerArray.Add((T)value);
		if (Pushed != null)
		{
			Pushed((T)value);
		}
		return _innerArray.Count - 1;
	}

	public virtual void RemoveAt(int index)
	{
		_innerArray.RemoveAt(index);
	}

	public virtual void Clear()
	{
		_innerArray.Clear();
		if (Cleared != null)
		{
			Cleared(null);
		}
	}

	public virtual IEnumerator GetEnumerator()
	{
		return _innerArray.GetEnumerator();
	}

	public virtual void CopyTo(Array array, int index)
	{
		_innerArray.ToArray().CopyTo(array, index);
	}

	public virtual T Check()
	{
		if (_innerArray.Count > 0)
		{
			return _innerArray[_innerArray.Count - 1];
		}
		return Invalid;
	}

	public virtual T[] Get(int length)
	{
		T[] array = new T[length];
		int num = Mathf.Min(_innerArray.Count, length);
		for (int i = 0; i < num; i++)
		{
			array[i] = _innerArray[_innerArray.Count - 1 - i];
		}
		if (Peeked != null)
		{
			Peeked(array);
		}
		return array;
	}

	public virtual T Pop()
	{
		if (_innerArray.Count == 0)
		{
			return Invalid;
		}
		T val = _innerArray[_innerArray.Count - 1];
		_innerArray.RemoveAt(_innerArray.Count - 1);
		if (Popped != null)
		{
			Popped(val);
		}
		return val;
	}

	public virtual void Trim(int length)
	{
		int num = Mathf.Min(_innerArray.Count, length);
		_innerArray = _innerArray.GetRange(_innerArray.Count - num, num);
		if (Trimmed != null)
		{
			Trimmed(length);
		}
	}

	public virtual void Concat(ICollection array)
	{
		foreach (object item2 in array)
		{
			T item = (T)item2;
			_innerArray.Add(item);
		}
		if (Concatenated != null)
		{
			Concatenated(array);
		}
	}

	public virtual bool Contains(object value)
	{
		return _innerArray.Contains((T)value);
	}

	public virtual int IndexOf(object value)
	{
		return _innerArray.IndexOf((T)value);
	}

	public virtual void Insert(int index, object value)
	{
		_innerArray.Insert(index, (T)value);
	}

	public virtual void Remove(object value)
	{
		_innerArray.Remove((T)value);
	}
}
