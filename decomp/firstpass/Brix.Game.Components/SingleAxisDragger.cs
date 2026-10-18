using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Components;

public class SingleAxisDragger : DefaultDragger
{
	protected override List<Vector3> GetDragPoints(Vector3 pointA, Vector3 pointB)
	{
		AxisOrient axisOrient = GetComponent<VoxelTransform>().AxisOrient;
		float num = 1f;
		List<Vector3> list = new List<Vector3>();
		orient = Vector3.zero;
		pointB.y = pointA.y;
		switch (axisOrient)
		{
		case AxisOrient.forward:
		case AxisOrient.back:
		{
			float num4 = pointB.z - pointA.z;
			if (num4 < 0f)
			{
				num = -1f;
			}
			pointB.x = pointA.x;
			for (float num5 = 0f; Mathf.Abs(num4) - Mathf.Abs(num5) >= 0f; num5 += num)
			{
				list.Add(new Vector3(pointA.x, pointA.y, pointA.z + num5));
			}
			orient.z += num;
			break;
		}
		case AxisOrient.right:
		case AxisOrient.left:
		{
			float num2 = pointB.x - pointA.x;
			if (num2 < 0f)
			{
				num = -1f;
			}
			pointB.z = pointA.z;
			for (float num3 = 0f; Mathf.Abs(num2) - Mathf.Abs(num3) >= 0f; num3 += num)
			{
				list.Add(new Vector3(pointA.x + num3, pointA.y, pointA.z));
			}
			orient.x += num;
			break;
		}
		}
		return list;
	}
}
