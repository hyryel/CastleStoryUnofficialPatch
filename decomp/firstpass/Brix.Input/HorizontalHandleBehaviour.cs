using UnityEngine;

namespace Brix.Input;

public class HorizontalHandleBehaviour : BaseHandleBehaviour
{
	public override HandleType Type => HandleType.Horizontal;

	public override Plane DragPlane(DragHandle drag)
	{
		return new Plane(Vector3.up, drag.transform.position);
	}

	public Vector3 DragDirection(DragHandle drag)
	{
		return drag.transform.forward;
	}

	public override Vector3 GetDragDestination(DragHandle drag, Vector3 dragOrigin, Vector3 rayCollisionPoint)
	{
		Vector3 vector = dragOrigin + Vector3.Project(rayCollisionPoint - dragOrigin, DragDirection(drag));
		if (!drag.canGoBackward)
		{
			float num = Vector3.Dot(vector - dragOrigin, DragDirection(drag));
			if (num < 0f)
			{
				vector = dragOrigin;
			}
		}
		return vector;
	}
}
