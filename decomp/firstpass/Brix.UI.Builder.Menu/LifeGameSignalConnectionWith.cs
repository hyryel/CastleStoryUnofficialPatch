using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class LifeGameSignalConnectionWith<TWith> : LifeSignalConnectionBase
{
	private GameSignals.GameSignal _signal;

	private Action<Action<TWith>, TWith> _action;

	private Action<TWith> _actionWith;

	private TWith _with;

	private void Init(GameSignals.GameSignal signal, Action<Action<TWith>, TWith> action, Action<TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeGameSignalConnectionWith<TWith> Make(GameSignals.GameSignal signal, Action<Action<TWith>, TWith> action, Action<TWith> actionWith, TWith with)
	{
		LifeGameSignalConnectionWith<TWith> lifeGameSignalConnectionWith = ObjectPool<LifeGameSignalConnectionWith<TWith>>.Reserve();
		lifeGameSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeGameSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeGameSignalConnectionWith<TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, _action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
public class LifeGameSignalConnectionWith<T1, TWith> : LifeSignalConnectionBase
{
	private GameSignals.GameSignal<T1> _signal;

	private Action<T1, Action<T1, TWith>, TWith> _action;

	private Action<T1, TWith> _actionWith;

	private TWith _with;

	private void Init(GameSignals.GameSignal<T1> signal, Action<T1, Action<T1, TWith>, TWith> action, Action<T1, TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeGameSignalConnectionWith<T1, TWith> Make(GameSignals.GameSignal<T1> signal, Action<T1, Action<T1, TWith>, TWith> action, Action<T1, TWith> actionWith, TWith with)
	{
		LifeGameSignalConnectionWith<T1, TWith> lifeGameSignalConnectionWith = ObjectPool<LifeGameSignalConnectionWith<T1, TWith>>.Reserve();
		lifeGameSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeGameSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeGameSignalConnectionWith<T1, TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, _action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
public class LifeGameSignalConnectionWith<T1, T2, TWith> : LifeSignalConnectionBase
{
	private GameSignals.GameSignal<T1, T2> _signal;

	private Action<T1, T2, Action<T1, T2, TWith>, TWith> _action;

	private Action<T1, T2, TWith> _actionWith;

	private TWith _with;

	private void Init(GameSignals.GameSignal<T1, T2> signal, Action<T1, T2, Action<T1, T2, TWith>, TWith> action, Action<T1, T2, TWith> actionWith, TWith with)
	{
		_signal = signal;
		_action = action;
		_actionWith = actionWith;
		_with = with;
	}

	public static LifeGameSignalConnectionWith<T1, T2, TWith> Make(GameSignals.GameSignal<T1, T2> signal, Action<T1, T2, Action<T1, T2, TWith>, TWith> action, Action<T1, T2, TWith> actionWith, TWith with)
	{
		LifeGameSignalConnectionWith<T1, T2, TWith> lifeGameSignalConnectionWith = ObjectPool<LifeGameSignalConnectionWith<T1, T2, TWith>>.Reserve();
		lifeGameSignalConnectionWith.Init(signal, action, actionWith, with);
		return lifeGameSignalConnectionWith;
	}

	public override void Dispose()
	{
		ObjectPool<LifeGameSignalConnectionWith<T1, T2, TWith>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, _action, _actionWith, _with).DisconnectWhen(disconnectSignal);
	}
}
