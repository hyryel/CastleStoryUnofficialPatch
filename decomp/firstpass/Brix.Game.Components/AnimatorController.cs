using UnityEngine;

namespace Brix.Game.Components;

[RequireComponent(typeof(Animator))]
public class AnimatorController : MonoBehaviour
{
	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void SetTrigger(string trigger)
	{
		if (animator.isInitialized)
		{
			animator.SetTrigger(trigger);
			animator.enabled = true;
		}
	}

	public void SetBool(string name, bool value)
	{
		if (animator.isInitialized)
		{
			animator.SetBool(name, value);
			animator.enabled = true;
		}
	}

	public void SetFloat(string name, float value)
	{
		if (animator.isInitialized)
		{
			animator.SetFloat(name, value);
			animator.enabled = true;
		}
	}

	public bool GetBool(string name)
	{
		if (!animator.isInitialized)
		{
			return false;
		}
		return animator.GetBool(name);
	}
}
