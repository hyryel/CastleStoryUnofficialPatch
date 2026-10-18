using System;

namespace Brix.Input.WorldEditor;

[Flags]
internal enum VoxelDirtyProperties
{
	Terrain = 1,
	Trees = 2,
	Plants = 4,
	Minerals = 8
}
