using System.Collections.Generic;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public class RampDragger : SingleAxisDragger
{
	public override IEnumerable<Slave> GenerateSlaves(VoxelRaycastHit vrh)
	{
		_sizeChanged = false;
		RampFitter component = GetComponent<RampFitter>();
		Vector3 horizontalDragPoint = PickingUtility.GetHorizontalDragPoint(_initialPoint.WorldHitVector + DefaultDragger.safetyOffset, vrh.LastRay);
		component.FitAndOrientFrom((!(horizontalDragPoint != Vector3.zero)) ? vrh : utilVRH.PlaceAtVector(horizontalDragPoint), out var absolutePosition, _initialPoint.HitGameObject);
		component.FitAndOrient(_initialPoint, out var absolutePosition2);
		_dragPoints = GetDragPoints(absolutePosition2, absolutePosition);
		if (base.AutoOrient && _dragPoints.Count > 1)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector3 forward = base.transform.forward;
				if (forward == orient || (base.AutoOrient && forward == reverseOrient))
				{
					break;
				}
				component.ChangeFit();
				component.FitAndOrient(_initialPoint, out absolutePosition2);
			}
		}
		_filteredPoints = FilterDragPoints(_dragPoints);
		ResizeSlavesTo(_filteredPoints.Count);
		if (_sizeChanged)
		{
			GameObject heldObject = base.gameObject;
			for (int j = 0; j < _slaves.Count; j++)
			{
				Vector3 vector = _filteredPoints[j] + _initialPoint.LocalHitVector;
				vector.y = heldObject.transform.position.y;
				utilVRH.PlaceAtVector(vector);
				_slaves[j].GetHeldComponent<RampFitter>().FitAndOrientFrom(utilVRH, heldObject);
				heldObject = _slaves[j].HeldObject;
			}
			for (int k = 0; k < _slaves.Count; k++)
			{
				_slaves[k].GetHeldComponent<ValidatorComponent>().Validate(GetValidationData());
			}
			GetComponent<ValidatorComponent>().Validate(GetValidationData());
		}
		return _slaves;
	}
}
