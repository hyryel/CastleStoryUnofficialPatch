using System;
using Brix.Common;
using Brix.External.Signals;
using Brix.Utils;

namespace Motus.Behavior;

public abstract class AbstractSignalCondition<TSignalCondition> : PoolableObject<TSignalCondition>, ICondition, IDisposable where TSignalCondition : AbstractSignalCondition<TSignalCondition>
{
	protected SignalConnection _connection = SignalConnection.Empty;

	protected bool _invokedSuccessfully;

	protected IDisposable _filterHandle;

	public void Init()
	{
		_invokedSuccessfully = false;
		_connection.Disconnect();
		_connection = ConnectToSignal();
	}

	public bool Test()
	{
		bool invokedSuccessfully = _invokedSuccessfully;
		_invokedSuccessfully = false;
		return invokedSuccessfully;
	}

	public void Dispose()
	{
		ReleaseToPool();
	}

	protected abstract SignalConnection ConnectToSignal();

	protected override void ResetObject()
	{
		_connection.Disconnect();
		_filterHandle.Dispose();
		_filterHandle = Empty.Disposable;
		_invokedSuccessfully = false;
	}
}
