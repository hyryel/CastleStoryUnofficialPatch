using System;
using Brix.Game.AI;
using Brix.Utils.Logging;

namespace Brix.Game.Locomotion;

[Serializable]
public class Fighting : LocomotionStateUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(Fighting).FullName);

	public ReadyToStrike ReadyToStrike;

	public TurningTo TurningTo;

	public Stepping Stepping;

	public Striking Striking;

	public Blocking Blocking;

	public bool Stopping;

	public void StartFighting(Tool weapon)
	{
		base.controller.IsTakingCombatStance.Set(b: true);
		base.controller.IsAbortingStrike.Set(b: false);
	}

	public void StopFighting()
	{
		base.controller.IsTakingCombatStance.Set(b: false);
		base.controller.IsAbortingStrike.Set(b: true);
	}

	public void FollowPath(IPath path)
	{
		if (!Stopping)
		{
			logger.WarnFormat("Receiving a FollowPath message while in combat stance. This may lead to strange behaviours! {0} at {1} pathing from {2}", base.name, base.trf.position, path.Start);
			Stopping = true;
		}
		StopFighting();
	}

	public override void Enter()
	{
		Locomotion.InCombatStance = true;
		Stopping = false;
	}

	public override void Exit()
	{
		Locomotion.InCombatStance = false;
		base.controller.AttackWillHit.Set(b: false);
		base.controller.AttackIsBlocked.Set(b: false);
		Stopping = false;
	}

	public override void Update()
	{
		if (base.PathIterator.Active && !Stopping)
		{
			logger.InfoFormat("Receiving a FollowPath message while in combat stance. This may lead to strange behaviours! {0} at {1} pathing from {2}", base.name, base.trf.position, base.PathIterator.Path.Start);
			Stopping = true;
		}
		if (Stopping)
		{
			StopFighting();
		}
	}
}
