using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Motus.Behavior;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class LandscapeProject : Project
{
	[SerializeField]
	private LandscapeGoalProvider goalProvider;

	protected override void Awake()
	{
		base.Awake();
		SetGoalSelectionModel(GoalSelector.Type.GoodEnoughFromSample);
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionRepair;
		AddProvider(goalProvider);
	}

	protected bool HasGoal()
	{
		return goalProvider.GoalCount() > 0;
	}

	public override void OnBecameEmpty()
	{
		base.OnBecameEmpty();
		CheckForAutoDelete();
	}

	private void CheckForAutoDelete()
	{
		if (!AutoDeleteDisabled && !base.selection && base.faction != null && base.faction.OwnerSyncList.Count <= 1 && CrewCount() == 0 && goalProvider.GoalCount() <= 0)
		{
			RequestDelete();
		}
	}

	public override bool HasWorkFor(Labor labor)
	{
		return (base.HasWorkFor(labor) && HasDigGoals(labor, reacheableOnly: false) && labor.recepteur.HasRoomInBag()) || (HasFillGoals(labor, reacheableOnly: false) && (bool)labor.Find(Adjectif.stones.Profile));
	}

	public override Node VerifyWorkFor(Labor labor)
	{
		if (HasFillGoals(labor, reacheableOnly: true) && HasDigGoals(labor, reacheableOnly: true))
		{
			return Node.Empty;
		}
		return base.VerifyWorkFor(labor);
	}

	private bool HasFillGoals(IGameComponent target, bool reacheableOnly)
	{
		Slinq<LandscapeGoal, PredicateContext<LandscapeGoal, SelectContext<LandscapeGoal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>>> slinq = from g in GetAvailableGoals()
			where g.GetLandscapeType() == LandscapeGoal.Landscape.Fill
			select g;
		return (!reacheableOnly) ? slinq.Any() : slinq.Any((LandscapeGoal g, IGameComponent _target) => Knowledge.Instance.CanReach(g, _target), target);
	}

	private bool HasDigGoals(IGameComponent target, bool reacheableOnly)
	{
		Slinq<LandscapeGoal, PredicateContext<LandscapeGoal, SelectContext<LandscapeGoal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>>> slinq = from g in GetAvailableGoals()
			where g.GetLandscapeType() == LandscapeGoal.Landscape.Dig
			select g;
		return (!reacheableOnly) ? slinq.Any() : slinq.Any((LandscapeGoal g, IGameComponent _target) => Knowledge.Instance.CanReach(g, _target), target);
	}

	public bool HasBothKindOfGoals()
	{
		return HasDigGoals(null, reacheableOnly: false) && HasFillGoals(null, reacheableOnly: false);
	}

	public Slinq<LandscapeGoal, SelectContext<LandscapeGoal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>> GetAvailableGoals()
	{
		return goalProvider.LandscapeGoals();
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		SlinqEnumerator<Goal, PredicateContext<Goal, SelectContext<Goal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>, Tuple<Project, Labor>>> enumerator = GoalsNeedingEvaluation(goalProvider.Goals(), labor).GetEnumerator();
		return SelectBestGoalPooledList(enumerator, labor, result);
	}
}
