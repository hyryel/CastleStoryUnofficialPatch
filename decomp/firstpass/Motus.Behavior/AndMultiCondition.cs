using System;
using Brix.Common;

namespace Motus.Behavior;

public class AndMultiCondition : PoolableObject<AndMultiCondition>, ICondition, IDisposable
{
	public ICondition C1;

	public ICondition C2;

	public static AndMultiCondition Request(ICondition c1, ICondition c2)
	{
		AndMultiCondition andMultiCondition = PoolableObject<AndMultiCondition>.Request();
		andMultiCondition.C1 = c1;
		andMultiCondition.C2 = c2;
		return andMultiCondition;
	}

	public void Init()
	{
	}

	public bool Test()
	{
		return C1.Test() && C2.Test();
	}

	protected override void ResetObject()
	{
		C1.Dispose();
		C2.Dispose();
	}

	public void Dispose()
	{
		ReleaseToPool();
	}
}
