using System;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.InstructionType;

public class WorkInstruction<TLabor> where TLabor : BaseLabor
{
	public static WorkInstruction<TLabor> Empty = new EmptyInstruction<TLabor>();

	public IconKey IconKey;

	public string Description;

	public bool CanInterrupt = true;

	protected Func<TLabor, Task> GetTask;

	public WorkInstruction()
	{
	}

	public WorkInstruction(string description, IconKey iconKey, Func<TLabor, Node> getNode)
	{
		WorkInstruction<TLabor> instruction = this;
		IconKey = iconKey;
		Description = description;
		GetTask = (TLabor labor) => Task.GetTaskFromInstruction(instruction, labor, getNode(labor));
	}

	public void Send(BaseLabor receiver)
	{
		Send(receiver, Labor.DecisionSource.Unspecified);
	}

	public void Send(BaseLabor receiver, Labor.DecisionSource decisionSource)
	{
		Send(receiver, decisionSource, null);
	}

	public void Send(BaseLabor receiver, Labor.DecisionSource decisionSource, GameObject giver)
	{
		if (!(receiver is TLabor))
		{
			throw new Exception("Instruction has been send to the wrong type of Labor");
		}
		Task task = GetTask(receiver as TLabor);
		if (task != null)
		{
			if (giver != null)
			{
				task.Giver = giver;
			}
			OrderPackage<TLabor> orderPackage = Order.Package.Of(this, decisionSource);
			orderPackage.Giver = giver;
			task.InitialPackage = orderPackage;
			receiver.SendOrder(task, decisionSource);
		}
	}
}
public class WorkInstruction<TLabor, TParameter> : WorkInstruction<TLabor> where TLabor : BaseLabor
{
	public new Func<TLabor, TParameter, Task> GetTask;

	public WorkInstruction()
	{
	}

	public WorkInstruction(string description, IconKey iconKey, Func<TLabor, TParameter, Node> getNode)
	{
		WorkInstruction<TLabor, TParameter> instruction = this;
		IconKey = iconKey;
		Description = description;
		GetTask = delegate(TLabor labor, TParameter value)
		{
			Task taskFromInstruction = Task.GetTaskFromInstruction(instruction, labor, getNode(labor, value));
			taskFromInstruction.Target = ((!(value is GameObject)) ? null : (value as GameObject));
			return taskFromInstruction;
		};
	}

	public void Send(BaseLabor receiver, TParameter value)
	{
		Send(receiver, value, Labor.DecisionSource.Unspecified);
	}

	public void Send(BaseLabor receiver, TParameter value, Labor.DecisionSource decisionSource)
	{
		if (!(receiver is TLabor))
		{
			throw new Exception("Instruction has been send to the wrong type of Labor");
		}
		Task task = GetTask(receiver as TLabor, value);
		if (task != null)
		{
			task.InitialPackage = new OrderPackage<TLabor, TParameter>(this, value, decisionSource);
			receiver.SendOrder(task, decisionSource);
		}
	}
}
