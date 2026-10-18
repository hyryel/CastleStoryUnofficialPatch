using System;

namespace Brix.Game.Components.Tracking;

[Flags]
public enum TrackingLayer
{
	Character = 1,
	SmallResources = 2,
	Structure = 4,
	Firefly = 8,
	BigResources = 0x10,
	Crystal = 0x20,
	Accessories = 0x40,
	WardLink = 0x80,
	WardLinkBlueprint = 0x100,
	All = 0x1FF,
	FogTracked = 0x1FF
}
