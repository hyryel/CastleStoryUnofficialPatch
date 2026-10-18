using System;
using Brix.Game.AI;
using Brix.Utils.Unity;

namespace Brix.Game.Locomotion;

[Serializable]
public class Trapped : LocomotionCommonState
{
	[DisplayRules(DisplayFlags.ReadOnly)]
	public Location TrapLocation;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float TimeToLeaveTrap;

	public void Trap(Location l, float duration)
	{
		base.controller.IsGettingTrapped.Set(b: true);
		TrapLocation = l;
		TimeToLeaveTrap = base.Time + duration;
	}

	public void LeaveTrap()
	{
		base.controller.IsLeavingTrap.Set(b: true);
	}

	public override void Enter()
	{
		MatchTarget(base.controller.GettingTrapped, TrapLocation.Position, TrapLocation.Orientation, 0.1f, 0.9f);
	}

	public override void Update()
	{
		if (TimeToLeaveTrap > 0f && base.Time >= TimeToLeaveTrap)
		{
			TimeToLeaveTrap = 0f;
			LeaveTrap();
		}
	}
}
