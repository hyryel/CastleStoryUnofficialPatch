using System;

namespace Brix.Engine.Volumes;

public struct VoxelSurfaceConfig : IComparable<VoxelSurfaceConfig>, IEquatable<VoxelSurfaceConfig>
{
	public XYZ p;

	public int config;

	public int CompareTo(VoxelSurfaceConfig other)
	{
		int num = p.CompareTo(other.p);
		return (num != 0) ? num : config.CompareTo(other.config);
	}

	public bool Equals(VoxelSurfaceConfig other)
	{
		return p.Equals(other.p) && config.Equals(other.config);
	}
}
