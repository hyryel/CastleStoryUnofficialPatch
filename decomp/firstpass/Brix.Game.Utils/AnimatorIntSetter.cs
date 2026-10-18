using UnityEngine;

namespace Brix.Game.Utils;

public class AnimatorIntSetter : AnimatorSetter<int>
{
	public AnimatorIntSetter(string key, int value)
		: base(key, value)
	{
	}

	public override void Apply(Animator animator)
	{
		if (Value != animator.GetInteger(Key))
		{
			animator.SetInteger(Key, Value);
		}
	}
}
