using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;

namespace Brix.Utils.Collections;

public class LayeredCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : GameComponent
{
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private SortedList<int, List<T>> _collection;

		private int _iCollection;

		private List<T>.Enumerator _layerEnumerator;

		object IEnumerator.Current => Current;

		public T Current => _layerEnumerator.Current;

		public Enumerator(LayeredCollection<T> c)
		{
			_collection = c._collection;
			_iCollection = -1;
			_layerEnumerator = Empty.List<T>().GetEnumerator();
		}

		public bool MoveNext()
		{
			while (!_layerEnumerator.MoveNext())
			{
				_layerEnumerator.Dispose();
				_iCollection++;
				if (_iCollection >= _collection.Count)
				{
					_layerEnumerator = Empty.List<T>().GetEnumerator();
					return false;
				}
				_layerEnumerator = _collection.GetValueAtIndex(_iCollection).GetEnumerator();
			}
			return true;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			_layerEnumerator.Dispose();
		}
	}

	private SortedList<int, List<T>> _collection = new SortedList<int, List<T>>();

	private int count;

	public int Count => count;

	public bool IsReadOnly => false;

	public LayeredCollection(ICollection<T> collection)
	{
		collection.Slinq().ForEach(delegate(T o, LayeredCollection<T> self)
		{
			self.Add(o);
		}, this);
	}

	public LayeredCollection()
	{
	}

	public Slinq<T, TemplatedEnumeratorContext<T, Enumerator>> Slinq()
	{
		return TemplatedEnumeratorContext<T, Enumerator>.Slinq(new Enumerator(this));
	}

	public List<T> MinLayer()
	{
		return (_collection.Count <= 0) ? Empty.List<T>() : _collection.GetValueAtIndex(0);
	}

	public void Clear()
	{
		_collection.Clear();
		count = 0;
	}

	public bool Contains(T item)
	{
		int key = Y(item);
		if (!_collection.TryGetValue(key, out var value))
		{
			return false;
		}
		return value.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public bool Remove(T item)
	{
		int key = Y(item);
		int num = _collection.IndexOfKey(key);
		if (num < 0)
		{
			return false;
		}
		List<T> list = _collection.GetValueAtIndex(num);
		if (!list.Remove(item))
		{
			return false;
		}
		count--;
		if (list.Count == 0)
		{
			CollectionPool.Release(ref list);
			_collection.RemoveAtIndex(num);
		}
		return true;
	}

	public void Add(T item)
	{
		int key = Y(item);
		int num = _collection.IndexOfKey(key);
		List<T> list;
		if (num < 0)
		{
			list = CollectionPool.RequestList<T>();
			_collection.InsertAtIndex(~num, key, list);
		}
		else
		{
			list = _collection.GetValueAtIndex(num);
		}
		if (!list.Contains(item))
		{
			list.Add(item);
			count++;
		}
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

	private int Y(T item)
	{
		return (int)item.transform.position.y;
	}
}
