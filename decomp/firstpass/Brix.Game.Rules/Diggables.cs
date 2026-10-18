using System;

namespace Brix.Game.Rules;

[Flags]
public enum Diggables
{
	None = 0,
	Rock = 1,
	Dirt = 2,
	Crust = 4,
	Artificial = 8,
	All = 0xF,
	Any = 0xFF
}
