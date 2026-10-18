using System.Collections.Generic;

namespace Brix.Game.AI;

public class PlanIterator
{
	public IEnumerator<Plan> Enumerator;

	public PlanIterator ChildIterator;

	public SimplePlan Current => (ChildIterator == null) ? (Enumerator.Current as SimplePlan) : ChildIterator.Current;

	public bool MoveNext()
	{
		while (true)
		{
			if (ChildIterator != null)
			{
				if (ChildIterator.MoveNext())
				{
					return true;
				}
				ChildIterator = null;
			}
			if (!Enumerator.MoveNext())
			{
				return false;
			}
			Plan current = Enumerator.Current;
			if (current == null)
			{
				return false;
			}
			if (!(current is RecursivePlan recursivePlan))
			{
				break;
			}
			ChildIterator = new PlanIterator
			{
				Enumerator = recursivePlan.GetPlans()
			};
		}
		return true;
	}

	public void Reset(IEnumerator<Plan> enumerator)
	{
		Enumerator = enumerator;
		ChildIterator = null;
	}
}
