using Brix.Common;
using Brix.External.Signals;
using Brix.Utils.Closures;

namespace Motus.Behavior;

public class SignalCondition : AbstractSignalCondition<SignalCondition>
{
	internal IReadOnlySignal signal;

	public IClosure<IFunc<bool>> Filter
	{
		get
		{
			return _filterHandle as IClosure<IFunc<bool>>;
		}
		set
		{
			_filterHandle = value;
		}
	}

	protected override SignalConnection ConnectToSignal()
	{
		return signal.Connect(delegate(SignalCondition self)
		{
			self.MarkedAsInvoked();
		}, this);
	}

	private void MarkedAsInvoked()
	{
		if (Filter.Delegate.Invoke())
		{
			_invokedSuccessfully = true;
			_connection.Disconnect();
		}
	}

	public static SignalCondition Request(IReadOnlySignal s, IClosure<IFunc<bool>> filter)
	{
		SignalCondition signalCondition = PoolableObject<SignalCondition>.Request();
		signalCondition.signal = s;
		signalCondition.Filter = filter;
		return signalCondition;
	}

	public static SignalCondition<T1> Request<T1>(IReadOnlySignal<T1> s, IClosure<IFunc<T1, bool>> filter)
	{
		SignalCondition<T1> signalCondition = PoolableObject<SignalCondition<T1>>.Request();
		signalCondition.signal = s;
		signalCondition.Filter = filter;
		return signalCondition;
	}

	public static SignalCondition<T1, T2> Request<T1, T2>(IReadOnlySignal<T1, T2> s, IClosure<IFunc<T1, T2, bool>> filter)
	{
		SignalCondition<T1, T2> signalCondition = PoolableObject<SignalCondition<T1, T2>>.Request();
		signalCondition.signal = s;
		signalCondition.Filter = filter;
		return signalCondition;
	}

	public static SignalCondition<T1, T2, T3> Request<T1, T2, T3>(IReadOnlySignal<T1, T2, T3> s, IClosure<IFunc<T1, T2, T3, bool>> filter)
	{
		SignalCondition<T1, T2, T3> signalCondition = PoolableObject<SignalCondition<T1, T2, T3>>.Request();
		signalCondition.signal = s;
		signalCondition.Filter = filter;
		return signalCondition;
	}

	public static SignalCondition<T1, T2, T3, T4> Request<T1, T2, T3, T4>(IReadOnlySignal<T1, T2, T3, T4> s, IClosure<IFunc<T1, T2, T3, T4, bool>> filter)
	{
		SignalCondition<T1, T2, T3, T4> signalCondition = PoolableObject<SignalCondition<T1, T2, T3, T4>>.Request();
		signalCondition.signal = s;
		signalCondition.Filter = filter;
		return signalCondition;
	}
}
public class SignalCondition<T1> : AbstractSignalCondition<SignalCondition<T1>>
{
	internal IReadOnlySignal<T1> signal;

	public IClosure<IFunc<T1, bool>> Filter
	{
		get
		{
			return _filterHandle as IClosure<IFunc<T1, bool>>;
		}
		set
		{
			_filterHandle = value;
		}
	}

	protected override SignalConnection ConnectToSignal()
	{
		return signal.Connect(delegate(T1 t1, SignalCondition<T1> self)
		{
			self.MarkedAsInvoked(t1);
		}, this);
	}

	private void MarkedAsInvoked(T1 t1)
	{
		if (Filter.Delegate.Invoke(t1))
		{
			_invokedSuccessfully = true;
		}
	}
}
public class SignalCondition<T1, T2> : AbstractSignalCondition<SignalCondition<T1, T2>>
{
	internal IReadOnlySignal<T1, T2> signal;

	public IClosure<IFunc<T1, T2, bool>> Filter
	{
		get
		{
			return _filterHandle as IClosure<IFunc<T1, T2, bool>>;
		}
		set
		{
			_filterHandle = value;
		}
	}

	protected override SignalConnection ConnectToSignal()
	{
		return signal.Connect(delegate(T1 t1, T2 t2, SignalCondition<T1, T2> self)
		{
			self.MarkedAsInvoked(t1, t2);
		}, this);
	}

	private void MarkedAsInvoked(T1 t1, T2 t2)
	{
		if (Filter.Delegate.Invoke(t1, t2))
		{
			_invokedSuccessfully = true;
		}
	}
}
public class SignalCondition<T1, T2, T3> : AbstractSignalCondition<SignalCondition<T1, T2, T3>>
{
	internal IReadOnlySignal<T1, T2, T3> signal;

	public IClosure<IFunc<T1, T2, T3, bool>> Filter
	{
		get
		{
			return _filterHandle as IClosure<IFunc<T1, T2, T3, bool>>;
		}
		set
		{
			_filterHandle = value;
		}
	}

	protected override SignalConnection ConnectToSignal()
	{
		return signal.Connect(delegate(T1 t1, T2 t2, T3 t3, SignalCondition<T1, T2, T3> self)
		{
			self.MarkedAsInvoked(t1, t2, t3);
		}, this);
	}

	private void MarkedAsInvoked(T1 t1, T2 t2, T3 t3)
	{
		if (Filter.Delegate.Invoke(t1, t2, t3))
		{
			_invokedSuccessfully = true;
		}
	}
}
public class SignalCondition<T1, T2, T3, T4> : AbstractSignalCondition<SignalCondition<T1, T2, T3, T4>>
{
	internal IReadOnlySignal<T1, T2, T3, T4> signal;

	public IClosure<IFunc<T1, T2, T3, T4, bool>> Filter
	{
		get
		{
			return _filterHandle as IClosure<IFunc<T1, T2, T3, T4, bool>>;
		}
		set
		{
			_filterHandle = value;
		}
	}

	protected override SignalConnection ConnectToSignal()
	{
		return signal.Connect(delegate(T1 t1, T2 t2, T3 t3, T4 t4, SignalCondition<T1, T2, T3, T4> self)
		{
			self.MarkedAsInvoked(t1, t2, t3, t4);
		}, this);
	}

	private void MarkedAsInvoked(T1 t1, T2 t2, T3 t3, T4 t4)
	{
		if (Filter.Delegate.Invoke(t1, t2, t3, t4))
		{
			_invokedSuccessfully = true;
		}
	}
}
