using System.Collections.Generic;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class BuildProject : Project
{
	[SerializeField]
	private BuildGoalProvider goals;

	public HashSet<BuildGoal> GoalsToMap = new HashSet<BuildGoal>();

	protected override void Awake()
	{
		base.Awake();
		base.ConflictResolutionModel = ConflictResolver.ResolutionModel.Construction;
		SetGoalSelectionModel(GoalSelector.Type.LocalMaximum, chooseWithAccess: true);
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionBuild;
		AddProvider(goals);
	}

	public override void OnBecameEmpty()
	{
		base.OnBecameEmpty();
		CheckForAutoDelete();
	}

	private void CheckForAutoDelete()
	{
		if (!AutoDeleteDisabled && !base.selection && base.faction != null && base.faction.OwnerSyncList.Count <= 1 && CrewCount() == 0 && goals.TrackedBlueprints.Count <= 0)
		{
			RequestDelete();
		}
	}

	public override void OnUnselect()
	{
		base.OnUnselect();
		CheckForAutoDelete();
	}

	public override bool HasRequiredResources()
	{
		return true;
	}

	public override void Commit()
	{
		goals.Commit();
		base.Commit();
	}

	public override void CancelCommit()
	{
		goals.DeleteUncommited();
	}

	public override bool HasUncommitedGoals()
	{
		return goals.HasUncommited();
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		return SelectBestGoal(goals.FulfillableBy(labor), labor, result);
	}
}
