using System.Collections.Generic;
using System.Linq;
using Brix.Engine;

namespace Brix.Pathfinding;

public abstract class PathLowerLod : PathLod
{
	public PathLod coarserLod = PathLod.Empty;

	public override int Lod => 0;

	public override PathLod FinerLod => PathLod.Empty;

	public override PathLod CoarserLod => coarserLod;

	public override IEnumerable<PathNode> PathNodes => PathDirectNodes.Cast<PathNode>();

	public abstract IEnumerable<PathDirectNode> PathDirectNodes { get; }

	public override void GetAllPathNodesAt(XYZ lowResPosition, ICollection<PathNode> results)
	{
		PathNode pathNode = GetPathNode(lowResPosition);
		if (!pathNode.IsEmpty)
		{
			results.Add(pathNode);
		}
	}
}
