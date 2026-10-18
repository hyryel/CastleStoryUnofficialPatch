using System.Collections.Generic;

namespace Brix.Game.AI;

public abstract class RecursivePlan : Plan
{
	public virtual bool Active => GetNth(0) != null;

	public abstract Plan GetNth(int n);

	public virtual IEnumerator<Plan> GetPlans()
	{
		int i = 0;
		while (true)
		{
			Plan nth = GetNth(i);
			if (nth == null)
			{
				break;
			}
			yield return nth;
			i++;
		}
	}
}
