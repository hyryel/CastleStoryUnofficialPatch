using UnityEngine.UI;

public class EnhancedButton : Button
{
	public bool WakeAnimatorOnAction = true;

	protected override void Start()
	{
		base.Start();
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		if (WakeAnimatorOnAction)
		{
			base.animator.enabled = true;
		}
		base.DoStateTransition(state, instant);
	}
}
