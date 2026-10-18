using System;

namespace Brix.Input;

public class DynamicCursorState : CursorState
{
	public Func<CursorIcon> iconGetter;

	public DynamicCursorState(CursorController controller)
		: base(controller)
	{
	}

	protected override void SetIcon()
	{
		icon = iconGetter();
		base.SetIcon();
	}
}
