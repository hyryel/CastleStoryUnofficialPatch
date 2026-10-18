using System;

namespace Brix.Game.Components;

[Flags]
public enum SupportState
{
	Unsupported = 1,
	BPSupport = 2,
	CCSupport = 4,
	NearSupport = 8,
	FarSupport = 0x10,
	BPSolid = 0x20,
	CCSolid = 0x40,
	Valid = 0x80,
	ValidPath = 0x100,
	ConcreteEnd = 0x200
}
