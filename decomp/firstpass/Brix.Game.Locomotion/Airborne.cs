using System;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Airborne : LocomotionCommonState
{
	public override void Enter()
	{
		Locomotion.IsAirborne = true;
		Locomotion.ClearPath();
	}

	public override void Exit()
	{
		Locomotion.IsAirborne = false;
	}

	private Vector3 GetGoodStandingPosition()
	{
		Vector3 position = base.trf.position;
		return Locomotion.GetNearestAligned(position);
	}

	public void StartStabilizing()
	{
		Locomotion.AutoSnapToFloor = false;
		Vector3 goodStandingPosition = GetGoodStandingPosition();
		goodStandingPosition.y += Locomotion.RegularCollider.GetComponent<CapsuleCollider>().radius;
		Quaternion orientation = Quaternion.LookRotation((!(base.trf.forward.y > 0f)) ? (-Vector3.up) : Vector3.up, base.trf.up);
		MatchTargetWithY(base.controller.Stabilizing, goodStandingPosition, orientation, 0.1f, 0.9f);
	}

	public void StartStandingUp()
	{
		if (base.trf.forward.y > 0f)
		{
			base.controller.FacingUp.Set(1f);
		}
		else
		{
			base.controller.FacingUp.Set(0f);
		}
		Vector3 goodStandingPosition = GetGoodStandingPosition();
		MatchTargetWithY(base.controller.StandingUp, goodStandingPosition, 0.1f, 0.9f);
	}
}
