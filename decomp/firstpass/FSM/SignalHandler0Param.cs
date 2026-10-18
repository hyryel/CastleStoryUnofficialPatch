using Brix.External.Signals;

namespace FSM;

public abstract class SignalHandler0Param : SignalHandler
{
	protected SignalHandler0Param(Signal sig, TimingType timing, bool trap)
		: base(sig, timing, trap)
	{
	}

	public sealed override SignalConnection CreateConnection(SignalStateRun run)
	{
		return SignalConnection.Create(Signal as Signal, run);
	}

	public abstract SignalTransition Handle();
}
