using System;

[Flags]
public enum PlacementFlags
{
	none = 0,
	valid = 1,
	mayRemove = 2,
	placed = 4,
	buildable = 8,
	anti = 0x10,
	commited = 0x20
}
