using System.Collections.Generic;
using UnityEngine;

namespace Brix.Game.Components;

public class StairDragger : DefaultDragger
{
	protected override List<Vector3> GetDragPoints(Vector3 pointA, Vector3 pointB)
	{
		if (base.transform.forward == Vector3.forward || base.transform.forward == -Vector3.forward)
		{
			if (Mathf.Abs(pointA.z - pointB.z) > Mathf.Abs(pointA.x - pointB.x))
			{
				if (base.transform.forward.z * (pointA.z - pointB.z) > 0f)
				{
					return GetDiagonalDragPoints(pointA, pointB, Vector3.up);
				}
				return GetDiagonalDragPoints(pointA, pointB, Vector3.down);
			}
			return base.GetDragPoints(pointA, pointB);
		}
		if (Mathf.Abs(pointA.z - pointB.z) <= Mathf.Abs(pointA.x - pointB.x))
		{
			if (base.transform.forward.x * (pointA.x - pointB.x) > 0f)
			{
				return GetDiagonalDragPoints(pointA, pointB, Vector3.up);
			}
			return GetDiagonalDragPoints(pointA, pointB, Vector3.down);
		}
		return base.GetDragPoints(pointA, pointB);
	}

	private List<Vector3> GetDiagonalDragPoints(Vector3 pointA, Vector3 pointB, Vector3 yStep)
	{
		float num = 1f;
		List<Vector3> list = new List<Vector3>();
		pointB.y = pointA.y;
		float num2;
		float num3;
		if (Mathf.Abs(num2 = pointB.x - pointA.x) < Mathf.Abs(num3 = pointB.z - pointA.z))
		{
			if (num3 < 0f)
			{
				num = -1f;
			}
			pointB.x = pointA.x;
			for (float num4 = 0f; Mathf.Abs(num3) - Mathf.Abs(num4) >= 0f; num4 += num)
			{
				list.Add(new Vector3(pointA.x, pointA.y, pointA.z + num4) + yStep * Mathf.Abs(num4));
			}
		}
		else
		{
			if (num2 < 0f)
			{
				num = -1f;
			}
			pointB.z = pointA.z;
			for (float num5 = 0f; Mathf.Abs(num2) - Mathf.Abs(num5) >= 0f; num5 += num)
			{
				list.Add(new Vector3(pointA.x + num5, pointA.y, pointA.z) + yStep * Mathf.Abs(num5));
			}
		}
		return list;
	}
}
