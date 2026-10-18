namespace Brix.Pathfinding;

public class PathNegativeBranchEvaluator : PathBranchEvaluator
{
	public override bool CanCross(PathBranch branch)
	{
		return branch.Node.IsFull && branch.IsFull;
	}
}
