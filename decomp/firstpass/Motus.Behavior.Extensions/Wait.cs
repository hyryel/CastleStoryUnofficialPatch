using Brix.External.Signals;

namespace Motus.Behavior.Extensions;

public static class Wait
{
	public static StaticNode ForLateSignal(Signal sig)
	{
		return Node.WaitFor.Signal(sig).Or().Condition((Signal s) => s.InvokedCount > 0, sig);
	}
}
