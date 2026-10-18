using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Collections;
using Motus.Behavior;
using Smooth.Delegates;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class TreeGoalProvider : Behavior, IGoalProvider
{
	public Dictionary<XYZ, TreeGoal> goals = new Dictionary<XYZ, TreeGoal>();

	public Beacon _balise;

	private int previousSearchRadius = 100;

	private XYZ previousPosition = XYZ.zero;

	public float TimeBetweenRefresh = 5f;

	private float timeAtLastRefresh;

	public bool active { private get; set; }

	public Beacon Balise
	{
		get
		{
			if (_balise == null)
			{
				_balise = base.transform.parent.GetComponentInChildren<Beacon>();
			}
			return _balise;
		}
		set
		{
			_balise = value;
		}
	}

	private int TreeSearchRadius => (int)Balise.radius;

	private XYZ TreeSearchPosition => Balise.position;

	private bool DueForRefresh => Time.time - timeAtLastRefresh > TimeBetweenRefresh;

	public Slinq<TreeGoal, SelectContext<TreeGoal, KeyValuePair<XYZ, TreeGoal>, DictionaryContext<XYZ, TreeGoal>>> Goals()
	{
		return from kvp in ((!active) ? Empty.Dictionary<XYZ, TreeGoal>() : goals).Slinq()
			select kvp.Value;
	}

	public Slinq<Goal, SelectContext<Goal, TreeGoal, SelectContext<TreeGoal, KeyValuePair<XYZ, TreeGoal>, DictionaryContext<XYZ, TreeGoal>>>> All()
	{
		return Goals().Select((DelegateFunc<TreeGoal, Goal>)((TreeGoal g) => g));
	}

	public void GetAll(ICollection<Goal> results)
	{
		All().AddTo(results);
	}

	public bool HasNonStorable()
	{
		return GoalCount() > 0;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}

	protected override Node CreateTree()
	{
		return (StaticNode)Node.Do(delegate(TreeGoalProvider self)
		{
			self.RefreshIfNeeded();
		}, this).Loop().WithDelay(10L);
	}

	public bool Any()
	{
		return goals.Count > 0;
	}

	public int GoalCount()
	{
		return active ? goals.Count : 0;
	}

	private void RefreshIfNeeded()
	{
		if (!Balise.IsNullOrReleased() && (DueForRefresh || RefreshNow()))
		{
			Refresh();
			previousSearchRadius = TreeSearchRadius;
			previousPosition = TreeSearchPosition;
		}
	}

	private bool RefreshNow()
	{
		return !Balise.Editing && (previousSearchRadius != TreeSearchRadius || previousPosition != TreeSearchPosition);
	}

	private void Refresh()
	{
		HashSet<XYZ> hashSet = GetRegisteredTreesAroundBalise().ToPooledHashSet();
		RemoveOutdatedGoals(hashSet);
		AddNewGoals(hashSet);
		hashSet.Dispose();
		timeAtLastRefresh = Time.time;
	}

	private Slinq<XYZ, PredicateContext<XYZ, SelectContext<XYZ, KeyValuePair<XYZ, int>, DictionaryContext<XYZ, int>>, int>> GetRegisteredTreesAroundBalise()
	{
		return TreeRegistry.Instance.GetTreesInRadius(TreeSearchPosition, TreeSearchRadius);
	}

	private void RemoveOutdatedGoals(HashSet<XYZ> registeredTrees)
	{
		List<KeyValuePair<XYZ, TreeGoal>> list = goals.Slinq().Where((KeyValuePair<XYZ, TreeGoal> kvp, HashSet<XYZ> hashSet) => !hashSet.Contains(kvp.Key), registeredTrees).ToPooledList();
		list.ForEach(delegate(KeyValuePair<XYZ, TreeGoal> kvp)
		{
			ObjectPoolSingleton.Release(kvp.Value.gameObject);
		});
		list.Dispose();
	}

	private void AddNewGoals(HashSet<XYZ> registeredTrees)
	{
		registeredTrees.Slinq().Where((XYZ t, Dictionary<XYZ, TreeGoal> list) => !list.ContainsKey(t), goals).ForEach(delegate(XYZ t, TreeGoalProvider project)
		{
			CreateAndAddTreeGoal(t, project);
		}, this);
	}

	private static TreeGoal CreateAndAddTreeGoal(XYZ position, TreeGoalProvider self)
	{
		ObjectPoolSingleton.RequestComponent<TreeGoal>(out var t, Test.TreeGoal, position.ToVector3(), Quaternion.identity);
		t.OnReleaseOrDestroySignal.ConnectOnce(delegate
		{
			self.goals.Remove(position);
		});
		t.transform.SetParent(self.transform);
		self.goals.Add(position, t);
		return t;
	}
}
