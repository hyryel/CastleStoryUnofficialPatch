using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Common;

namespace Brix.Utils;

public static class EnumeratorTypecastExtender
{
	public class TypecastEnumerator<TOut> : PoolableObject<TypecastEnumerator<TOut>>, IEnumerator<TOut>, IDisposable, IEnumerator where TOut : class
	{
		private IEnumerator _enumerator;

		object IEnumerator.Current => Current;

		public TOut Current => _enumerator.Current as TOut;

		public static TypecastEnumerator<TOut> Request(IEnumerator e)
		{
			TypecastEnumerator<TOut> typecastEnumerator = PoolableObject<TypecastEnumerator<TOut>>.Request();
			typecastEnumerator._enumerator = e;
			return typecastEnumerator;
		}

		public bool MoveNext()
		{
			while (_enumerator.MoveNext())
			{
				if (_enumerator.Current == null || _enumerator.Current is TOut)
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
			ReleaseToPool();
		}

		protected override void ResetObject()
		{
			if (_enumerator is IDisposable disposable)
			{
				disposable.Dispose();
			}
			_enumerator = null;
		}
	}

	public static TypecastEnumerator<TOut> OfTypeOrNull<TOut>(this IEnumerator e) where TOut : class
	{
		return TypecastEnumerator<TOut>.Request(e);
	}
}
