using Brix.External.Signals;

namespace FSM;

public abstract class SignalConnection
{
	public SignalInterface Signal;

	public SignalConnection(SignalInterface sig)
	{
		Signal = sig;
	}

	public static SignalConnection Create(Signal sig, SignalStateRun run)
	{
		return new SignalConnectionImpl(sig, run);
	}

	public static SignalConnection Create<P1>(Signal<P1> sig, SignalStateRun run)
	{
		return new SignalConnectionImpl<P1>(sig, run);
	}

	public static SignalConnection Create<P1, P2>(Signal<P1, P2> sig, SignalStateRun run)
	{
		return new SignalConnectionImpl<P1, P2>(sig, run);
	}

	public static SignalConnection Create<P1, P2, P3>(Signal<P1, P2, P3> sig, SignalStateRun run)
	{
		return new SignalConnectionImpl<P1, P2, P3>(sig, run);
	}

	public static SignalConnection Create<P1, P2, P3, P4>(Signal<P1, P2, P3, P4> sig, SignalStateRun run)
	{
		return new SignalConnectionImpl<P1, P2, P3, P4>(sig, run);
	}

	public abstract void Disconnect();
}
