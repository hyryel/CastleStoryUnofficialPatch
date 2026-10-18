using System.Collections.Generic;
using Smooth.Slinq;
using Smooth.Slinq.Collections;

namespace Brix.Common.Collections;

public static class DualGenericWrapperEnumerable<T, C>
{
	public static EnumerableBoxer<Dictionary<T, C>, Dictionary<T, C>.Enumerator, KeyValuePair<T, C>> WrapDictionary(Dictionary<T, C> enumerable)
	{
		return new EnumerableBoxer<Dictionary<T, C>, Dictionary<T, C>.Enumerator, KeyValuePair<T, C>>
		{
			Enumerable = enumerable,
			GetEnumeratorFunc = (Dictionary<T, C> dictionary) => dictionary.GetEnumerator()
		};
	}

	public static EnumerableBoxer<SingleEnumerationSlinqEnumerable<T, C>, SlinqEnumerator<T, C>, T> WrapSlinq(Slinq<T, C> slinq)
	{
		return new EnumerableBoxer<SingleEnumerationSlinqEnumerable<T, C>, SlinqEnumerator<T, C>, T>
		{
			Enumerable = new SingleEnumerationSlinqEnumerable<T, C>(slinq),
			GetEnumeratorFunc = (SingleEnumerationSlinqEnumerable<T, C> _slinqEnumerable) => _slinqEnumerable.GetEnumerator()
		};
	}
}
