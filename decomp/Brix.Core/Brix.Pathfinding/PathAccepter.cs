namespace Brix.Pathfinding;

public abstract class PathAccepter
{
	public abstract bool Visit(PathLink link, PathLink fromSource);
}
