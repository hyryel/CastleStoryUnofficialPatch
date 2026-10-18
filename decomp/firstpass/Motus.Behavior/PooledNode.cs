using System.Collections.Generic;
using Brix.Common;
using Brix.Utils;
using Brix.Utils.Logging;

namespace Motus.Behavior;

public abstract class PooledNode<T> : StaticNode, IPooledNode where T : PooledNode<T>
{
	private static BrixLogger logger = new BrixLogger(typeof(PooledNode<T>).FullName);

	private static Stack<T> pool = new Stack<T>();

	private bool _disposed;

	public static T Request()
	{
		T val = ((pool.Count == 0) ? ((T)typeof(T).MakeInstance()) : pool.Pop());
		val.Label = string.Empty;
		val._disposed = false;
		return val;
	}

	protected abstract void Clear();

	public bool IsDisposed()
	{
		return _disposed;
	}

	public override void Dispose()
	{
		if (_disposed)
		{
			logger.ErrorFormat("Node was disposed multiple times. Node type was : {0}", GetType());
			return;
		}
		_disposed = true;
		Context = null;
		base.Finalizer = Brix.Utils.Empty.Closure;
		Clear();
		pool.Push((T)this);
	}
}
