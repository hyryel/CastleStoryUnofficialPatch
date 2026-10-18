using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathDoubleSearchAccepter : PathAccepter
{
	public PathLink fromA;

	public PathLink fromB;

	public Dictionary<PathNode, PathLink> visitedA;

	public Dictionary<PathNode, PathLink> visitedB;

	public PathLink ResultA => fromA;

	public PathLink ResultB => fromB;

	public PathDoubleSearchAccepter(PathLink fromA, PathLink fromB)
	{
		this.fromA = fromA;
		this.fromB = fromB;
		visitedA = new Dictionary<PathNode, PathLink>();
		visitedB = new Dictionary<PathNode, PathLink>();
	}

	public override bool Visit(PathLink link, PathLink fromSource)
	{
		if (fromSource == fromA)
		{
			if (visitedB.ContainsKey(link.head.Node))
			{
				fromA.Copy = link;
				fromB.tail = null;
				return true;
			}
			visitedA[link.head.Node] = link;
		}
		else
		{
			if (visitedA.ContainsKey(link.head.Node))
			{
				fromA.tail = null;
				fromB.Copy = link;
				return true;
			}
			visitedB[link.head.Node] = link;
		}
		return false;
	}
}
