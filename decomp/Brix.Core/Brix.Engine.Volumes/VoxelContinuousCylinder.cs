using System;
using UnityEngine;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelContinuousCylinder : VoxelContinuousPrimitive
{
	public VoxelCylinder.Orientation orientation = VoxelCylinder.Orientation.Up;

	protected override bool CheckPoint(Vector3 d)
	{
		return orientation switch
		{
			VoxelCylinder.Orientation.Right => CheckPointRight(d), 
			VoxelCylinder.Orientation.Up => CheckPointUp(d), 
			VoxelCylinder.Orientation.Forward => CheckPointForward(d), 
			_ => throw new Exception(string.Concat("VoxelContinuousCylinder at ", center, " has improper orientation: ", orientation)), 
		};
	}

	private bool CheckPointRight(Vector3 d)
	{
		return Sq(d.y * extents.z) + Sq(d.z * extents.y) <= SqE(extents.y * extents.z) && Sq(d.x) <= SqE(extents.x);
	}

	private bool CheckPointUp(Vector3 d)
	{
		return Sq(d.x * extents.z) + Sq(d.z * extents.x) <= SqE(extents.x * extents.z) && Sq(d.y) <= SqE(extents.y);
	}

	private bool CheckPointForward(Vector3 d)
	{
		return Sq(d.x * extents.y) + Sq(d.y * extents.x) <= SqE(extents.x * extents.y) && Sq(d.z) <= SqE(extents.z);
	}
}
