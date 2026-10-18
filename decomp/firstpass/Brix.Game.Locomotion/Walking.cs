using System;
using Brix.Game.AI;
using Brix.Util;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Walking : LocomotionWalkableCycleState
{
	public float WalkArriveSpeedMperS = 2f;

	public float WalkSpeedMperS = 3f;

	public float WalkTurningTopSpeed = 1f;

	public float WalkBlockedDelay = 0.1f;

	public float WalkBlockedMoveFactor = 0.1f;

	public float WalkBlockedMaxCount = 20f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public WalkNode WalkingCurrent;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Location WalkStopDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float WalkBlockedTimeout;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int WalkBlockedCount;

	protected override float SpeedMperS => WalkSpeedMperS;

	protected override float TurningTopSpeed => WalkTurningTopSpeed;

	protected override float BlockedDelay => WalkBlockedDelay;

	protected override float BlockedMoveFactor => WalkBlockedMoveFactor;

	protected override float BlockedMaxCount => WalkBlockedMaxCount;

	protected override float BlockedTimeout
	{
		get
		{
			return WalkBlockedTimeout;
		}
		set
		{
			WalkBlockedTimeout = value;
		}
	}

	protected override int BlockedCount
	{
		get
		{
			return WalkBlockedCount;
		}
		set
		{
			WalkBlockedCount = value;
		}
	}

	protected override void Stop()
	{
		WalkStop();
	}

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = true;
		Locomotion.Walking = true;
		WalkingCurrent = base.PathIterator.Current;
	}

	public override void Exit()
	{
		Locomotion.Walking = false;
		WalkingCurrent = null;
	}

	public override void Update()
	{
		if (Locomotion.CanRun && base.PathIterator.Peek != null && base.PathIterator.Peek.IsWalkable() && !base.animator.IsInTransition(0))
		{
			DoRun();
			return;
		}
		Move();
		WalkingCurrent = base.PathIterator.Current;
	}

	private void WalkStop()
	{
		WalkStopDestination = new SpaceLocation(Calcule.RoundVecteur(base.trf.position), base.trf.rotation, specificOrientation: false);
		Vector3 vector = (WalkStopDestination.Position - base.trf.position).Flatten();
		if (base.PathIterator.HasCurrent && base.PathIterator.Current.HasSpecificOrientation)
		{
			WalkStopDestination.Orientation = base.PathIterator.Current.Orientation;
		}
		else if (vector.sqrMagnitude >= 0.010000001f && Vector3.Dot(vector, base.trf.forward) > 0f)
		{
			WalkStopDestination.Orientation = Quaternion.LookRotation(vector);
		}
		float num = vector.magnitude / WalkArriveSpeedMperS;
		float rotationTime = Mathf.Min(num, Quaternion.Angle(base.trf.rotation, WalkStopDestination.Orientation) / (WalkTurningTopSpeed * 360f));
		OverrideMatchTarget(num, rotationTime, WalkStopDestination.Position, WalkStopDestination.Orientation);
		base.controller.IsWalkCycling.Set(b: false);
	}
}
