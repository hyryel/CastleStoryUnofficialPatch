using System;
using Smooth.Algebraics;

namespace Brix.External.Signals;

public class Wrapper : IWrapper<EmptyTuple, Action>
{
	public Action<EmptyTuple> Wrap(Action action)
	{
		return delegate
		{
			action();
		};
	}

	public Action Unwrap(Action<EmptyTuple> wrappedAction)
	{
		return delegate
		{
			wrappedAction(default(EmptyTuple));
		};
	}
}
public class Wrapper<T1> : IWrapper<Tuple<T1>, Action<T1>>
{
	public Action<Tuple<T1>> Wrap(Action<T1> action)
	{
		return delegate(Tuple<T1> t)
		{
			action(t._1);
		};
	}

	public Action<T1> Unwrap(Action<Tuple<T1>> wrappedAction)
	{
		return delegate(T1 a)
		{
			wrappedAction(Tuple.Create(a));
		};
	}
}
public class Wrapper<T1, T2> : IWrapper<Tuple<T1, T2>, Action<T1, T2>>
{
	public Action<Tuple<T1, T2>> Wrap(Action<T1, T2> action)
	{
		return delegate(Tuple<T1, T2> t)
		{
			action(t._1, t._2);
		};
	}

	public Action<T1, T2> Unwrap(Action<Tuple<T1, T2>> wrappedAction)
	{
		return delegate(T1 a, T2 b)
		{
			wrappedAction(Tuple.Create(a, b));
		};
	}
}
public class Wrapper<T1, T2, T3> : IWrapper<Tuple<T1, T2, T3>, Action<T1, T2, T3>>
{
	public Action<Tuple<T1, T2, T3>> Wrap(Action<T1, T2, T3> action)
	{
		return delegate(Tuple<T1, T2, T3> t)
		{
			action(t._1, t._2, t._3);
		};
	}

	public Action<T1, T2, T3> Unwrap(Action<Tuple<T1, T2, T3>> wrappedAction)
	{
		return delegate(T1 a, T2 b, T3 c)
		{
			wrappedAction(Tuple.Create(a, b, c));
		};
	}
}
public class Wrapper<T1, T2, T3, T4> : IWrapper<Tuple<T1, T2, T3, T4>, Action<T1, T2, T3, T4>>
{
	public Action<Tuple<T1, T2, T3, T4>> Wrap(Action<T1, T2, T3, T4> action)
	{
		return delegate(Tuple<T1, T2, T3, T4> t)
		{
			action(t._1, t._2, t._3, t._4);
		};
	}

	public Action<T1, T2, T3, T4> Unwrap(Action<Tuple<T1, T2, T3, T4>> wrappedAction)
	{
		return delegate(T1 a, T2 b, T3 c, T4 d)
		{
			wrappedAction(Tuple.Create(a, b, c, d));
		};
	}
}
public class Wrapper<T1, T2, T3, T4, T5> : IWrapper<Tuple<T1, T2, T3, T4, T5>, Action<T1, T2, T3, T4, T5>>
{
	public Action<Tuple<T1, T2, T3, T4, T5>> Wrap(Action<T1, T2, T3, T4, T5> action)
	{
		return delegate(Tuple<T1, T2, T3, T4, T5> t)
		{
			action(t._1, t._2, t._3, t._4, t._5);
		};
	}

	public Action<T1, T2, T3, T4, T5> Unwrap(Action<Tuple<T1, T2, T3, T4, T5>> wrappedAction)
	{
		return delegate(T1 a, T2 b, T3 c, T4 d, T5 e)
		{
			wrappedAction(Tuple.Create(a, b, c, d, e));
		};
	}
}
