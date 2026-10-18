using System;

namespace Brix.External.Signals;

public interface IConnectableOnceSignal : IConnectableOnceSignalBase<Action>, SignalInterface
{
	SignalConnection ConnectOnce<T1>(Action<T1> action, T1 t1);

	SignalConnection ConnectOnce<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2);

	SignalConnection ConnectOnce<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3);

	SignalConnection ConnectOnce<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4);

	SignalConnection ConnectOnce<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}
public interface IConnectableOnceSignal<T1> : IConnectableOnceSignalBase<Action<T1>>, SignalInterface
{
	SignalConnection ConnectOnce<T2>(Action<T1, T2> action, T2 t2);

	SignalConnection ConnectOnce<T2, T3>(Action<T1, T2, T3> action, T2 t2, T3 t3);

	SignalConnection ConnectOnce<T2, T3, T4>(Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4);

	SignalConnection ConnectOnce<T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T2 t2, T3 t3, T4 t4, T5 t5);
}
public interface IConnectableOnceSignal<T1, T2> : IConnectableOnceSignalBase<Action<T1, T2>>, SignalInterface
{
	SignalConnection ConnectOnce<T3>(Action<T1, T2, T3> action, T3 t3);

	SignalConnection ConnectOnce<T3, T4>(Action<T1, T2, T3, T4> action, T3 t3, T4 t4);

	SignalConnection ConnectOnce<T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T3 t3, T4 t4, T5 t5);
}
public interface IConnectableOnceSignal<T1, T2, T3> : IConnectableOnceSignalBase<Action<T1, T2, T3>>, SignalInterface
{
	SignalConnection ConnectOnce<T4>(Action<T1, T2, T3, T4> action, T4 t4);

	SignalConnection ConnectOnce<T4, T5>(Action<T1, T2, T3, T4, T5> action, T4 t4, T5 t5);
}
public interface IConnectableOnceSignal<T1, T2, T3, T4> : IConnectableOnceSignalBase<Action<T1, T2, T3, T4>>, SignalInterface
{
	SignalConnection ConnectOnce<T5>(Action<T1, T2, T3, T4, T5> action, T5 t5);
}
public interface IConnectableOnceSignal<T1, T2, T3, T4, T5> : IConnectableOnceSignalBase<Action<T1, T2, T3, T4, T5>>, SignalInterface
{
}
