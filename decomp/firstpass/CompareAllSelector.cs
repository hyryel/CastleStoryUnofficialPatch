using Brix.Game.AI;

public class CompareAllSelector : GoalSelector
{
	protected override bool FoundBestGoal(GoalSelectionData data, Goal goal, float TimeConsumed = -1f)
	{
		return false;
	}
}
