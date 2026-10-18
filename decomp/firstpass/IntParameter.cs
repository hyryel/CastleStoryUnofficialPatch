using UnityEngine;

public class IntParameter : AnimatorParameter
{
	public IntParameter(Animator animator, string parameter, bool sendOnNetwork = false)
		: base(animator, parameter, sendOnNetwork)
	{
	}

	public int Get()
	{
		return Animator.GetInteger(ParameterHash);
	}

	public void Set(int i)
	{
		if (base.SendOnNetwork && unetAnimator != null)
		{
			unetAnimator.SetInteger(ParameterHash, i);
		}
		Animator.SetInteger(ParameterHash, i);
	}

	public static implicit operator int(IntParameter p)
	{
		return p.Get();
	}
}
