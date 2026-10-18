using System.Collections.Generic;

namespace Brix.Game.AI;

public class MovementPlanner
{
	protected readonly List<Plan> Plans = new List<Plan>();

	protected readonly List<SimplePlan> NextPlans = new List<SimplePlan>();

	protected PlanIterator Iterator = new PlanIterator();

	public int CurrentIndex;

	public int MaxIndex;

	public bool Active => GetNext(0) != null;

	public SimplePlan Current => GetNext(0);

	public SimplePlan Entering => GetNext(1);

	public SimplePlan Projected => GetNext(2);

	public MovementPlanner()
	{
		Iterator.Reset(GetNextPlanned());
	}

	public void Clear()
	{
		Plans.Clear();
		NextPlans.Clear();
		CurrentIndex = 0;
		MaxIndex = 0;
		Iterator.Reset(GetNextPlanned());
	}

	public void AddMovement(Plan plan)
	{
		Plans.Add(plan);
	}

	public void AddMovements(params Plan[] planArray)
	{
		Plans.AddRange(planArray);
	}

	public void AdvanceToNext()
	{
		NextPlans.Clear();
		CurrentIndex++;
		Iterator.Reset(GetNextPlanned());
	}

	private IEnumerator<Plan> GetNextPlanned()
	{
		for (int i = CurrentIndex; i < Plans.Count; i++)
		{
			yield return Plans[i];
		}
	}

	public SimplePlan GetNext(int i)
	{
		if (i < 0)
		{
			return null;
		}
		while (NextPlans.Count <= i)
		{
			if (!Iterator.MoveNext())
			{
				return null;
			}
			NextPlans.Add(Iterator.Current);
		}
		return NextPlans[i];
	}
}
