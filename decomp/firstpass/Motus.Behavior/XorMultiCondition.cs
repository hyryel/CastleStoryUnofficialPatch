using System;
using Brix.Common;

namespace Motus.Behavior;

public class XorMultiCondition : PoolableObject<OrMultiCondition>, ICondition, IDisposable
{
	public ICondition C1;

	public ICondition C2;

	public static OrMultiCondition Request(ICondition c1, ICondition c2)
	{
		OrMultiCondition orMultiCondition = PoolableObject<OrMultiCondition>.Request();
		orMultiCondition.C1 = c1;
		orMultiCondition.C2 = c2;
		return orMultiCondition;
	}

	public void Init()
	{
	}

	public bool Test()
	{
		bool flag = C1.Test();
		bool flag2 = C2.Test();
		return (flag || flag2) && (!flag || !flag2);
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
