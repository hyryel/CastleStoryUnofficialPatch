using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils;

public class TypecastEnumerator<TIn, TOut> : IEnumerator<TOut>, IDisposable, IEnumerator where TOut : class
{
	private readonly IEnumerator<TIn> _enumerator;

	public TOut Current => _enumerator.Current as TOut;

	object IEnumerator.Current => Current;

	public TypecastEnumerator(IEnumerator<TIn> e)
	{
		_enumerator = e;
	}

	public bool MoveNext()
	{
		while (_enumerator.MoveNext())
		{
			if (_enumerator.Current is TOut)
			{
				return true;
			}
		}
		return false;
	}

	public void Reset()
	{
		_enumerator.Reset();
	}

	public void Dispose()
	{
		_enumerator.Dispose();
	}
}
