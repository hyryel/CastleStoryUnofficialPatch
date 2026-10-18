using System.Collections.Generic;

namespace Brix.Game.AI;

public class PlanList : RecursivePlan
{
	public readonly List<Plan> Movements = new List<Plan>();

	public override bool Active => Movements.Count > 0;

	public PlanList(IEnumerable<Plan> movements)
	{
		Movements.AddRange(movements);
	}

	public override IEnumerator<Plan> GetPlans()
	{
		return Movements.GetEnumerator();
	}

	public override Plan GetNth(int n)
	{
		if (n < 0 || n >= Movements.Count)
		{
			return null;
		}
		return Movements[n];
	}
}
