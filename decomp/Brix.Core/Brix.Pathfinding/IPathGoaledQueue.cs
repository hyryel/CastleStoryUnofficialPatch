using Brix.Engine;

namespace Brix.Pathfinding;

public interface IPathGoaledQueue : IPathQueue
{
	XYZ GoalPosition { get; set; }
}
