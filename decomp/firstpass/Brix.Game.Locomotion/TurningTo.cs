using System;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class TurningTo : LocomotionFightingState
{
	[DisplayRules(DisplayFlags.ReadOnly)]
	public Location TurnDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool IsTurning;

	public void CombatTurnTo(Location location)
	{
		TurnDestination = location;
		if (base.Striking.SwordStrike != null)
		{
			base.Striking.SwordStrike.Target = null;
		}
		base.controller.IsTurningOnSpotCombat.Set(b: true);
	}

	public override void Enter()
	{
		IsTurning = true;
		Vector3 forward = base.trf.forward;
		Vector3 a;
		if (base.Striking.SwordStrike == null || base.Striking.SwordStrike.Target.IsNullOrReleased())
		{
			a = ((TurnDestination == null) ? base.trf.forward : (TurnDestination.Position - base.trf.position));
		}
		else
		{
			a = base.Striking.SwordStrike.Target.GameObject.transform.position - base.trf.position;
			a += Vector3.up;
		}
		float num = Vector3.Angle(forward.Flatten(), a.Flatten());
		if (Vector3.Dot(base.trf.right.Flatten(), a.Flatten()) < 0f)
		{
			num = 0f - num;
		}
		base.controller.TurnDirection.Set(num / 180f);
		OverrideMatchTarget(num / (360f * base.Movement.Running.RunTurningTopSpeed), base.trf.position, Quaternion.LookRotation(a.Flatten()));
	}

	public override void Exit()
	{
		IsTurning = false;
	}
}
