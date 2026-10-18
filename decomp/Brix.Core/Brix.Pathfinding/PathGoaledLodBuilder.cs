namespace Brix.Pathfinding;

public class PathGoaledLodBuilder : PathLodBuilder
{
	public PathNode Goal;

	public PathNode[] GoalParents;

	public PathGoaledLodBuilder(PathLod pathLod, PathBranchEvaluator branchEvaluator, PathNode goal, int lodCount = -1)
		: base(pathLod, branchEvaluator, lodCount)
	{
		Goal = goal;
		GoalParents = new PathNode[lodMax + 2 + 5];
		GoalParents[goal.Lod] = goal;
		for (int i = goal.Lod + 1; i <= lodMax; i++)
		{
			GoalParents[i] = GoalParents[i - 1].ParentNode;
		}
	}

	protected override bool IsGoalParent(PathNode node)
	{
		return node == GoalParents[node.Lod];
	}
}
