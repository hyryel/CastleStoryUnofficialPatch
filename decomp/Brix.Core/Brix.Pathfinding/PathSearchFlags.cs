using System;

namespace Brix.Pathfinding;

[Flags]
public enum PathSearchFlags
{
	None = 0,
	AllowRequeueing = 1
}
