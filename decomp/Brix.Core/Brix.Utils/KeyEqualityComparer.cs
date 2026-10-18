using System;
using System.Collections.Generic;

namespace Brix.Utils;

public static class KeyEqualityComparer
{
	public static IEqualityComparer<TValue> Make<TValue, TComparable>(Func<TValue, TComparable> getComparable)
	{
		return new KeyEqualityComparer<TValue, TComparable>(getComparable);
	}

	public static IEqualityComparer<TValue> Make<TValue, TComparable>(Func<TValue, TComparable> getComparable, IEqualityComparer<TComparable> comparer)
	{
		return new KeyEqualityComparer<TValue, TComparable>(getComparable, comparer);
	}
}
public class KeyEqualityComparer<TValue, TComparable> : IEqualityComparer<TValue>
{
	protected Func<TValue, TComparable> GetComparable;

	protected IEqualityComparer<TComparable> Comparer;

	public KeyEqualityComparer(Func<TValue, TComparable> getComparable)
	{
		GetComparable = getComparable;
		Comparer = EqualityComparer<TComparable>.Default;
	}

	public KeyEqualityComparer(Func<TValue, TComparable> getComparable, IEqualityComparer<TComparable> comparer)
	{
		GetComparable = getComparable;
		Comparer = comparer;
	}

	public bool Equals(TValue x, TValue y)
	{
		return Comparer.Equals(GetComparable(x), GetComparable(y));
	}

	public int GetHashCode(TValue obj)
	{
		return Comparer.GetHashCode(GetComparable(obj));
	}
}
