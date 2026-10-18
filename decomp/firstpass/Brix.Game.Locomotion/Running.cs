using System;
using Brix.Game.AI;
using Brix.Util;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Running : LocomotionWalkableCycleState
{
	public float RunArriveSpeedMperS = 2f;

	public float RunSpeedMperS = 6f;

	public float RunArrivalDistance = 1f;

	public float RunTurningTopSpeed = 1f;

	public float RunAbruptStopDistance = 0.75f;

	public float RunSpeedStrikeBonus = 3f;

	public float RunBlockedDelay = 0.1f;

	public float RunBlockedMoveFactor = 0.1f;

	public float RunBlockedMaxCount = 20f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public WalkNode RunningCurrent;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Location RunStopDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float RunStopDistance;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float RunBlockedTimeout;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int RunBlockedCount;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool IsRunningToStrike;

	protected override float SpeedMperS => (!Locomotion.IsInStrikingPath) ? RunSpeedMperS : GetStrikingRunSpeed();

	protected override float TurningTopSpeed => RunTurningTopSpeed;

	protected override float BlockedDelay => RunBlockedDelay;

	protected override float BlockedMoveFactor => RunBlockedMoveFactor;

	protected override float BlockedMaxCount => RunBlockedMaxCount;

	protected override float BlockedTimeout
	{
		get
		{
			return RunBlockedTimeout;
		}
		set
		{
			RunBlockedTimeout = value;
		}
	}

	protected override int BlockedCount
	{
		get
		{
			return RunBlockedCount;
		}
		set
		{
			RunBlockedCount = value;
		}
	}

	protected override void Stop()
	{
		if (!base.animator.IsInTransition(0))
		{
			RunStop();
		}
	}

	public override void Enter()
	{
		Locomotion.AutoSnapToFloor = true;
		Locomotion.Running = true;
		RunningCurrent = base.PathIterator.Current;
		RunAbruptStopDistance = 0.1f;
	}

	public override void Exit()
	{
		Locomotion.Running = false;
		RunningCurrent = null;
	}

	public override void Update()
	{
		if (!Locomotion.CanRun && !base.animator.IsInTransition(0))
		{
			DoWalk();
			return;
		}
		Move();
		RunningCurrent = base.PathIterator.Current;
	}

	private void PrepareStop()
	{
		RunStopDestination = new SpaceLocation(Calcule.RoundVecteur(base.trf.position), base.trf.rotation, specificOrientation: false);
		if (base.PathIterator.HasCurrent && base.PathIterator.Current.HasSpecificOrientation)
		{
			RunStopDestination.Orientation = base.PathIterator.Current.Orientation;
		}
		RunStopDistance = (RunStopDestination.Position - base.trf.position.Flatten()).Flatten().magnitude;
	}

	private void RunStop()
	{
		PrepareStop();
		base.controller.ArrivalDirection.Set(RunStopDestination.GetDirectionToOrientation(base.trf.rotation));
		base.controller.IsRunCycling.Set(b: false);
		float num = ((!Locomotion.IsInStrikingPath) ? RunArriveSpeedMperS : GetStrikingRunSpeed());
		OverrideMatchTarget(RunStopDistance / num, RunStopDestination.Position, RunStopDestination.Orientation);
	}

	private float GetStrikingRunSpeed()
	{
		float num = ((!base.PathIterator.Active) ? 0f : (base.PathIterator.Destination.Position - base.trf.position).magnitude);
		return RunSpeedMperS + RunSpeedStrikeBonus * Mathf.Clamp01(1f - num / 4f);
	}
}
