using System;

namespace Brix.Pathfinding;

[Serializable]
public class PathSelectableBranchEvaluator : PathBranchEvaluator
{
	public enum ObstacleAccessibility
	{
		None,
		Lod0,
		Always
	}

	public PathBranch.BranchType AcceptedType = PathBranch.BranchType.Empty;

	public ObstacleAccessibility AcceptObstacle;

	public override bool CanCross(PathBranch branch)
	{
		if (branch.Type.CompareTo(AcceptedType) < 0)
		{
			return false;
		}
		if (branch.Node.IsFull)
		{
			return true;
		}
		if (AcceptObstacle == ObstacleAccessibility.None)
		{
			return false;
		}
		if (AcceptObstacle == ObstacleAccessibility.Always)
		{
			return true;
		}
		return branch.Node.Lod == 0;
	}
}
