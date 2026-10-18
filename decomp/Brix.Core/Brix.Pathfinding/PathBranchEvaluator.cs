namespace Brix.Pathfinding;

public abstract class PathBranchEvaluator
{
	public abstract bool CanCross(PathBranch branch);

	public virtual void PostProcessBranch(ref PathBranch branch)
	{
	}
}
