using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Pathfinding;

public abstract class PathLod
{
	public static PathEmptyLod Empty = new PathEmptyLod();

	public abstract int Lod { get; }

	public abstract PathLod FinerLod { get; }

	public abstract PathLod CoarserLod { get; }

	public abstract IEnumerable<PathNode> PathNodes { get; }

	public abstract PathNode GetPathNode(XYZ lowResPosition);

	public abstract void GetAllPathNodesAt(XYZ lowResPosition, ICollection<PathNode> results);

	public abstract PathBranchIterator GetIterator();

	public IEnumerable<PathBranch> GetChildren(PathNode node)
	{
		return GetIterator().GetChildren(node);
	}

	public IEnumerable<PathBranch> GetSiblings(PathNode node)
	{
		return GetIterator().GetSiblings(node);
	}

	public PathBranch GetParent(PathNode node)
	{
		return GetIterator().GetParent(node);
	}
}
