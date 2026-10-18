using System;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelContinuousBox : VoxelContinuousPrimitive
{
	protected override bool CheckPoint(Vector3 d)
	{
		return Sq(d.x) <= SqE(extents.x) && Sq(d.y) <= SqE(extents.y) && Sq(d.z) <= SqE(extents.z);
	}
}
