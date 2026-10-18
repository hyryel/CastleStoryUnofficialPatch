using System.Collections.Generic;

namespace Brix.Common;

public abstract class PoolableObject<T> where T : PoolableObject<T>
{
	private static Stack<T> pool = new Stack<T>();

	public static T Request()
	{
		if (pool.Count == 0)
		{
			return (T)typeof(T).MakeInstance();
		}
		return pool.Pop();
	}

	protected abstract void ResetObject();

	public void ReleaseToPool()
	{
		ResetObject();
		pool.Push((T)this);
	}
}
