using UnityEngine;

namespace Brix.Input;

public class HorizontalPlaneBehaviour : BaseHandleBehaviour
{
	public override HandleType Type => HandleType.HorizontalPlane;

	public override void SetDefaultOrientation(DragHandle drag)
	{
		drag.Towards(Vector3.up);
	}
}
