using System;

namespace Brix.External.Signals;

public class SynchronousListener : SignalListener
{
	public SignalConnection Connect(IReadonlySignalBase<Action> signal, Action action)
	{
		return signal.Connect(action);
	}

	public SignalConnection Connect<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		return signal.Connect(action);
	}

	public SignalConnection Connect<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		return signal.Connect(action);
	}

	public SignalConnection Connect<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		return signal.Connect(action);
	}

	public SignalConnection Connect<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		return signal.Connect(action);
	}

	public SignalConnection ConnectOnce(IReadonlySignalBase<Action> signal, Action action)
	{
		return signal.ConnectOnce(action);
	}

	public SignalConnection ConnectOnce<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		return signal.ConnectOnce(action);
	}

	public SignalConnection ConnectOnce<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		return signal.ConnectOnce(action);
	}

	public SignalConnection ConnectOnce<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		return signal.ConnectOnce(action);
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		return signal.ConnectOnce(action);
	}

	public void Poll()
	{
	}
}
