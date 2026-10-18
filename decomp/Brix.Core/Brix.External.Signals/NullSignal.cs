using System;

namespace Brix.External.Signals;

public class NullSignal : ISignal, IReadOnlySignal, IReadonlySignalBase<Action>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action>, IConnectableOnceSignalBase<Action>, IDisconnectableSignal, IConnectableOnceSignal, IConnectableSignal, IInvokableSignal
{
	private static readonly NullSignal instance = new NullSignal();

	private static readonly SignalConnection emptyConnection = default(SignalConnection);

	public static NullSignal Get => instance;

	public int InvokedCount => 0;

	public int ConnectionCount => 0;

	public int MaximumConnections
	{
		set
		{
		}
	}

	public void Invoke()
	{
	}

	public SignalConnection Connect(Action action)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1>(Action<T1> action, T1 t1)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce(Action action)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1>(Action<T1> action, T1 t1)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return emptyConnection;
	}

	public void Clear()
	{
	}

	public bool IsConnected(SignalConnection connection)
	{
		return false;
	}

	public bool Disconnect(SignalConnection connection)
	{
		return false;
	}

	public Action Wrap(Action action, Action<Action> wrapper)
	{
		return action;
	}
}
public class NullSignal<T> : ISignal<T>, IReadOnlySignal<T>, IReadonlySignalBase<Action<T>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T>>, IConnectableOnceSignalBase<Action<T>>, IDisconnectableSignal, IConnectableOnceSignal<T>, IConnectableSignal<T>, IInvokableSignal<T>
{
	private readonly SignalConnection emptyConnection = default(SignalConnection);

	public int InvokedCount => 0;

	public int ConnectionCount => 0;

	public int MaximumConnections
	{
		set
		{
		}
	}

	public void Clear()
	{
	}

	public void Invoke(T t1)
	{
	}

	public SignalConnection Connect(Action<T> action)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1>(Action<T, T1> action, T1 t1)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1, T2>(Action<T, T1, T2> action, T1 t1, T2 t2)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T1, T2, T3>(Action<T, T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> action, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce(Action<T> action)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1>(Action<T, T1> action, T1 t1)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1, T2>(Action<T, T1, T2> action, T1 t1, T2 t2)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T1, T2, T3>(Action<T, T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T2, T3, T4, T5>(Action<T, T2, T3, T4, T5> action, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return emptyConnection;
	}

	public bool IsConnected(SignalConnection connection)
	{
		return false;
	}

	public bool Disconnect(SignalConnection connection)
	{
		return false;
	}

	public Action<T> Wrap(Action<T> action, Action<Action> wrapper)
	{
		return action;
	}
}
public class NullSignal<T1, T2> : ISignal<T1, T2>, IReadOnlySignal<T1, T2>, IReadonlySignalBase<Action<T1, T2>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2>>, IConnectableOnceSignalBase<Action<T1, T2>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2>, IConnectableSignal<T1, T2>, IInvokableSignal<T1, T2>
{
	private readonly SignalConnection emptyConnection = default(SignalConnection);

	public int InvokedCount => 0;

	public int ConnectionCount => 0;

	public int MaximumConnections
	{
		set
		{
		}
	}

	public void Clear()
	{
	}

	public void Invoke(T1 t1, T2 t2)
	{
	}

	public SignalConnection Connect(Action<T1, T2> action)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<TA>(Action<T1, T2, TA> action, TA a)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<TA, TB>(Action<T1, T2, TA, TB> action, TA a, TB b)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<TA, TB, TC>(Action<T1, T2, TA, TB, TC> action, TA a, TB b, TC c)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce(Action<T1, T2> action)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<TA>(Action<T1, T2, TA> action, TA a)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<TA, TB>(Action<T1, T2, TA, TB> action, TA a, TB b)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<TA, TB, TC>(Action<T1, T2, TA, TB, TC> action, TA a, TB b, TC c)
	{
		return emptyConnection;
	}

	public bool IsConnected(SignalConnection connection)
	{
		return false;
	}

	public bool Disconnect(SignalConnection connection)
	{
		return false;
	}

	public Action<T1, T2> Wrap(Action<T1, T2> action, Action<Action> wrapper)
	{
		return action;
	}
}
public class NullSignal<T1, T2, T3> : ISignal<T1, T2, T3>, IReadOnlySignal<T1, T2, T3>, IReadonlySignalBase<Action<T1, T2, T3>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3>>, IConnectableOnceSignalBase<Action<T1, T2, T3>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3>, IConnectableSignal<T1, T2, T3>, IInvokableSignal<T1, T2, T3>
{
	private readonly SignalConnection emptyConnection = default(SignalConnection);

	public int InvokedCount => 0;

	public int ConnectionCount => 0;

	public int MaximumConnections
	{
		set
		{
		}
	}

	public void Clear()
	{
	}

	public void Invoke(T1 t1, T2 t2, T3 t3)
	{
	}

	public SignalConnection Connect(Action<T1, T2, T3> action)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T4>(Action<T1, T2, T3, T4> action, T4 t4)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T4, T5>(Action<T1, T2, T3, T4, T5> action, T4 t4, T5 t5)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce(Action<T1, T2, T3> action)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T4>(Action<T1, T2, T3, T4> action, T4 t4)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T4, T5>(Action<T1, T2, T3, T4, T5> action, T4 t4, T5 t5)
	{
		return emptyConnection;
	}

	public bool IsConnected(SignalConnection connection)
	{
		return false;
	}

	public bool Disconnect(SignalConnection connection)
	{
		return false;
	}
}
public class NullSignal<T1, T2, T3, T4> : ISignal<T1, T2, T3, T4>, IReadOnlySignal<T1, T2, T3, T4>, IReadonlySignalBase<Action<T1, T2, T3, T4>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3, T4>>, IConnectableOnceSignalBase<Action<T1, T2, T3, T4>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3, T4>, IConnectableSignal<T1, T2, T3, T4>, IInvokableSignal<T1, T2, T3, T4>
{
	private readonly SignalConnection emptyConnection = default(SignalConnection);

	public int InvokedCount => 0;

	public int ConnectionCount => 0;

	public int MaximumConnections
	{
		set
		{
		}
	}

	public void Clear()
	{
	}

	public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
	{
	}

	public SignalConnection Connect(Action<T1, T2, T3, T4> action)
	{
		return emptyConnection;
	}

	public SignalConnection Connect<T5>(Action<T1, T2, T3, T4, T5> action, T5 t5)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce(Action<T1, T2, T3, T4> action)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce<T5>(Action<T1, T2, T3, T4, T5> action, T5 t5)
	{
		return emptyConnection;
	}

	public bool IsConnected(SignalConnection connection)
	{
		return false;
	}

	public bool Disconnect(SignalConnection connection)
	{
		return false;
	}
}
public class NullSignal<T1, T2, T3, T4, T5> : ISignal<T1, T2, T3, T4, T5>, IReadOnlySignal<T1, T2, T3, T4, T5>, IReadonlySignalBase<Action<T1, T2, T3, T4, T5>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3, T4, T5>>, IConnectableOnceSignalBase<Action<T1, T2, T3, T4, T5>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3, T4, T5>, IConnectableSignal<T1, T2, T3, T4, T5>, IInvokableSignal<T1, T2, T3, T4, T5>
{
	private readonly SignalConnection emptyConnection = default(SignalConnection);

	public int InvokedCount => 0;

	public int ConnectionCount => 0;

	public int MaximumConnections
	{
		set
		{
		}
	}

	public void Clear()
	{
	}

	public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
	}

	public SignalConnection Connect(Action<T1, T2, T3, T4, T5> action)
	{
		return emptyConnection;
	}

	public SignalConnection ConnectOnce(Action<T1, T2, T3, T4, T5> action)
	{
		return emptyConnection;
	}

	public bool IsConnected(SignalConnection connection)
	{
		return false;
	}

	public bool Disconnect(SignalConnection connection)
	{
		return false;
	}
}
