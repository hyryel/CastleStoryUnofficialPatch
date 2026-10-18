using System.Collections.Generic;
using Brix.Utils;

namespace Brix.Pathfinding;

public class PathSearch
{
	public Dictionary<PathNode, PathLink> Searched;

	public IPathQueue Queue;

	public PathBuilder Builder;

	public PathAccepter Accepter;

	public PathLink LastVisited;

	public PathSearchFlags Flags;

	protected PathLink Source;

	public PathSearch(IPathQueue queue, PathBuilder builder, PathAccepter accepter)
		: this(queue, builder, accepter, new Dictionary<PathNode, PathLink>())
	{
	}

	public PathSearch(IPathQueue queue, PathBuilder builder, PathAccepter accepter, Dictionary<PathNode, PathLink> searched)
	{
		Searched = searched;
		Queue = queue;
		Builder = builder;
		Accepter = accepter;
	}

	public void Clear()
	{
		Dictionary<PathNode, PathLink>.Enumerator enumerator = Searched.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PathLink value = enumerator.Current.Value;
			ObjectPool<PathLink>.Release(value);
		}
		Queue.Clear();
		Searched.Clear();
		Builder.ClearPaths();
		LastVisited = null;
	}

	public void Start(PathNode origin)
	{
		Source = GetLink();
		Source.Reset(origin);
		Start();
	}

	public void Start(PathLink source)
	{
		Source = GetLink();
		Source.Copy = source;
		Start();
	}

	private void Start()
	{
		Queue.Enqueue(Source);
		Searched.Add(Source.head.Node, Source);
	}

	public bool MoveNext()
	{
		LastVisited = Queue.Dequeue();
		if (Accepter.Visit(LastVisited, Source))
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

	internal void Push(PathBranch outerBranch, PathLink fromLink)
	{
		if (!Searched.TryGetValue(outerBranch.Node, out var value))
		{
			Builder.GetPermanentPath(ref fromLink);
			PathLink link = GetLink();
			link.Reset(Builder, outerBranch, fromLink);
			Searched.Add(link.head.Node, link);
			Queue.Enqueue(link);
			return;
		}
		int num = Builder.ComputePathWeight(outerBranch, fromLink);
		if (num < value.weight)
		{
			Builder.GetPermanentPath(ref fromLink);
			int length = Builder.ComputePathLength(outerBranch, fromLink);
			value.Reset(outerBranch, fromLink, length, num);
			if ((Flags & PathSearchFlags.AllowRequeueing) != PathSearchFlags.None)
			{
				Queue.Enqueue(value);
			}
		}
	}

	private PathLink GetLink()
	{
		return ObjectPool<PathLink>.Reserve();
	}
}
