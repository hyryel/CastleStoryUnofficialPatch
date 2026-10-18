using System;
using System.Collections.Generic;
using System.Text;
using Brix.Utils;
using Brix.Utils.Closures;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.External.Signals;

public class SignalStorePoco : SignalListener
{
	public static class Wrapper
	{
		public static Action<Tuple<Action, SignalStorePoco>> WrapAction = WrapFull;

		public static void WrapFull(Tuple<Action, SignalStorePoco> t)
		{
			ClosurePoolHandle<Closure.APooled<Closure.ABase>, Closure.ABase, IAction> closurePoolHandle = t._1.Make().Pooled2();
			t._2.ConnectAction(closurePoolHandle.Value.Action);
			t._2.PrivateHandles.Add(closurePoolHandle);
		}
	}

	public static class Wrapper<T1>
	{
		public static Action<T1, Tuple<Action<T1>, SignalStorePoco>> WrapAction = WrapFull;

		public static void WrapFull(T1 t1, Tuple<Action<T1>, SignalStorePoco> t)
		{
			ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.ABase<T1>, T1>>, Closure.AClosed<Closure.ABase<T1>, T1>, IAction> closurePoolHandle = t._1.Make().Close(t1).Pooled2();
			t._2.ConnectAction(closurePoolHandle.Value.Action);
			t._2.PrivateHandles.Add(closurePoolHandle);
		}
	}

	public static class Wrapper<T1, T2>
	{
		public static Action<T1, T2, Tuple<Action<T1, T2>, SignalStorePoco>> WrapAction = WrapFull;

		public static void WrapFull(T1 t1, T2 t2, Tuple<Action<T1, T2>, SignalStorePoco> t)
		{
			ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.AClosed<Closure.ABase<T1, T2>, T1, T2>, T1>>, Closure.AClosed<Closure.AClosed<Closure.ABase<T1, T2>, T1, T2>, T1>, IAction> closurePoolHandle = t._1.Make().Close(t2).Close(t1)
				.Pooled2();
			t._2.ConnectAction(closurePoolHandle.Value.Action);
			t._2.PrivateHandles.Add(closurePoolHandle);
		}
	}

	public static class Wrapper<T1, T2, T3>
	{
		public static Action<T1, T2, T3, Tuple<Action<T1, T2, T3>, SignalStorePoco>> WrapAction = WrapFull;

		public static void WrapFull(T1 t1, T2 t2, T3 t3, Tuple<Action<T1, T2, T3>, SignalStorePoco> t)
		{
			ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.AClosed<Closure.AClosed<Closure.ABase<T1, T2, T3>, T1, T2, T3>, T1, T2>, T1>>, Closure.AClosed<Closure.AClosed<Closure.AClosed<Closure.ABase<T1, T2, T3>, T1, T2, T3>, T1, T2>, T1>, IAction> closurePoolHandle = t._1.Make().Close(t3).Close(t2)
				.Close(t1)
				.Pooled2();
			t._2.ConnectAction(closurePoolHandle.Value.Action);
			t._2.PrivateHandles.Add(closurePoolHandle);
		}
	}

	public static class Wrapper<T1, T2, T3, T4>
	{
		public static Action<T1, T2, T3, T4, Tuple<Action<T1, T2, T3, T4>, SignalStorePoco>> WrapAction = WrapFull;

		public static void WrapFull(T1 t1, T2 t2, T3 t3, T4 t4, Tuple<Action<T1, T2, T3, T4>, SignalStorePoco> t)
		{
			ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.AClosed<Closure.AClosed<Closure.AClosed<Closure.ABase<T1, T2, T3, T4>, T1, T2, T3, T4>, T1, T2, T3>, T1, T2>, T1>>, Closure.AClosed<Closure.AClosed<Closure.AClosed<Closure.AClosed<Closure.ABase<T1, T2, T3, T4>, T1, T2, T3, T4>, T1, T2, T3>, T1, T2>, T1>, IAction> closurePoolHandle = t._1.Make().Close(t4).Close(t3)
				.Close(t2)
				.Close(t1)
				.Pooled2();
			t._2.ConnectAction(closurePoolHandle.Value.Action);
			t._2.PrivateHandles.Add(closurePoolHandle);
		}
	}

	public static class Test
	{
		public static StringBuilder sb;

		public static Action<int> TestAction = TestFunc;

		public static Action<int, string> TestAction2 = TestFunc2;

		public static void Test1()
		{
			SignalStorePoco store = new SignalStorePoco();
			Signal<int> signal = new Signal<int>();
			Signal<int, string> signal2 = new Signal<int, string>();
			sb = new StringBuilder();
			TestInternal(store, signal, signal2);
			for (int i = 0; i < 100; i++)
			{
				TestInternal(store, signal, signal2);
			}
			Debug.Log(sb.ToString());
		}

		private static void TestFunc(int x)
		{
		}

		private static void TestFunc2(int x, string s)
		{
		}

		private static void TestInternal(SignalStorePoco store, Signal<int> signal, Signal<int, string> signal2)
		{
			store.Connect(signal, TestAction);
			store.Connect(signal2, TestAction2);
			store.Poll();
			signal.Invoke(3);
			signal2.Invoke(4, "yo");
			signal.Invoke(5);
			store.Poll();
			store.Poll();
			store.Clear();
		}
	}

	protected List<Action> ConnectedActions = new List<Action>();

	protected List<IDisposable> PrivateHandles = new List<IDisposable>();

	protected Dictionary<Tuple<object, object>, Tuple<IDisposable, SignalConnection>> ConnectionHandles = new Dictionary<Tuple<object, object>, Tuple<IDisposable, SignalConnection>>();

	private void ConnectAction(Action action)
	{
		ConnectedActions.Add(action);
	}

	public Tuple<IDisposable, Action> Wrap(IReadonlySignalBase<Action> signal, Action action)
	{
		ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.ABase<Tuple<Action, SignalStorePoco>>, Tuple<Action, SignalStorePoco>>>, Closure.AClosed<Closure.ABase<Tuple<Action, SignalStorePoco>>, Tuple<Action, SignalStorePoco>>, IAction> closurePoolHandle = Wrapper.WrapAction.Make().Close(Tuple.Create(action, this)).Pooled2();
		return Tuple.Create((IDisposable)closurePoolHandle, closurePoolHandle.Value.Action);
	}

	public Tuple<IDisposable, Action<T1>> Wrap<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.ABase<T1, Tuple<Action<T1>, SignalStorePoco>>, T1, Tuple<Action<T1>, SignalStorePoco>>, T1>, Closure.AClosed<Closure.ABase<T1, Tuple<Action<T1>, SignalStorePoco>>, T1, Tuple<Action<T1>, SignalStorePoco>>, IAction<T1>> closurePoolHandle = Wrapper<T1>.WrapAction.Make().Close(Tuple.Create(action, this)).Pooled2();
		return Tuple.Create((IDisposable)closurePoolHandle, closurePoolHandle.Value.Action);
	}

	public Tuple<IDisposable, Action<T1, T2>> Wrap<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.ABase<T1, T2, Tuple<Action<T1, T2>, SignalStorePoco>>, T1, T2, Tuple<Action<T1, T2>, SignalStorePoco>>, T1, T2>, Closure.AClosed<Closure.ABase<T1, T2, Tuple<Action<T1, T2>, SignalStorePoco>>, T1, T2, Tuple<Action<T1, T2>, SignalStorePoco>>, IAction<T1, T2>> closurePoolHandle = Wrapper<T1, T2>.WrapAction.Make().Close(Tuple.Create(action, this)).Pooled2();
		return Tuple.Create((IDisposable)closurePoolHandle, closurePoolHandle.Value.Action);
	}

	public Tuple<IDisposable, Action<T1, T2, T3>> Wrap<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.ABase<T1, T2, T3, Tuple<Action<T1, T2, T3>, SignalStorePoco>>, T1, T2, T3, Tuple<Action<T1, T2, T3>, SignalStorePoco>>, T1, T2, T3>, Closure.AClosed<Closure.ABase<T1, T2, T3, Tuple<Action<T1, T2, T3>, SignalStorePoco>>, T1, T2, T3, Tuple<Action<T1, T2, T3>, SignalStorePoco>>, IAction<T1, T2, T3>> closurePoolHandle = Wrapper<T1, T2, T3>.WrapAction.Make().Close(Tuple.Create(action, this)).Pooled2();
		return Tuple.Create((IDisposable)closurePoolHandle, closurePoolHandle.Value.Action);
	}

	public Tuple<IDisposable, Action<T1, T2, T3, T4>> Wrap<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.ABase<T1, T2, T3, T4, Tuple<Action<T1, T2, T3, T4>, SignalStorePoco>>, T1, T2, T3, T4, Tuple<Action<T1, T2, T3, T4>, SignalStorePoco>>, T1, T2, T3, T4>, Closure.AClosed<Closure.ABase<T1, T2, T3, T4, Tuple<Action<T1, T2, T3, T4>, SignalStorePoco>>, T1, T2, T3, T4, Tuple<Action<T1, T2, T3, T4>, SignalStorePoco>>, IAction<T1, T2, T3, T4>> closurePoolHandle = Wrapper<T1, T2, T3, T4>.WrapAction.Make().Close(Tuple.Create(action, this)).Pooled2();
		return Tuple.Create((IDisposable)closurePoolHandle, closurePoolHandle.Value.Action);
	}

	private SignalConnection _Connect<TSignal, TAction>(TSignal signal, TAction action, Tuple<IDisposable, TAction> t) where TSignal : IConnectableSignalBase<TAction>
	{
		SignalConnection signalConnection = signal.Connect(t._2);
		ConnectionHandles.Add(Tuple.Create((object)signal, (object)action), Tuple.Create(t._1, signalConnection));
		return signalConnection;
	}

	private SignalConnection _ConnectOnce<TSignal, TAction>(TSignal signal, TAction action, Tuple<IDisposable, TAction> t) where TSignal : IConnectableOnceSignalBase<TAction>
	{
		SignalConnection signalConnection = signal.ConnectOnce(t._2);
		ConnectionHandles.Add(Tuple.Create((object)signal, (object)action), Tuple.Create(t._1, signalConnection));
		return signalConnection;
	}

	public SignalConnection Connect(IReadonlySignalBase<Action> signal, Action action)
	{
		return _Connect(signal, action, Wrap(signal, action));
	}

	public SignalConnection Connect<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		return _Connect(signal, action, Wrap(signal, action));
	}

	public SignalConnection Connect<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		return _Connect(signal, action, Wrap(signal, action));
	}

	public SignalConnection Connect<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		return _Connect(signal, action, Wrap(signal, action));
	}

	public SignalConnection Connect<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		return _Connect(signal, action, Wrap(signal, action));
	}

	public SignalConnection ConnectOnce(IReadonlySignalBase<Action> signal, Action action)
	{
		return _ConnectOnce(signal, action, Wrap(signal, action));
	}

	public SignalConnection ConnectOnce<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		return _ConnectOnce(signal, action, Wrap(signal, action));
	}

	public SignalConnection ConnectOnce<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		return _ConnectOnce(signal, action, Wrap(signal, action));
	}

	public SignalConnection ConnectOnce<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		return _ConnectOnce(signal, action, Wrap(signal, action));
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		return _ConnectOnce(signal, action, Wrap(signal, action));
	}

	public void Disconnect(IReadonlySignalBase<Action> signal, Action action)
	{
		_Disconnect(signal, action);
	}

	public void Disconnect<T1>(IReadonlySignalBase<Action<T1>> signal, Action<T1> action)
	{
		_Disconnect(signal, action);
	}

	public void Disconnect<T1, T2>(IReadonlySignalBase<Action<T1, T2>> signal, Action<T1, T2> action)
	{
		_Disconnect(signal, action);
	}

	public void Disconnect<T1, T2, T3>(IReadonlySignalBase<Action<T1, T2, T3>> signal, Action<T1, T2, T3> action)
	{
		_Disconnect(signal, action);
	}

	public void Disconnect<T1, T2, T3, T4>(IReadonlySignalBase<Action<T1, T2, T3, T4>> signal, Action<T1, T2, T3, T4> action)
	{
		_Disconnect(signal, action);
	}

	private void _Disconnect<TSignal, TAction>(TSignal signal, TAction action) where TAction : class
	{
		Dictionary<Tuple<object, object>, Tuple<IDisposable, SignalConnection>>.Enumerator enumerator = ConnectionHandles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Tuple<object, object> key = enumerator.Current.Key;
			if (signal.Equals(key._1) && action.Equals(key._2))
			{
				enumerator.Current.Value._2.Disconnect();
				enumerator.Current.Value._1.Dispose();
				ConnectionHandles.Remove(enumerator.Current.Key);
				break;
			}
		}
	}

	private void AddCall<TAction>(TAction action, Action wrappedAction)
	{
		ConnectAction(delegate
		{
			Delegate obj = (Delegate)(object)action;
			if (obj.Target != null || obj.Method.IsStatic)
			{
				wrappedAction();
			}
		});
	}

	public void Poll()
	{
		List<Action> list = ConnectedActions;
		ConnectedActions = CollectionPool.RequestList<Action>();
		List<IDisposable> list2 = PrivateHandles;
		PrivateHandles = CollectionPool.RequestList<IDisposable>();
		List<Action>.Enumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current();
		}
		List<IDisposable>.Enumerator enumerator2 = list2.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.Dispose();
		}
		CollectionPool.Release(ref list);
		CollectionPool.Release(ref list2);
	}

	public void Clear()
	{
		ConnectedActions.Clear();
		Dictionary<Tuple<object, object>, Tuple<IDisposable, SignalConnection>>.Enumerator enumerator = ConnectionHandles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value._2.Disconnect();
			enumerator.Current.Value._1.Dispose();
		}
		ConnectionHandles.Clear();
		List<IDisposable>.Enumerator enumerator2 = PrivateHandles.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			enumerator2.Current.Dispose();
		}
		PrivateHandles.Clear();
	}
}
