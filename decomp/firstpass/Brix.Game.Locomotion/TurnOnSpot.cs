using System;
using Brix.Game.AI;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class TurnOnSpot : LocomotionMovementState
{
	public float TurningSpeed = 2f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float TurnRelativeDirection;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Quaternion TurnToOrientation;

	public void TurnToDestinationOrientation(WalkNode destination)
	{
		TurnRelativeDirection = destination.GetDirectionToOrientation(base.trf.rotation);
		TurnToOrientation = destination.Orientation;
		Turn();
	}

	public void TurnToDestinationPosition(WalkNode destination)
	{
		TurnRelativeDirection = destination.GetDirectionToPosition(base.trf.position, base.trf.rotation);
		TurnToOrientation = GetOrientationToPosition(destination);
		Turn();
	}

	private Quaternion GetOrientationToPosition(WalkNode destination)
	{
		Vector3 normalized = (destination.Position - base.trf.position).Flatten().normalized;
		return Quaternion.LookRotation(normalized, Vector3.up);
	}

	private void Turn()
	{
		base.controller.TurnDirection.Set(TurnRelativeDirection);
		base.controller.IsTurningOnSpot.Set(b: true);
	}

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = true;
		Locomotion.TurningOnSpot = true;
		float time = Quaternion.Angle(base.trf.rotation, TurnToOrientation) / (TurningSpeed * 360f);
		OverrideMatchTarget(time, base.trf.position, TurnToOrientation);
	}

	public override void Exit()
	{
		Locomotion.TurningOnSpot = false;
	}

	public override void Update()
	{
	}
}
