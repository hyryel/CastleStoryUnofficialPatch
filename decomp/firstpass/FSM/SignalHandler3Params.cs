using Brix.External.Signals;

namespace FSM;

public abstract class SignalHandler3Params<P1, P2, P3> : SignalHandler
{
	protected SignalHandler3Params(Signal<P1, P2, P3> sig, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
	}

	public sealed override SignalConnection CreateConnection(SignalStateRun run)
	{
		return SignalConnection.Create(Signal as Signal<P1, P2, P3>, run);
	}

	public abstract SignalTransition<P1, P2, P3> Handle(P1 p1, P2 p2, P3 p3);
}
