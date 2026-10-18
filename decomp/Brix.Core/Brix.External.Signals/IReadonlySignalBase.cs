namespace Brix.External.Signals;

public interface IReadonlySignalBase<TAction> : IClearableSignal, SignalInterface, IConnectableSignalBase<TAction>, IConnectableOnceSignalBase<TAction>, IDisconnectableSignal
{
}
