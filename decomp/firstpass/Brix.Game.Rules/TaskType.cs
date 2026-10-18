using System;

namespace Brix.Game.Rules;

[Flags]
public enum TaskType
{
	None = 0,
	Construction = 1,
	Harvest = 2,
	Cleaning = 4,
	Tunnel = 8,
	Repair = 0x10,
	Landscape = 0x20,
	Quarry = 0x40,
	All = 0x7F,
	Any = 0xFF
}
