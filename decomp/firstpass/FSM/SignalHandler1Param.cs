using Brix.External.Signals;

namespace FSM;

public abstract class SignalHandler1Param<P1> : SignalHandler
{
	protected SignalHandler1Param(Signal<P1> sig, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
	}

	public sealed override SignalConnection CreateConnection(SignalStateRun run)
	{
		return SignalConnection.Create(Signal as Signal<P1>, run);
	}

	public abstract SignalTransition<P1> Handle(P1 p1);
}
