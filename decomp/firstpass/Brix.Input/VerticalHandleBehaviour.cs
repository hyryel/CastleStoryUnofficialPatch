using UnityEngine;

namespace Brix.Input;

public class VerticalHandleBehaviour : BaseHandleBehaviour
{
	public override HandleType Type => HandleType.Vertical;

	public override Plane DragPlane(DragHandle drag)
	{
		return new Plane(Vector3.Cross(Camera.main.transform.right, Vector3.up), drag.transform.position);
	}

	public Vector3 DragDirection()
	{
		return Vector3.up;
	}

	public override Vector3 GetDragDestination(DragHandle drag, Vector3 dragOrigin, Vector3 rayCollisionPoint)
	{
		return dragOrigin + Vector3.Project(rayCollisionPoint - dragOrigin, DragDirection());
	}

	public override void SetDefaultOrientation(DragHandle drag)
	{
		drag.Towards(Vector3.up);
	}
}
