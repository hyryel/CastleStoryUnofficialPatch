using System;

namespace Brix.Steam;

[Flags]
public enum WorkshopItemTags
{
	Sandbox = 0x400000,
	Invasion = 0x800000,
	Conquest = 0x1000000
}
