using System;

namespace Brix.External.Signals;

public interface IConnectableSignal : IConnectableSignalBase<Action>, SignalInterface
{
	SignalConnection Connect<T1>(Action<T1> action, T1 t1);

	SignalConnection Connect<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2);

	SignalConnection Connect<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3);

	SignalConnection Connect<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4);

	SignalConnection Connect<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}
public interface IConnectableSignal<T1> : IConnectableSignalBase<Action<T1>>, SignalInterface
{
	SignalConnection Connect<T2>(Action<T1, T2> action, T2 t2);

	SignalConnection Connect<T2, T3>(Action<T1, T2, T3> action, T2 t2, T3 t3);

	SignalConnection Connect<T2, T3, T4>(Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4);

	SignalConnection Connect<T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T2 t2, T3 t3, T4 t4, T5 t5);
}
public interface IConnectableSignal<T1, T2> : IConnectableSignalBase<Action<T1, T2>>, SignalInterface
{
	SignalConnection Connect<T3>(Action<T1, T2, T3> action, T3 t3);

	SignalConnection Connect<T3, T4>(Action<T1, T2, T3, T4> action, T3 t3, T4 t4);

	SignalConnection Connect<T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T3 t3, T4 t4, T5 t5);
}
public interface IConnectableSignal<T1, T2, T3> : IConnectableSignalBase<Action<T1, T2, T3>>, SignalInterface
{
	SignalConnection Connect<T4>(Action<T1, T2, T3, T4> action, T4 t4);

	SignalConnection Connect<T4, T5>(Action<T1, T2, T3, T4, T5> action, T4 t4, T5 t5);
}
public interface IConnectableSignal<T1, T2, T3, T4> : IConnectableSignalBase<Action<T1, T2, T3, T4>>, SignalInterface
{
	SignalConnection Connect<T5>(Action<T1, T2, T3, T4, T5> action, T5 t5);
}
public interface IConnectableSignal<T1, T2, T3, T4, T5> : IConnectableSignalBase<Action<T1, T2, T3, T4, T5>>, SignalInterface
{
}
