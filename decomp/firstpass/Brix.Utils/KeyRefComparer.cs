using System.Collections.Generic;

namespace Brix.Utils;

public class KeyRefComparer<T> : IEqualityComparer<KeyRef<T>>
{
	public bool Equals(KeyRef<T> x, KeyRef<T> y)
	{
		ref T referenceKey = ref x.referenceKey;
		object obj = y.referenceKey;
		return referenceKey.Equals(obj);
	}

	public int GetHashCode(KeyRef<T> obj)
	{
		return obj.referenceKey.GetHashCode();
	}
}
