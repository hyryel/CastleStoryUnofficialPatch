using UnityEngine;

namespace Brix.Game.Utils;

public abstract class AnimatorAction
{
	public string Key;

	protected AnimatorAction(string key)
	{
		Key = key;
	}

	public abstract void Apply(Animator animator);

	public AnimatorAction SetKey(string key)
	{
		Key = key;
		return this;
	}
}
