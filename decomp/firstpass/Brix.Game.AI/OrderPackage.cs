using System;
using Brix.Game.AI.InstructionType;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class OrderPackage<TLabor, T> : OrderPackage<TLabor>, IOrderPackageValue, IOrderPackage where TLabor : BaseLabor
{
	public WorkInstruction<TLabor, T> InstructionValue;

	public T data;

	public object value => data;

	public OrderPackage(WorkInstruction<TLabor, T> instruction, T data, Labor.DecisionSource decisionSource = Labor.DecisionSource.Unspecified)
	{
		base.DecisionSource = decisionSource;
		InstructionValue = instruction;
		_iconKey = instruction.IconKey;
		this.data = data;
	}

	public override void Send(BaseLabor orderReceiver)
	{
		InstructionValue.Send(orderReceiver, data, base.DecisionSource);
	}

	public override void Queue(BaseLabor orderReceiver)
	{
		base.DecisionSource = Labor.DecisionSource.User;
		orderReceiver.QueueOrder(this);
	}
}
public class OrderPackage<TLabor> : IOrderPackage where TLabor : BaseLabor
{
	public static OrderPackage<TLabor> Empty = new EmptyPackage<TLabor>();

	private Func<BaseLabor, bool> prepare;

	public WorkInstruction<TLabor> Instruction;

	private Labor.DecisionSource decisionSource;

	protected IconKey _iconKey;

	public GameObject Giver;

	public Labor.DecisionSource DecisionSource
	{
		get
		{
			return decisionSource;
		}
		set
		{
			decisionSource = value;
		}
	}

	public IconKey IconKey
	{
		get
		{
			return _iconKey;
		}
		set
		{
			_iconKey = value;
		}
	}

	public OrderPackage()
	{
	}

	public OrderPackage(WorkInstruction<TLabor> Instruction)
	{
		this.Instruction = Instruction;
		_iconKey = Instruction.IconKey;
	}

	public ComputationProgress<OrderPackage<TLabor>> AsNewComputation()
	{
		return ComputationProgress<OrderPackage<TLabor>>.Return(this);
	}

	public void SetPrepare(Func<BaseLabor, bool> p)
	{
		prepare = p;
	}

	public StaticNode Prepare(BaseLabor orderReceiver)
	{
		if (prepare != null && !prepare(orderReceiver))
		{
			return Node.Fail(FailException.Generic);
		}
		return Node.Empty;
	}

	public virtual void Send(BaseLabor orderReceiver)
	{
		Instruction.Send(orderReceiver, DecisionSource, Giver);
	}

	public virtual void Queue(BaseLabor orderReceiver)
	{
		DecisionSource = Labor.DecisionSource.User;
		orderReceiver.QueueOrder(this);
	}
}
