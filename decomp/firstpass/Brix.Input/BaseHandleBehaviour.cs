using UnityEngine;

namespace Brix.Input;

public class BaseHandleBehaviour : HandleBehaviour
{
	public override HandleType Type => HandleType.Base;

	public override void DragStarted(DragHandle handle)
	{
		handle.ResetBallPosition();
		handle.DontIntersect();
		handle.ResetLine();
	}

	public override void Drag(DragHandle handle)
	{
		handle.SetBallPosition(handle.Destination);
		handle.SetLine(handle.Origin, handle.Destination);
	}

	public override void DragEnded(DragHandle handle)
	{
		handle.ResetBallPosition();
		handle.DontIntersect();
		handle.ResetLine();
	}

	public override Vector3 GetDragDestination(DragHandle drag, Vector3 dragOrigin, Vector3 rayCollisionPoint)
	{
		return rayCollisionPoint;
	}

	public override Plane DragPlane(DragHandle drag)
	{
		return new Plane(Vector3.up, drag.transform.position);
	}

	public override void PickerBehaviour(DragHandle drag, HandleDragPicker handleDragPicker)
	{
		handleDragPicker.DefaultDragBehavior();
	}

	public override void SetDefaultOrientation(DragHandle drag)
	{
		drag.transform.localRotation = Quaternion.identity;
	}
}
