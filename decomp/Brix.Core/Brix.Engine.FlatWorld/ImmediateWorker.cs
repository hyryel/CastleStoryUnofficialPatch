using System;
using System.Collections;

namespace Brix.Engine.FlatWorld;

public class ImmediateWorker : IWorker
{
	public void AddWork(IEnumerator work, bool lifo = false)
	{
		while (work.MoveNext())
		{
		}
	}

	public void AddWork(Action work)
	{
		work();
	}

	public void AddWork<T1>(Action<T1> work, T1 p1)
	{
		work(p1);
	}

	public void AddWork<T1, T2>(Action<T1, T2> work, T1 p1, T2 p2)
	{
		work(p1, p2);
	}

	public void AddWork<T1, T2, T3>(Action<T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		work(p1, p2, p3);
	}

	public void AddWork<T1, T2, T3, T4>(Action<T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		work(p1, p2, p3, p4);
	}

	public void AddWork<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		work(p1, p2, p3, p4, p5);
	}

	public void AddWork(Action<IWorker> work)
	{
		work(this);
	}

	public void AddWork<T1>(Action<IWorker, T1> work, T1 p1)
	{
		work(this, p1);
	}

	public void AddWork<T1, T2>(Action<IWorker, T1, T2> work, T1 p1, T2 p2)
	{
		work(this, p1, p2);
	}

	public void AddWork<T1, T2, T3>(Action<IWorker, T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		work(this, p1, p2, p3);
	}

	public void AddWork<T1, T2, T3, T4>(Action<IWorker, T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		work(this, p1, p2, p3, p4);
	}

	public void AddWork<T1, T2, T3, T4, T5>(Action<IWorker, T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		work(this, p1, p2, p3, p4, p5);
	}

	public void FinishAllWork()
	{
	}

	public void Clear()
	{
	}
}
