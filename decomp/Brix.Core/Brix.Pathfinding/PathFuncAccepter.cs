using System;

namespace Brix.Pathfinding;

public class PathFuncAccepter : PathAccepter
{
	public Func<PathLink, bool> Accept;

	private PathLink result;

	public PathLink Result => result;

	public PathFuncAccepter(Func<PathLink, bool> accept)
	{
		Accept = accept;
	}

	public void Reset(Func<PathLink, bool> accept)
	{
		Accept = accept;
		result = null;
	}

	public override bool Visit(PathLink link, PathLink fromSource)
	{
		if (!Accept(link))
		{
			return false;
		}
		result = link;
		return true;
	}
}
