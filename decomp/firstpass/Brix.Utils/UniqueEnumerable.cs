using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public struct UniqueEnumerable<T, TEnumerator, P1> : IEnumerable<T>, IEnumerable where TEnumerator : IEnumerator<T>
{
	public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private HashSet<T> keySet;

		private TEnumerator allKeys;

		object IEnumerator.Current => Current;

		public T Current { get; private set; }

		public Enumerator(TEnumerator enumerable)
		{
			this = default(Enumerator);
			keySet = CollectionPool.RequestHashSet<T>();
			allKeys = enumerable;
		}

		public bool MoveNext()
		{
			T current;
			do
			{
				if (!allKeys.MoveNext())
				{
					return false;
				}
				current = allKeys.Current;
			}
			while (keySet.Contains(current));
			keySet.Add(current);
			Current = current;
			return true;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			keySet.Dispose();
			allKeys.Dispose();
		}
	}

	private Func<P1, TEnumerator> getter;

	private P1 p1;

	public UniqueEnumerable(P1 p1, Func<P1, TEnumerator> getter)
	{
		this.getter = getter;
		this.p1 = p1;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(getter(p1));
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
