namespace Brix.External.Signals;

public interface IConnectableSignalBase<TAction> : SignalInterface
{
	int InvokedCount { get; }

	int ConnectionCount { get; }

	int MaximumConnections { set; }

	SignalConnection Connect(TAction action);
}
