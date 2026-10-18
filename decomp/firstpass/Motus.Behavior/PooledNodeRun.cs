using System.Collections.Generic;
using Brix.Common;
using Brix.Utils;
using Brix.Utils.Closures;
using Brix.Utils.Logging;
using UnityEngine;

namespace Motus.Behavior;

public abstract class PooledNodeRun<T> : NodeRun where T : PooledNodeRun<T>
{
	private static BrixLogger logger = new BrixLogger(typeof(PooledNodeRun<T>).FullName);

	private static Stack<T> pool = new Stack<T>();

	private bool _disposed;

	public static T Request(string label, Object context, IAction finalizer)
	{
		T val = ((pool.Count == 0) ? ((T)typeof(T).MakeInstance()) : pool.Pop());
		val._disposed = false;
		val.State = NodeState.New;
		val.Label = label;
		val.Context = context;
		val.Finalizer = finalizer;
		return val;
	}

	protected abstract void Clear();

	public override void Dispose()
	{
		if (_disposed)
		{
			logger.ErrorFormat("Node was disposed multiple times. Node type was : {0}", GetType());
			return;
		}
		_disposed = true;
		Halt();
		Clear();
		Label = string.Empty;
		Context = null;
		Exception = null;
		Finalizer = Empty.Closure.Delegate;
		pool.Push(this as T);
	}
}
