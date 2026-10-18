using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class CloseStepEnd : LocomotionMovementState
{
	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepOrigin;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Quaternion ArrivalOrientation;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ArrivalDir;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public WalkNode Destination;

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = true;
		StepOrigin = base.CloseStepStart.StepOrigin;
		Destination = base.CloseStepStart.Destination;
		if (Destination != null && Destination.IsWalkable())
		{
			StepDestination = Destination.Position;
			StepDestination = Architecte.SurfaceRaycast(StepDestination);
			if (Destination.HasSpecificOrientation)
			{
				ArrivalOrientation = Destination.Orientation;
				ArrivalDir = Destination.GetDirectionToOrientation(base.trf.rotation);
			}
			else
			{
				ArrivalOrientation = Quaternion.LookRotation((StepDestination - StepOrigin).Flatten());
				ArrivalDir = Destination.GetDirectionToPosition(base.trf.position, base.trf.rotation);
			}
			if ((bool)base.controller.StepWithLeftFoot)
			{
				ArrivalDir = 0f - ArrivalDir;
			}
		}
		else
		{
			StepDestination = StepOrigin;
			ArrivalOrientation = base.trf.rotation;
			ArrivalDir = 0f;
		}
		base.controller.ArrivalDirection.Set(ArrivalDir);
		MatchTarget(base.controller.CloseStepEnd, StepDestination, ArrivalOrientation, 0.1f, 0.9f);
	}
}
