using System;
using Brix.Common;

namespace Motus.Behavior;

public class TickCondition : PoolableObject<TickCondition>, ICondition, IDisposable
{
	public int Delay;

	private int TickToWaitLeft;

	public static TickCondition Request(int delay)
	{
		TickCondition tickCondition = PoolableObject<TickCondition>.Request();
		tickCondition.Delay = delay;
		return tickCondition;
	}

	public void Init()
	{
		TickToWaitLeft = Delay;
	}

	public bool Test()
	{
		return TickToWaitLeft-- <= 0;
	}

	protected override void ResetObject()
	{
		TickToWaitLeft = int.MaxValue;
	}

	public void Dispose()
	{
		ReleaseToPool();
	}
}
