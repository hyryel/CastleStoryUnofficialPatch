using System;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelBox : VoxelPrimitive
{
	protected override bool CheckPoint(XYZ d)
	{
		return Sq(d.x) <= SqE(extents.x) && Sq(d.y) <= SqE(extents.y) && Sq(d.z) <= SqE(extents.z);
	}
}
