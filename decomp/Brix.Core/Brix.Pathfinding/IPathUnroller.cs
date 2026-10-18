using System.Collections.Generic;

namespace Brix.Pathfinding;

public interface IPathUnroller
{
	bool PathExists { get; }

	IEnumerable<PathNode> Nodes { get; }

	IEnumerable<PathBranch> Branches { get; }

	void GetResultNodes(List<PathNode> outNodes);

	void GetResultBranches(List<PathBranch> outBranches);
}
