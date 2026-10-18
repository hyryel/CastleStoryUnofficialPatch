using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathSimplifiedSearch
{
	public HashSet<PathNode> Searched;

	public PathSimplifiedQueue Queue;

	public PathBuilder Builder;

	public PathSimplifiedAccepter Accepter;

	public PathNode LastVisited;

	public int LastWeight;

	public PathSearchFlags Flags;

	public PathSimplifiedSearch(PathSimplifiedQueue queue, PathBuilder builder, PathSimplifiedAccepter accepter)
		: this(queue, builder, accepter, new HashSet<PathNode>())
	{
	}

	public PathSimplifiedSearch(PathSimplifiedQueue queue, PathBuilder builder, PathSimplifiedAccepter accepter, HashSet<PathNode> searched)
	{
		Searched = searched;
		Queue = queue;
		Builder = builder;
		Accepter = accepter;
	}

	public void Clear()
	{
		Queue.Clear();
		Searched.Clear();
		Builder.ClearPaths();
		LastVisited = null;
	}

	public void Start(PathNode origin)
	{
		origin = origin ?? PathNode.Empty;
		Queue.Enqueue(origin, 0);
		Searched.Add(origin);
	}

	public bool MoveNext()
	{
		LastVisited = Queue.Dequeue(out LastWeight);
		if (Accepter.Visit(LastVisited))
		{
			return false;
		}
		Builder.PushOuterPaths(LastVisited, this);
		Builder.ClearTmpPaths();
		return !Queue.Empty;
	}

	public void ForceImmediate()
	{
		while (MoveNext())
		{
		}
	}

	internal bool Push(PathNode node, int weight)
	{
		if (Searched.Add(node))
		{
			Queue.Enqueue(node, LastWeight + weight);
			return true;
		}
		return false;
	}
}
