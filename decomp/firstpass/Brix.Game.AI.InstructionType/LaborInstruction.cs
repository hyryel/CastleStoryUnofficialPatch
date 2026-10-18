using System;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.InstructionType;

public class LaborInstruction : WorkInstruction<Labor>
{
	public LaborInstruction(string description, IconKey iconKey, Func<Labor, Node> getNode)
		: base(description, iconKey, getNode)
	{
		LaborInstruction instruction = this;
		GetTask = (Labor labor) => Task.GetTaskFromInstruction(instruction, labor, getNode(labor));
	}
}
public class LaborInstruction<T> : WorkInstruction<Labor, T>
{
	public LaborInstruction()
	{
	}

	public LaborInstruction(string description, IconKey iconKey, Func<Labor, T, Node> getNode)
		: base(description, iconKey, getNode)
	{
		LaborInstruction<T> instruction = this;
		GetTask = delegate(Labor labor, T value)
		{
			Task taskFromInstruction = Task.GetTaskFromInstruction(instruction, labor, getNode(labor, value));
			taskFromInstruction.Target = ((!(value is GameObject)) ? null : (value as GameObject));
			return taskFromInstruction;
		};
	}
}
