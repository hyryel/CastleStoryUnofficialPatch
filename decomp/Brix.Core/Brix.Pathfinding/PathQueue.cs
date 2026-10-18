namespace Brix.Pathfinding;

public abstract class PathQueue : IPathQueue
{
	public abstract bool Empty { get; }

	public abstract void Clear();

	public abstract PathLink Dequeue();

	public abstract void Enqueue(PathLink link);
}
