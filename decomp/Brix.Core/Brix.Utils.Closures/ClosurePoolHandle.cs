using System;

namespace Brix.Utils.Closures;

public sealed class ClosurePoolHandle<TPooled, TClosed, TDelegate> : IResetable, IClosure<TDelegate>, IDisposable where TPooled : Pooling.PooledBase<TClosed>, TDelegate, new() where TClosed : struct
{
	private static readonly ObjectPool2<ClosurePoolHandle<TPooled, TClosed, TDelegate>> Handles = new ObjectPool2<ClosurePoolHandle<TPooled, TClosed, TDelegate>>();

	private static readonly ObjectPool2<TPooled> Pool = new ObjectPool2<TPooled>();

	private TPooled _value;

	TDelegate IClosure<TDelegate>.Delegate => (TDelegate)_value;

	public TPooled Delegate => _value;

	public TPooled Value => _value;

	public bool IsAlive => _value != null;

	public static ClosurePoolHandle<TPooled, TClosed, TDelegate> Request()
	{
		ClosurePoolHandle<TPooled, TClosed, TDelegate> closurePoolHandle = Handles.Reserve();
		closurePoolHandle._value = Pool.Reserve();
		return closurePoolHandle;
	}

	public void Dispose()
	{
		if (IsAlive)
		{
			Pool.Release(_value);
			Handles.Release(this);
		}
	}

	public void ResetObject()
	{
		_value = null;
	}
}
