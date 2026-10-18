using System;
using Brix.UI.Icons;
using Motus.Behavior;

namespace Brix.Game.AI.InstructionType;

public class CombatInstruction<T> : LaborInstruction<T>
{
	public CombatInstruction(string description, IconKey iconKey, Func<Labor, T, Node> getNode, int delayOnFail = 1, int delayOnFinished = 0)
		: base(description, iconKey, getNode)
	{
		Func<Labor, T, Task> oldGetTask = GetTask;
		GetTask = delegate(Labor labor, T value)
		{
			Task task = oldGetTask(labor, value);
			task.DelayOnFail = delayOnFail;
			task.DelayOnFinished = delayOnFinished;
			return task;
		};
	}
}
