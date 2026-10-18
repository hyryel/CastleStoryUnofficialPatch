using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Common.Collections;

public class EnumeratorBoxer<TEnumerator, T> : PoolableObject<EnumeratorBoxer<TEnumerator, T>>, IEnumerator<T>, IEnumerator, IDisposable where TEnumerator : IEnumerator<T>
{
	private TEnumerator _enum;

	public TEnumerator Enumerator
	{
		get
		{
			return _enum;
		}
		set
		{
			_enum = value;
		}
	}

	public T Current => _enum.Current;

	object IEnumerator.Current => _enum.Current;

	public bool MoveNext()
	{
		return _enum.MoveNext();
	}

	public void Reset()
	{
	}

	protected override void ResetObject()
	{
		Enumerator.Dispose();
		_enum = default(TEnumerator);
	}

	public void Dispose()
	{
		ReleaseToPool();
	}
}
