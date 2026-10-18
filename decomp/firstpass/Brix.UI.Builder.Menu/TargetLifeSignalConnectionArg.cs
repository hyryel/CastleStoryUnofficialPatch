using System;
using Brix.External.Signals;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class TargetLifeSignalConnectionArg<TTarget, T1> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly Signal _signal;

	private readonly Action<TTarget, T1> _action;

	private readonly T1 _arg1;

	public TargetLifeSignalConnectionArg(Signal signal, Action<TTarget, T1> action, T1 arg1)
	{
		_signal = signal;
		_action = action;
		_arg1 = arg1;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(delegate(TargetLifeSignalConnectionArg<TTarget, T1> self, TTarget m)
		{
			self._action(m, self._arg1);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
