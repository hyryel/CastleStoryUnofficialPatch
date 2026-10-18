using Brix.Game.AI;

public class FirstSelector : GoalSelector
{
	protected override bool FoundBestGoal(GoalSelectionData data, Goal goal, float TimeConsumed = -1f)
	{
		return true;
	}
}
