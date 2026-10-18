using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Common.Collections;

public struct EnumerableBoxer<TEnumerable, TEnumerator, T> : IEnumerable<T>, IEnumerable where TEnumerable : IEnumerable<T> where TEnumerator : IEnumerator<T>
{
	public TEnumerable Enumerable;

	public Func<TEnumerable, TEnumerator> GetEnumeratorFunc;

	public EnumeratorBoxer<TEnumerator, T> GetEnumerator()
	{
		EnumeratorBoxer<TEnumerator, T> enumeratorBoxer = PoolableObject<EnumeratorBoxer<TEnumerator, T>>.Request();
		enumeratorBoxer.Enumerator = GetEnumeratorFunc(Enumerable);
		return enumeratorBoxer;
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
