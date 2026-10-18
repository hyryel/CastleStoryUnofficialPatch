using System;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelContinuousSphere : VoxelContinuousPrimitive
{
	protected override bool CheckPoint(Vector3 d)
	{
		return Sq(d.x * extents.y * extents.z) + Sq(d.y * extents.x * extents.z) + Sq(d.z * extents.x * extents.y) <= SqE(extents.x * extents.y * extents.z);
	}
}
