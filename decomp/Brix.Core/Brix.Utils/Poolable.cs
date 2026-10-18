using System;

namespace Brix.Utils;

public class Poolable : IPoolable, IResetable, IDisposable
{
	public virtual void ResetObject()
	{
	}

	public virtual void Dispose()
	{
	}
}
