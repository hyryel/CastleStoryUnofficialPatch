using Brix.Game.Semantique;

namespace Brix.Pathfinding.Test;

public class TestBranchEvaluator : PathBranchEvaluator
{
	public Recepteur BricktronRecepteur;

	public override bool CanCross(PathBranch branch)
	{
		if (branch.Type == PathBranch.BranchType.Climbing && !BricktronRecepteur.IsEmpty())
		{
			return false;
		}
		if (branch.Node.IsFull)
		{
			return true;
		}
		return false;
	}
}
