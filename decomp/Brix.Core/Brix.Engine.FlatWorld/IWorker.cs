using System;
using System.Collections;

namespace Brix.Engine.FlatWorld;

public interface IWorker
{
	void AddWork(IEnumerator work, bool lifo = false);

	void AddWork(Action work);

	void AddWork<T1>(Action<T1> work, T1 p1);

	void AddWork<T1, T2>(Action<T1, T2> work, T1 p1, T2 p2);

	void AddWork<T1, T2, T3>(Action<T1, T2, T3> work, T1 p1, T2 p2, T3 p3);

	void AddWork<T1, T2, T3, T4>(Action<T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4);

	void AddWork<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);

	void AddWork(Action<IWorker> work);

	void AddWork<T1>(Action<IWorker, T1> work, T1 p1);

	void AddWork<T1, T2>(Action<IWorker, T1, T2> work, T1 p1, T2 p2);

	void AddWork<T1, T2, T3>(Action<IWorker, T1, T2, T3> work, T1 p1, T2 p2, T3 p3);

	void AddWork<T1, T2, T3, T4>(Action<IWorker, T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4);

	void AddWork<T1, T2, T3, T4, T5>(Action<IWorker, T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);

	void FinishAllWork();

	void Clear();
}
