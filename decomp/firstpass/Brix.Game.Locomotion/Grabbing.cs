using System;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Grabbing : LocomotionCommonState
{
	public Transform GrabbedTarget;

	public Transform GrabHand;

	public Transform GrabbedPivot;

	public float GrabRotationSpeed = 1f;

	private ObjectLocation GrabTargetLocation;

	private Transform parentBeforeGrab;

	public void StartGrabbingTarget(ObjectLocation target)
	{
		GrabbedTarget = null;
		GrabTargetLocation = target;
		base.controller.IsTakingOneHand.Set(b: true);
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
		CompleteGrabbingTarget();
	}

	public override void Update()
	{
		if (GrabTargetLocation != null && !(GrabTargetLocation.Object == null) && !(GrabbedTarget != null))
		{
			float num = GrabTargetLocation.GetAngleToPosition(base.trf.position, base.trf.rotation) / (GrabRotationSpeed * 360f);
			if (num > 0f && (GrabTargetLocation.Position - base.trf.position).magnitude > 0.1f)
			{
				OverrideMatchTarget(num, base.trf.position, Quaternion.LookRotation(GrabTargetLocation.Position - base.trf.position));
			}
		}
	}

	public void OnHurt(Locomotion4.Impact impact)
	{
	}

	private void CompleteGrabbingTarget()
	{
		if (GrabTargetLocation != null && !(GrabTargetLocation.Object == null) && !(GrabbedTarget != null))
		{
			GrabbedTarget = GrabTargetLocation.Object.transform;
			if (GrabbedTarget.GetComponent<Locomotion4>() != null)
			{
				GrabbedTarget.GetComponent<Locomotion4>().DoGetGrabbed(GrabHand);
			}
			else
			{
				GrabbedTarget.GetComponent<Rigidbody>().isKinematic = true;
				GrabbedTarget.SetParent(GrabHand);
				GrabbedTarget.localRotation = Quaternion.identity;
				GrabbedTarget.localPosition = Vector3.zero;
			}
			GrabTargetLocation = null;
			base.controller.CarryMode.Set(1f);
		}
	}

	public void StartGrabbedOnGrabbed(Transform GrabHand)
	{
		parentBeforeGrab = base.trf.parent;
		base.trf.parent = GrabHand;
		base.trf.localRotation = Quaternion.identity;
		base.trf.localPosition = -base.trf.InverseTransformPoint(GrabbedPivot.position);
		base.controller.IsBeingGrabbed.Set(b: true);
	}

	public void StartGrabbed()
	{
		Locomotion.IsGrabbed = true;
	}

	public void UpdateGrabbed()
	{
	}

	public void EndGrabbed()
	{
		Locomotion.IsGrabbed = false;
		base.trf.SetParent(parentBeforeGrab);
	}

	public void Grab(ObjectLocation o)
	{
		if (GrabbedTarget == null)
		{
			StartGrabbingTarget(o);
		}
	}

	public bool IsInPlaceReadyForGrabbing()
	{
		return GrabTargetLocation != null;
	}
}
