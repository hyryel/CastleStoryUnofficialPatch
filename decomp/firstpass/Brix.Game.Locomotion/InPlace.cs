using System;
using Brix.Game.AI;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;

namespace Brix.Game.Locomotion;

[Serializable]
public class InPlace : LocomotionMovementState
{
	private static BrixLogger logger = new BrixLogger(typeof(InPlace).FullName);

	public float EnterTime;

	public override void Enter()
	{
		Locomotion.InPlace = true;
		Locomotion.AutoSnapToFloor = true;
		EnterTime = base.Time;
	}

	public override void Exit()
	{
		Locomotion.InPlace = false;
	}

	public override void Update()
	{
		if (base.animator.IsInTransition(0))
		{
			return;
		}
		bool hasCurrent = base.PathIterator.HasCurrent;
		if (hasCurrent && IsOnSpot(base.PathIterator.Current))
		{
			Locomotion.AdvancePath();
			hasCurrent = base.PathIterator.HasCurrent;
		}
		if (hasCurrent)
		{
			if (IsOnSpot(base.PathIterator.Current))
			{
				Locomotion.AdvancePath();
			}
			WalkNode current = base.PathIterator.Current;
			if (current.IsBlockBreaking() && Locomotion.Labor.Profession.CanMakeWay() && HitBrick.IsBreakable(current.Voxel))
			{
				if (!IsTowards(current))
				{
					base.TurnOnSpot.TurnToDestinationPosition(current);
				}
				else
				{
					base.Movement.HitBrick.DoHitBrick(current.Voxel);
				}
			}
			else if ((current.MustFail() || IsBlockedByCharacterOrObstacle(allowPassThrough: false)) && !Locomotion.RepathMoveNext())
			{
				logger.InfoFormat("{0} at {1} has reached an auto-fail path destination: {2}", base.name, base.trf.position, current.Position);
				Locomotion.PathBlockedAt(current.Position);
			}
			else if (current.IsLadder())
			{
				if (Locomotion.CanClimb)
				{
					OverrideMatchTarget(0f, base.trf.position.Rounded(), base.trf.rotation);
					base.Ladder.UseLadder(current.Position);
				}
				else
				{
					logger.WarnFormat("{0} wants to climb ladder at {1} but can't.", base.name, current.Position);
					Locomotion.PathBlockedAt(current.Position);
				}
			}
			else if (current.MustClimbUp())
			{
				if (!IsTowards(current))
				{
					base.TurnOnSpot.TurnToDestinationPosition(current);
					return;
				}
				if (Locomotion.CanClimb)
				{
					DoClimbUp();
					return;
				}
				logger.WarnFormat("{0} must climb at {1} but can't.", base.name, current.Position);
				Locomotion.PathBlockedAt(current.Position);
			}
			else if (current.MustStepDown())
			{
				if (!IsTowards(current))
				{
					base.TurnOnSpot.TurnToDestinationPosition(current);
				}
				else
				{
					DoStepDown();
				}
			}
			else if (Locomotion.CanRun)
			{
				if (base.PathIterator.Peek == null || !base.PathIterator.Peek.IsWalkable() || LocomotionStateUpdater.IsAdjacent(base.trf.position, base.PathIterator.Peek.Position))
				{
					DoWalk();
				}
				else
				{
					DoRun();
				}
			}
			else
			{
				DoWalk();
			}
		}
		else if (base.PathIterator.Destination != null)
		{
			WalkNode destination = base.PathIterator.Destination;
			if (!IsOnSpot(destination))
			{
				logger.WarnFormat("{0} at {1} has never reached their path destination: {2}", base.name, base.trf.position, destination.Position);
				Locomotion.PathBlockedAt(destination.Position);
			}
			else if (!IsClose(destination, 0.1f))
			{
				DoWalk();
			}
			else if (destination.HasSpecificOrientation && !IsAligned(destination))
			{
				base.TurnOnSpot.TurnToDestinationOrientation(destination);
			}
			else
			{
				Locomotion.HasArrived.Invoke();
				Locomotion.ClearPath();
			}
		}
	}
}
