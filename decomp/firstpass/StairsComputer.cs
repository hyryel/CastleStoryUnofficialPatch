using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Input;
using Brix.Util;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

public class StairsComputer : MonoBehaviour
{
	private List<XYZ> _stairs = new List<XYZ>();

	private List<XYZ> _directions = new List<XYZ>();

	public StairOrientation CreateStairGoals(VoxelBounds bounds, StairOrientation first, out int y, ref Dictionary<XYZ, SpawnOrder> orders)
	{
		StairOrientation result = NextValidStair(bounds, first, 0, out y, validate: true, ref _stairs, ref _directions);
		for (int i = 0; i < _stairs.Count; i++)
		{
			SpawnOrder value = new SpawnOrder(_stairs[i], _directions[i], Test.BuildStairsGoal);
			orders[value.pos] = value;
		}
		return result;
	}

	public StairOrientation ExtendStairs(VoxelBounds bounds, StairOrientation first, int y, ref Dictionary<XYZ, SpawnOrder> orders)
	{
		int outY;
		StairOrientation result = NextValidStair(bounds, first, y, out outY, validate: false, ref _stairs, ref _directions);
		for (int i = 0; i < _stairs.Count; i++)
		{
			SpawnOrder value = new SpawnOrder(_stairs[i], _directions[i], Test.BuildStairsGoal, _alwaysReplace: true);
			orders[value.pos] = value;
		}
		return result;
	}

	public void GetStairs(ref List<XYZ> pos, ref List<Faces> dir)
	{
		_stairs.Slinq().AddTo(pos);
		(from d in _directions.Slinq()
			select (Faces)Calcule.VecteurToFaceIndex(d.ToVector3())).AddTo(dir);
	}

	private static StairOrientation NextValidStair(VoxelBounds bounds, StairOrientation stair, int inY, out int outY, bool validate, ref List<XYZ> stairs, ref List<XYZ> directions)
	{
		StairOrientation stairOrientation = stair;
		StairOrientation stairOrientation2 = null;
		outY = inY;
		if (stair == StairOrientation.Invalid)
		{
			return stair;
		}
		do
		{
			if (stairOrientation2 != null)
			{
				stairOrientation = stairOrientation2;
			}
			stairs.Clear();
			directions.Clear();
			stairOrientation2 = stairOrientation.Next();
			if (stairOrientation2 == stair)
			{
				stairOrientation = StairOrientation.Invalid;
				break;
			}
			XYZ stairStartPos = ComputeCorner(stairOrientation.corner, inY, bounds);
			outY = stairStartPos.y;
			ComputeStairs(bounds, stairStartPos, stairOrientation.direction, ref stairs, ref directions);
		}
		while (validate && !StairAreValid(stairs, outY, bounds));
		FilterStairs(ref stairs, ref directions);
		return stairOrientation;
	}

	private static void FilterStairs(ref List<XYZ> positions, ref List<XYZ> directions)
	{
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		List<XYZ> list2 = CollectionPool.RequestList<XYZ>();
		for (int i = 0; i < positions.Count; i++)
		{
			if (!Architecte.IsUnderIsland(positions[i]))
			{
				list.Add(positions[i]);
				list2.Add(directions[i]);
			}
		}
		positions.Dispose();
		directions.Dispose();
		positions = list;
		directions = list2;
	}

	private static void ComputeStairs(VoxelBounds bounds, XYZ stairStartPos, XYZ stairDir, ref List<XYZ> resultPositions, ref List<XYZ> resultDirections)
	{
		XYZ xYZ = stairStartPos;
		XYZ xYZ2 = stairDir;
		while (xYZ.y >= bounds.Min.y)
		{
			resultPositions.Add(xYZ);
			switch (((xYZ.x == bounds.Min.x) ? 1 : 0) + ((xYZ.x == bounds.Max.x) ? 1 : 0) + ((xYZ.z == bounds.Min.z) ? 1 : 0) + ((xYZ.z == bounds.Max.z) ? 1 : 0))
			{
			case 2:
			{
				resultDirections.Add(XYZ.up);
				XYZ xYZ3 = new XYZ(-xYZ2.z, xYZ2.y, xYZ2.x);
				if (!(xYZ + xYZ3 >= bounds.Min) || !(xYZ + xYZ3 <= bounds.Max))
				{
					xYZ3 = new XYZ(xYZ2.z, xYZ2.y, -xYZ2.x);
				}
				xYZ2 = xYZ3;
				xYZ += xYZ2;
				break;
			}
			case 1:
				resultDirections.Add(xYZ2);
				xYZ += xYZ2;
				xYZ.y--;
				break;
			default:
				resultDirections.Add(XYZ.up);
				return;
			}
		}
	}

	private static XYZ ComputeCorner(XYZ corner, int y, VoxelBounds bounds)
	{
		XYZ xYZ = ComputeCornerXY(corner, bounds);
		if (y == 0)
		{
			xYZ.y = ComputeCornerY(xYZ, bounds);
		}
		else
		{
			xYZ.y = y;
		}
		return xYZ;
	}

	private static int ComputeCornerY(XYZ pos, VoxelBounds bounds)
	{
		XYZ xyz = new XYZ(pos.x, 128, pos.z);
		if (Architecte.TerrainDessus(XZ.FromXYZ(xyz), out var result))
		{
			if (Voxel.IsCrust(result) || Voxel.IsEmpty(result))
			{
				return result.y - 1;
			}
			return result.y;
		}
		return bounds.max.y;
	}

	private static XYZ ComputeCornerXY(XYZ corner, VoxelBounds bounds)
	{
		int x = ((corner.x <= 0) ? bounds.Min.x : bounds.Max.x);
		int z = ((corner.z <= 0) ? bounds.Min.z : bounds.Max.z);
		return new XYZ(x, 0, z);
	}

	private static bool StairAreValid(List<XYZ> stairs, int outY, VoxelBounds b)
	{
		bool result = true;
		if (outY < b.min.y)
		{
			result = false;
		}
		else
		{
			for (int i = 0; i < stairs.Count; i++)
			{
				if (!StairIsValid(stairs[i]) || !b.Contains(stairs[i]))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private static bool StairIsValid(XYZ pos)
	{
		XYZ xYZ = pos - XYZ.up;
		if (Voxel.IsEmpty(xYZ) && !Architecte.IsUnderIsland(xYZ))
		{
			return false;
		}
		return true;
	}
}
