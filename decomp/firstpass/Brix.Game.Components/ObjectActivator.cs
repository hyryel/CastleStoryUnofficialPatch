using System;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.InstructionType;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.Components;

public class ObjectActivator : GameComponent, IOperable<Labor>
{
	public Signal ActivationSignal = new Signal();

	public Signal DeactivationSignal = new Signal();

	public bool DesiredState = true;

	private WorkInstruction<Labor> activate;

	private WorkForce Crew;

	private bool _active;

	private Labor operateur;

	public IconKey Icon => IconKeys.BearTrap;

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			if (_active != value)
			{
				if (value)
				{
					ActivationSignal.Invoke();
				}
				else
				{
					DeactivationSignal.Invoke();
				}
				_active = value;
				VerifyDesieredState();
			}
		}
	}

	public bool IsStillAlive => !this.IsNullOrReleased();

	public ProjectPriority PriorityWeights => ProjectPriority.WeightsFor(this);

	public ProjectPriority.Bias PriorityBias
	{
		get
		{
			return ProjectPriority.Bias.Medium;
		}
		set
		{
		}
	}

	public int MaximumCrewSize => 1;

	public ConflictResolver ConflictResolver => null;

	public Labor Operator
	{
		get
		{
			return operateur;
		}
		set
		{
			operateur = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Crew = WorkForce.Request();
		activate = new WorkInstruction<Labor>("Activate", IconKeys.Operator, Operate);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Crew.Dispose();
	}

	private void VerifyDesieredState()
	{
		if (_active == DesiredState)
		{
			ProjectDatabase.For(base.faction).Register(this);
		}
		else
		{
			ProjectDatabase.For(base.faction).Register(this);
		}
	}

	[ContextMenu("Toggle Activation")]
	public void ToggleActivation()
	{
		Active = !Active;
	}

	private unsafe Node Operate(Labor labor)
	{
		Node simpleAction = new WorkNodeGenerator<ObjectActivator>
		{
			Animation = Working.WorkStates.Hammer,
			WorkAction = (ObjectActivator a, Labor l) => (StaticNode)Node.Sequence.Do(delegate
			{
				ToggleActivation();
			})
		}.BuildNode(this, labor);
		return (StaticNode)Node.Selector.When(() => this.IsNullOrReleased()).Do((Func<StaticNode>)Node.FailWork<Exceptions.NullTarget>).When(() => Active == DesiredState)
			.Do(new Func<StaticNode>(null, (nint)(delegate*<StaticNode>)(&Node.get_Empty)))
			.Else()
			.Do(() => simpleAction);
	}

	public bool HasWorkFor(Labor labor)
	{
		if (Operator != null && Operator != labor)
		{
			return false;
		}
		if (DesiredState == _active)
		{
			return false;
		}
		return DesiredState != _active;
	}

	public Node VerifyWorkFor(Labor labor)
	{
		return Node.Empty;
	}

	public Node GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		float radius = Vector3.Distance(base.transform.position, labor.transform.position) + 10f;
		if (labor.enemyTracker.EnemyInRadius(radius))
		{
			return Node.FailWork<Exceptions.CantWorkNearEnemy>();
		}
		result.Set(Order.Package.Of(activate, Labor.DecisionSource.Project));
		return Node.Empty;
	}

	public ProjectPriority PriorityInput(Labor labor)
	{
		return new ProjectPriority
		{
			bias = 1f,
			crewSize = Crew.Count,
			globalCrewSize = GlobalCrewCount(),
			remainingCrewSpace = MaximumCrewSize - CrewCount(),
			distance = Project.DistanceIncrement(this, labor),
			goalCount = 1f,
			isCurrent = (Crew.Contains(labor) ? 1 : 0),
			deathCount = 0f,
			manualBias = (float)PriorityBias
		};
	}

	public int CrewCount()
	{
		return Crew.Count;
	}

	public int GlobalCrewCount()
	{
		return ProjectDatabase.For(base.faction).WorkerInTaskSimilarTasksTo(this);
	}

	public bool Hire(Labor labor)
	{
		return IOperableExtender.Hire(this, labor);
	}

	public void Fire(Labor labor)
	{
		IOperableExtender.Fire(this, labor);
	}

	IReadOnlySignal<GameObject> IOperable<Labor>.get_OnReleaseOrDestroySignal()
	{
		return base.OnReleaseOrDestroySignal;
	}

	GameObject IOperable<Labor>.get_gameObject()
	{
		return base.gameObject;
	}
}
