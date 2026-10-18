using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using Smooth.Algebraics;

namespace Brix.UI.Builder.Menu;

public abstract class LifeSignalConnectionBase : IPoolable, IResetable, IDisposable
{
	private static class Utility<TLife> where TLife : struct
	{
		public static readonly Action<ISignalConnector<TLife>> ClearAction = delegate(ISignalConnector<TLife> t)
		{
			Clear(t);
		};

		public static readonly Func<Dictionary<TLife, Tuple<List<LifeSignalConnectionBase>, Signal>>> NewDictionary = () => new Dictionary<TLife, Tuple<List<LifeSignalConnectionBase>, Signal>>(FastEnumIntEqualityComparer<TLife>.Instance);
	}

	public abstract void ConnectUntil(IReadOnlySignal disconnectSignal);

	public abstract void Dispose();

	public void ResetObject()
	{
	}

	public static void Connect<TLife>(ISignalConnector<TLife> connector, TLife life) where TLife : struct
	{
		GetOrCreate(connector, life, out var lifeSignalConnections, out var disconnectSignal);
		foreach (LifeSignalConnectionBase item in lifeSignalConnections)
		{
			item.ConnectUntil(disconnectSignal);
		}
	}

	public static void Disconnect<TLife>(ISignalConnector<TLife> connector, TLife life) where TLife : struct
	{
		GetOrCreate(connector, life, out Signal disconnectSignal);
		disconnectSignal.Invoke();
	}

	public static void GetOrCreate<TLife>(ISignalConnector<TLife> connector, TLife life, out List<LifeSignalConnectionBase> lifeSignalConnections, out Signal disconnectSignal) where TLife : struct
	{
		if (connector.SignalMap == null)
		{
			connector.SignalMap = CollectionPool.RequestDictionary(Utility<TLife>.NewDictionary);
			connector.OnClearSignal.Connect(Utility<TLife>.ClearAction, connector);
		}
		if (!connector.SignalMap.TryGetValue(life, out var value))
		{
			value = new Tuple<List<LifeSignalConnectionBase>, Signal>(CollectionPool.RequestList<LifeSignalConnectionBase>(), ObjectPool<Signal>.Reserve());
			connector.SignalMap.Add(life, value);
		}
		lifeSignalConnections = value._1;
		disconnectSignal = value._2;
	}

	public static void GetOrCreate<TLife>(ISignalConnector<TLife> connector, TLife life, out List<LifeSignalConnectionBase> lifeSignalConnections) where TLife : struct
	{
		GetOrCreate(connector, life, out lifeSignalConnections, out var _);
	}

	public static void GetOrCreate<TLife>(ISignalConnector<TLife> connector, TLife life, out Signal disconnectSignal) where TLife : struct
	{
		GetOrCreate(connector, life, out var _, out disconnectSignal);
	}

	public static void Clear<TLife>(ISignalConnector<TLife> connector, bool disconnect = true) where TLife : struct
	{
		if (connector.SignalMap == null)
		{
			return;
		}
		foreach (KeyValuePair<TLife, Tuple<List<LifeSignalConnectionBase>, Signal>> item in connector.SignalMap)
		{
			Signal _ = item.Value._2;
			_.Invoke();
			_.Clear();
			ObjectPool<Signal>.Release(_);
			List<LifeSignalConnectionBase> list = item.Value._1;
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].Dispose();
			}
			list.Clear();
			CollectionPool.Release(ref list);
		}
		connector.SignalMap.Clear();
		Dictionary<TLife, Tuple<List<LifeSignalConnectionBase>, Signal>> dictionary = connector.SignalMap;
		CollectionPool.Release(ref dictionary);
		connector.SignalMap = null;
	}
}
