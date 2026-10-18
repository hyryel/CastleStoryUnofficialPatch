using System;

namespace Brix.Assets;

[Flags]
public enum AssetPlayableContext
{
	Singleplayer = 1,
	Multiplayer = 2,
	SPandMP = 3,
	WorldEditor = 4,
	Any = 0xFF
}
