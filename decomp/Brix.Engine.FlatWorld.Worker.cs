using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class Worker : MonoBehaviour, IWorker
{
	public abstract class WorkLoadBase
	{
		public abstract bool MoveNext(IWorker worker);
	}

	public abstract class WorkLoadRecursiveBase : WorkLoadBase
	{
	}

	public class WorkLoad : WorkLoadBase
	{
		public Action work;

		public override bool MoveNext(IWorker worker)
		{
			work();
			return false;
		}
	}

	public class WorkLoad<T1> : WorkLoadBase
	{
		public Action<T1> work;

		public T1 p1;

		public override bool MoveNext(IWorker worker)
		{
			work(p1);
			return false;
		}
	}

	public class WorkLoad<T1, T2> : WorkLoadBase
	{
		public Action<T1, T2> work;

		public T1 p1;

		public T2 p2;

		public override bool MoveNext(IWorker worker)
		{
			work(p1, p2);
			return false;
		}
	}

	public class WorkLoad<T1, T2, T3> : WorkLoadBase
	{
		public Action<T1, T2, T3> work;

		public T1 p1;

		public T2 p2;

		public T3 p3;

		public override bool MoveNext(IWorker worker)
		{
			work(p1, p2, p3);
			return false;
		}
	}

	public class WorkLoad<T1, T2, T3, T4> : WorkLoadBase
	{
		public Action<T1, T2, T3, T4> work;

		public T1 p1;

		public T2 p2;

		public T3 p3;

		public T4 p4;

		public override bool MoveNext(IWorker worker)
		{
			work(p1, p2, p3, p4);
			return false;
		}
	}

	public class WorkLoad<T1, T2, T3, T4, T5> : WorkLoadBase
	{
		public Action<T1, T2, T3, T4, T5> work;

		public T1 p1;

		public T2 p2;

		public T3 p3;

		public T4 p4;

		public T5 p5;

		public override bool MoveNext(IWorker worker)
		{
			work(p1, p2, p3, p4, p5);
			return false;
		}
	}

	public class WorkLoadRecursion : WorkLoadRecursiveBase
	{
		public Action<IWorker> work;

		public override bool MoveNext(IWorker worker)
		{
			work(worker);
			return false;
		}
	}

	public class WorkLoadRecursion<T1> : WorkLoadRecursiveBase
	{
		public Action<IWorker, T1> work;

		public T1 p1;

		public override bool MoveNext(IWorker worker)
		{
			work(worker, p1);
			return false;
		}
	}

	public class WorkLoadRecursion<T1, T2> : WorkLoadRecursiveBase
	{
		public Action<IWorker, T1, T2> work;

		public T1 p1;

		public T2 p2;

		public override bool MoveNext(IWorker worker)
		{
			work(worker, p1, p2);
			return false;
		}
	}

	public class WorkLoadRecursion<T1, T2, T3> : WorkLoadRecursiveBase
	{
		public Action<IWorker, T1, T2, T3> work;

		public T1 p1;

		public T2 p2;

		public T3 p3;

		public override bool MoveNext(IWorker worker)
		{
			work(worker, p1, p2, p3);
			return false;
		}
	}

	public class WorkLoadRecursion<T1, T2, T3, T4> : WorkLoadRecursiveBase
	{
		public Action<IWorker, T1, T2, T3, T4> work;

		public T1 p1;

		public T2 p2;

		public T3 p3;

		public T4 p4;

		public override bool MoveNext(IWorker worker)
		{
			work(worker, p1, p2, p3, p4);
			return false;
		}
	}

	public class WorkLoadRecursion<T1, T2, T3, T4, T5> : WorkLoadRecursiveBase
	{
		public Action<IWorker, T1, T2, T3, T4, T5> work;

		public T1 p1;

		public T2 p2;

		public T3 p3;

		public T4 p4;

		public T5 p5;

		public override bool MoveNext(IWorker worker)
		{
			work(worker, p1, p2, p3, p4, p5);
			return false;
		}
	}

	public class WorkLoadEnumerator : WorkLoadBase
	{
		public IEnumerator enumerator;

		public override bool MoveNext(IWorker worker)
		{
			return enumerator.MoveNext();
		}
	}

	public class InternalWorker : IWorker
	{
		public Queue<WorkLoadBase> q = new Queue<WorkLoadBase>();

		public void AddWork(IEnumerator work, bool lifo = false)
		{
			if (lifo)
			{
				if (work.MoveNext())
				{
					throw new NotImplementedException();
				}
			}
			else
			{
				Enqueue(new WorkLoadEnumerator
				{
					enumerator = work
				});
			}
		}

		public void AddWork(Action work)
		{
			Enqueue(ActionAsWork(work));
		}

		public void AddWork<T1>(Action<T1> work, T1 p1)
		{
			Enqueue(ActionAsWork(work, p1));
		}

		public void AddWork<T1, T2>(Action<T1, T2> work, T1 p1, T2 p2)
		{
			Enqueue(ActionAsWork(work, p1, p2));
		}

		public void AddWork<T1, T2, T3>(Action<T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
		{
			Enqueue(ActionAsWork(work, p1, p2, p3));
		}

		public void AddWork<T1, T2, T3, T4>(Action<T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			Enqueue(ActionAsWork(work, p1, p2, p3, p4));
		}

		public void AddWork<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			Enqueue(ActionAsWork(work, p1, p2, p3, p4, p5));
		}

		public void AddWork(Action<IWorker> work)
		{
			Enqueue(ActionAsWork(work));
		}

		public void AddWork<T1>(Action<IWorker, T1> work, T1 p1)
		{
			Enqueue(ActionAsWork(work, p1));
		}

		public void AddWork<T1, T2>(Action<IWorker, T1, T2> work, T1 p1, T2 p2)
		{
			Enqueue(ActionAsWork(work, p1, p2));
		}

		public void AddWork<T1, T2, T3>(Action<IWorker, T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
		{
			Enqueue(ActionAsWork(work, p1, p2, p3));
		}

		public void AddWork<T1, T2, T3, T4>(Action<IWorker, T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			Enqueue(ActionAsWork(work, p1, p2, p3, p4));
		}

		public void AddWork<T1, T2, T3, T4, T5>(Action<IWorker, T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
		{
			Enqueue(ActionAsWork(work, p1, p2, p3, p4, p5));
		}

		public void FinishAllWork()
		{
			while (MoveNext())
			{
			}
		}

		public void Clear()
		{
			q.Clear();
		}

		public bool HasWork()
		{
			return q.Count > 0;
		}

		private void Enqueue(WorkLoadBase work)
		{
			q.Enqueue(work);
		}

		public bool MoveNext()
		{
			if (q.Count > 0)
			{
				WorkLoadBase workLoadBase = q.Peek();
				try
				{
					if (!workLoadBase.MoveNext(this))
					{
						q.Dequeue();
					}
					return true;
				}
				catch (Exception ex)
				{
					q.Dequeue();
					throw ex;
				}
			}
			return false;
		}
	}

	public float WorkTime = 0.02f;

	public static ImmediateWorker Immediate = new ImmediateWorker();

	private InternalWorker _internalWorker = new InternalWorker();

	public static void AddWork(IWorker worker, IEnumerator work, bool lifo = false)
	{
		(worker ?? Immediate).AddWork(work, lifo);
	}

	public static void AddWork(IWorker worker, Action work)
	{
		(worker ?? Immediate).AddWork(work);
	}

	public static void AddWork<T1>(IWorker worker, Action<T1> work, T1 p1)
	{
		(worker ?? Immediate).AddWork(work, p1);
	}

	public static void AddWork<T1, T2>(IWorker worker, Action<T1, T2> work, T1 p1, T2 p2)
	{
		(worker ?? Immediate).AddWork(work, p1, p2);
	}

	public static void AddWork<T1, T2, T3>(IWorker worker, Action<T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		(worker ?? Immediate).AddWork(work, p1, p2, p3);
	}

	public static void AddWork<T1, T2, T3, T4>(IWorker worker, Action<T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		(worker ?? Immediate).AddWork(work, p1, p2, p3, p4);
	}

	public static void AddWork<T1, T2, T3, T4, T5>(IWorker worker, Action<T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		(worker ?? Immediate).AddWork(work, p1, p2, p3, p4, p5);
	}

	public static void AddWork(IWorker worker, Action<IWorker> work)
	{
		(worker ?? Immediate).AddWork(work);
	}

	public static void AddWork<T1>(IWorker worker, Action<IWorker, T1> work, T1 p1)
	{
		(worker ?? Immediate).AddWork(work, p1);
	}

	public static void AddWork<T1, T2>(IWorker worker, Action<IWorker, T1, T2> work, T1 p1, T2 p2)
	{
		(worker ?? Immediate).AddWork(work, p1, p2);
	}

	public static void AddWork<T1, T2, T3>(IWorker worker, Action<IWorker, T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		(worker ?? Immediate).AddWork(work, p1, p2, p3);
	}

	public static void AddWork<T1, T2, T3, T4>(IWorker worker, Action<IWorker, T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		(worker ?? Immediate).AddWork(work, p1, p2, p3, p4);
	}

	public static void AddWork<T1, T2, T3, T4, T5>(IWorker worker, Action<IWorker, T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		(worker ?? Immediate).AddWork(work, p1, p2, p3, p4, p5);
	}

	public static void FinishAllWork(IWorker worker)
	{
		(worker ?? Immediate).FinishAllWork();
	}

	public static void Clear(IWorker worker)
	{
		(worker ?? Immediate).Clear();
	}

	public void AddWork(IEnumerator work, bool lifo = false)
	{
		_internalWorker.AddWork(work, lifo);
	}

	public void AddWork(Action work)
	{
		_internalWorker.AddWork(work);
	}

	public void AddWork<T1>(Action<T1> work, T1 p1)
	{
		_internalWorker.AddWork(work, p1);
	}

	public void AddWork<T1, T2>(Action<T1, T2> work, T1 p1, T2 p2)
	{
		_internalWorker.AddWork(work, p1, p2);
	}

	public void AddWork<T1, T2, T3>(Action<T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		_internalWorker.AddWork(work, p1, p2, p3);
	}

	public void AddWork<T1, T2, T3, T4>(Action<T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		_internalWorker.AddWork(work, p1, p2, p3, p4);
	}

	public void AddWork<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		_internalWorker.AddWork(work, p1, p2, p3, p4, p5);
	}

	public void AddWork(Action<IWorker> work)
	{
		_internalWorker.AddWork(work);
	}

	public void AddWork<T1>(Action<IWorker, T1> work, T1 p1)
	{
		_internalWorker.AddWork(work, p1);
	}

	public void AddWork<T1, T2>(Action<IWorker, T1, T2> work, T1 p1, T2 p2)
	{
		_internalWorker.AddWork(work, p1, p2);
	}

	public void AddWork<T1, T2, T3>(Action<IWorker, T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		_internalWorker.AddWork(work, p1, p2, p3);
	}

	public void AddWork<T1, T2, T3, T4>(Action<IWorker, T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		_internalWorker.AddWork(work, p1, p2, p3, p4);
	}

	public void AddWork<T1, T2, T3, T4, T5>(Action<IWorker, T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		_internalWorker.AddWork(work, p1, p2, p3, p4, p5);
	}

	public void FinishAllWork()
	{
		_internalWorker.FinishAllWork();
	}

	public void Clear()
	{
		_internalWorker.Clear();
	}

	public bool HasWork()
	{
		return _internalWorker.HasWork();
	}

	public static WorkLoad ActionAsWork(Action work)
	{
		return new WorkLoad
		{
			work = work
		};
	}

	public static WorkLoad<T1> ActionAsWork<T1>(Action<T1> work, T1 p1)
	{
		return new WorkLoad<T1>
		{
			work = work,
			p1 = p1
		};
	}

	public static WorkLoad<T1, T2> ActionAsWork<T1, T2>(Action<T1, T2> work, T1 p1, T2 p2)
	{
		return new WorkLoad<T1, T2>
		{
			work = work,
			p1 = p1,
			p2 = p2
		};
	}

	public static WorkLoad<T1, T2, T3> ActionAsWork<T1, T2, T3>(Action<T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		return new WorkLoad<T1, T2, T3>
		{
			work = work,
			p1 = p1,
			p2 = p2,
			p3 = p3
		};
	}

	public static WorkLoad<T1, T2, T3, T4> ActionAsWork<T1, T2, T3, T4>(Action<T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		return new WorkLoad<T1, T2, T3, T4>
		{
			work = work,
			p1 = p1,
			p2 = p2,
			p3 = p3,
			p4 = p4
		};
	}

	public static WorkLoad<T1, T2, T3, T4, T5> ActionAsWork<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		return new WorkLoad<T1, T2, T3, T4, T5>
		{
			work = work,
			p1 = p1,
			p2 = p2,
			p3 = p3,
			p4 = p4,
			p5 = p5
		};
	}

	public static WorkLoadRecursion ActionAsWork(Action<IWorker> work)
	{
		return new WorkLoadRecursion
		{
			work = work
		};
	}

	public static WorkLoadRecursion<T1> ActionAsWork<T1>(Action<IWorker, T1> work, T1 p1)
	{
		return new WorkLoadRecursion<T1>
		{
			work = work,
			p1 = p1
		};
	}

	public static WorkLoadRecursion<T1, T2> ActionAsWork<T1, T2>(Action<IWorker, T1, T2> work, T1 p1, T2 p2)
	{
		return new WorkLoadRecursion<T1, T2>
		{
			work = work,
			p1 = p1,
			p2 = p2
		};
	}

	public static WorkLoadRecursion<T1, T2, T3> ActionAsWork<T1, T2, T3>(Action<IWorker, T1, T2, T3> work, T1 p1, T2 p2, T3 p3)
	{
		return new WorkLoadRecursion<T1, T2, T3>
		{
			work = work,
			p1 = p1,
			p2 = p2,
			p3 = p3
		};
	}

	public static WorkLoadRecursion<T1, T2, T3, T4> ActionAsWork<T1, T2, T3, T4>(Action<IWorker, T1, T2, T3, T4> work, T1 p1, T2 p2, T3 p3, T4 p4)
	{
		return new WorkLoadRecursion<T1, T2, T3, T4>
		{
			work = work,
			p1 = p1,
			p2 = p2,
			p3 = p3,
			p4 = p4
		};
	}

	public static WorkLoadRecursion<T1, T2, T3, T4, T5> ActionAsWork<T1, T2, T3, T4, T5>(Action<IWorker, T1, T2, T3, T4, T5> work, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
	{
		return new WorkLoadRecursion<T1, T2, T3, T4, T5>
		{
			work = work,
			p1 = p1,
			p2 = p2,
			p3 = p3,
			p4 = p4,
			p5 = p5
		};
	}

	private bool DoWork()
	{
		return _internalWorker.MoveNext();
	}

	private void Awake()
	{
		_internalWorker = new InternalWorker();
	}

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = realtimeSinceStartup + WorkTime;
		while (DoWork() && Time.realtimeSinceStartup < num)
		{
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
