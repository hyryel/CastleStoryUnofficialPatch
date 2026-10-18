using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;

namespace Brix.UI.Builder.Menu;

public struct TargetSignalHandle<TTarget, TLife> where TLife : struct
{
	private static class Utility<TTargetBase>
	{
		public static Action<TTarget, Action<TTargetBase>> Action = delegate(TTarget t, Action<TTargetBase> a)
		{
			a((TTargetBase)(object)t);
		};
	}

	private readonly ITargetSignalConnector<TTarget, TLife> _connector;

	public TargetSignalHandle(ITargetSignalConnector<TTarget, TLife> connector)
	{
		_connector = connector;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection(TLife life, Signal signal, Action<TTarget> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeSignalConnection<TTarget>(signal, action));
		return this;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection<T1>(TLife life, Signal<T1> signal, Action<TTarget, T1> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeSignalConnection<TTarget, T1>(signal, action));
		return this;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection<T1, T2>(TLife life, Signal<T1, T2> signal, Action<TTarget, T1, T2> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeSignalConnection<TTarget, T1, T2>(signal, action));
		return this;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection(TLife life, GameSignals.GameSignal signal, Action<TTarget> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeGameSignalConnection<TTarget>(signal, action));
		return this;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection<T1>(TLife life, GameSignals.GameSignal<T1> signal, Action<TTarget, T1> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeGameSignalConnection<TTarget, T1>(signal, action));
		return this;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection<T1, T2>(TLife life, GameSignals.GameSignal<T1, T2> signal, Action<TTarget, T1, T2> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeGameSignalConnection<TTarget, T1, T2>(signal, action));
		return this;
	}

	public TargetSignalHandle<TTarget, TLife> AddSignalConnection<TTargetBase>(TLife life, Signal signal, Action<TTargetBase> action)
	{
		TargetLifeSignalConnectionBase<TTarget>.GetOrCreate(_connector, life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections);
		lifeSignalConnections.Add(new TargetLifeSignalConnectionArg<TTarget, Action<TTargetBase>>(signal, Utility<TTargetBase>.Action, action));
		return this;
	}
}
