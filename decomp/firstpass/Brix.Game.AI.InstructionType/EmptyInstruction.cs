using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.AI.InstructionType;

public class EmptyInstruction<TLabor> : WorkInstruction<TLabor> where TLabor : BaseLabor
{
	public EmptyInstruction()
	{
		IconKey = IconKeys.Missing;
		Description = "Empty instruction";
		CanInterrupt = true;
		GetTask = (TLabor labor) => (Task)null;
	}

	public new void Send(BaseLabor receiver)
	{
	}

	public new void Send(BaseLabor receiver, Labor.DecisionSource decisionSource)
	{
	}

	public new void Send(BaseLabor receiver, Labor.DecisionSource decisionSource, GameObject giver)
	{
	}
}
