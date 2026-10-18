using Brix.External.Signals;
using UnityEngine;
using UnityEngine.Networking;

public class AnimatorStateProgress
{
	private Animator animator;

	private int layer;

	public string state;

	public int StateHash;

	public Signal<AnimatorStateInfo> StateEnter = new Signal<AnimatorStateInfo>();

	public Signal<AnimatorStateInfo> StateExit = new Signal<AnimatorStateInfo>();

	public BoolParameter BoolAsTrigger;

	public bool Running;

	public bool Transiting;

	public bool Started;

	public bool Finished;

	public bool Looped;

	private float time;

	public AnimatorStateProgress(Animator animator, int layer, string state, AnimatorTracker tracker, BoolParameter boolAsTrigger = null)
	{
		this.animator = animator;
		this.layer = layer;
		this.state = state;
		StateHash = Animator.StringToHash(state);
		BoolAsTrigger = boolAsTrigger;
		if (tracker != null)
		{
			tracker.Add(this);
		}
	}

	public void EnterState(AnimatorStateInfo stateInfo)
	{
		StateEnter.Invoke(stateInfo);
	}

	public void ExitState(AnimatorStateInfo stateInfo)
	{
		StateExit.Invoke(stateInfo);
		if (NetworkServer.active && BoolAsTrigger != null)
		{
			BoolAsTrigger.Set(b: false);
		}
	}

	public bool IsActive()
	{
		return animator.GetCurrentAnimatorStateInfo(layer).fullPathHash == StateHash;
	}

	public bool Reached(float progress)
	{
		return IsActive() && animator.GetCurrentAnimatorStateInfo(layer).normalizedTime - Mathf.Floor(animator.GetCurrentAnimatorStateInfo(layer).normalizedTime) >= progress;
	}

	public float GetTotalNormalizedTime()
	{
		if (IsActive())
		{
			return animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
		}
		return 0f;
	}

	public float GetNormalizedTime()
	{
		if (IsActive())
		{
			float normalizedTime = animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
			return normalizedTime - Mathf.Floor(normalizedTime);
		}
		return 0f;
	}

	public float GetTotalDuration()
	{
		if (IsActive())
		{
			return animator.GetCurrentAnimatorStateInfo(layer).length;
		}
		return 0f;
	}

	public float GetDuration()
	{
		if (IsActive())
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
			return currentAnimatorStateInfo.length * (currentAnimatorStateInfo.normalizedTime - Mathf.Floor(currentAnimatorStateInfo.normalizedTime));
		}
		return 0f;
	}

	public void Update()
	{
		Started = false;
		Finished = false;
		Looped = false;
		Transiting = false;
		if (Running)
		{
			if (!IsActive())
			{
				Running = false;
				Finished = true;
			}
			else if (animator.IsInTransition(layer))
			{
				Transiting = true;
			}
			else if (time > GetNormalizedTime())
			{
				Looped = true;
			}
		}
		else if (IsActive())
		{
			Started = true;
			Running = true;
			time = GetNormalizedTime();
		}
	}
}
