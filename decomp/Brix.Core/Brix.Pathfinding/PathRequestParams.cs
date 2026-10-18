using System.Collections.Generic;

namespace Brix.Pathfinding;

public class PathRequestParams
{
	public PathNode Origin;

	public PathNode Destination;

	public PathLod PathLod;

	public int LodCount;

	public List<PathLink> OutPath;

	public PathBranchEvaluator BranchEvaluator = new PathNegativeBranchEvaluator();

	public bool UnrollLod;

	public bool AllowRequeueing;
}
