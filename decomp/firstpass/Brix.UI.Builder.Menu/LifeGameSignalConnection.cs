using System;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Signals;

namespace Brix.UI.Builder.Menu;

public class LifeGameSignalConnection : LifeSignalConnectionBase
{
	private GameSignals.GameSignal _signal;

	private Action _action;

	private void Init(GameSignals.GameSignal signal, Action action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeGameSignalConnection Make(GameSignals.GameSignal signal, Action action)
	{
		LifeGameSignalConnection lifeGameSignalConnection = ObjectPool<LifeGameSignalConnection>.Reserve();
		lifeGameSignalConnection.Init(signal, action);
		return lifeGameSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeGameSignalConnection>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, _action).DisconnectWhen(disconnectSignal);
	}
}
public class LifeGameSignalConnection<T1> : LifeSignalConnectionBase
{
	private GameSignals.GameSignal<T1> _signal;

	private Action<T1> _action;

	public LifeGameSignalConnection()
	{
	}

	public LifeGameSignalConnection(GameSignals.GameSignal<T1> signal, Action<T1> action)
	{
		_signal = signal;
		_action = action;
	}

	private void Init(GameSignals.GameSignal<T1> signal, Action<T1> action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeGameSignalConnection<T1> Make(GameSignals.GameSignal<T1> signal, Action<T1> action)
	{
		LifeGameSignalConnection<T1> lifeGameSignalConnection = ObjectPool<LifeGameSignalConnection<T1>>.Reserve();
		lifeGameSignalConnection.Init(signal, action);
		return lifeGameSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeGameSignalConnection<T1>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, _action).DisconnectWhen(disconnectSignal);
	}
}
public class LifeGameSignalConnection<T1, T2> : LifeSignalConnectionBase
{
	private GameSignals.GameSignal<T1, T2> _signal;

	private Action<T1, T2> _action;

	private void Init(GameSignals.GameSignal<T1, T2> signal, Action<T1, T2> action)
	{
		_signal = signal;
		_action = action;
	}

	public static LifeGameSignalConnection<T1, T2> Make(GameSignals.GameSignal<T1, T2> signal, Action<T1, T2> action)
	{
		LifeGameSignalConnection<T1, T2> lifeGameSignalConnection = ObjectPool<LifeGameSignalConnection<T1, T2>>.Reserve();
		lifeGameSignalConnection.Init(signal, action);
		return lifeGameSignalConnection;
	}

	public override void Dispose()
	{
		ObjectPool<LifeGameSignalConnection<T1, T2>>.Release(this);
	}

	public override void ConnectUntil(IReadOnlySignal disconnectSignal)
	{
		GameSignals.Connect(_signal, _action).DisconnectWhen(disconnectSignal);
	}
}
