using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathDirectBuilder : PathBuilder
{
	protected PathLod PathLod;

	protected PathBranchEvaluator BranchEvaluator;

	protected PathBranchIterator Iterator;

	public PathDirectBuilder(PathLod pathLod, PathBranchEvaluator branchEvaluator)
	{
		PathLod = pathLod;
		BranchEvaluator = branchEvaluator;
		Iterator = pathLod.GetIterator();
	}

	public void Reset(PathLod pathLod)
	{
		if (PathLod != pathLod)
		{
			PathLod = pathLod;
			Iterator = pathLod.GetIterator();
		}
	}

	public void Reset(PathLod pathLod, PathBranchEvaluator branchEvaluator)
	{
		Reset(pathLod);
		BranchEvaluator = branchEvaluator;
	}

	protected virtual bool IsSuitableBranch(PathBranch branch)
	{
		return BranchEvaluator.CanCross(branch);
	}

	public override void GetOuterPaths(PathLink link, List<PathLink> outPaths)
	{
		foreach (PathBranch sibling in Iterator.GetSiblings(link.head.Node))
		{
			if (IsSuitableBranch(sibling))
			{
				PathBranch branch = sibling;
				BranchEvaluator.PostProcessBranch(ref branch);
				outPaths.Add(GetTmpPath(branch, link));
			}
		}
	}

	public override void PushOuterPaths(PathLink link, PathSearch search)
	{
		foreach (PathBranch sibling in Iterator.GetSiblings(link.head.Node))
		{
			if (IsSuitableBranch(sibling))
			{
				PathBranch branch = sibling;
				BranchEvaluator.PostProcessBranch(ref branch);
				search.Push(branch, link);
			}
		}
	}

	public override void PushOuterPaths(PathNode node, PathSimplifiedSearch search)
	{
		foreach (PathBranch sibling in Iterator.GetSiblings(node))
		{
			if (IsSuitableBranch(sibling))
			{
				search.Push(sibling.Node, sibling.Weight);
			}
		}
	}
}
