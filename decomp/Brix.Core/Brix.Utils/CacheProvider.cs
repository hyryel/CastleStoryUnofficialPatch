using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class CacheProvider
{
	public static Func<T, TResult> Decorate<T, TResult>(Func<T, TResult> function)
	{
		Dictionary<T, TResult> cache = new Dictionary<T, TResult>();
		return delegate(T x)
		{
			if (!cache.ContainsKey(x))
			{
				cache.Add(x, function(x));
			}
			return cache[x];
		};
	}
}
