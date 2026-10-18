namespace Brix.External.Signals;

public interface IDisconnectableSignal : SignalInterface
{
	bool Disconnect(SignalConnection connection);

	bool IsConnected(SignalConnection connection);
}
