using System;
using Brix.External.Signals;

namespace Brix.Utils.Closures;

public interface IFunc<TReturn> : IFuncBase<Func<TReturn>>
{
	TReturn Invoke();
}
public interface IFunc<T1, TReturn> : IFuncBase<Func<T1, TReturn>>
{
	TReturn Invoke(T1 t1);
}
public interface IFunc<T1, T2, TReturn> : IFuncBase<Func<T1, T2, TReturn>>
{
	TReturn Invoke(T1 t1, T2 t2);
}
public interface IFunc<T1, T2, T3, TReturn> : IFuncBase<Func<T1, T2, T3, TReturn>>
{
	TReturn Invoke(T1 t1, T2 t2, T3 t3);
}
public interface IFunc<T1, T2, T3, T4, TReturn> : IFuncBase<Func<T1, T2, T3, T4, TReturn>>
{
	TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4);
}
public interface IFunc<T1, T2, T3, T4, T5, TReturn> : IFuncBase<Func<T1, T2, T3, T4, T5, TReturn>>
{
	TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}
