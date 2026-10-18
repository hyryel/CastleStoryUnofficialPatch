using System;

namespace Motus.Behavior;

public interface ICondition : IDisposable
{
	void Init();

	bool Test();
}
