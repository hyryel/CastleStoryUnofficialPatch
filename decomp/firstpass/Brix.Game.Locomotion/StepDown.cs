using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class StepDown : LocomotionMovementState
{
	public float StepDownMatchFraction = 0.7f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepOrigin;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepDestination;

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = false;
		StepOrigin = base.trf.position;
		WalkNode current = base.PathIterator.Current;
		if (current != null)
		{
			StepDestination = current.Position;
			StepDestination.y = FloorPicker.GetHeight(current.Position, Locomotion.FloorOffset);
			Quaternion orientation = Quaternion.LookRotation((StepDestination - base.trf.position).Flatten());
			MatchTargetWithY(base.controller.StepDown, StepDestination, orientation, 0.4f, StepDownMatchFraction);
		}
		else
		{
			StepDestination = base.trf.position;
			Halt();
		}
	}

	public override void Exit()
	{
		Locomotion.AutoSnapToFloor = true;
	}

	public override void Update()
	{
		Locomotion.Common.Grounded.GroundedLastTime = base.Time;
		FastForwardPath();
		if (IsBlocked())
		{
			Halt();
		}
	}

	private bool IsBlocked()
	{
		if (base.Entering != XYZ.FromVector3(StepDestination))
		{
			return false;
		}
		return IsBlockedByCharacterOrObstacle(allowPassThrough: true);
	}

	private void Halt()
	{
		OverrideMatchTarget(0f, base.trf.position, base.trf.rotation);
		base.controller.IsHalting.Set(b: true);
	}
}
