using UnityEngine;

public class FloatParameter : AnimatorParameter
{
	private float Target;

	private float DampTime;

	public FloatParameter(Animator animator, string parameter, bool sendOnNetwork = false)
		: base(animator, parameter, sendOnNetwork)
	{
	}

	public float Get()
	{
		return Animator.GetFloat(ParameterHash);
	}

	public void Set(float f)
	{
		if (base.SendOnNetwork && unetAnimator != null)
		{
			unetAnimator.SetFloat(ParameterHash, f);
		}
		Animator.SetFloat(ParameterHash, f);
	}

	public void Set(float f, float dampTime, float deltaTime)
	{
		if (base.SendOnNetwork && unetAnimator != null)
		{
			unetAnimator.ApplyDamp(ParameterHash, f, dampTime, deltaTime);
		}
		Animator.SetFloat(ParameterHash, f, dampTime, deltaTime);
	}

	public void DampTo(float f, float dampTime)
	{
		Target = f;
		DampTime = dampTime;
	}

	public void ApplyDamp(float deltaTime)
	{
		if (base.SendOnNetwork && unetAnimator != null)
		{
			unetAnimator.ApplyDamp(ParameterHash, Target, DampTime, deltaTime);
		}
		Animator.SetFloat(ParameterHash, Target, DampTime, deltaTime);
	}

	public static implicit operator float(FloatParameter p)
	{
		return p.Get();
	}
}
