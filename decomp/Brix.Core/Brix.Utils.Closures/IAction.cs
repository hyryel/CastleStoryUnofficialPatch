using System;
using Brix.External.Signals;

namespace Brix.Utils.Closures;

public interface IAction : IActionBase<Action>
{
	void Invoke();
}
public interface IAction<T1> : IActionBase<Action<T1>>
{
	void Invoke(T1 t1);
}
public interface IAction<T1, T2> : IActionBase<Action<T1, T2>>
{
	void Invoke(T1 t1, T2 t2);
}
public interface IAction<T1, T2, T3> : IActionBase<Action<T1, T2, T3>>
{
	void Invoke(T1 t1, T2 t2, T3 t3);
}
public interface IAction<T1, T2, T3, T4> : IActionBase<Action<T1, T2, T3, T4>>
{
	void Invoke(T1 t1, T2 t2, T3 t3, T4 t4);
}
public interface IAction<T1, T2, T3, T4, T5> : IActionBase<Action<T1, T2, T3, T4, T5>>
{
	void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}
