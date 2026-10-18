using System;

namespace Brix.Game.Components.Volumes;

[Flags]
public enum VolumeRegistryMode
{
	OnStart = 1,
	OnMove = 2,
	OnParentTracked = 4
}
