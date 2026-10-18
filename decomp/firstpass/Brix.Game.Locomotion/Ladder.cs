using System;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Ladder : LocomotionMovementState
{
	public float LadderSpeed = 2f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 LadderDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool LadderGoingUp;

	public void UseLadder(Vector3 destination)
	{
		LadderDestination = destination;
		LadderGoingUp = destination.y > base.trf.position.y;
		if (LadderGoingUp)
		{
			base.controller.IsGrabbingLadderFromBottom.Set(b: true);
		}
		else
		{
			base.controller.IsGrabbingLadderFromTop.Set(b: true);
		}
	}

	public override void Enter()
	{
		Locomotion.IsUsingLadder = true;
		Locomotion.AutoSnapToFloor = false;
		OverrideMatchTarget(Mathf.Abs(LadderDestination.y - base.trf.position.y) / LadderSpeed, LadderDestination, base.trf.rotation);
	}

	public override void Exit()
	{
		Locomotion.IsUsingLadder = false;
		Locomotion.AutoSnapToFloor = true;
	}

	public override void Update()
	{
		if (Mathf.Abs(LadderDestination.y - base.trf.position.y) < 0.5f)
		{
			if (LadderGoingUp)
			{
				base.controller.IsGrabbingLadderFromTop.Set(b: false);
			}
			else
			{
				base.controller.IsGrabbingLadderFromBottom.Set(b: false);
			}
		}
		OverrideMatchTarget(Mathf.Abs(LadderDestination.y - base.trf.position.y) / LadderSpeed, LadderDestination, base.trf.rotation);
	}
}
