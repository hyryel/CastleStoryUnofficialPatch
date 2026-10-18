namespace Brix.External.Signals;

public interface IConnectableOnceSignalBase<TAction> : SignalInterface
{
	SignalConnection ConnectOnce(TAction action);
}
