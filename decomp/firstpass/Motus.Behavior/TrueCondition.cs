using System;

namespace Motus.Behavior;

public class TrueCondition : ICondition, IDisposable
{
	private static TrueCondition _instance = new TrueCondition();

	public static TrueCondition Instance => _instance;

	private TrueCondition()
	{
	}

	public void Init()
	{
	}

	public bool Test()
	{
		return true;
	}

	public void Dispose()
	{
	}
}
