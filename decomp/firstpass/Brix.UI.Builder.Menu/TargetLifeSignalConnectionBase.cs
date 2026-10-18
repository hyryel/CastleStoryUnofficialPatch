using System;
using System.Collections.Generic;
using Brix.External.Signals;

namespace Brix.UI.Builder.Menu;

public abstract class TargetLifeSignalConnectionBase<TTarget>
{
	private static class Utility<TLife> where TLife : struct
	{
		public static readonly Action<ITargetSignalConnector<TTarget, TLife>> ClearAction = delegate(ITargetSignalConnector<TTarget, TLife> t)
		{
			TargetLifeSignalConnectionBase<TTarget>.Clear(t, disconnect: true);
		};
	}

	public abstract void ConnectUntil(TTarget target, IReadOnlySignal disconnectSignal);

	public static void Connect<TLife>(TTarget target, ITargetSignalConnector<TTarget, TLife> connector, TLife life) where TLife : struct
	{
		GetOrCreate(connector, life, out var lifeSignalConnections, out var disconnectSignal);
		foreach (TargetLifeSignalConnectionBase<TTarget> item in lifeSignalConnections)
		{
			item.ConnectUntil(target, disconnectSignal);
		}
	}

	public static void Disconnect<TLife>(ITargetSignalConnector<TTarget, TLife> connector, TLife life) where TLife : struct
	{
		GetOrCreate(connector, life, out Signal disconnectSignal);
		disconnectSignal.Invoke();
	}

	public static void GetOrCreate<TLife>(ITargetSignalConnector<TTarget, TLife> connector, TLife life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections, out Signal disconnectSignal) where TLife : struct
	{
		if (connector.TargetSignalMap == null)
		{
			connector.TargetSignalMap = new Dictionary<TLife, KeyValuePair<List<TargetLifeSignalConnectionBase<TTarget>>, Signal>>();
			connector.OnClearSignal.Connect(Utility<TLife>.ClearAction, connector);
		}
		if (!connector.TargetSignalMap.TryGetValue(life, out var value))
		{
			value = new KeyValuePair<List<TargetLifeSignalConnectionBase<TTarget>>, Signal>(new List<TargetLifeSignalConnectionBase<TTarget>>(), new Signal());
			connector.TargetSignalMap.Add(life, value);
		}
		lifeSignalConnections = value.Key;
		disconnectSignal = value.Value;
	}

	public static void GetOrCreate<TLife>(ITargetSignalConnector<TTarget, TLife> connector, TLife life, out List<TargetLifeSignalConnectionBase<TTarget>> lifeSignalConnections) where TLife : struct
	{
		GetOrCreate(connector, life, out lifeSignalConnections, out var _);
	}

	public static void GetOrCreate<TLife>(ITargetSignalConnector<TTarget, TLife> connector, TLife life, out Signal disconnectSignal) where TLife : struct
	{
		GetOrCreate(connector, life, out var _, out disconnectSignal);
	}

	public static void Clear<TLife>(ITargetSignalConnector<TTarget, TLife> connector, bool disconnect = true) where TLife : struct
	{
		if (connector.TargetSignalMap == null)
		{
			return;
		}
		foreach (KeyValuePair<TLife, KeyValuePair<List<TargetLifeSignalConnectionBase<TTarget>>, Signal>> item in connector.TargetSignalMap)
		{
			Signal value = item.Value.Value;
			value.Invoke();
			value.Clear();
			List<TargetLifeSignalConnectionBase<TTarget>> key = item.Value.Key;
			key.Clear();
		}
		connector.TargetSignalMap.Clear();
		connector.TargetSignalMap = null;
	}
}
