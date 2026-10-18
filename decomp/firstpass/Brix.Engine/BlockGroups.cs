using System;

namespace Brix.Engine;

[Flags]
public enum BlockGroups
{
	None = 0,
	Block = 1,
	Structure = 2,
	Item = 4,
	Wood = 8,
	Terrain = 0x10
}
