using UnityEngine;

public class TriggerParameter : AnimatorParameter
{
	public TriggerParameter(Animator animator, string parameter)
		: base(animator, parameter, sendOnNetwork: true)
	{
	}

	public void Trigger()
	{
		if (base.SendOnNetwork && unetAnimator != null)
		{
			unetAnimator.SetTrigger(ParameterHash);
		}
		Animator.SetTrigger(ParameterHash);
	}

	public void Reset()
	{
		Animator.ResetTrigger(ParameterHash);
	}
}
