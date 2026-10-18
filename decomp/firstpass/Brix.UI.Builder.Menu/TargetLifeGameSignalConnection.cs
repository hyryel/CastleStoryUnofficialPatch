using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class TargetLifeGameSignalConnection<TTarget> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly GameSignals.GameSignal _signal;

	private readonly Action<TTarget> _action;

	public TargetLifeGameSignalConnection(GameSignals.GameSignal signal, Action<TTarget> action)
	{
		_signal = signal;
		_action = action;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, delegate(TargetLifeGameSignalConnection<TTarget> self, TTarget m)
		{
			self._action(m);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
public class TargetLifeGameSignalConnection<TTarget, T1> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly GameSignals.GameSignal<T1> _signal;

	private readonly Action<TTarget, T1> _action;

	public TargetLifeGameSignalConnection(GameSignals.GameSignal<T1> signal, Action<TTarget, T1> action)
	{
		_signal = signal;
		_action = action;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, delegate(T1 arg1, TargetLifeGameSignalConnection<TTarget, T1> self, TTarget m)
		{
			self._action(m, arg1);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
public class TargetLifeGameSignalConnection<TTarget, T1, T2> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly GameSignals.GameSignal<T1, T2> _signal;

	private readonly Action<TTarget, T1, T2> _action;

	public TargetLifeGameSignalConnection(GameSignals.GameSignal<T1, T2> signal, Action<TTarget, T1, T2> action)
	{
		_signal = signal;
		_action = action;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, delegate(T1 arg1, T2 arg2, TargetLifeGameSignalConnection<TTarget, T1, T2> self, TTarget m)
		{
			self._action(m, arg1, arg2);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
