using System.Collections;
using System.Collections.Generic;
using Smooth.Slinq;
using Smooth.Slinq.Collections;

namespace Brix.Common.Collections;

public struct SingleEnumerationSlinqEnumerable<T, TCommand> : IEnumerable<T>, IEnumerable
{
	private Slinq<T, TCommand> _slinq;

	public SingleEnumerationSlinqEnumerable(Slinq<T, TCommand> slinq)
	{
		_slinq = slinq;
	}

	public Slinq<T, TCommand> Slinq()
	{
		return _slinq;
	}

	public SlinqEnumerator<T, TCommand> GetEnumerator()
	{
		return Slinq().GetEnumerator();
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
