using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI;

public abstract class GoalSelector
{
	public enum Type
	{
		First,
		CompareAll,
		GoodEnoughFromSample,
		LocalMaximum,
		SampleSelector,
		GoodEnoughInTypeCategory
	}

	public class GoalSelectionData : IPoolable, IResetable, IDisposable
	{
		public int SAMPLE_SIZE = 10;

		public float MAX_SEARCH_FRAMES = 60f;

		public int count;

		public Goal bestGoalBuffer;

		public int bestPriorityBuffer;

		public Goal BestGoal;

		public int BestPriority = int.MinValue;

		public bool foundNewBest;

		public float timeConsumed;

		public float timeSkipped;

		public bool foundUnmapped;

		public bool isDone;

		public Type Type
		{
			set
			{
				if (value == Type.LocalMaximum)
				{
					SAMPLE_SIZE = 20;
				}
			}
		}

		public static GoalSelectionData Get()
		{
			return ObjectPool<GoalSelectionData>.Reserve();
		}

		public void Dispose()
		{
			ObjectPool<GoalSelectionData>.Release(this);
		}

		public void ResetObject()
		{
			SAMPLE_SIZE = 10;
			MAX_SEARCH_FRAMES = 60f;
			count = 0;
			bestGoalBuffer = null;
			bestPriorityBuffer = 0;
			BestGoal = null;
			BestPriority = int.MinValue;
			foundNewBest = false;
			timeConsumed = 0f;
			timeSkipped = 0f;
			foundUnmapped = false;
			isDone = false;
		}
	}

	private AIErrorsStore errorStore;

	public Type SelectorType;

	private Func<Labor, Goal, int, Goal, int, bool> IsBetter = HasBetterPriority;

	protected abstract bool FoundBestGoal(GoalSelectionData data, Goal goal, float TimeConsumed = -1f);

	public static GoalSelector GetSelectorFromType(Type type, AIErrorsStore store, bool chooseWithAccess = false)
	{
		GoalSelector goalSelector = new CompareAllSelector();
		if (type == Type.First)
		{
			goalSelector = new FirstSelector();
		}
		if (type == Type.CompareAll)
		{
			goalSelector = new CompareAllSelector();
		}
		if (type == Type.GoodEnoughFromSample)
		{
			goalSelector = new GoodEnoughFromSampleSelector();
		}
		if (type == Type.LocalMaximum)
		{
			goalSelector = new LocalMaximumSelector();
		}
		if (type == Type.SampleSelector)
		{
			goalSelector = new SampleSelector();
		}
		if (type == Type.GoodEnoughInTypeCategory)
		{
			goalSelector = new GoodEnoughInTypeCategory();
		}
		if (chooseWithAccess)
		{
			goalSelector.IsBetter = HasBetterPriorityOrAccess;
		}
		goalSelector.errorStore = store;
		goalSelector.SelectorType = type;
		return goalSelector;
	}

	public Node SelectBestGoal<TEnumerator>(Labor labor, TEnumerator goals, Project project, Var<Goal> result) where TEnumerator : IEnumerator<Goal>
	{
		PooledBoxedEnumerator<Goal, TEnumerator> goals2 = PooledBoxedEnumerator<Goal>.Request(goals);
		return _SelectBestGoal(labor, goals2, project, result);
	}

	private Node _SelectBestGoal(Labor labor, IEnumerator<Goal> goals, Project project, Var<Goal> result)
	{
		GoalSelectionData goalSelectionData = GoalSelectionData.Get();
		Looper.Builder n = Node.Sequence.Do((GoalSelector self, Tuple<Labor, IEnumerator<Goal>, Project, GoalSelectionData> t) => self.ProcessGoal(t._1, t._2, t._3, t._4), this, Tuple.Create(labor, goals, project, goalSelectionData)).Loop().Until((GoalSelectionData _data) => _data.isDone, goalSelectionData);
		Brancher.Builder n2 = Node.Selector.If((GoalSelector self, Labor _labor, GoalSelectionData _data) => self.GoalIsRejected(_labor, _data.BestGoal, _data.foundUnmapped), this, labor, goalSelectionData).Do(Node.Fail(SharedWorkException<Exceptions.GoalUnavailable>.Get)).Else()
			.Do(delegate(GoalSelector self, Labor _labor)
			{
				self.errorStore.Reset(_labor);
			}, this, labor)
			.Do(delegate(GoalSelectionData _data, Var<Goal> _result)
			{
				_result.Set(_data.BestGoal);
			}, goalSelectionData, result);
		return (StaticNode)Node.Sequence.Do(n).Do(n2).Finally(delegate(GoalSelectionData _data, IEnumerator<Goal> _goals)
		{
			_data.Dispose();
			_goals.Dispose();
		}, goalSelectionData, goals);
	}

	private Node ProcessGoal(Labor labor, IEnumerator<Goal> goals, Project project, GoalSelectionData data)
	{
		if (labor.IsNullOrReleased())
		{
			return Node.Fail(SharedWorkException<Exceptions.NullLabor>.Get);
		}
		while (goals.MoveNext())
		{
			Goal current = goals.Current;
			if (current == null)
			{
				return Node.WaitFor.Ticks(1);
			}
			if (ProcessGoalAndReturnIsBest(labor, current, project, data))
			{
				break;
			}
			if (CheckEnoughTimeConsumed(data))
			{
				return Node.WaitFor.Ticks(1);
			}
		}
		data.isDone = true;
		return Node.Empty;
	}

	private static bool CheckEnoughTimeConsumed(GoalSelectionData selectionData)
	{
		if (selectionData.timeConsumed - selectionData.timeSkipped > 1f)
		{
			selectionData.timeSkipped = selectionData.timeConsumed;
			return true;
		}
		return false;
	}

	private bool ProcessGoalAndReturnIsBest(Labor labor, Goal goal, Project project, GoalSelectionData selectionData)
	{
		if (goal.IsNullOrReleased())
		{
			return false;
		}
		if (!goal.IsMatchFor(labor, errorStore, ref selectionData.timeConsumed, ref selectionData.foundUnmapped) || selectionData.foundUnmapped)
		{
			return false;
		}
		project.GoalMatchedSignal.Invoke(labor, goal);
		int num = goal.PriorityFor(labor, ref selectionData.timeConsumed);
		if (IsBetter(labor, goal, num, selectionData.BestGoal, selectionData.BestPriority))
		{
			selectionData.BestGoal = goal;
			selectionData.BestPriority = num;
		}
		return FoundBestGoal(selectionData, goal, selectionData.timeConsumed);
	}

	private bool GoalIsRejected(Labor labor, Goal bestGoal, bool foundUnmapped)
	{
		if (bestGoal.IsNullOrReleased())
		{
			return true;
		}
		if (!foundUnmapped)
		{
			return false;
		}
		return false;
	}

	private static bool HasBetterPriority(Labor labor, Goal currentGoal, int currentPriority, Goal bestGoal, int bestPriority)
	{
		return (currentPriority > bestPriority) ? true : false;
	}

	private static bool HasBetterPriorityOrAccess(Labor labor, Goal currentGoal, int currentPriority, Goal bestGoal, int bestPriority)
	{
		if (bestGoal == null)
		{
			return true;
		}
		if (currentPriority > bestPriority)
		{
			return true;
		}
		if (currentPriority < bestPriority)
		{
			return false;
		}
		if (currentGoal == null)
		{
			return false;
		}
		return IsPlusProche(labor, currentGoal, bestGoal);
	}

	private static bool HasImmediateAccessTo(Labor labor, Goal goal)
	{
		if (goal == null)
		{
			return false;
		}
		return VolumeRegistry.VolumesWithPropertyAt(labor.presence.CurrentVoxel, VProperty.access).Slinq().Any((IVolume vol, Goal _goal) => vol.GameObject == _goal.gameObject, goal);
	}

	private static bool IsPlusProche(Labor labor, Goal current, Goal best)
	{
		Vector3 vector = Calcule.RoundVecteur(labor.transform.position);
		Vector3 vector2 = Calcule.RoundVecteur(current.transform.position);
		Vector3 vector3 = Calcule.RoundVecteur(best.transform.position);
		return (vector - vector2).sqrMagnitude < (vector - vector3).sqrMagnitude;
	}
}
