using Motus.Behavior;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class CleanupProject : Project
{
	[SerializeField]
	private LitterGoalProvider LitterGoals;

	[SerializeField]
	private CleanUpGoalProvider CleanupGoals;

	protected override void Awake()
	{
		base.Awake();
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionClean;
		AddProvider(LitterGoals);
		AddProvider(CleanupGoals);
	}

	public override bool HasCleanup()
	{
		return GoalCounts() > 0;
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		Slinq<Goal, ConcatContext<SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>, Goal, SelectContext<Goal, LitterGoal, IListContext<LitterGoal>>>> goals = LitterGoals.All().Concat(CleanupGoals.All());
		SlinqEnumerator<Goal, PredicateContext<Goal, SelectContext<Goal, Goal, ConcatContext<SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>, Goal, SelectContext<Goal, LitterGoal, IListContext<LitterGoal>>>>, Tuple<Project, Labor>>> enumerator = GoalsNeedingEvaluation(goals, labor).GetEnumerator();
		return SelectBestGoalPooledList(enumerator, labor, result);
	}
}
