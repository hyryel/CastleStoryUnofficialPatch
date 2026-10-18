using System;

namespace Brix.Assets;

[Flags]
public enum AssetQueryParameters
{
	Official = 1,
	Multiplayer = 2,
	UGC = 4,
	Workshop = 8,
	AllSPSources = 0xD,
	AllSources = 0xF,
	EditableSources = 7,
	RefreshCache = 0x10
}
