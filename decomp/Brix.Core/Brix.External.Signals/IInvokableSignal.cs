namespace Brix.External.Signals;

public interface IInvokableSignal
{
	void Invoke();
}
public interface IInvokableSignal<T1>
{
	void Invoke(T1 t1);
}
public interface IInvokableSignal<T1, T2>
{
	void Invoke(T1 t1, T2 t2);
}
public interface IInvokableSignal<T1, T2, T3>
{
	void Invoke(T1 t1, T2 t2, T3 t3);
}
public interface IInvokableSignal<T1, T2, T3, T4>
{
	void Invoke(T1 t1, T2 t2, T3 t3, T4 t4);
}
public interface IInvokableSignal<T1, T2, T3, T4, T5>
{
	void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}
