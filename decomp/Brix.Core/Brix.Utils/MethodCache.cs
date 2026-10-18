using System;
using System.Collections.Generic;

namespace Brix.Utils;

public class MethodCache
{
	private readonly Dictionary<Delegate, Delegate> _delegates = new Dictionary<Delegate, Delegate>();

	public TResult Invoke<T, TResult>(Func<T, TResult> function, T arg)
	{
		if (!_delegates.TryGetValue(function, out var value))
		{
			value = CacheProvider.Decorate(function);
			_delegates[function] = value;
		}
		return ((Func<T, TResult>)value)(arg);
	}
}
