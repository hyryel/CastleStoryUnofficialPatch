using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Pathfinding;

public class PathEmptyLod : PathLod
{
	public override int Lod => 0;

	public override PathLod FinerLod => PathLod.Empty;

	public override PathLod CoarserLod => PathLod.Empty;

	public override IEnumerable<PathNode> PathNodes => PathNode.None;

	public override PathNode GetPathNode(XYZ lowResPosition)
	{
		return PathNode.Empty;
	}

	public override void GetAllPathNodesAt(XYZ lowResPosition, ICollection<PathNode> results)
	{
	}

	public override PathBranchIterator GetIterator()
	{
		return PathBranchIterator.Empty;
	}
}
