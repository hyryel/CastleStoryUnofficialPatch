using System;

namespace Brix.Game.AI;

[Flags]
public enum ReleaseNodeWhen
{
	Ended = 4,
	Abandoned = 2,
	Invalid = 1,
	Dont = 0
}
