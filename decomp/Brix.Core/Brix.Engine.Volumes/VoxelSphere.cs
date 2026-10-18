using System;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelSphere : VoxelPrimitive
{
	protected override bool CheckPoint(XYZ d)
	{
		return Sq(d.x * extents.y * extents.z) + Sq(d.y * extents.x * extents.z) + Sq(d.z * extents.x * extents.y) <= SqE(extents.x * extents.y * extents.z);
	}
}
