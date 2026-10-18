using System;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Grounded : LocomotionCommonState
{
	public float TimeBeforeFalling = 0.2f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float GroundedLastTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float UngroundedLastTime;

	public float UngroundedTime => Mathf.Max(0f, UngroundedLastTime - GroundedLastTime);

	public override void Update()
	{
		if (Locomotion.AutoSnapToFloor)
		{
			UpdateGround();
		}
		else
		{
			GroundedLastTime = base.Time;
		}
	}

	public bool IsGrounded()
	{
		return Locomotion.Common.Grounded.UngroundedTime < TimeBeforeFalling;
	}

	private void UpdateGround()
	{
		if (Locomotion.CheckGrounded() || Locomotion.CheckObstacle() || GroundedLastTime <= 0f || base.animator.IsInTransition(0))
		{
			GroundedLastTime = base.Time;
		}
		else
		{
			UngroundedLastTime = base.Time;
		}
		if (!IsGrounded())
		{
			base.Common.Flailing.Flail();
		}
	}
}
