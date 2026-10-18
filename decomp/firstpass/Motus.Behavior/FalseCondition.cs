using System;

namespace Motus.Behavior;

public class FalseCondition : ICondition, IDisposable
{
	private static FalseCondition _instance = new FalseCondition();

	public static FalseCondition Instance => _instance;

	private FalseCondition()
	{
	}

	public void Init()
	{
	}

	public bool Test()
	{
		return false;
	}

	public void Dispose()
	{
	}
}
