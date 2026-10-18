using System;
using Brix.Utils.Closures;
using Brix.Utils.Logging;

namespace Brix.External.Signals;

public class Signal : SignalBase<IClosure<IAction>>, ISignal, IReadOnlySignal, IReadonlySignalBase<Action>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action>, IConnectableOnceSignalBase<Action>, IDisconnectableSignal, IConnectableOnceSignal, IConnectableSignal, IInvokableSignal, IAction, IActionBase<Action>
{
	private static BrixLogger logger = new BrixLogger();

	public void Invoke()
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		try
		{
			while (signalInvokeListEnumerator.MoveNext())
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke();
			}
		}
		finally
		{
			signalInvokeListEnumerator.Dispose();
		}
	}

	public void SafeInvoke()
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		while (signalInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke();
			}
			catch (Exception e)
			{
				logger.ErrorFormat(e, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalInvokeListEnumerator.Dispose();
	}

	public void UnorderedInvoke()
	{
		SignalUnorderedInvokeListEnumerator signalUnorderedInvokeListEnumerator = new SignalUnorderedInvokeListEnumerator(this);
		try
		{
			while (signalUnorderedInvokeListEnumerator.MoveNext())
			{
				signalUnorderedInvokeListEnumerator.Current.Delegate.Invoke();
			}
		}
		finally
		{
			signalUnorderedInvokeListEnumerator.Dispose();
		}
	}

	public void UnorderedSafeInvoke()
	{
		SignalUnorderedInvokeListEnumerator signalUnorderedInvokeListEnumerator = new SignalUnorderedInvokeListEnumerator(this);
		while (signalUnorderedInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalUnorderedInvokeListEnumerator.Current.Delegate.Invoke();
			}
			catch (Exception e)
			{
				logger.ErrorFormat(e, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalUnorderedInvokeListEnumerator.Dispose();
	}

	public SignalConnection Connect(Action action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection Connect<T1>(Action<T1> action, T1 t1)
	{
		return Connect(action.Make().Close(t1).Pooled2());
	}

	public SignalConnection Connect<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
	{
		return Connect(action.Make().Close(t1, t2).Pooled2());
	}

	public SignalConnection Connect<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
	{
		return Connect(action.Make().Close(t1, t2, t3).Pooled2());
	}

	public SignalConnection Connect<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return Connect(action.Make().Close(t1, t2, t3, t4).Pooled2());
	}

	public SignalConnection Connect<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Connect(action.Make().Close(t1, t2, t3, t4, t5).Pooled2());
	}

	public SignalConnection ConnectOnce(Action action)
	{
		return ConnectOnce(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce<T1>(Action<T1> action, T1 t1)
	{
		return ConnectOnce(action.Make().Close(t1).Pooled2());
	}

	public SignalConnection ConnectOnce<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
	{
		return ConnectOnce(action.Make().Close(t1, t2).Pooled2());
	}

	public SignalConnection ConnectOnce<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
	{
		return ConnectOnce(action.Make().Close(t1, t2, t3).Pooled2());
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
	{
		return ConnectOnce(action.Make().Close(t1, t2, t3, t4).Pooled2());
	}

	public SignalConnection ConnectOnce<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return ConnectOnce(action.Make().Close(t1, t2, t3, t4, t5).Pooled2());
	}
}
public class Signal<T1> : SignalBase<IClosure<IAction<T1>>>, ISignal<T1>, IReadOnlySignal<T1>, IReadonlySignalBase<Action<T1>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1>>, IConnectableOnceSignalBase<Action<T1>>, IDisconnectableSignal, IConnectableOnceSignal<T1>, IConnectableSignal<T1>, IInvokableSignal<T1>, IAction<T1>, IActionBase<Action<T1>>
{
	private static BrixLogger logger = new BrixLogger();

	public void Invoke(T1 a)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		try
		{
			while (signalInvokeListEnumerator.MoveNext())
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a);
			}
		}
		finally
		{
			signalInvokeListEnumerator.Dispose();
		}
	}

	public void SafeInvoke(T1 a)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		while (signalInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a);
			}
			catch (Exception e)
			{
				logger.ErrorFormat(e, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalInvokeListEnumerator.Dispose();
	}

	public SignalConnection Connect(Action<T1> action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection Connect<T2>(Action<T1, T2> action, T2 t2)
	{
		return Connect(action.Make().Close(t2).Pooled2());
	}

	public SignalConnection Connect<T2, T3>(Action<T1, T2, T3> action, T2 t2, T3 t3)
	{
		return Connect(action.Make().Close(t2, t3).Pooled2());
	}

	public SignalConnection Connect<T2, T3, T4>(Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4)
	{
		return Connect(action.Make().Close(t2, t3, t4).Pooled2());
	}

	public SignalConnection Connect<T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return Connect(action.Make().Close(t2, t3, t4, t5).Pooled2());
	}

	public SignalConnection ConnectOnce(Action<T1> action)
	{
		return ConnectOnce(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce<T2>(Action<T1, T2> action, T2 t2)
	{
		return ConnectOnce(action.Make().Close(t2).Pooled2());
	}

	public SignalConnection ConnectOnce<T2, T3>(Action<T1, T2, T3> action, T2 t2, T3 t3)
	{
		return ConnectOnce(action.Make().Close(t2, t3).Pooled2());
	}

	public SignalConnection ConnectOnce<T2, T3, T4>(Action<T1, T2, T3, T4> action, T2 t2, T3 t3, T4 t4)
	{
		return ConnectOnce(action.Make().Close(t2, t3, t4).Pooled2());
	}

	public SignalConnection ConnectOnce<T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T2 t2, T3 t3, T4 t4, T5 t5)
	{
		return ConnectOnce(action.Make().Close(t2, t3, t4, t5).Pooled2());
	}
}
public class Signal<T1, T2> : SignalBase<IClosure<IAction<T1, T2>>>, ISignal<T1, T2>, IReadOnlySignal<T1, T2>, IReadonlySignalBase<Action<T1, T2>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2>>, IConnectableOnceSignalBase<Action<T1, T2>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2>, IConnectableSignal<T1, T2>, IInvokableSignal<T1, T2>, IAction<T1, T2>, IActionBase<Action<T1, T2>>
{
	private static BrixLogger logger = new BrixLogger();

	public void Invoke(T1 a, T2 b)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		try
		{
			while (signalInvokeListEnumerator.MoveNext())
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b);
			}
		}
		finally
		{
			signalInvokeListEnumerator.Dispose();
		}
	}

	public void SafeInvoke(T1 a, T2 b)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		while (signalInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b);
			}
			catch (Exception e)
			{
				logger.ErrorFormat(e, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalInvokeListEnumerator.Dispose();
	}

	public SignalConnection Connect(Action<T1, T2> action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection Connect<T3>(Action<T1, T2, T3> action, T3 t3)
	{
		return Connect(action.Make().Close(t3).Pooled2());
	}

	public SignalConnection Connect<T3, T4>(Action<T1, T2, T3, T4> action, T3 t3, T4 t4)
	{
		return Connect(action.Make().Close(t3, t4).Pooled2());
	}

	public SignalConnection Connect<T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T3 t3, T4 t4, T5 t5)
	{
		return Connect(action.Make().Close(t3, t4, t5).Pooled2());
	}

	public SignalConnection ConnectOnce(Action<T1, T2> action)
	{
		return ConnectOnce(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce<T3>(Action<T1, T2, T3> action, T3 t3)
	{
		return ConnectOnce(action.Make().Close(t3).Pooled2());
	}

	public SignalConnection ConnectOnce<T3, T4>(Action<T1, T2, T3, T4> action, T3 t3, T4 t4)
	{
		return ConnectOnce(action.Make().Close(t3, t4).Pooled2());
	}

	public SignalConnection ConnectOnce<T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T3 t3, T4 t4, T5 t5)
	{
		return ConnectOnce(action.Make().Close(t3, t4, t5).Pooled2());
	}
}
public class Signal<T1, T2, T3> : SignalBase<IClosure<IAction<T1, T2, T3>>>, ISignal<T1, T2, T3>, IReadOnlySignal<T1, T2, T3>, IReadonlySignalBase<Action<T1, T2, T3>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3>>, IConnectableOnceSignalBase<Action<T1, T2, T3>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3>, IConnectableSignal<T1, T2, T3>, IInvokableSignal<T1, T2, T3>, IAction<T1, T2, T3>, IActionBase<Action<T1, T2, T3>>
{
	private static BrixLogger logger = new BrixLogger();

	public void Invoke(T1 a, T2 b, T3 c)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		try
		{
			while (signalInvokeListEnumerator.MoveNext())
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b, c);
			}
		}
		finally
		{
			signalInvokeListEnumerator.Dispose();
		}
	}

	public void SafeInvoke(T1 a, T2 b, T3 c)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		while (signalInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b, c);
			}
			catch (Exception e)
			{
				logger.ErrorFormat(e, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalInvokeListEnumerator.Dispose();
	}

	public SignalConnection Connect(Action<T1, T2, T3> action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection Connect<T4>(Action<T1, T2, T3, T4> action, T4 t4)
	{
		return Connect(action.Make().Close(t4).Pooled2());
	}

	public SignalConnection Connect<T4, T5>(Action<T1, T2, T3, T4, T5> action, T4 t4, T5 t5)
	{
		return Connect(action.Make().Close(t4, t5).Pooled2());
	}

	public SignalConnection ConnectOnce(Action<T1, T2, T3> action)
	{
		return ConnectOnce(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce<T4>(Action<T1, T2, T3, T4> action, T4 t4)
	{
		return ConnectOnce(action.Make().Close(t4).Pooled2());
	}

	public SignalConnection ConnectOnce<T4, T5>(Action<T1, T2, T3, T4, T5> action, T4 t4, T5 t5)
	{
		return ConnectOnce(action.Make().Close(t4, t5).Pooled2());
	}
}
public class Signal<T1, T2, T3, T4> : SignalBase<IClosure<IAction<T1, T2, T3, T4>>>, ISignal<T1, T2, T3, T4>, IReadOnlySignal<T1, T2, T3, T4>, IReadonlySignalBase<Action<T1, T2, T3, T4>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3, T4>>, IConnectableOnceSignalBase<Action<T1, T2, T3, T4>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3, T4>, IConnectableSignal<T1, T2, T3, T4>, IInvokableSignal<T1, T2, T3, T4>, IAction<T1, T2, T3, T4>, IActionBase<Action<T1, T2, T3, T4>>
{
	private static BrixLogger logger = new BrixLogger();

	public void Invoke(T1 a, T2 b, T3 c, T4 d)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		try
		{
			while (signalInvokeListEnumerator.MoveNext())
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b, c, d);
			}
		}
		finally
		{
			signalInvokeListEnumerator.Dispose();
		}
	}

	public void SafeInvoke(T1 a, T2 b, T3 c, T4 d)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		while (signalInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b, c, d);
			}
			catch (Exception e)
			{
				logger.ErrorFormat(e, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalInvokeListEnumerator.Dispose();
	}

	public SignalConnection Connect(Action<T1, T2, T3, T4> action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection Connect<T5>(Action<T1, T2, T3, T4, T5> action, T5 t5)
	{
		return Connect(action.Make().Close(t5).Pooled2());
	}

	public SignalConnection ConnectOnce(Action<T1, T2, T3, T4> action)
	{
		return ConnectOnce(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce<T5>(Action<T1, T2, T3, T4, T5> action, T5 t5)
	{
		return ConnectOnce(action.Make().Close(t5).Pooled2());
	}
}
public class Signal<T1, T2, T3, T4, T5> : SignalBase<IClosure<IAction<T1, T2, T3, T4, T5>>>, ISignal<T1, T2, T3, T4, T5>, IReadOnlySignal<T1, T2, T3, T4, T5>, IReadonlySignalBase<Action<T1, T2, T3, T4, T5>>, IClearableSignal, SignalInterface, IConnectableSignalBase<Action<T1, T2, T3, T4, T5>>, IConnectableOnceSignalBase<Action<T1, T2, T3, T4, T5>>, IDisconnectableSignal, IConnectableOnceSignal<T1, T2, T3, T4, T5>, IConnectableSignal<T1, T2, T3, T4, T5>, IInvokableSignal<T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4, T5>, IActionBase<Action<T1, T2, T3, T4, T5>>
{
	private static BrixLogger logger = new BrixLogger();

	public void Invoke(T1 a, T2 b, T3 c, T4 d, T5 e)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		try
		{
			while (signalInvokeListEnumerator.MoveNext())
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b, c, d, e);
			}
		}
		finally
		{
			signalInvokeListEnumerator.Dispose();
		}
	}

	public void SafeInvoke(T1 a, T2 b, T3 c, T4 d, T5 e)
	{
		SignalInvokeListEnumerator signalInvokeListEnumerator = new SignalInvokeListEnumerator(this);
		while (signalInvokeListEnumerator.MoveNext())
		{
			try
			{
				signalInvokeListEnumerator.Current.Delegate.Invoke(a, b, c, d, e);
			}
			catch (Exception e2)
			{
				logger.ErrorFormat(e2, "{0}.SafeInvoke", GetType().Name);
			}
		}
		signalInvokeListEnumerator.Dispose();
	}

	public SignalConnection Connect(Action<T1, T2, T3, T4, T5> action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce(Action<T1, T2, T3, T4, T5> action)
	{
		return ConnectOnce(action.Make().Pooled2());
	}
}
