namespace Brix.Pathfinding;

public abstract class PathSimplifiedQueue
{
	public abstract bool Empty { get; }

	public abstract void Clear();

	public abstract PathNode Dequeue(out int weight);

	public abstract void Enqueue(PathNode node, int weight);
}
