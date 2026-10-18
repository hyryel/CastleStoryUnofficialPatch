using System;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.InstructionType;

public class OperableInstruction<T> : LaborInstruction<T>
{
	public OperableInstruction(string description, IconKey iconKey, Func<Labor, T, Node> getNode, IOperable<Labor> operable)
		: base(description, iconKey, getNode)
	{
		OperableInstruction<T> instruction = this;
		GetTask = delegate(Labor labor, T value)
		{
			Task taskFromInstruction = Task.GetTaskFromInstruction(instruction, labor, getNode(labor, value));
			taskFromInstruction.Target = ((!(value is GameObject)) ? null : (value as GameObject));
			taskFromInstruction.ValidateFunc = () => operable.IsStillAlive;
			return taskFromInstruction;
		};
	}
}
