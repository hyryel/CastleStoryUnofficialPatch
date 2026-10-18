using System.Collections.Generic;

namespace Brix.Game.AI;

public class MovementQueue
{
	protected CachedRecursivePlan Cached;

	protected readonly List<SimplePlan> Plans = new List<SimplePlan>();

	public int CurrentIndex;

	public bool Active => GetNth(0) != null;

	public SimplePlan Previous => GetNth(CurrentIndex - 1);

	public SimplePlan Current => GetNth(CurrentIndex);

	public SimplePlan Entering => GetNth(CurrentIndex + 1);

	public SimplePlan Projected => GetNth(CurrentIndex + 2);

	public MovementQueue(params Plan[] planArray)
	{
		IEnumerator<Plan> enumerator = ((IEnumerable<Plan>)planArray).GetEnumerator();
		Cached = new CachedRecursivePlan(GetSimplePlans(enumerator));
	}

	public MovementQueue(IEnumerator<Plan> plans)
	{
		Cached = new CachedRecursivePlan(GetSimplePlans(plans));
	}

	public void Reset(IEnumerator<Plan> plans)
	{
		Plans.Clear();
		Cached.Reset(GetSimplePlans(plans));
		CurrentIndex = 0;
	}

	public SimplePlan GetNth(int i)
	{
		return (SimplePlan)Cached.GetNth(i);
	}

	private static IEnumerator<Plan> GetSimplePlans(IEnumerator<Plan> plans)
	{
		while (plans.MoveNext())
		{
			Plan current = plans.Current;
			if (current is RecursivePlan recursive)
			{
				IEnumerator<Plan> e2 = GetSimplePlans(recursive.GetPlans());
				while (e2.MoveNext())
				{
					yield return e2.Current;
				}
			}
			else if (current is SimplePlan simple)
			{
				yield return simple;
			}
			else
			{
				yield return new FailPlan();
			}
		}
	}
}
