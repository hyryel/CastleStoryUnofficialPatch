using System;
using System.Collections.Generic;
using Brix.Utils;

namespace Brix.Game.AI;

public class WorkForce : HashSet<Labor>, IPoolable, IResetable, IDisposable
{
	public void ResetObject()
	{
		Clear();
	}

	public void Dispose()
	{
		ObjectPool<WorkForce>.Release(this);
	}

	public static WorkForce Request()
	{
		return ObjectPool<WorkForce>.Reserve();
	}
}
