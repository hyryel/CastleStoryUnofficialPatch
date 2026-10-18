using Brix.Game.AI;
using Brix.Game.Components;
using UnityEngine;

public class LocalMaximumSelector : GoalSelector
{
	protected override bool FoundBestGoal(GoalSelectionData data, Goal goal, float timeConsumed)
	{
		data.count++;
		if (data.BestPriority > data.bestPriorityBuffer || data.bestGoalBuffer.IsNullOrReleased())
		{
			data.bestGoalBuffer = goal;
			data.bestPriorityBuffer = data.BestPriority;
			data.foundNewBest = true;
		}
		if (data.count <= data.SAMPLE_SIZE)
		{
			return false;
		}
		if (data.foundNewBest)
		{
			data.SAMPLE_SIZE += Mathf.CeilToInt((float)data.count / 2f);
			data.foundNewBest = false;
			return false;
		}
		if (!data.bestGoalBuffer.IsNullOrReleased())
		{
			return true;
		}
		if (timeConsumed > data.MAX_SEARCH_FRAMES)
		{
			return true;
		}
		return false;
	}
}
