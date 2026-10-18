namespace Brix.Game.AI;

public class WalkOnPathPlan : RecursivePlan
{
	public DestinationIterator PathIterator;

	public override bool Active => PathIterator.Active;

	public override Plan GetNth(int n)
	{
		WalkNode nth = PathIterator.GetNth(n);
		if (nth != null)
		{
			WalkOnLocationPlan walkOnLocationPlan = new WalkOnLocationPlan();
			walkOnLocationPlan.Location = nth;
			return walkOnLocationPlan;
		}
		return null;
	}
}
