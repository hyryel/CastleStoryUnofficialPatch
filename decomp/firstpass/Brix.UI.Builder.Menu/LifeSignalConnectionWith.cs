using System;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class LifeSignalConnectionWith<TWith> : LifeSignalConnectionBase
{
	private IConnectableSignal _signal;

	private Action<Action<TWith>, TWith> _action;

	private Action<TWith> _actionWith;

	private TWith _with;

	private void Init(IConnectableSignal signal, Action<Action<TWith>, TWith> action, Action<TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeSignalConnectionWith<TWith> Make(IConnectableSignal signal, Action<Action<TWith>, TWith> action, Action<TWith> actionWith, TWith with)
	{
		LifeSignalConnectionWith<TWith> lifeSignalConnectionWith = ObjectPool<LifeSignalConnectionWith<TWith>>.Reserve();
		lifeSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnectionWith<TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
public class LifeSignalConnectionWith<T1, TWith> : LifeSignalConnectionBase
{
	private IConnectableSignal<T1> _signal;

	private Action<T1, Action<T1, TWith>, TWith> _action;

	private Action<T1, TWith> _actionWith;

	private TWith _with;

	private void Init(IConnectableSignal<T1> signal, Action<T1, Action<T1, TWith>, TWith> action, Action<T1, TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeSignalConnectionWith<T1, TWith> Make(IConnectableSignal<T1> signal, Action<T1, Action<T1, TWith>, TWith> action, Action<T1, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionWith<T1, TWith> lifeSignalConnectionWith = ObjectPool<LifeSignalConnectionWith<T1, TWith>>.Reserve();
		lifeSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnectionWith<T1, TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
public class LifeSignalConnectionWith<T1, T2, TWith> : LifeSignalConnectionBase
{
	private IConnectableSignal<T1, T2> _signal;

	private Action<T1, T2, Action<T1, T2, TWith>, TWith> _action;

	private Action<T1, T2, TWith> _actionWith;

	private TWith _with;

	private void Init(IConnectableSignal<T1, T2> signal, Action<T1, T2, Action<T1, T2, TWith>, TWith> action, Action<T1, T2, TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeSignalConnectionWith<T1, T2, TWith> Make(IConnectableSignal<T1, T2> signal, Action<T1, T2, Action<T1, T2, TWith>, TWith> action, Action<T1, T2, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionWith<T1, T2, TWith> lifeSignalConnectionWith = ObjectPool<LifeSignalConnectionWith<T1, T2, TWith>>.Reserve();
		lifeSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnectionWith<T1, T2, TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
public class LifeSignalConnectionWith<T1, T2, T3, TWith> : LifeSignalConnectionBase
{
	private IConnectableSignal<T1, T2, T3> _signal;

	private Action<T1, T2, T3, Action<T1, T2, T3, TWith>, TWith> _action;

	private Action<T1, T2, T3, TWith> _actionWith;

	private TWith _with;

	private void Init(IConnectableSignal<T1, T2, T3> signal, Action<T1, T2, T3, Action<T1, T2, T3, TWith>, TWith> action, Action<T1, T2, T3, TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeSignalConnectionWith<T1, T2, T3, TWith> Make(IConnectableSignal<T1, T2, T3> signal, Action<T1, T2, T3, Action<T1, T2, T3, TWith>, TWith> action, Action<T1, T2, T3, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionWith<T1, T2, T3, TWith> lifeSignalConnectionWith = ObjectPool<LifeSignalConnectionWith<T1, T2, T3, TWith>>.Reserve();
		lifeSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeSignalConnectionWith<T1, T2, T3, TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		_signal.Connect(_action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
