using Brix.External.Signals;

namespace FSM;

public abstract class SignalHandler
{
	public enum TimingType
	{
		PreChildren,
		PostChildren
	}

	public SignalInterface Signal;

	public TimingType Timing;

	public bool Trap;

	protected SignalHandler(SignalInterface sig, TimingType timing, bool trap)
	{
		Signal = sig;
		Timing = timing;
		Trap = trap;
	}

	public abstract SignalConnection CreateConnection(SignalStateRun run);
}
