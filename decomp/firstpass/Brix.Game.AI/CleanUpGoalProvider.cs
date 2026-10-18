using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.Utils;
using Brix.Utils.Unity;
using Motus.Behavior;
using Smooth.Delegates;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class CleanUpGoalProvider : Behavior, IGoalProvider
{
	[SerializeField]
	private bool OnlyWoodLogs;

	private HashSet<CleanUpGoal> goals = new HashSet<CleanUpGoal>();

	private Func<GameObject, bool> Filter;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	public BeaconDriver _baliseProvider;

	private const int LAYERS = 1073152;

	public Beacon Balise
	{
		get
		{
			if (_baliseProvider != null)
			{
				return _baliseProvider.Beacon;
			}
			return null;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		Filter = (GameObject go) => NeedsGoal(go, OnlyWoodLogs);
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (CleanUpGoal goal in goals)
		{
			if (!goal.IsNullOrReleased())
			{
				goal.UniqueItems(ref keys);
			}
		}
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Sequence.Do(Node.WaitFor.Condition((CleanUpGoalProvider self) => self.Balise != null, this)).Do(delegate(CleanUpGoalProvider self)
		{
			self.RefreshCleanupGoals();
		}, this).Loop()
			.WithDelay(10L);
	}

	public bool Any()
	{
		return goals.Count > 0;
	}

	public int GoalCount()
	{
		return goals.Count;
	}

	public HashSet<CleanUpGoal> Goals()
	{
		return goals;
	}

	public Slinq<Goal, SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>> All()
	{
		return goals.Slinq().Select((DelegateFunc<CleanUpGoal, Goal>)((CleanUpGoal g) => g));
	}

	public Slinq<Goal, SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>> All(bool yes)
	{
		return ((!yes) ? Empty.HashSet<CleanUpGoal>() : goals).Slinq().Select((DelegateFunc<CleanUpGoal, Goal>)((CleanUpGoal g) => g));
	}

	public void GetAll(ICollection<Goal> results)
	{
		All().AddTo(results);
	}

	private static bool NeedsGoal(GameObject go, bool woodOnly)
	{
		if (woodOnly && !(Apparence.PrincipaleOf(go) is WoodBlock))
		{
			return false;
		}
		if (Apparence.Is<Fireflies>(go) || Apparence.Is<Granulaire>(go))
		{
			return false;
		}
		if (Apparence.Is<ExplosiveBarrel>(go) && !GeneralRules.AllowStoringBarrels)
		{
			return false;
		}
		return true;
	}

	private void RefreshCleanupGoals()
	{
		goals.Clear();
		KnowledgeUtility.FindAllInRadius(Balise.transform.position, Balise.radius, 1073152, Filter, GameObjectExtender.GetOrAddComponent<CleanUpGoal>, ref goals);
	}

	public bool HasNonStorable()
	{
		return false;
	}
}
