using Brix.Game.AI;
using Brix.Game.Components;

public class SampleSelector : GoalSelector
{
	protected override bool FoundBestGoal(GoalSelectionData data, Goal goal, float TimeConsumed)
	{
		data.count++;
		if (data.BestPriority > data.bestPriorityBuffer || data.bestGoalBuffer.IsNullOrReleased())
		{
			data.bestGoalBuffer = goal;
			data.bestPriorityBuffer = data.BestPriority;
		}
		if (data.count <= data.SAMPLE_SIZE)
		{
			return false;
		}
		if (!data.bestGoalBuffer.IsNullOrReleased())
		{
			return true;
		}
		if (TimeConsumed > data.MAX_SEARCH_FRAMES)
		{
			return true;
		}
		return false;
	}
}
