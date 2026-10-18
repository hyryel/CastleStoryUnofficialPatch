using System;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class LifeSignalConnection : LifeSignalConnectionBase
{
	private IConnectableSignal _signal;

	private Action _action;

	private void Init(IConnectableSignal signal, Action action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeSignalConnection Make(IConnectableSignal signal, Action action)
	{
		LifeSignalConnection lifeSignalConnection = ObjectPool<LifeSignalConnection>.Reserve();
		lifeSignalConnection.Init(signal, action);
		return lifeSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnection>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action).DisconnectWhen(disconnectSignal);
	}
}
public class LifeSignalConnection<T1> : LifeSignalConnectionBase
{
	private IConnectableSignal<T1> _signal;

	private Action<T1> _action;

	private void Init(IConnectableSignal<T1> signal, Action<T1> action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeSignalConnection<T1> Make(IConnectableSignal<T1> signal, Action<T1> action)
	{
		LifeSignalConnection<T1> lifeSignalConnection = ObjectPool<LifeSignalConnection<T1>>.Reserve();
		lifeSignalConnection.Init(signal, action);
		return lifeSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnection<T1>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action).DisconnectWhen(disconnectSignal);
	}
}
public class LifeSignalConnection<T1, T2> : LifeSignalConnectionBase
{
	private IConnectableSignal<T1, T2> _signal;

	private Action<T1, T2> _action;

	private void Init(IConnectableSignal<T1, T2> signal, Action<T1, T2> action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeSignalConnection<T1, T2> Make(IConnectableSignal<T1, T2> signal, Action<T1, T2> action)
	{
		LifeSignalConnection<T1, T2> lifeSignalConnection = ObjectPool<LifeSignalConnection<T1, T2>>.Reserve();
		lifeSignalConnection.Init(signal, action);
		return lifeSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnection<T1, T2>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action).DisconnectWhen(disconnectSignal);
	}
}
public class LifeSignalConnection<T1, T2, T3> : LifeSignalConnectionBase
{
	private IConnectableSignal<T1, T2, T3> _signal;

	private Action<T1, T2, T3> _action;

	private void Init(IConnectableSignal<T1, T2, T3> signal, Action<T1, T2, T3> action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeSignalConnection<T1, T2, T3> Make(IConnectableSignal<T1, T2, T3> signal, Action<T1, T2, T3> action)
	{
		LifeSignalConnection<T1, T2, T3> lifeSignalConnection = ObjectPool<LifeSignalConnection<T1, T2, T3>>.Reserve();
		lifeSignalConnection.Init(signal, action);
		return lifeSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnection<T1, T2, T3>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action).DisconnectWhen(disconnectSignal);
	}
}
