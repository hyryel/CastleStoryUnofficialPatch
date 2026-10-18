using System.Collections.Generic;
using Brix.Engine;
using Motus.Behavior;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class RepairProject : Project
{
	[SerializeField]
	public RepairGoalProvider repairProvider;

	[SerializeField]
	public BuildGoalProvider buildProvider;

	[SerializeField]
	public CleanUpGoalProvider cleanupProvider;

	protected override void Awake()
	{
		base.Awake();
		repairProvider.Changed.Connect(delegate
		{
			buildProvider.ClearGoalsOutside(repairProvider.Bounds);
		});
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionRepair;
		SetGoalSelectionModel(GoalSelector.Type.CompareAll);
		base.ConflictResolutionModel = ConflictResolver.ResolutionModel.Construction;
		AddProvider(repairProvider);
		AddProvider(buildProvider);
		AddProvider(cleanupProvider);
	}

	public bool IsInRange(XYZ position)
	{
		return repairProvider.Bounds.Contains(position);
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		Slinq<Goal, ConcatContext<SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>, Goal, ConcatContext<SelectContext<Goal, BuildGoal, HashSetSlinqContext<BuildGoal>>, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>>> goals = repairProvider.All().Concat(buildProvider.All()).Concat(cleanupProvider.All());
		SlinqEnumerator<Goal, PredicateContext<Goal, SelectContext<Goal, Goal, ConcatContext<SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>, Goal, ConcatContext<SelectContext<Goal, BuildGoal, HashSetSlinqContext<BuildGoal>>, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>>>, Tuple<Project, Labor>>> enumerator = GoalsNeedingEvaluation(goals, labor).GetEnumerator();
		return SelectBestGoalPooledList(enumerator, labor, result);
	}
}
