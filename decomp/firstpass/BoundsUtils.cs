using System.Collections.Generic;
using Brix.Utils;
using Brix.Utils.Locomotion;
using UnityEngine;

public static class BoundsUtils
{
	public static void Corners(this Bounds bounds, ref List<Vector3> Corners, float y = 0f)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Corners.Add(new Vector3(min.x, 0f, min.z));
		Corners.Add(new Vector3(min.x, 0f, max.z));
		Corners.Add(new Vector3(max.x, 0f, min.z));
		Corners.Add(new Vector3(max.x, 0f, max.z));
	}

	public static void CenteredSides(this Bounds bounds, ref List<Vector3> Sides, float y = 0f)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Sides.Add(new Vector3((min.x + max.x) / 2f, 0f, max.z));
		Sides.Add(new Vector3((min.x + max.x) / 2f, 0f, min.z));
		Sides.Add(new Vector3(max.x, 0f, (min.z + max.z) / 2f));
		Sides.Add(new Vector3(min.x, 0f, (min.z + max.z) / 2f));
	}

	public static void SpecialPoints(this Bounds bounds, ref List<Vector3> features, float y = 0f)
	{
		bounds.CenteredSides(ref features, y);
		bounds.Corners(ref features, y);
	}

	public static Vector3 PointInDirection(this Bounds bounds, Vector3 direction)
	{
		Vector3 center = bounds.center;
		if (direction.x < 0f)
		{
			center.x = bounds.min.x;
		}
		else if (direction.x > 0f)
		{
			center.x = bounds.max.x;
		}
		if (direction.z < 0f)
		{
			center.z = bounds.min.z;
		}
		else if (direction.z > 0f)
		{
			center.z = bounds.max.z;
		}
		return center;
	}

	public static Bounds ExtendToPoint(this Bounds bounds, Vector3 direction, Vector3 point, int minSize = 3, int maxSize = 16)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		if (direction.x < 0f)
		{
			min.x = Mathf.Clamp(point.x, max.x - (float)maxSize, max.x - (float)minSize);
		}
		else if (direction.x > 0f)
		{
			max.x = Mathf.Clamp(point.x, min.x + (float)minSize, min.x + (float)maxSize);
		}
		if (direction.z < 0f)
		{
			min.z = Mathf.Clamp(point.z, max.z - (float)maxSize, max.z - (float)minSize);
		}
		else if (direction.z > 0f)
		{
			max.z = Mathf.Clamp(point.z, min.z + (float)minSize, min.z + (float)maxSize);
		}
		Bounds result = default(Bounds);
		result.SetMinMax(min, max);
		return result;
	}

	public static void PointsOnAxies(this Bounds bounds, Vector3 point, ref List<Vector3> Sides)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Bounds bounds2 = new Bounds(bounds.center, bounds.size.Flatten(Vector3.up * float.MaxValue));
		Vector3 vector = new Vector3(min.x, 0f, point.z);
		if (bounds2.Contains(vector))
		{
			Sides.Add(vector);
		}
		Vector3 vector2 = new Vector3(max.x, 0f, point.z);
		if (bounds2.Contains(vector2))
		{
			Sides.Add(vector2);
		}
		Vector3 vector3 = new Vector3(point.x, 0f, min.z);
		if (bounds2.Contains(vector3))
		{
			Sides.Add(vector3);
		}
		Vector3 vector4 = new Vector3(point.x, 0f, max.z);
		if (bounds2.Contains(vector4))
		{
			Sides.Add(vector4);
		}
	}

	private static Vector3 ClosestTo(List<Vector3> points, Vector3 reference)
	{
		Vector3 result = points[0];
		float num = Vector3.SqrMagnitude(points[0] - reference);
		for (int i = 1; i < points.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(points[i] - reference);
			if (num2 < num)
			{
				num = num2;
				result = points[i];
			}
		}
		return result;
	}

	public static Vector3 ClosestCornerTo(this Bounds bounds, Vector3 reference)
	{
		List<Vector3> Corners = CollectionPool.RequestList<Vector3>();
		bounds.Corners(ref Corners);
		Vector3 result = ClosestTo(Corners, reference);
		Corners.Dispose();
		return result;
	}

	public static Vector3 ClosestCenteredSide(this Bounds bounds, Vector3 reference)
	{
		List<Vector3> Sides = CollectionPool.RequestList<Vector3>();
		bounds.CenteredSides(ref Sides);
		Vector3 result = ClosestTo(Sides, reference);
		Sides.Dispose();
		return result;
	}

	public static Vector3 ClosestAlignedPointTo(this Bounds bounds, Vector3 reference)
	{
		List<Vector3> Sides = CollectionPool.RequestList<Vector3>();
		bounds.PointsOnAxies(reference, ref Sides);
		if (Sides.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 result = ClosestTo(Sides, reference);
		Sides.Dispose();
		return result;
	}

	public static Vector3 ClosestSpecialPointTo(this Bounds bounds, Vector3 reference)
	{
		List<Vector3> Sides = CollectionPool.RequestList<Vector3>();
		bounds.CenteredSides(ref Sides);
		bounds.Corners(ref Sides);
		Vector3 result = ClosestTo(Sides, reference);
		Sides.Dispose();
		return result;
	}

	public static Vector3 ClosestPointTo(this Bounds bounds, Vector3 reference)
	{
		List<Vector3> Sides = CollectionPool.RequestList<Vector3>();
		bounds.PointsOnAxies(reference, ref Sides);
		bounds.Corners(ref Sides);
		Vector3 result = ClosestTo(Sides, reference);
		Sides.Dispose();
		return result;
	}

	public static Vector3 SideOf(this Bounds bound, Vector3 p)
	{
		float x = ((p.x == bound.max.x) ? 1 : ((p.x == bound.min.x) ? (-1) : 0));
		float z = ((p.z == bound.max.z) ? 1 : ((p.z == bound.min.z) ? (-1) : 0));
		return new Vector3(x, 0f, z);
	}

	public static bool IsCorner(this Bounds bounds, Vector3 p)
	{
		return bounds.SideOf(p).sqrMagnitude > 1f;
	}

	public static bool IsCenter(this Bounds bounds, Vector3 p)
	{
		return p.Flatten() == bounds.center.Flatten();
	}
}
