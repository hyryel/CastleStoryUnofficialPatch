using System;
using Brix.Game.AI.Damage;
using Brix.Utils.Unity;

namespace Brix.Game.Locomotion;

[Serializable]
public class ReadyToStrike : LocomotionFightingState
{
	public float IdleTimeoutDelay = 20f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float IdleTimeout;

	public override void Enter()
	{
		Locomotion.IsReadyToStrike = true;
		IdleTimeout = base.Time + IdleTimeoutDelay;
		base.controller.IsAbortingStrike.Set(b: false);
	}

	public override void Exit()
	{
		Locomotion.IsReadyToStrike = false;
	}

	public override void Update()
	{
		if (base.Striking.SwordStrike != null && !base.Striking.SwordStrike.Target.IsNullOrReleased())
		{
			base.controller.IsAttacking.Set(b: true);
		}
		else if (base.Blocking.BlockingDuringStrike)
		{
			base.Blocking.BlockingDuringStrike = false;
			base.Blocking.EnableShieldBlock(base.Blocking.BlockingDuration);
		}
		if (IdleTimeoutDelay > 0f && base.Time >= IdleTimeout)
		{
			base.Fighting.StopFighting();
		}
	}
}
