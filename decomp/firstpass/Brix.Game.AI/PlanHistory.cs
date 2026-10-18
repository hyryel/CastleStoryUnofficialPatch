namespace Brix.Game.AI;

public class PlanHistory
{
	public Plan Previous;

	public void Push(Plan plan)
	{
		Previous = plan;
	}
}
