using System;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Stepping : LocomotionFightingState
{
	public float CombatStepSpeed = 5f;

	public Vector3 CombatStepDestination;

	public Quaternion CombatStepOrientation;

	public bool IsCombatStepping;

	public void CombatStep(Location dest)
	{
		CombatStepDestination = dest.Position;
		CombatStepOrientation = dest.Orientation;
		base.controller.IsCombatStepping.Set(b: true);
	}

	public override void Enter()
	{
		IsCombatStepping = true;
		if (WantsStop(CombatStepDestination, null))
		{
			Stop();
		}
		else
		{
			Match();
		}
	}

	public override void Exit()
	{
		IsCombatStepping = false;
	}

	public override void Update()
	{
		if (WantsStop(CombatStepDestination, null))
		{
			Stop();
		}
	}

	private void Stop()
	{
		CombatStepDestination = Locomotion.PositionOccupied.ToVector3();
		Match();
	}

	private void Match()
	{
		OverrideMatchTarget(Vector3.Distance(CombatStepDestination, base.trf.position) / CombatStepSpeed, CombatStepDestination, CombatStepOrientation);
	}
}
