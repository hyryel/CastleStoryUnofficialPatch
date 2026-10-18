using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class CloseStepStart : LocomotionMovementState
{
	public float StepStartFraction = 0.5f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepOrigin;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Quaternion StepOrientation;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StepStartDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float StepDir;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public WalkNode Destination;

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = true;
		StepOrigin = base.trf.position;
		Destination = ((!base.PathIterator.HasCurrent) ? null : base.PathIterator.Current);
		if (Destination != null)
		{
			StepDestination = Destination.Position;
			StepDestination = Architecte.SurfaceRaycast(StepDestination);
			StepOrientation = Quaternion.LookRotation((StepDestination - StepOrigin).Flatten());
			StepDir = Destination.GetDirectionToPosition(base.trf.position, base.trf.rotation);
		}
		else
		{
			StepDestination = StepOrigin;
			StepOrientation = base.trf.rotation;
			StepDir = 0f;
		}
		StepStartDestination = Vector3.Lerp(base.trf.position, StepDestination, StepStartFraction);
		base.controller.StepWithLeftFoot.Set(b: true);
		base.controller.CloseStepStartDirection.Set(StepDir);
		MatchTarget(base.controller.CloseStepStart, StepStartDestination, StepOrientation, 0.1f, 0.9f);
	}
}
