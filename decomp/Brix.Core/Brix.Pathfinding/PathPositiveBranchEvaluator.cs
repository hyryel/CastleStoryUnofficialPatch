namespace Brix.Pathfinding;

public class PathPositiveBranchEvaluator : PathBranchEvaluator
{
	public override bool CanCross(PathBranch branch)
	{
		return (branch.Node.IsFull && branch.IsFull) || branch.Node.Lod == 0;
	}
}
