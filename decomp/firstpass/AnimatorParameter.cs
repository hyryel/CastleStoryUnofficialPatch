using UnityEngine;

public abstract class AnimatorParameter
{
	protected Animator Animator;

	public int ParameterHash;

	public string ParameterString;

	protected UNetAnimator unetAnimator;

	protected bool SendOnNetwork { get; set; }

	public AnimatorParameter(Animator animator, string parameter, bool sendOnNetwork)
	{
		Animator = animator;
		unetAnimator = animator.gameObject.GetComponent<UNetAnimator>();
		ParameterString = parameter;
		ParameterHash = Animator.StringToHash(parameter);
		SendOnNetwork = sendOnNetwork;
	}
}
