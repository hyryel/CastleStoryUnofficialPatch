using Brix.Game.AI.InstructionType;
using Brix.UI.Icons;

namespace Brix.Game.AI;

public class EmptyPackage<TLabor> : OrderPackage<TLabor> where TLabor : BaseLabor
{
	public new IconKey IconKey => IconKeys.Missing;

	public EmptyPackage()
		: base(WorkInstruction<TLabor>.Empty)
	{
	}

	public new void Send(BaseLabor orderReceiver)
	{
	}

	public new void Queue(BaseLabor orderReceiver)
	{
	}
}
