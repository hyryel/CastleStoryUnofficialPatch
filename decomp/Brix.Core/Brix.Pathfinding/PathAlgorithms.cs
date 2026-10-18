using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathAlgorithms
{
	private static bool WritePath(PathLink successLink, PathLink sourceLink, PathBuilder builder, List<PathLink> outPath)
	{
		List<PathLink> list = new List<PathLink>();
		PathLink pathLink = successLink;
		while (true)
		{
			list.Add(pathLink);
			if (pathLink.head.Node == sourceLink.head.Node || pathLink.tail == null)
			{
				break;
			}
			pathLink = pathLink.tail;
		}
		list.Reverse();
		outPath.AddRange(list);
		return true;
	}

	public static IEnumerable<PathLink> Queued(PathLink source, IPathQueue queue, PathBuilder builder, PathAccepter accepter, bool allowRequeueing)
	{
		Dictionary<PathNode, PathLink> visited = new Dictionary<PathNode, PathLink>();
		PathSearch search = new PathSearch(queue, builder, accepter, visited);
		search.Flags = (allowRequeueing ? PathSearchFlags.AllowRequeueing : PathSearchFlags.None);
		search.Start(source);
		while (search.MoveNext())
		{
			yield return search.LastVisited;
		}
		yield return search.LastVisited;
	}

	public static IEnumerable<PathLink> DoubleQueued(PathLink source, PathLink goal, IPathQueue sourceQueue, PathQueue goalQueue, PathBuilder builder, bool allowRequeueing)
	{
		PathDoubleSearchAccepter accepter = new PathDoubleSearchAccepter(source, goal);
		new PathLink(source.head);
		new PathLink(goal.head);
		IEnumerator<PathLink> s1 = Queued(source, sourceQueue, builder, accepter, allowRequeueing).GetEnumerator();
		IEnumerator<PathLink> s2 = Queued(goal, goalQueue, builder, accepter, allowRequeueing).GetEnumerator();
		while (s1.MoveNext())
		{
			yield return s1.Current;
			if (!s2.MoveNext())
			{
				break;
			}
			yield return s2.Current;
		}
		PathLink goalOut;
		PathLink sourceOut;
		if (accepter.ResultA != null)
		{
			goalOut = accepter.ResultA;
			sourceOut = accepter.visitedB[goalOut.head.Node];
		}
		else
		{
			if (accepter.ResultB == null)
			{
				yield break;
			}
			sourceOut = accepter.ResultB;
			goalOut = accepter.visitedA[sourceOut.head.Node];
		}
		PathLink link = sourceOut;
		while (link.tail != null)
		{
			goalOut = new PathLink(builder, link.head, goalOut);
			link = link.tail;
		}
		goal.Copy = goalOut;
	}

	public static IEnumerable<PathLink> Queued(PathNode source, PathNode goal, List<PathLink> outPath, PathQueue queue, PathBuilder builder, bool allowRequeueing)
	{
		outPath.Clear();
		PathLink sourceLink = new PathLink(source);
		PathLink goalLink = new PathLink(goal);
		PathGoalAccepter accepter = new PathGoalAccepter(goal);
		foreach (PathLink item in Queued(sourceLink, queue, builder, accepter, allowRequeueing))
		{
			yield return item;
		}
		if (accepter.Result != null)
		{
			goalLink.Copy = accepter.Result;
			WritePath(goalLink, sourceLink, builder, outPath);
		}
	}

	public static IEnumerable<PathLink> DoubleQueued(PathNode source, PathNode goal, List<PathLink> outPath, IPathQueue sourceQueue, PathQueue goalQueue, PathBuilder builder, bool allowRequeueing)
	{
		outPath.Clear();
		PathLink sourceLink = new PathLink(source);
		PathLink goalLink = new PathLink(goal);
		foreach (PathLink item in DoubleQueued(sourceLink, goalLink, sourceQueue, goalQueue, builder, allowRequeueing))
		{
			yield return item;
		}
		if (goalLink.tail != null)
		{
			WritePath(goalLink, sourceLink, builder, outPath);
		}
	}

	public static IEnumerable<PathLink> QueuedLod(PathNode source, PathNode goal, List<PathLink> outPath, IPathQueue queue, PathLodBuilder builder, bool allowRequeueing)
	{
		outPath.Clear();
		PathLink sourceLink = new PathLink(source);
		PathLink goalLink = new PathLink(goal);
		PathGoalAccepter accepter = new PathGoalAccepter(goal);
		foreach (PathLink item in Queued(sourceLink, queue, builder, accepter, allowRequeueing))
		{
			yield return item;
		}
		if (accepter.Result != null)
		{
			goalLink.Copy = accepter.Result;
			WritePath(goalLink, sourceLink, builder, outPath);
		}
	}

	public static IEnumerable<PathLink> DoubleQueuedLod(PathNode source, PathNode goal, List<PathLink> outPath, IPathQueue sourceQueue, PathQueue goalQueue, PathLodBuilder builder, bool allowRequeueing)
	{
		outPath.Clear();
		PathLink sourceLink = new PathLink(source);
		PathLink goalLink = new PathLink(goal);
		foreach (PathLink item in DoubleQueued(sourceLink, goalLink, sourceQueue, goalQueue, builder, allowRequeueing))
		{
			yield return item;
		}
		if (goalLink.tail != null)
		{
			WritePath(goalLink, sourceLink, builder, outPath);
		}
	}

	public static IEnumerable<PathLink> Breadth(PathRequestParams p)
	{
		return Queued(p.Origin, p.Destination, p.OutPath, new PathBreadthFirstQueue(), new PathDirectBuilder(p.PathLod, p.BranchEvaluator), p.AllowRequeueing);
	}

	public static IEnumerable<PathLink> AStar(PathRequestParams p)
	{
		return Queued(p.Origin, p.Destination, p.OutPath, new PathAStarQueue(p.Destination.RepresentativePosition), new PathDirectBuilder(p.PathLod, p.BranchEvaluator), p.AllowRequeueing);
	}

	public static IEnumerable<PathLink> DoubleAStar(PathRequestParams p)
	{
		return DoubleQueued(p.Origin, p.Destination, p.OutPath, new PathAStarQueue(p.Destination.RepresentativePosition), new PathAStarQueue(p.Origin.RepresentativePosition), new PathDirectBuilder(p.PathLod, p.BranchEvaluator), p.AllowRequeueing);
	}

	public static IEnumerable<PathLink> BreadthLod(PathRequestParams p)
	{
		return QueuedLod(p.Origin, p.Destination, p.OutPath, new PathBreadthFirstQueue(), new PathGoaledLodBuilder(p.PathLod, p.BranchEvaluator, p.Destination, p.LodCount), p.AllowRequeueing);
	}

	public static IEnumerable<PathLink> AStarLod(PathRequestParams p)
	{
		return QueuedLod(p.Origin, p.Destination, p.OutPath, new PathAStarQueue(p.Destination.RepresentativePosition), new PathGoaledLodBuilder(p.PathLod, p.BranchEvaluator, p.Destination, p.LodCount), p.AllowRequeueing);
	}

	public static IEnumerable<PathLink> DoubleAStarLod(PathRequestParams p)
	{
		return DoubleQueuedLod(p.Origin, p.Destination, p.OutPath, new PathAStarQueue(p.Destination.RepresentativePosition), new PathAStarQueue(p.Origin.RepresentativePosition), new PathGoaledLodBuilder(p.PathLod, p.BranchEvaluator, p.Destination, p.LodCount), p.AllowRequeueing);
	}

	public static IEnumerable<PathLink> AStarLodUnrolled(PathRequestParams p)
	{
		return QueuedLod(p.Origin, p.Destination, p.OutPath, new PathAStarQueue(p.Destination.RepresentativePosition), new PathGoaledLodBuilder(p.PathLod, p.BranchEvaluator, p.Destination, p.LodCount), p.AllowRequeueing);
	}
}
