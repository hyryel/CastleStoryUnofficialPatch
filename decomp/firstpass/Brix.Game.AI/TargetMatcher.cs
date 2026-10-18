using System;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI;

[RequireComponent(typeof(Animator))]
public class TargetMatcher : MonoBehaviour
{
	public enum MatchState
	{
		Sleeping,
		Waiting,
		ManualOverride,
		FinalizingOverride,
		Matching
	}

	private static BrixLogger logger = new BrixLogger(typeof(TargetMatcher).FullName);

	private Animator Animator;

	private Transform trf;

	public MatchState State;

	public int StateHash;

	[NonSerialized]
	public float Time;

	[NonSerialized]
	public float Start;

	[NonSerialized]
	public float End;

	[NonSerialized]
	private Vector3 Position;

	[NonSerialized]
	private Quaternion Orientation;

	private MatchTargetWeightMask Mask;

	public bool Finished;

	public bool MatchFailed;

	public bool AnimatorStartedMatching;

	[NonSerialized]
	public Vector3 OverrideOriginPosition;

	[NonSerialized]
	public Quaternion OverrideOriginOrientation;

	[NonSerialized]
	public float OverrideTimeOfEnd;

	[NonSerialized]
	public float OverrideTimeOfStart;

	[NonSerialized]
	public float OverrideTimeOfRotationEnd;

	private void Awake()
	{
		Animator = GetComponent<Animator>();
		trf = base.transform;
		Time = 0f;
	}

	public void AnimatorUpdate(float deltaTime)
	{
		Finished = false;
		Time += deltaTime;
		if (State == MatchState.Sleeping)
		{
			return;
		}
		if (State == MatchState.Waiting)
		{
			if (Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == StateHash && !Animator.IsInTransition(0))
			{
				Animator.MatchTarget(Position, Orientation, AvatarTarget.Root, Mask, Start, End);
				State = MatchState.Matching;
				AnimatorStartedMatching = Animator.isMatchingTarget;
			}
		}
		else if (State == MatchState.Matching)
		{
			if (!AnimatorStartedMatching && Animator.isMatchingTarget)
			{
				AnimatorStartedMatching = true;
			}
			else if (AnimatorStartedMatching && !Animator.isMatchingTarget)
			{
				Finished = true;
				AnimatorStartedMatching = false;
				State = MatchState.Sleeping;
			}
			else if (Animator.isMatchingTarget && Animator.IsInTransition(0))
			{
				if (!MatchFailed)
				{
					logger.Warn("Transition has started but match targeting not finished, will not match correctly");
				}
				MatchFailed = true;
			}
		}
		else if (State == MatchState.ManualOverride)
		{
			if (Time >= OverrideTimeOfEnd && Time >= OverrideTimeOfRotationEnd)
			{
				State = MatchState.FinalizingOverride;
			}
		}
		else if (State == MatchState.FinalizingOverride)
		{
			Finished = true;
			State = MatchState.Sleeping;
		}
	}

	public Vector3 GetAnimatorPosition()
	{
		return trf.position + Animator.deltaPosition;
	}

	public Quaternion GetAnimatorOrientation()
	{
		return trf.rotation * Animator.deltaRotation;
	}

	public Vector3 ConsumePosition()
	{
		if (State == MatchState.ManualOverride || State == MatchState.FinalizingOverride)
		{
			if (Time - OverrideTimeOfStart >= OverrideTimeOfEnd - OverrideTimeOfStart)
			{
				return Position;
			}
			return Vector3.Lerp(OverrideOriginPosition, Position, (Time - OverrideTimeOfStart) / (OverrideTimeOfEnd - OverrideTimeOfStart));
		}
		if (State == MatchState.Matching || State == MatchState.Waiting)
		{
			return GetAnimatorPosition();
		}
		return trf.position;
	}

	public Quaternion ConsumeOrientation()
	{
		if (State == MatchState.ManualOverride || State == MatchState.FinalizingOverride)
		{
			if (Time - OverrideTimeOfStart >= OverrideTimeOfRotationEnd - OverrideTimeOfStart)
			{
				return Orientation;
			}
			return Quaternion.Lerp(OverrideOriginOrientation, Orientation, (Time - OverrideTimeOfStart) / (OverrideTimeOfRotationEnd - OverrideTimeOfStart));
		}
		if (State == MatchState.Matching || State == MatchState.Waiting)
		{
			return GetAnimatorOrientation();
		}
		return trf.rotation;
	}

	public void OverrideMatchTarget(float time, float rotationTime, Vector3 position, Quaternion orientation)
	{
		if (State == MatchState.ManualOverride)
		{
			OverrideOriginPosition = ConsumePosition();
			OverrideOriginOrientation = ConsumeOrientation();
		}
		else
		{
			OverrideOriginPosition = trf.position;
			OverrideOriginOrientation = trf.rotation;
		}
		Position = position;
		Orientation = orientation;
		OverrideTimeOfStart = Time;
		OverrideTimeOfEnd = Time + time;
		OverrideTimeOfRotationEnd = Time + rotationTime;
		if (Animator.isMatchingTarget)
		{
			Animator.InterruptMatchTarget(completeMatch: false);
		}
		State = MatchState.ManualOverride;
	}

	public void OverrideMatchTarget(float time, Vector3 position, Quaternion orientation)
	{
		OverrideMatchTarget(time, time, position, orientation);
	}

	public void MatchTarget(int stateHash, Vector3 position, float start, float end)
	{
		MatchTarget(stateHash, position, Quaternion.identity, start, end, new MatchTargetWeightMask(Vector3.one, 0f));
	}

	public void MatchTarget(int stateHash, Quaternion orientation, float start, float end)
	{
		MatchTarget(stateHash, Vector3.zero, orientation, start, end, new MatchTargetWeightMask(Vector3.zero, 1f));
	}

	public void MatchTarget(int stateHash, Vector3 position, Quaternion orientation, float start, float end)
	{
		MatchTarget(stateHash, position, orientation, start, end, new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 1f));
	}

	public void MatchTargetWithY(int stateHash, Vector3 position, Quaternion orientation, float start, float end)
	{
		MatchTarget(stateHash, position, orientation, start, end, new MatchTargetWeightMask(new Vector3(1f, 1f, 1f), 1f));
	}

	public void MatchTargetYOnly(int stateHash, float y, float start, float end)
	{
		MatchTarget(stateHash, new Vector3(0f, y, 0f), Quaternion.identity, start, end, new MatchTargetWeightMask(new Vector3(0f, 1f, 0f), 0f));
	}

	public void MatchTargetWithY(int stateHash, Vector3 position, float start, float end)
	{
		MatchTarget(stateHash, position, Quaternion.identity, start, end, new MatchTargetWeightMask(new Vector3(1f, 1f, 1f), 0f));
	}

	private void MatchTarget(int stateHash, Vector3 position, Quaternion orientation, float start, float end, MatchTargetWeightMask mask)
	{
		if (Animator.isMatchingTarget)
		{
			Animator.InterruptMatchTarget(completeMatch: false);
		}
		MatchFailed = false;
		StateHash = stateHash;
		Position = position;
		Orientation = orientation;
		Mask = mask;
		Start = start;
		End = end;
		if (Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != stateHash || Animator.IsInTransition(0))
		{
			State = MatchState.Waiting;
			return;
		}
		Animator.MatchTarget(position, orientation, AvatarTarget.Root, mask, start, end);
		State = MatchState.Matching;
	}

	public void Cancel()
	{
		if (Animator.isMatchingTarget)
		{
			Animator.InterruptMatchTarget(completeMatch: false);
		}
		State = MatchState.Sleeping;
	}

	public void Rush()
	{
		Animator.InterruptMatchTarget(completeMatch: true);
		State = MatchState.Sleeping;
	}
}
