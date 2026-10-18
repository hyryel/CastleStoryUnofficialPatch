using System;

namespace Brix.Engine.Volumes;

[Serializable]
public class VoxelCylinder : VoxelPrimitive
{
	public enum Orientation
	{
		Right,
		Up,
		Forward
	}

	public Orientation orientation = Orientation.Up;

	protected override bool CheckPoint(XYZ d)
	{
		return orientation switch
		{
			Orientation.Right => CheckPointRight(d), 
			Orientation.Up => CheckPointUp(d), 
			Orientation.Forward => CheckPointForward(d), 
			_ => throw new Exception(string.Concat("VoxelCylinder at ", center, " has improper orientation: ", orientation)), 
		};
	}

	public bool CheckPointRight(XYZ d)
	{
		return Sq(d.y * extents.z) + Sq(d.z * extents.y) <= SqE(extents.y * extents.z) && Sq(d.x) <= SqE(extents.x);
	}

	public bool CheckPointUp(XYZ d)
	{
		return Sq(d.x * extents.z) + Sq(d.z * extents.x) <= SqE(extents.x * extents.z) && Sq(d.y) <= SqE(extents.y);
	}

	public bool CheckPointForward(XYZ d)
	{
		return Sq(d.x * extents.y) + Sq(d.y * extents.x) <= SqE(extents.x * extents.y) && Sq(d.z) <= SqE(extents.z);
	}
}
