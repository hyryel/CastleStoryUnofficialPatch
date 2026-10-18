using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Utils;

namespace Brix.Pathfinding;

public class PathLink : IPoolable, IResetable, IDisposable
{
	public PathBranch head;

	public PathLink tail;

	public int length;

	public int weight;

	public IEnumerable<PathLink> Links
	{
		get
		{
			for (PathLink link = this; link != null; link = link.tail)
			{
				yield return link;
			}
		}
	}

	public IEnumerable<PathNode> Nodes
	{
		get
		{
			for (PathLink link = this; link != null; link = link.tail)
			{
				yield return link.head.Node;
			}
		}
	}

	public IEnumerable<PathNode> ReversedNodes => Nodes.Reverse();

	public IEnumerable<PathBranch> Branches
	{
		get
		{
			for (PathLink link = this; link != null; link = link.tail)
			{
				yield return link.head;
			}
		}
	}

	public IEnumerable<PathBranch> ReversedBranches => Branches.Reverse();

	public PathLink Copy
	{
		get
		{
			return new PathLink
			{
				head = head,
				tail = tail,
				weight = weight,
				length = length
			};
		}
		set
		{
			head = value.head;
			tail = value.tail;
			weight = value.weight;
			length = value.length;
		}
	}

	public void GetNodes(List<PathNode> outNodes)
	{
		for (PathLink pathLink = this; pathLink != null; pathLink = pathLink.tail)
		{
			outNodes.Add(pathLink.head.Node);
		}
	}

	public void GetBranches(List<PathBranch> outBranches)
	{
		for (PathLink pathLink = this; pathLink != null; pathLink = pathLink.tail)
		{
			outBranches.Add(pathLink.head);
		}
	}

	public PathLink()
	{
	}

	public PathLink(PathNode node)
	{
		head = new PathBranch(node);
	}

	public PathLink(PathBranch head)
	{
		this.head = head;
	}

	public PathLink(PathBuilder builder, PathBranch head, PathLink tail)
	{
		this.head = head;
		this.tail = tail;
		if (tail != null)
		{
			length = builder.ComputePathLength(head, tail);
			weight = builder.ComputePathWeight(head, tail);
		}
	}

	public void Reset(PathBranch head, PathLink tail, int length, int weight)
	{
		this.head = head;
		this.tail = tail;
		this.length = length;
		this.weight = weight;
	}

	public void Reset(PathBuilder builder, PathBranch head, PathLink tail)
	{
		this.head = head;
		this.tail = tail;
		if (tail != null)
		{
			length = builder.ComputePathLength(head, tail);
			weight = builder.ComputePathWeight(head, tail);
		}
		else
		{
			length = 0;
			weight = head.weight;
		}
	}

	public void Reset(PathNode node)
	{
		head = new PathBranch(node);
		tail = null;
		length = 0;
		weight = 0;
	}

	public void Dispose()
	{
	}

	public void ResetObject()
	{
		head = default(PathBranch);
		tail = null;
		length = 0;
		weight = 0;
	}
}
