using System;
using Brix.UI.Icons;
using Motus.Behavior;

namespace Brix.Game.AI.InstructionType;

public class MoveInstruction : LaborInstruction
{
	public MoveInstruction(string description, IconKey iconKey, Func<Labor, Node> getNode, int delayOnFail = 30, int delayOnFinished = 0)
		: base(description, iconKey, getNode)
	{
		Func<Labor, Task> oldGetTask = GetTask;
		GetTask = delegate(Labor labor)
		{
			Task task = oldGetTask(labor);
			task.DelayOnFail = delayOnFail;
			task.DelayOnFinished = delayOnFinished;
			return task;
		};
	}
}
public class MoveInstruction<T> : LaborInstruction<T>
{
	public MoveInstruction(string description, IconKey iconKey, Func<Labor, T, Node> getNode, int delayOnFail = 30, int delayOnFinished = 0)
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
