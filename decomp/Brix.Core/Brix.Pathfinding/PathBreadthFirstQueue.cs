using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathBreadthFirstQueue : PathQueue
{
	private Queue<PathLink> queue = new Queue<PathLink>();

	public override bool Empty => queue.Count == 0;

	public override void Clear()
	{
		queue.Clear();
	}

	public override PathLink Dequeue()
	{
		return queue.Dequeue();
	}

	public override void Enqueue(PathLink link)
	{
		queue.Enqueue(link);
	}
}
