using System.Collections;
using System.Collections.Generic;

namespace Brix.Util;

public struct EnumeratorWrapper<T, TEnum> : IEnumerable<T>, IEnumerable where TEnum : IEnumerator<T>
{
	private TEnum _enumerator;

	public EnumeratorWrapper(TEnum enumerator)
	{
		_enumerator = enumerator;
	}

	public TEnum GetEnumerator()
	{
		return _enumerator;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return _enumerator;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _enumerator;
	}
}
