using System;

namespace Brix.Game.Components;

[Flags]
public enum ValidationResult
{
	Obstructed = 1,
	Invalid = 2,
	Valid = 4,
	Buildable = 8
}
