using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathLodBuilder : PathBuilder
{
	public PathLod pathLod;

	public List<PathLod> pathLods = new List<PathLod>();

	public List<PathBranchIterator> pathLodIterators = new List<PathBranchIterator>();

	public int lodMin = 0;

	public int lodMax = 0;

	public PathBranchEvaluator branchEvaluator;

	public PathBranchEvaluator BranchEvaluator => branchEvaluator;

	public PathLodBuilder(PathLod pathLod, PathBranchEvaluator branchEvaluator, int lodCount = -1)
	{
		this.pathLod = pathLod;
		this.branchEvaluator = branchEvaluator;
		Reset(lodCount);
	}

	public void Reset(PathLod pathLod, int lodCount = -1)
	{
		this.pathLod = pathLod;
		Reset(lodCount);
	}

	public void Reset(int lodCount = -1)
	{
		int num = 0;
		PathLod coarserLod = pathLod;
		while ((lodCount == -1 || num < lodCount) && coarserLod != PathLod.Empty)
		{
			if (num >= pathLods.Count)
			{
				pathLods.Add(coarserLod);
				pathLodIterators.Add(coarserLod.GetIterator());
			}
			else if (pathLods[num] != coarserLod)
			{
				pathLods[num] = coarserLod;
				pathLodIterators[num] = coarserLod.GetIterator();
			}
			num++;
			coarserLod = coarserLod.CoarserLod;
		}
		lodMin = pathLod.Lod;
		lodMax = lodMin + num - 1;
	}

	protected virtual bool IsGoalParent(PathNode node)
	{
		return false;
	}

	protected virtual bool IsSuitableBranch(PathBranch branch)
	{
		if (branch.Node.Lod > lodMin && IsGoalParent(branch.Node))
		{
			return false;
		}
		return branchEvaluator.CanCross(branch);
	}

	protected virtual bool IsSuitableParent(PathNode parentNode)
	{
		if (IsGoalParent(parentNode))
		{
			return false;
		}
		return (parentNode.Lod == lodMin && !parentNode.IsEmpty) || (parentNode.Lod <= lodMax && parentNode.IsFull);
	}

	public override void GetOuterPaths(PathLink link, List<PathLink> outPaths)
	{
		PathBranchIterator pathBranchIterator = pathLodIterators[link.head.Node.Lod - lodMin];
		PathBranch parent = pathBranchIterator.GetParent(link.head.Node);
		if (IsSuitableParent(parent.Node))
		{
			outPaths.Add(GetTmpPath(parent, link));
			return;
		}
		foreach (PathBranch sibling in pathBranchIterator.GetSiblings(link.head.Node))
		{
			if (IsSuitableBranch(sibling))
			{
				PathBranch branch = sibling;
				branchEvaluator.PostProcessBranch(ref branch);
				outPaths.Add(GetTmpPath(branch, link));
			}
			else
			{
				GetBranchesToSibling(link, sibling.Node, outPaths);
			}
		}
	}

	private void GetBranchesToSibling(PathLink link, PathNode sibling, List<PathLink> outPaths)
	{
		PathNode node = link.head.Node;
		if (node.Lod <= lodMin)
		{
			return;
		}
		PathBranchIterator pathBranchIterator = pathLodIterators[node.Lod - lodMin];
		PathBranchIterator pathBranchIterator2 = pathLodIterators[node.Lod - 1 - lodMin];
		foreach (PathBranch child in pathBranchIterator.GetChildren(node))
		{
			PathTmpLink tmpPath = GetTmpPath(child, link);
			PathNode node2 = child.Node;
			foreach (PathBranch sibling2 in pathBranchIterator2.GetSiblings(node2))
			{
				PathNode node3 = sibling2.Node;
				if (node3.ParentNode == sibling)
				{
					if (IsSuitableBranch(sibling2))
					{
						PathBranch branch = sibling2;
						branchEvaluator.PostProcessBranch(ref branch);
						outPaths.Add(GetTmpPath(branch, tmpPath));
					}
					else
					{
						GetBranchesToSibling(tmpPath, node3, outPaths);
					}
				}
			}
		}
	}

	public override void PushOuterPaths(PathLink link, PathSearch search)
	{
		PathBranchIterator pathBranchIterator = pathLodIterators[link.head.Node.Lod - lodMin];
		PathBranch parent = pathBranchIterator.GetParent(link.head.Node);
		if ((link.tail == null || link.tail.head.Node != parent.Node) && IsSuitableParent(parent.Node))
		{
			search.Push(parent, link);
			return;
		}
		foreach (PathBranch sibling in pathBranchIterator.GetSiblings(link.head.Node))
		{
			if (IsSuitableBranch(sibling))
			{
				PathBranch branch = sibling;
				branchEvaluator.PostProcessBranch(ref branch);
				search.Push(branch, link);
			}
			else
			{
				PushBranchesToSibling(link, sibling.Node, search);
			}
		}
	}

	private void PushBranchesToSibling(PathLink link, PathNode sibling, PathSearch search)
	{
		PathNode node = link.head.Node;
		if (node.Lod <= lodMin)
		{
			return;
		}
		PathBranchIterator pathBranchIterator = pathLodIterators[node.Lod - lodMin];
		PathBranchIterator pathBranchIterator2 = pathLodIterators[node.Lod - 1 - lodMin];
		foreach (PathBranch child in pathBranchIterator.GetChildren(node))
		{
			PathLink path = GetPath(child, link);
			PathNode node2 = child.Node;
			foreach (PathBranch sibling2 in pathBranchIterator2.GetSiblings(node2))
			{
				PathNode node3 = sibling2.Node;
				if (node3.ParentNode == sibling)
				{
					if (IsSuitableBranch(sibling2))
					{
						PathBranch branch = sibling2;
						branchEvaluator.PostProcessBranch(ref branch);
						search.Push(branch, path);
					}
					else
					{
						PushBranchesToSibling(path, node3, search);
					}
				}
			}
		}
	}

	public override void PushOuterPaths(PathNode node, PathSimplifiedSearch search)
	{
		PathBranchIterator pathBranchIterator = pathLodIterators[node.Lod - lodMin];
		PathBranch parent = pathBranchIterator.GetParent(node);
		if (IsSuitableParent(parent.Node))
		{
			search.Push(parent.Node, parent.Weight);
			return;
		}
		foreach (PathBranch sibling in pathBranchIterator.GetSiblings(node))
		{
			if (IsSuitableBranch(sibling))
			{
				search.Push(sibling.Node, sibling.Weight);
			}
			else
			{
				PushBranchesToSibling(node, sibling.Node, sibling.Weight, search);
			}
		}
	}

	private void PushBranchesToSibling(PathNode node, PathNode sibling, int weight, PathSimplifiedSearch search)
	{
		if (node.Lod <= lodMin)
		{
			return;
		}
		PathBranchIterator pathBranchIterator = pathLodIterators[node.Lod - lodMin];
		PathBranchIterator pathBranchIterator2 = pathLodIterators[node.Lod - 1 - lodMin];
		foreach (PathBranch child in pathBranchIterator.GetChildren(node))
		{
			PathNode node2 = child.Node;
			foreach (PathBranch sibling2 in pathBranchIterator2.GetSiblings(node2))
			{
				PathNode node3 = sibling2.Node;
				if (node3.ParentNode == sibling)
				{
					if (IsSuitableBranch(sibling2))
					{
						search.Push(node3, weight + sibling2.Weight);
					}
					else
					{
						PushBranchesToSibling(node2, node3, weight + sibling2.Weight, search);
					}
				}
			}
		}
	}
}
