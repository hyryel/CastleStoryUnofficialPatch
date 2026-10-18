using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Pathfinding;

public abstract class PathBuilder
{
	protected class PathTmpLink : PathLink
	{
		public int i;

		public PathTmpLink(PathBuilder builder, PathBranch branch, PathLink tail, int i)
			: base(builder, branch, tail)
		{
			this.i = i;
		}
	}

	protected class PathTmpLinkPool : PathLinkPool<PathTmpLink>
	{
		protected override PathTmpLink Make(PathBuilder builder, PathBranch branch, PathLink tail, int internalIndex)
		{
			return new PathTmpLink(builder, branch, tail, internalIndex);
		}

		protected override void Reset(PathTmpLink res, PathBuilder builder, PathBranch branch, PathLink tail, int internalIndex)
		{
			res.Reset(builder, branch, tail);
		}
	}

	protected PathLinkPool Links = new PathLinkPool();

	protected PathTmpLinkPool TmpLinks = new PathTmpLinkPool();

	public abstract void GetOuterPaths(PathLink link, List<PathLink> outPaths);

	public abstract void PushOuterPaths(PathLink link, PathSearch search);

	public abstract void PushOuterPaths(PathNode node, PathSimplifiedSearch search);

	public virtual int ComputePathLength(PathBranch head, PathLink tail)
	{
		return tail.length + XYZ.ManhattanMagnitude(head.Node.RepresentativePosition - tail.head.Node.RepresentativePosition);
	}

	public virtual int ComputePathWeight(PathBranch head, PathLink tail)
	{
		return tail.weight + head.Weight;
	}

	public void GetPermanentPath(ref PathLink link)
	{
		if (link is PathTmpLink)
		{
			if (link.tail != null)
			{
				PathLink link2 = link.tail;
				link.tail = null;
				GetPermanentPath(ref link2);
				link.tail = link2;
			}
			link = GetPath(link.head, link.tail);
		}
	}

	public PathLink GetPath(PathBranch branch, PathLink tail)
	{
		return Links.GetPath(this, branch, tail);
	}

	protected PathTmpLink GetTmpPath(PathBranch branch, PathLink tail)
	{
		return TmpLinks.GetPath(this, branch, tail);
	}

	public void ClearTmpPaths()
	{
		TmpLinks.Clear();
	}

	public void ClearPaths()
	{
		Links.Clear();
	}
}
