using System;

namespace Brix.Utils.Closures;

public static class Pooling
{
	public interface IPooledBase : IPoolable, IResetable, IDisposable
	{
	}

	public class PooledBase<TClosed> : IPooledBase, IPoolable, IResetable, IDisposable where TClosed : struct
	{
		public TClosed Closed;

		void IResetable.ResetObject()
		{
			Closed = default(TClosed);
		}

		void IDisposable.Dispose()
		{
			this.Release();
		}
	}

	public static TPooled Request<TPooled, TClosed>(this TPooled pooled, TClosed closed) where TPooled : PooledBase<TClosed>, new() where TClosed : struct
	{
		TPooled val = ObjectPool<TPooled>.Reserve();
		val.Closed = closed;
		return val;
	}

	public static void Release<TPooled>(this TPooled pooled) where TPooled : class, IPooledBase, new()
	{
		ObjectPool<TPooled>.Release(pooled);
	}

	public static ClosurePoolHandle<TPooled, TClosed, TAction> Request2<TPooled, TClosed, TAction>(this TPooled pooled, TAction action, TClosed closed) where TPooled : PooledBase<TClosed>, TAction, new() where TClosed : struct
	{
		ClosurePoolHandle<TPooled, TClosed, TAction> closurePoolHandle = ClosurePoolHandle<TPooled, TClosed, TAction>.Request();
		closurePoolHandle.Value.Closed = closed;
		return closurePoolHandle;
	}
}
