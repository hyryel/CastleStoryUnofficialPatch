using System.Collections.Generic;

namespace Brix.Game.AI;

public class CachedRecursivePlan : RecursivePlan
{
	protected IEnumerator<Plan> Enumerator;

	protected readonly List<Plan> Movements;

	public CachedRecursivePlan(IEnumerator<Plan> movements)
	{
		Movements = new List<Plan>();
		Enumerator = movements;
	}

	public void Reset(IEnumerator<Plan> movements)
	{
		Movements.Clear();
		Enumerator = movements;
	}

	public override Plan GetNth(int i)
	{
		if (i < 0)
		{
			return null;
		}
		if (i < Movements.Count)
		{
			return Movements[i];
		}
		if (Enumerator == null)
		{
			return null;
		}
		do
		{
			if (!Enumerator.MoveNext())
			{
				Enumerator = null;
				return null;
			}
			Movements.Add(Enumerator.Current);
		}
		while (Movements.Count <= i);
		return Movements[i];
	}
}
