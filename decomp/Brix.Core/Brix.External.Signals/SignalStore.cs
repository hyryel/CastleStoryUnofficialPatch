using System;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.External.Signals;

public class SignalStore : MonoBehaviour, SignalListener, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	private SignalStorePoco store = new SignalStorePoco();

	public bool AutoPoll = false;

	public SignalConnection Connect(IReadonlySignalBase<Action> signal, Action action)
	{
		return store.Connect(signal, action);
	}

	public SignalConnection Connect<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		return store.Connect(signal, action);
	}

	public SignalConnection Connect<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		return store.Connect(signal, action);
	}

	public SignalConnection Connect<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		return store.Connect(signal, action);
	}

	public SignalConnection Connect<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		return store.Connect(signal, action);
	}

	public SignalConnection ConnectOnce(IReadonlySignalBase<Action> signal, Action action)
	{
		return store.ConnectOnce(signal, action);
	}

	public SignalConnection ConnectOnce<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		return store.ConnectOnce(signal, action);
	}

	public SignalConnection ConnectOnce<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		return store.ConnectOnce(signal, action);
	}

	public SignalConnection ConnectOnce<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		return store.ConnectOnce(signal, action);
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		return store.ConnectOnce(signal, action);
	}

	public void Poll()
	{
		store.Poll();
	}

	private void Start()
	{
		base.enabled = AutoPoll;
	}

	private void FixedUpdate()
	{
		Poll();
	}

	public void Initialize()
	{
	}

	public void OnRequested()
	{
	}

	public void ResetComponent()
	{
		store.Clear();
	}
}
