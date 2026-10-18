using System.Collections.Generic;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public class VerticalDragger : DefaultDragger
{
	public override IEnumerable<Slave> GenerateSlaves(VoxelRaycastHit vrh)
	{
		_sizeChanged = false;
		IFitter component = GetComponent<IFitter>();
		Vector3 verticalDragPoint = PickingUtility.GetVerticalDragPoint(_initialPoint.HitVector, vrh.LastRay);
		component.FitAndOrient((!(verticalDragPoint != Vector3.zero)) ? vrh : new VoxelRaycastHit().PlaceAtVector(verticalDragPoint), out var absolutePosition);
		component.FitAndOrient(_initialPoint, out var absolutePosition2);
		_dragPoints = FilterDragPoints(GetDragPoints(absolutePosition2, absolutePosition));
		ResizeSlavesTo(_dragPoints.Count);
		if (_sizeChanged)
		{
			for (int i = 0; i < _slaves.Count; i++)
			{
				utilVRH.PlaceAtVector(_dragPoints[i] + _initialPoint.LocalHitVector);
				_slaves[i].GetHeldComponent<IFitter>().CloneInternals(component);
				_slaves[i].GetHeldComponent<IFitter>().FitForDrag(utilVRH);
				_slaves[i].GetHeldComponent<ValidatorComponent>().Validate(GetValidationData());
			}
		}
		return _slaves;
	}

	protected override List<Vector3> GetDragPoints(Vector3 pointA, Vector3 pointB)
	{
		List<Vector3> list = new List<Vector3>();
		pointB.x = pointA.x;
		pointB.z = pointA.z;
		float num = 1f;
		float num2 = pointB.y - pointA.y;
		if (num2 < 0f)
		{
			num = -1f;
		}
		for (float num3 = 0f; Mathf.Abs(num2 - num3) != 0f; num3 += num)
		{
			list.Add(new Vector3(pointA.x, pointA.y + num3, pointA.z));
		}
		return list;
	}
}
