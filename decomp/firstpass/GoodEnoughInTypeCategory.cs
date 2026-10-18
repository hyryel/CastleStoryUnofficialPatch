using Brix.Game.AI;
using Brix.Game.Components;

public class GoodEnoughInTypeCategory : GoalSelector
{
	protected override bool FoundBestGoal(GoalSelectionData data, Goal goal, float TimeConsumed)
	{
		bool flag = false;
		data.count++;
		if (!data.bestGoalBuffer.IsNullOrReleased() && goal.GetType() == data.bestGoalBuffer.GetType())
		{
			return true;
		}
		if (data.BestPriority > data.bestPriorityBuffer || data.bestGoalBuffer.IsNullOrReleased())
		{
			data.bestGoalBuffer = goal;
			data.bestPriorityBuffer = data.BestPriority;
			flag = true;
		}
		if (data.count <= data.SAMPLE_SIZE)
		{
			return false;
		}
		if (flag)
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
