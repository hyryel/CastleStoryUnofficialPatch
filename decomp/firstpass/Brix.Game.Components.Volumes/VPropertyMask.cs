using System;

namespace Brix.Game.Components.Volumes;

[Flags]
public enum VPropertyMask : long
{
	access = 1L,
	obstacle = 2L,
	occupy = 4L,
	walkable = 8L,
	operation = 0x10L,
	fantome = 0x20L,
	geostatic = 0x40L,
	breakableAccess = 0x80L,
	invincible = 0x100L,
	natureBlocker = 0x200L,
	character = 0x400L,
	treeBase = 0x800L,
	debloc = 0x1000L,
	path = 0x2000L,
	fallback = 0x4000L,
	stoneSupport = 0x8000L,
	logSupport = 0x10000L,
	braceSupport = 0x20000L,
	beamSupport = 0x40000L,
	plankSupport = 0x80000L,
	mainSupportReq = 0x100000L,
	auxiliarySupportReq = 0x200000L,
	meshSwapTrigger = 0x400000L,
	preventMergeback = 0x800000L,
	deblocTerrain = 0x1000000L,
	_unused_25 = 0x2000000L,
	notWallSupport = 0x4000000L,
	canHookBeam = 0x8000000L,
	ropeSupport = 0x10000000L,
	stickySupport = 0x20000000L,
	characterAttackAccess = 0x40000000L,
	All = -1L
}
