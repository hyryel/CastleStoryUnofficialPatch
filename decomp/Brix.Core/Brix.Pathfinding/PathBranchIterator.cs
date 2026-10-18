namespace Brix.Pathfinding;

public abstract class PathBranchIterator
{
	public class EmptyIterator : PathBranchIterator
	{
		public override PathBranchEnumerator GetChildren(PathNode node)
		{
			return PathBranchEnumerator.Empty;
		}

		public override PathBranchEnumerator GetSiblings(PathNode node)
		{
			return PathBranchEnumerator.Empty;
		}

		public override PathBranch GetParent(PathNode node)
		{
			return PathBranch.Empty;
		}
	}

	public static EmptyIterator Empty = new EmptyIterator();

	public abstract PathBranchEnumerator GetChildren(PathNode node);

	public abstract PathBranchEnumerator GetSiblings(PathNode node);

	public virtual PathBranch GetParent(PathNode node)
	{
		return new PathBranch(node.ParentNode, node.WeightToParent);
	}
}
