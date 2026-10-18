namespace Brix.Input;

public class VerticalSliderHandleBehaviour : VerticalHandleBehaviour
{
	public override HandleType Type => HandleType.VerticalSlider;

	public override void DragStarted(DragHandle handle)
	{
		base.DragStarted(handle);
		SetLine(handle);
	}

	public override void Drag(DragHandle drag)
	{
		base.Drag(drag);
		SetLine(drag);
	}

	private void SetLine(DragHandle drag)
	{
		drag.SetLine(drag.Min, drag.Max);
	}
}
