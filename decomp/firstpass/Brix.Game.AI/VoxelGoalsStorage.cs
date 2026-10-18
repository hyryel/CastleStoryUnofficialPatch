using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class VoxelGoalsStorage : GoalStorage, IGoalProvider
{
	public delegate void GoalOverrides(Goal goal);

	private Factory.AssetKey goalKey;

	private Func<XYZ, bool> filter;

	private Dictionary<XYZ, Goal> goalsPerVoxel;

	private GoalOverrides OverrideGoal;

	public IDigGoalResourceSetting DigGoalSetting;

	public VoxelGoalsStorage(IVoxelVolume volume, GameObject parent, Factory.AssetKey goalKey, GoalOverrides overrides, Func<XYZ, bool> filter)
	{
		goalsPerVoxel = new Dictionary<XYZ, Goal>();
		bounds = volume;
		SetParent(parent);
		this.goalKey = goalKey;
		this.filter = filter;
		OverrideGoal = overrides;
	}

	public override void AddGoal(Goal goal)
	{
		goalsPerVoxel.Add(goal.voxel.position, goal);
	}

	public override void RemoveGoal(Goal goal)
	{
		goalsPerVoxel.Remove(goal.voxel.position);
	}

	public override void RemoveOutdatedGoals()
	{
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		(from kvp in goalsPerVoxel.Slinq().Where((KeyValuePair<XYZ, Goal> kvp, VoxelGoalsStorage self) => kvp.Value == null || !self.filter(kvp.Key) || !self.bounds.Contains(kvp.Key), this)
			select kvp.Key).AddTo(list);
		RemoveGoals(goalsPerVoxel, list);
		list.Dispose();
	}

	protected void RemoveGoals<T>(Dictionary<XYZ, T> goalsPerVoxel, List<XYZ> toRemove) where T : Goal
	{
		int num = 0;
		foreach (XYZ item in toRemove)
		{
			if (!goalsPerVoxel.TryGetValue(item, out var value))
			{
				continue;
			}
			goalsPerVoxel.Remove(item);
			if (value != null)
			{
				ObjectPoolSingleton.Release(value.gameObject);
				if (++num % 30 == 0)
				{
					break;
				}
			}
		}
	}

	public override void FetchNewGoals()
	{
		GetNewGoals(bounds.Voxels, goalKey);
	}

	public Slinq<Goal, SelectContext<Goal, KeyValuePair<XYZ, Goal>, DictionaryContext<XYZ, Goal>>> Goals()
	{
		return from kv in goalsPerVoxel.Slinq()
			select kv.Value;
	}

	public int GoalCount()
	{
		return goalsPerVoxel.Count;
	}

	public Slinq<Goal, SelectContext<Goal, KeyValuePair<XYZ, Goal>, DictionaryContext<XYZ, Goal>>> All()
	{
		return Goals();
	}

	public void GetAll(ICollection<Goal> goals)
	{
		(from kv in goalsPerVoxel.Slinq()
			select kv.Value).AddTo(goals);
	}

	private bool WantsGoal(XYZ xyz)
	{
		Goal value;
		return (!goalsPerVoxel.TryGetValue(xyz, out value) || value == null) && filter(xyz);
	}

	private void GetNewGoals(IEnumerable<XYZ> voxels, Factory.AssetKey goalKey)
	{
		foreach (XYZ item in voxels.Slinq().Where((XYZ xyz, VoxelGoalsStorage self) => self.WantsGoal(xyz), this).NoLeak())
		{
			if (ObjectPoolSingleton.RequestComponent<Goal>(out var t, goalKey, item.ToVector3(), Quaternion.identity))
			{
				t.transform.SetParent(parent.transform);
				goalsPerVoxel[item] = t;
				OverrideGoal(t);
				DigGoal digGoal = t as DigGoal;
				if (!digGoal.IsNullOrReleased() && DigGoalSetting != null)
				{
					digGoal.ResourceSetting = DigGoalSetting;
				}
			}
		}
	}

	public override void RebuildDigGoalNodes()
	{
		if (!(goalKey == Test.DigGoal))
		{
			return;
		}
		foreach (Goal value in goalsPerVoxel.Values)
		{
			((DigGoal)value).RebuildNode();
		}
	}

	public bool HasNonStorable()
	{
		return goalKey != Test.DigGoal;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		if (goalKey != Test.DigGoal)
		{
			return;
		}
		foreach (KeyValuePair<XYZ, Goal> item in goalsPerVoxel)
		{
			DigGoal digGoal = item.Value as DigGoal;
			if (!digGoal.IsNullOrReleased())
			{
				digGoal.UniqueItems(ref keys);
			}
		}
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
	}
}
