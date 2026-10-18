using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;

namespace Brix.UI.Builder.Menu;

public struct SignalHandle<TLife> where TLife : struct
{
	private static class Utility<TWith>
	{
		public static readonly Action<Action<TWith>, TWith> Action = delegate(Action<TWith> a, TWith t)
		{
			a(t);
		};
	}

	private static class Utility<T1, TWith>
	{
		public static readonly Action<T1, Action<T1, TWith>, TWith> Action = delegate(T1 arg1, Action<T1, TWith> a, TWith t)
		{
			a(arg1, t);
		};
	}

	private static class Utility<T1, T2, TWith>
	{
		public static readonly Action<T1, T2, Action<T1, T2, TWith>, TWith> Action = delegate(T1 arg1, T2 arg2, Action<T1, T2, TWith> a, TWith t)
		{
			a(arg1, arg2, t);
		};
	}

	private static class Utility<T1, T2, T3, TWith>
	{
		public static readonly Action<T1, T2, T3, Action<T1, T2, T3, TWith>, TWith> Action = delegate(T1 arg1, T2 arg2, T3 arg3, Action<T1, T2, T3, TWith> a, TWith t)
		{
			a(arg1, arg2, arg3, t);
		};
	}

	private readonly ISignalConnector<TLife> _connector;

	public SignalHandle(ISignalConnector<TLife> connector)
	{
		_connector = connector;
	}

	public SignalHandle<TLife> AddSignalConnection(TLife life, IConnectableSignal signal, Action action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnection.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1>(TLife life, IConnectableSignal<T1> signal, Action<T1> action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnection<T1>.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, T2>(TLife life, IConnectableSignal<T1, T2> signal, Action<T1, T2> action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnection<T1, T2>.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, T2, T3>(TLife life, IConnectableSignal<T1, T2, T3> signal, Action<T1, T2, T3> action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnection<T1, T2, T3>.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<TWith>(TLife life, IConnectableSignal signal, Action<TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnectionWith<TWith>.Make(signal, Utility<TWith>.Action, actionWith, with));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, TWith>(TLife life, IConnectableSignal<T1> signal, Action<T1, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnectionWith<T1, TWith>.Make(signal, Utility<T1, TWith>.Action, actionWith, with));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, T2, TWith>(TLife life, IConnectableSignal<T1, T2> signal, Action<T1, T2, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnectionWith<T1, T2, TWith>.Make(signal, Utility<T1, T2, TWith>.Action, actionWith, with));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, T2, T3, TWith>(TLife life, IConnectableSignal<T1, T2, T3> signal, Action<T1, T2, T3, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeSignalConnectionWith<T1, T2, T3, TWith>.Make(signal, Utility<T1, T2, T3, TWith>.Action, actionWith, with));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection(TLife life, GameSignals.GameSignal signal, Action action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeGameSignalConnection.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1>(TLife life, GameSignals.GameSignal<T1> signal, Action<T1> action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeGameSignalConnection<T1>.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, T2>(TLife life, GameSignals.GameSignal<T1, T2> signal, Action<T1, T2> action)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeGameSignalConnection<T1, T2>.Make(signal, action));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<TWith>(TLife life, GameSignals.GameSignal signal, Action<TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeGameSignalConnectionWith<TWith>.Make(signal, Utility<TWith>.Action, actionWith, with));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, TWith>(TLife life, GameSignals.GameSignal<T1> signal, Action<T1, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeGameSignalConnectionWith<T1, TWith>.Make(signal, Utility<T1, TWith>.Action, actionWith, with));
		return this;
	}

	public SignalHandle<TLife> AddSignalConnection<T1, T2, TWith>(TLife life, GameSignals.GameSignal<T1, T2> signal, Action<T1, T2, TWith> actionWith, TWith with)
	{
		LifeSignalConnectionBase.GetOrCreate(_connector, life, out List<LifeSignalConnectionBase> lifeSignalConnections);
		lifeSignalConnections.Add(LifeGameSignalConnectionWith<T1, T2, TWith>.Make(signal, Utility<T1, T2, TWith>.Action, actionWith, with));
		return this;
	}
}
