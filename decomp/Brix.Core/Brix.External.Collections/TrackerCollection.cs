using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Signals;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.External.Collections;

public class TrackerCollection<T> : TrackerCollection<T, ICollection<T>>
{
	public TrackerCollection(ICollection<T> collection)
		: base(collection)
	{
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _collection.GetEnumerator();
	}
}
public class TrackerCollection<T, TCollection> : ReactiveCollection<T, TCollection> where TCollection : ICollection<T>
{
	private HashSet<Func<T, IConnectableSignal<T>>> allRemoveWhen;

	private HashSet<Func<T, IConnectableSignal<GameObject>>> allRemoveWhenVoid;

	private Dictionary<T, Signal> disconnectors;

	protected Signal Cleared = new Signal();

	public TrackerCollection(TCollection collection)
		: base(collection)
	{
		allRemoveWhen = new HashSet<Func<T, IConnectableSignal<T>>>();
		allRemoveWhenVoid = new HashSet<Func<T, IConnectableSignal<GameObject>>>();
		disconnectors = new Dictionary<T, Signal>();
	}

	public SignalConnection AddWhen(Signal<T> signal)
	{
		SignalConnection signalConnection = signal.Connect(Add);
		signalConnection.DisconnectWhen(Cleared);
		return signalConnection;
	}

	public override void Add(T obj)
	{
		ConnectAllRemoveWhen(obj);
		base.Add(obj);
	}

	protected override void _Remove(T obj)
	{
		DisconnectAllRemoveWhen(obj);
		base._Remove(obj);
	}

	protected void _Remove(GameObject obj)
	{
		T component = obj.GetComponent<T>();
		DisconnectAllRemoveWhen(component);
		disconnectors.Remove(component);
		base._Remove(component);
	}

	public void RemoveWhen(Func<T, IConnectableSignal<T>> getSignal)
	{
		allRemoveWhen.Add(getSignal);
	}

	public void RemoveWhenGO(Func<T, IConnectableSignal<GameObject>> getSignal)
	{
		allRemoveWhenVoid.Add(getSignal);
	}

	public SignalConnection ClearWhen(Signal signal)
	{
		SignalConnection signalConnection = signal.Connect(Clear);
		signalConnection.DisconnectWhen(Cleared);
		return signalConnection;
	}

	public override void Clear()
	{
		Cleared.Invoke();
		Slinq<KeyValuePair<T, Signal>, DictionaryContext<T, Signal>> slinq = disconnectors.Slinq();
		while (slinq.current.isSome)
		{
			slinq.current.value.Value.Invoke();
			slinq.Skip();
		}
		base.Clear();
	}

	private void ConnectAllRemoveWhen(T obj)
	{
		Signal signal = new Signal();
		disconnectors.GetValueOrCreate(obj, signal);
		foreach (Func<T, IConnectableSignal<T>> item in allRemoveWhen)
		{
			item(obj)?.Connect(_Remove).DisconnectWhen(signal);
		}
		foreach (Func<T, IConnectableSignal<GameObject>> item2 in allRemoveWhenVoid)
		{
			item2(obj)?.Connect(_Remove).DisconnectWhen(signal);
		}
	}

	private void DisconnectAllRemoveWhen(T obj)
	{
		if (disconnectors.ContainsKey(obj))
		{
			disconnectors[obj].Invoke();
			disconnectors.Remove(obj);
		}
	}
}
