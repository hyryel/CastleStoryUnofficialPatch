using Brix.External.Signals;

namespace FSM;

public abstract class SignalHandler4Params<P1, P2, P3, P4> : SignalHandler
{
	protected SignalHandler4Params(Signal<P1, P2, P3, P4> sig, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
	}

	public sealed override SignalConnection CreateConnection(SignalStateRun run)
	{
		return SignalConnection.Create(Signal as Signal<P1, P2, P3, P4>, run);
	}

	public abstract SignalTransition<P1, P2, P3, P4> Handle(P1 p1, P2 p2, P3 p3, P4 p4);
}
