using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathUnrollBuilder : PathDirectBuilder
{
	public PathNode ParentNode;

	public PathUnrollBuilder(PathNode parentNode, PathLod pathLod, PathBranchEvaluator branchEvaluator)
		: base(pathLod, branchEvaluator)
	{
		ParentNode = parentNode;
	}

	public void Reset(PathNode parentNode, PathLod pathLod, PathBranchEvaluator branchEvaluator)
	{
		Reset(pathLod);
		ParentNode = parentNode;
	}

	public override void GetOuterPaths(PathLink link, List<PathLink> outPaths)
	{
		if (link.head.Node.ParentNode == ParentNode)
		{
			base.GetOuterPaths(link, outPaths);
		}
	}

	public override void PushOuterPaths(PathLink link, PathSearch search)
	{
		if (link.head.Node.ParentNode == ParentNode)
		{
			base.PushOuterPaths(link, search);
		}
	}
}
