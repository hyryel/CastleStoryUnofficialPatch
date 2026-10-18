using System;

namespace Brix.External.Signals;

public interface IReadOnlySignal : IReadonlySignalBase<Action>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action>, IConnectableOnceSignalBase<Action>, IDisconnectableSignal, IConnectableOnceSignal, IConnectableSignal
{
}
public interface IReadOnlySignal<T1> : IReadonlySignalBase<Action<T1>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1>>, IConnectableOnceSignalBase<Action<T1>>, IDisconnectableSignal, IConnectableOnceSignal<T1>, IConnectableSignal<T1>
{
}
public interface IReadOnlySignal<T1, T2> : IReadonlySignalBase<Action<T1, T2>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2>>, IConnectableOnceSignalBase<Action<T1, T2>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2>, IConnectableSignal<T1, T2>
{
}
public interface IReadOnlySignal<T1, T2, T3> : IReadonlySignalBase<Action<T1, T2, T3>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3>>, IConnectableOnceSignalBase<Action<T1, T2, T3>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3>, IConnectableSignal<T1, T2, T3>
{
}
public interface IReadOnlySignal<T1, T2, T3, T4> : IReadonlySignalBase<Action<T1, T2, T3, T4>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3, T4>>, IConnectableOnceSignalBase<Action<T1, T2, T3, T4>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3, T4>, IConnectableSignal<T1, T2, T3, T4>
{
}
public interface IReadOnlySignal<T1, T2, T3, T4, T5> : IReadonlySignalBase<Action<T1, T2, T3, T4, T5>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3, T4, T5>>, IConnectableOnceSignalBase<Action<T1, T2, T3, T4, T5>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3, T4, T5>, IConnectableSignal<T1, T2, T3, T4, T5>
{
}
