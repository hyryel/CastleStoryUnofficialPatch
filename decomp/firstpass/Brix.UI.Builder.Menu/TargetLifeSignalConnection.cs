using System;
using Brix.External.Signals;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class TargetLifeSignalConnection<TTarget> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly IConnectableSignal _signal;

	private readonly Action<TTarget> _action;

	public TargetLifeSignalConnection(IConnectableSignal signal, Action<TTarget> action)
	{
		_signal = signal;
		_action = action;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(delegate(TargetLifeSignalConnection<TTarget> self, TTarget m)
		{
			self._action(m);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
public class TargetLifeSignalConnection<TTarget, T1> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly IConnectableSignal<T1> _signal;

	private readonly Action<TTarget, T1> _action;

	public TargetLifeSignalConnection(IConnectableSignal<T1> signal, Action<TTarget, T1> action)
	{
		_signal = signal;
		_action = action;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(delegate(T1 arg1, TargetLifeSignalConnection<TTarget, T1> self, TTarget m)
		{
			self._action(m, arg1);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
public class TargetLifeSignalConnection<TTarget, T1, T2> : TargetLifeSignalConnectionBase<TTarget>
{
	private readonly IConnectableSignal<T1, T2> _signal;

	private readonly Action<TTarget, T1, T2> _action;

	public TargetLifeSignalConnection(IConnectableSignal<T1, T2> signal, Action<TTarget, T1, T2> action)
	{
		_signal = signal;
		_action = action;
	}

	public override void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(delegate(T1 arg1, T2 arg2, TargetLifeSignalConnection<TTarget, T1, T2> self, TTarget m)
		{
			self._action(m, arg1, arg2);
		}, this, target).DisconnectWhen(disconnectSignal);
	}
}
