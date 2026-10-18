using System;
using Brix.Common;
using Brix.Utils.Closures;

namespace Motus.Behavior;

public class PredicateCondition : PoolableObject<PredicateCondition>, ICondition, IDisposable
{
	public IClosure<IFunc<bool>> F;

	public static PredicateCondition Request(IClosure<IFunc<bool>> f)
	{
		PredicateCondition predicateCondition = PoolableObject<PredicateCondition>.Request();
		predicateCondition.F = f;
		return predicateCondition;
	}

	public void Init()
	{
	}

	public bool Test()
	{
		return F.Delegate.Invoke();
	}

	protected override void ResetObject()
	{
		F.Dispose();
	}

	public void Dispose()
	{
		ReleaseToPool();
	}
}
