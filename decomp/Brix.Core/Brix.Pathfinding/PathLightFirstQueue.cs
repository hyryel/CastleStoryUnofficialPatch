namespace Brix.Pathfinding;

public class PathLightFirstQueue : PathPriorityQueue
{
	public override int Evaluate(PathLink link)
	{
		return link.length + link.weight;
	}
}
