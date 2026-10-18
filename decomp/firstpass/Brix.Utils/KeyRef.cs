using System;

namespace Brix.Utils;

public class KeyRef<T> : IEquatable<KeyRef<T>>
{
	public T referenceKey;

	public KeyRef(T t)
	{
		referenceKey = t;
	}

	public override int GetHashCode()
	{
		return referenceKey.GetHashCode();
	}

	public bool Equals(KeyRef<T> other)
	{
		ref T reference = ref referenceKey;
		object obj = other.referenceKey;
		return reference.Equals(obj);
	}

	public override bool Equals(object obj)
	{
		return referenceKey.Equals(obj);
	}

	public override string ToString()
	{
		return referenceKey.ToString();
	}
}
