using System;
using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Closures;

namespace Brix.External.Signals;

internal class Signal2
{
	public struct SignalConnection
	{
		public Signal2 Signal;

		public IClosure<IAction> Action;

		public void Disconnect()
		{
			Signal.Disconnect(Action);
		}
	}

	private List<IClosure<IAction>> disconnections;

	private List<IClosure<IAction>> actions;

	~Signal2()
	{
		Clear();
	}

	public void Clear()
	{
		if (disconnections != null)
		{
			CollectionPool.Release(ref disconnections);
		}
		if (actions != null)
		{
			int count = actions.Count;
			for (int i = 0; i < count; i++)
			{
				actions[i].Dispose();
			}
			CollectionPool.Release(ref actions);
		}
	}

	public SignalConnection Connect(Action action)
	{
		return Connect(action.Make().Pooled2());
	}

	public SignalConnection ConnectOnce(Action action)
	{
		IClosure<IAction> t = null;
		ClosurePoolHandle<Closure.APooled<Closure.AClosed<Closure.AClosed<Closure.ABase<Action, IClosure<IAction>>, Action, IClosure<IAction>>, Action>>, Closure.AClosed<Closure.AClosed<Closure.ABase<Action, IClosure<IAction>>, Action, IClosure<IAction>>, Action>, IAction> closurePoolHandle = Closure.Make(delegate(Action a, IClosure<IAction> c)
		{
			a();
			Disconnect(c);
		}).Close(t).Close(action)
			.Pooled2();
		closurePoolHandle.Delegate.Closed.Up.V = closurePoolHandle;
		return Connect(closurePoolHandle);
	}

	public SignalConnection Connect(IClosure<IAction> action)
	{
		(actions ?? (actions = CollectionPool.RequestList<IClosure<IAction>>())).Add(action);
		return new SignalConnection
		{
			Signal = this,
			Action = action
		};
	}

	public void Disconnect(IClosure<IAction> action)
	{
		(disconnections ?? (disconnections = CollectionPool.RequestList<IClosure<IAction>>())).Add(action);
	}

	public void Invoke()
	{
		if (disconnections != null)
		{
			if (actions != null)
			{
				int count = actions.Count;
				int num = 0;
				for (int i = 0; i < count; i++)
				{
					actions[num] = actions[i];
					if (disconnections.Contains(actions[i]))
					{
						actions[i].Dispose();
					}
					else
					{
						num++;
					}
				}
				if (num == 0)
				{
					CollectionPool.Release(ref actions);
				}
				else if (num < count)
				{
					actions.RemoveRange(num, count - num);
				}
			}
			CollectionPool.Release(ref disconnections);
		}
		if (actions != null)
		{
			int count2 = actions.Count;
			for (int j = 0; j < count2; j++)
			{
				actions[j].Delegate.Invoke();
			}
		}
	}
}
