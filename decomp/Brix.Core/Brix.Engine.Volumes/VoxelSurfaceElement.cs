using System;
using Brix.Engine.Geometrie;

namespace Brix.Engine.Volumes;

public struct VoxelSurfaceElement : IComparable<VoxelSurfaceElement>, IEquatable<VoxelSurfaceElement>
{
	public XYZ p;

	public int faceIndex;

	public XYZ InnerVoxel => p;

	public XYZ OuterVoxel => p + CubeModele.XYZDirs[faceIndex];

	public XYZ Normal => CubeModele.XYZDirs[faceIndex];

	public XYZ Tangent => CubeModele.XYZTangents[faceIndex];

	public XYZ Tangent2 => CubeModele.XYZTangents2[faceIndex];

	public VoxelSurfaceElement Inverse => new VoxelSurfaceElement
	{
		p = p + CubeModele.XYZDirs[faceIndex],
		faceIndex = Config.FacesInverses[faceIndex]
	};

	public int CompareTo(VoxelSurfaceElement other)
	{
		int num = p.CompareTo(other.p);
		return (num != 0) ? num : faceIndex.CompareTo(other.faceIndex);
	}

	public bool Equals(VoxelSurfaceElement other)
	{
		return p.Equals(other.p) && faceIndex.Equals(other.faceIndex);
	}
}
