using System.Collections.Generic;
using UnityEngine;

public class BoolParameter : AnimatorParameter
{
	public static Dictionary<int, BoolParameter> boolParamForID = new Dictionary<int, BoolParameter>();

	private int id;

	public static int HightestId;

	public BoolParameter(int id, Animator animator, string parameter, bool sendOnNetwork = false)
		: base(animator, parameter, sendOnNetwork)
	{
		this.id = id;
		if (id > HightestId)
		{
			HightestId = id;
		}
		boolParamForID[id] = this;
	}

	public bool Get()
	{
		return Animator.GetBool(ParameterHash);
	}

	public void Set(bool b)
	{
		if (base.SendOnNetwork && unetAnimator != null)
		{
			unetAnimator.SetFlag(id, b);
		}
		Animator.SetBool(ParameterHash, b);
	}

	public static implicit operator bool(BoolParameter p)
	{
		return p.Get();
	}
}
