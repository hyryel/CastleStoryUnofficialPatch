namespace Brix.Pathfinding;

public class PathGoalAccepter : PathAccepter
{
	public PathNode Goal;

	private PathLink result;

	public PathLink Result => result;

	public PathGoalAccepter(PathNode goal)
	{
		Goal = goal;
	}

	public override bool Visit(PathLink link, PathLink fromSource)
	{
		if (link.head.Node != Goal)
		{
			return false;
		}
		result = link.Copy;
		return true;
	}
}
