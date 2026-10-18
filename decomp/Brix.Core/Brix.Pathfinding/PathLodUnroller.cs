using System;
using System.Collections.Generic;
using System.Linq;

namespace Brix.Pathfinding;

public class PathLodUnroller : IPathUnroller
{
	public class PathGoalFuncAccepter : PathAccepter
	{
		public Func<PathNode, PathLink, bool> Accept;

		public PathNode Goal;

		private PathLink _result;

		public PathLink Result => _result;

		public void Reset(PathNode goal, Func<PathNode, PathLink, bool> accept)
		{
			Goal = goal;
			Accept = accept;
			_result = null;
		}

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			if (!Accept(Goal, link))
			{
				return false;
			}
			_result = link;
			return true;
		}
	}

	public PathNode Origin;

	public PathNode Destination;

	public PathLod PathLod;

	public IPathQueue Queue;

	public PathUnrollBuilder Builder;

	public PathSearch Search;

	public PathLinkPool ResultLinks;

	public PathLink SuccessLink;

	public PathLink ReverseSuccessLink;

	public PathGoalFuncAccepter Accepter;

	protected int LodMin;

	protected List<PathLod> PathLods;

	public bool PathExists => SuccessLink != null && !SuccessLink.head.Node.IsEmpty;

	public IEnumerable<PathNode> Nodes => from branch in GetDirectBranches()
		select branch.Node;

	public IEnumerable<PathBranch> Branches => GetDirectBranches();

	public PathLodUnroller(PathLink successLink, PathLod pathLod, PathBranchEvaluator branchEvaluator, IPathQueue queue)
	{
		InitPathLods(pathLod);
		ResultLinks = new PathLinkPool();
		Queue = queue;
		Builder = new PathUnrollBuilder(null, pathLod, branchEvaluator);
		Accepter = new PathGoalFuncAccepter();
		Search = new PathSearch(Queue, Builder, Accepter);
		InitSuccessLink(successLink);
	}

	public void Reset(PathLink successLink)
	{
		ResultLinks.Clear();
		Search.Clear();
		InitSuccessLink(successLink);
	}

	public void Reset(PathLink successLink, PathLod pathLod, PathBranchEvaluator branchEvaluator, IPathQueue queue)
	{
		InitPathLods(pathLod);
		ResultLinks.Clear();
		Queue = queue;
		Builder.Reset(null, pathLod, branchEvaluator);
		Accepter.Reset(null, null);
		Search.Clear();
		InitSuccessLink(successLink);
	}

	private void InitPathLods(PathLod pathLod)
	{
		PathLod = pathLod;
		LodMin = pathLod.Lod;
		if (PathLods == null)
		{
			PathLods = new List<PathLod>();
		}
		else
		{
			PathLods.Clear();
		}
		while (pathLod != PathLod.Empty)
		{
			PathLods.Add(pathLod);
			pathLod = pathLod.CoarserLod;
		}
	}

	private void InitSuccessLink(PathLink successLink)
	{
		PathLink pooledLink = GetPooledLink(successLink.head);
		for (PathLink tail = successLink.tail; tail != null; tail = tail.tail)
		{
			pooledLink = GetPooledLink(tail.head, pooledLink);
		}
		ReverseSuccessLink = pooledLink;
		SuccessLink = successLink;
		Origin = ReverseSuccessLink.head.Node;
		Destination = SuccessLink.head.Node;
	}

	private void Refine(PathLink link)
	{
		if (link.head.IsEmpty || link.tail == null || link.tail.head.Node.Lod <= link.head.Node.Lod)
		{
			return;
		}
		Refine(link.tail);
		if (link.head.Node == link.tail.head.Node)
		{
			link.tail = link.tail.tail;
			Refine(link);
		}
		else
		{
			if (link.tail.tail == null)
			{
				return;
			}
			if (link.head.Node == link.tail.tail.head.Node)
			{
				link.tail = link.tail.tail.tail;
				Refine(link);
				return;
			}
			PathNode node = link.tail.tail.head.Node;
			PathNode approxGoal = ((link.tail.tail.tail != null) ? link.tail.tail.tail.head.Node : node);
			PathLink pathLink = SearchPathTo(GetPooledLink(link.head), node, approxGoal);
			if (pathLink != null)
			{
				link.tail = link.tail.tail;
				if (pathLink.head.Node == node && pathLink.tail != null)
				{
					pathLink = pathLink.tail;
				}
				while (pathLink.tail != null)
				{
					link.tail = GetPooledLink(pathLink.head, link.tail);
					pathLink = pathLink.tail;
				}
			}
		}
	}

	private PathLink GetPooledLink(PathBranch head, PathLink tail = null)
	{
		return ResultLinks.GetPath(Builder, head, tail);
	}

	private IEnumerable<PathBranch> GetDirectBranches()
	{
		for (PathLink link = ReverseSuccessLink; link != null; link = link.tail)
		{
			Refine(link);
			yield return link.head;
		}
	}

	public void GetResultNodes(List<PathNode> outNodes)
	{
		for (PathLink pathLink = ReverseSuccessLink; pathLink != null; pathLink = pathLink.tail)
		{
			Refine(pathLink);
			outNodes.Add(pathLink.head.Node);
		}
	}

	public void GetResultBranches(List<PathBranch> outBranches)
	{
		for (PathLink pathLink = ReverseSuccessLink; pathLink != null; pathLink = pathLink.tail)
		{
			Refine(pathLink);
			outBranches.Add(pathLink.head);
		}
	}

	private PathLink SearchPathTo(PathLink source, PathNode goal, PathNode approxGoal)
	{
		if (goal.Lod == source.head.Node.Lod)
		{
			return SearchPathTo(source, goal, (PathNode _goal, PathLink link) => link.head.Node == _goal, approxGoal);
		}
		if (goal.Lod > source.head.Node.Lod)
		{
			if (goal.Lod == source.head.Node.Lod + 1)
			{
				return SearchPathTo(source, goal, (PathNode _goal, PathLink link) => link.head.Node.ParentNode == _goal, approxGoal);
			}
			PathNode parentNode = source.head.Node.ParentNode;
			if (parentNode.IsEmpty)
			{
				return null;
			}
			source = GetPooledLink(new PathBranch(parentNode), source);
			return SearchPathTo(source, goal, approxGoal);
		}
		PathLink pathLink = SearchPathTo(source, goal.ParentNode, approxGoal);
		if (pathLink != null)
		{
			pathLink.head = new PathBranch(goal, pathLink.head.Type, pathLink.head.Weight);
		}
		return pathLink;
	}

	private PathLink SearchPathTo(PathLink source, PathNode goal, Func<PathNode, PathLink, bool> accept, PathNode approxGoal)
	{
		Accepter.Reset(goal, accept);
		Builder.ParentNode = source.head.Node.ParentNode;
		Builder.Reset(PathLods[source.head.Node.Lod - LodMin]);
		if (Queue is IPathGoaledQueue pathGoaledQueue)
		{
			pathGoaledQueue.GoalPosition = approxGoal.RepresentativePosition;
		}
		Search.Clear();
		Search.Start(source);
		Search.ForceImmediate();
		return Accepter.Result;
	}
}
