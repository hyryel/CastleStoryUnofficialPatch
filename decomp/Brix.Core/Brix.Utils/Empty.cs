using System;
using System.Collections.Generic;
using Brix.Utils.Closures;

namespace Brix.Utils;

public static class Empty
{
	public static IDisposable Disposable => EmptyDisposable.Instance;

	public static IClosure<IAction> Closure => EmptyClosure.Instance;

	public static List<TElem> List<TElem>()
	{
		return Empty<System.Collections.Generic.List<TElem>, TElem>.Request();
	}

	public static HashSet<TElem> HashSet<TElem>()
	{
		return Empty<System.Collections.Generic.HashSet<TElem>, TElem>.Request();
	}

	public static Dictionary<TElem, TElem2> Dictionary<TElem, TElem2>()
	{
		return Empty<System.Collections.Generic.Dictionary<TElem, TElem2>, KeyValuePair<TElem, TElem2>>.Request();
	}

	public static TElem[] Array<TElem>()
	{
		return EmptyArray<TElem>.Request();
	}

	public static T RequestCollection<T, TElem>() where T : ICollection<TElem>, new()
	{
		return InternalPool<T, TElem>.Request();
	}
}
internal static class Empty<T, TElem> where T : ICollection<TElem>, new()
{
	private static readonly T collection = new T();

	public static T Request()
	{
		return collection;
	}
}
