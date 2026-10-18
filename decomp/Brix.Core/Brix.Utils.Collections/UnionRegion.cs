namespace Brix.Utils.Collections;

public class UnionRegion<T>
{
	public T Value;

	public UnionRegion<T> UnionParent;

	public UnionRegion<T> TopParent
	{
		get
		{
			if (UnionParent != UnionParent.UnionParent)
			{
				UnionParent = UnionParent.TopParent;
			}
			return UnionParent;
		}
	}

	public UnionRegion()
	{
		UnionParent = this;
	}

	public UnionRegion(T value)
	{
		Value = value;
		UnionParent = this;
	}

	public void UnionWith(UnionRegion<T> other)
	{
		TopParent.UnionParent = other.TopParent;
	}
}
