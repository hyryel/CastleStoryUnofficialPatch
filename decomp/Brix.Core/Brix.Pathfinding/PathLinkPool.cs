using System.Collections.Generic;

namespace Brix.Pathfinding;

public abstract class PathLinkPool<TPathLink> where TPathLink : PathLink
{
	protected List<TPathLink> Links = new List<TPathLink>();

	protected int LinkCount = 0;

	public TPathLink GetPath(PathBuilder builder, PathBranch branch, PathLink tail)
	{
		int num = LinkCount++;
		if (LinkCount > Links.Count)
		{
			TPathLink val = Make(builder, branch, tail, num);
			Links.Add(val);
			return val;
		}
		TPathLink val2 = Links[num];
		Reset(val2, builder, branch, tail, num);
		return val2;
	}

	public void Clear()
	{
		for (int i = 0; i < LinkCount; i++)
		{
			Links[i].head = PathBranch.Empty;
			Links[i].tail = null;
		}
		LinkCount = 0;
	}

	protected abstract TPathLink Make(PathBuilder builder, PathBranch branch, PathLink tail, int internalIndex);

	protected abstract void Reset(TPathLink res, PathBuilder builder, PathBranch branch, PathLink tail, int internalIndex);
}
public class PathLinkPool : PathLinkPool<PathLink>
{
	protected override PathLink Make(PathBuilder builder, PathBranch branch, PathLink tail, int internalIndex)
	{
		return new PathLink(builder, branch, tail);
	}

	protected override void Reset(PathLink res, PathBuilder builder, PathBranch branch, PathLink tail, int internalIndex)
	{
		res.Reset(builder, branch, tail);
	}
}
