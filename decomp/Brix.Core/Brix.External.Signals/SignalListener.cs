using System;

namespace Brix.External.Signals;

public interface SignalListener
{
	SignalConnection Connect(IReadonlySignalBase<Action> signal, Action action);

	SignalConnection Connect<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action);

	SignalConnection Connect<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action);

	SignalConnection Connect<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action);

	SignalConnection Connect<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action);

	SignalConnection ConnectOnce(IReadonlySignalBase<Action> signal, Action action);

	SignalConnection ConnectOnce<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action);

	SignalConnection ConnectOnce<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action);

	SignalConnection ConnectOnce<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action);

	SignalConnection ConnectOnce<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action);

	void Poll();
}
