using Brix.External.Signals;

namespace FSM;

public abstract class SignalHandler2Params<P1, P2> : SignalHandler
{
	protected SignalHandler2Params(Signal<P1, P2> sig, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
	}

	public sealed override SignalConnection CreateConnection(SignalStateRun run)
	{
		return SignalConnection.Create(Signal as Signal<P1, P2>, run);
	}

	public abstract SignalTransition<P1, P2> Handle(P1 p1, P2 p2);
}
