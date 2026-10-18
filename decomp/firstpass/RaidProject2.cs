using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.UI.Icons;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

public class RaidProject2 : Project, IOperable<Labor>
{
	private enum Actions
	{
		Nothing,
		Patrol,
		WalkTo,
		RunTo
	}

	private LaborInstruction Patrol;

	private LaborInstruction RunTo;

	protected virtual bool DoesPatrol => true;

	private XYZ BalisePosition => (!base.Balise.IsNullOrReleased()) ? base.Balise.position : XYZ.FromVector3(base.transform.position);

	private float BaliseRadius => (!base.Balise.IsNullOrReleased()) ? base.Balise.radius : 10f;

	public override void Initialize()
	{
		base.Initialize();
		Patrol = new LaborInstruction("Patrol", IconKeys.Missing, PatrolNode);
	}

	private Actions DecideWhatToDo(Labor labor)
	{
		if (this == null)
		{
			return Actions.Nothing;
		}
		if (IsInZone(labor))
		{
			return DoesPatrol ? Actions.Patrol : Actions.Nothing;
		}
		if (HasAWalker())
		{
			return Actions.WalkTo;
		}
		return Actions.RunTo;
	}

	private Node PatrolNode(Labor labor)
	{
		return labor.WalkToExactly(Location.RandomAround(BalisePosition, BaliseRadius - 1f, labor));
	}

	private bool HasAWalker()
	{
		return Workforce.Slinq().Any((Labor l) => !l.Profession.CanRun());
	}

	private OrderPackage<Labor> GetPackageFor(Actions action)
	{
		OrderPackage<Labor> orderPackage = null;
		switch (action)
		{
		case Actions.Nothing:
			orderPackage = Order.Package.OfFloat(Order.ForLabor.Wait, 1f);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		case Actions.Patrol:
			orderPackage = Order.Package.Of(Patrol, Labor.DecisionSource.Project);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		case Actions.WalkTo:
			orderPackage = Order.Package.OfLocation(Order.Move.WalkAggressivelyToward, Location.Of(BalisePosition), Labor.DecisionSource.Project);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		case Actions.RunTo:
			orderPackage = Order.Package.OfLocation(Order.Move.RunAggressivelyToward, Location.Of(BalisePosition), Labor.DecisionSource.Project);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		}
		return orderPackage;
	}

	private bool IsInZone(Labor labor)
	{
		return Vector3.Distance(BalisePosition.ToVector3(), labor.transform.position) < BaliseRadius;
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		return Node.Empty;
	}

	public override Node GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		return (StaticNode)Node.Sequence.Do(delegate(RaidProject2 self, Labor _labor, Var<OrderPackage<Labor>> _result)
		{
			self._GetInstruction(_labor, _result);
		}, this, labor, result);
	}

	private void _GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		Actions action = DecideWhatToDo(labor);
		OrderPackage<Labor> packageFor = GetPackageFor(action);
		result.Set(packageFor);
	}

	public override bool HasWorkFor(Labor labor)
	{
		return true;
	}
}
