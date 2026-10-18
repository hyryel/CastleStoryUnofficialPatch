namespace Brix.Pathfinding;

public interface IPathQueue
{
	bool Empty { get; }

	void Clear();

	PathLink Dequeue();

	void Enqueue(PathLink link);
}
