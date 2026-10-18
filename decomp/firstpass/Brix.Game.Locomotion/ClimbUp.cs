using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class ClimbUp : LocomotionMovementState
{
	[Serializable]
	public class ClimbMatchParameters
	{
		public float StartFraction;

		public float EndFraction;
	}

	public ClimbMatchParameters[] CarryModeParameters = new ClimbMatchParameters[5]
	{
		new ClimbMatchParameters
		{
			StartFraction = 0.2f,
			EndFraction = 0.55f
		},
		new ClimbMatchParameters
		{
			StartFraction = 0.425f,
			EndFraction = 0.85f
		},
		new ClimbMatchParameters
		{
			StartFraction = 0.425f,
			EndFraction = 0.85f
		},
		new ClimbMatchParameters
		{
			StartFraction = 0.425f,
			EndFraction = 0.85f
		},
		new ClimbMatchParameters
		{
			StartFraction = 0.425f,
			EndFraction = 0.85f
		}
	};

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ClimbStartMatchFraction;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ClimbEndMatchFraction;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 ClimbOrigin;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 ClimbDestination;

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = false;
		ClimbOrigin = base.trf.position;
		int num = Mathf.RoundToInt(base.controller.CarryMode.Get());
		ClimbStartMatchFraction = CarryModeParameters[num].StartFraction;
		ClimbEndMatchFraction = CarryModeParameters[num].EndFraction;
		WalkNode current = base.PathIterator.Current;
		if (current != null)
		{
			ClimbDestination = current.Position;
			ClimbDestination.y = FloorPicker.GetHeight(current.Position, Locomotion.FloorOffset);
			Quaternion orientation = Quaternion.LookRotation((ClimbDestination - base.trf.position).Flatten());
			MatchTargetWithY(base.controller.ClimbUp, ClimbDestination, orientation, ClimbStartMatchFraction, ClimbEndMatchFraction);
		}
		else
		{
			ClimbDestination = base.trf.position;
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
		if (base.Entering != XYZ.FromVector3(ClimbDestination))
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
