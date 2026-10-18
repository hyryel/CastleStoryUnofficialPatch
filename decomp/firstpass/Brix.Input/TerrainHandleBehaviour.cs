using UnityEngine;

namespace Brix.Input;

public class TerrainHandleBehaviour : BaseHandleBehaviour
{
	public override HandleType Type => HandleType.Terrain;

	public override void PickerBehaviour(DragHandle drag, HandleDragPicker handleDragPicker)
	{
		handleDragPicker.TerrainDragBehavior();
	}

	public override void Drag(DragHandle drag)
	{
	}

	public override void SetDefaultOrientation(DragHandle drag)
	{
		drag.Towards(Vector3.up);
	}
}
