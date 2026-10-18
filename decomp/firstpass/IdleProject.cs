using System.Collections.Generic;
using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.Lua;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Motus.Behavior;
using UnityEngine;

public class IdleProject : Project
{
	private enum IdleActions
	{
		None,
		WalkInPoint,
		Sit,
		GoStore
	}

	private static Dictionary<Faction, IdleProject> _faction2Projects = new Dictionary<Faction, IdleProject>();

	private Dictionary<Labor, float> _walkFailTimeout = new Dictionary<Labor, float>();

	private Dictionary<Labor, XYZ> _walkFailPosition = new Dictionary<Labor, XYZ>();

	private const float WalkFailTimeoutDelay = 60f;

	private float Range = 5f;

	private HashSet<Labor> assignedLabors = new HashSet<Labor>();

	public static IdleProject GetAlliedProject()
	{
		return GetMatchingProject(Faction.DefaultAllied);
	}

	public static IdleProject GetMatchingProject(Faction f)
	{
		return (!(f == null)) ? _faction2Projects.GetValueOrDefault(f) : null;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		base.affiliation.OnFactionChanged.Connect(delegate(Faction f1, Faction f2)
		{
			_faction2Projects.Remove(f1);
			_faction2Projects[f2] = this;
		});
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (this != null && (bool)base.faction)
		{
			_faction2Projects.Remove(base.faction);
		}
	}

	protected override void Start()
	{
		base.Start();
		GameSignals.Connect(UIGameActor.SelectIdleProjectSignal, OnSelectIdleProject).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.projects.Removed.Connect(OnProjectDeleted).DisconnectWhen(base.OnReleaseOrDestroySignal);
		base.transform.SetParent(IdleProjectInstatiator.instance.ParentTo);
	}

	private IdleActions DecideWhatToDo(Labor labor)
	{
		if (!labor.recepteur.IsEmpty())
		{
			return Knowledge.Instance.KnowsAbout(labor.gameObject) ? IdleActions.GoStore : IdleActions.None;
		}
		if (GatherAtIdlePoint(labor))
		{
			if (Random.value < 0.3f && !IsFar(labor))
			{
				return IdleActions.Sit;
			}
			return IdleActions.WalkInPoint;
		}
		return IdleActions.None;
	}

	private OrderPackage<Labor> PackageFor(IdleActions action, Labor labor)
	{
		OrderPackage<Labor> orderPackage = OrderPackage<Labor>.Empty;
		bool hire = Hires(action, labor);
		Activity activity = Activity.Idle;
		switch (action)
		{
		case IdleActions.GoStore:
		{
			XYZ wanderDestination2 = GetWanderDestination(labor, hire: true);
			orderPackage = Order.Package.OfLocation(Order.Transfer.GoStoreOrDropAt, Location.RandomAround(wanderDestination2, Range, labor));
			activity = Activity.Idle;
			break;
		}
		case IdleActions.WalkInPoint:
		{
			XYZ wanderDestination = GetWanderDestination(labor, hire);
			orderPackage = Order.Package.OfLocation(Order.Move.WalkToward, Location.RandomAround(wanderDestination, Range, labor), Labor.DecisionSource.Project);
			activity = Activity.Chilling;
			break;
		}
		case IdleActions.Sit:
			orderPackage = Order.Package.Of(Order.ForLabor.Sit, Labor.DecisionSource.Project);
			activity = Activity.Chilling;
			break;
		}
		if (action != IdleActions.None)
		{
			orderPackage.SetPrepare((BaseLabor worker) => Prepare(worker as Labor, hire, activity));
		}
		return orderPackage;
	}

	private bool Prepare(Labor labor, bool hire, Activity activity)
	{
		labor.Activity = activity;
		if (hire)
		{
			labor.SetTemporaryThought(1.5f, Exceptions.ProjectWithoutGoal.Thought);
			return Hire(labor);
		}
		return true;
	}

	private XYZ GetWanderDestination(Labor labor, bool hire)
	{
		if (_walkFailTimeout.ContainsKey(labor))
		{
			if (Time.time < _walkFailTimeout[labor])
			{
				return _walkFailPosition[labor];
			}
			_walkFailTimeout.Remove(labor);
			_walkFailPosition.Remove(labor);
		}
		if (!hire)
		{
			if (labor.Operable != null && !labor.Operable.gameObject.IsNullOrReleased())
			{
				return XYZ.FromVector3(labor.Operable.gameObject.transform.position);
			}
			Beacon balise = labor.Project.Balise;
			if (balise != null)
			{
				return balise.position;
			}
		}
		return base.Balise.position;
	}

	private bool Hires(IdleActions action, Labor labor)
	{
		return (action == IdleActions.Sit || action == IdleActions.WalkInPoint) && labor.Autonomy != AutonomyStatus.LockedToOperable;
	}

	public override void OnLaborError(Labor labor, Exceptions.WorkFailedException e)
	{
		base.OnLaborError(labor, e);
		if (labor.Activity == Activity.Chilling && e is Exceptions.PathException)
		{
			_walkFailTimeout[labor] = Time.time + 60f;
			_walkFailPosition[labor] = labor.voxel.position;
		}
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		return Node.Empty;
	}

	public override Node GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		return (StaticNode)Node.Sequence.Do((IdleProject self) => self.GetComponent<BeaconDriver>().NetworkNetworkPosition = self.faction.Gatherpoint, this).Do(delegate(IdleProject self, Labor _labor, Var<OrderPackage<Labor>> _result)
		{
			self._GetInstruction(_labor, _result);
		}, this, labor, result);
	}

	private void _GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		IdleActions action = DecideWhatToDo(labor);
		OrderPackage<Labor> x = PackageFor(action, labor);
		result.Set(x);
	}

	public override bool HasWorkFor(Labor labor)
	{
		return true;
	}

	private bool GatherAtIdlePoint(Labor labor)
	{
		return labor.TimeSinceLastOrder(Labor.DecisionSource.Project) > 3f && labor.recepteur.IsEmpty();
	}

	private bool IsFar(Labor labor)
	{
		if (labor.Project.Balise == null)
		{
			return false;
		}
		return Vector3.Distance(labor.transform.position, labor.Project.Balise.position.ToVector3()) > Range;
	}

	private void OnSelectIdleProject()
	{
		if (base.faction.IsOwnedByMe && !UIGameSelector.IsSelected(base.gameObject))
		{
			UIGameSelector.Select(base.gameObject);
		}
	}

	private void OnProjectDeleted(Project deletedProject)
	{
		if ((bool)this && (bool)base.faction && base.faction.IsOwnedByMe && !UIGameSelector.SomeIsSelected(SelectionType.Project))
		{
			UIGameSelector.Select(base.gameObject);
		}
	}

	public HashSet<Labor> AssignedLabors()
	{
		assignedLabors.Clear();
		foreach (Project item in UIGameObserver.projects.all)
		{
			foreach (Labor item2 in item.Crew())
			{
				assignedLabors.Add(item2);
			}
		}
		return assignedLabors;
	}

	[LuaDrivable("GetAlliedProject")]
	private static DynValue GetAlliedProject(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, GetAlliedProject());
	}
}
