using Brix.Engine;
using Brix.Game.AI;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.Locomotion;

public abstract class LocomotionWalkableCycleState : LocomotionMovementState
{
	public enum MoveIntention
	{
		PushForward,
		WaitActively,
		Stop,
		Transfer,
		Fail
	}

	private static BrixLogger logger = new BrixLogger(typeof(LocomotionWalkableCycleState).FullName);

	protected abstract float SpeedMperS { get; }

	protected abstract float TurningTopSpeed { get; }

	protected abstract float BlockedDelay { get; }

	protected abstract float BlockedMoveFactor { get; }

	protected abstract float BlockedMaxCount { get; }

	protected abstract float BlockedTimeout { get; set; }

	protected abstract int BlockedCount { get; set; }

	protected void Move()
	{
		if (!(base.Time < BlockedTimeout))
		{
			FastForwardPath();
			MoveIntention moveIntention = TryPushingForward();
			switch (moveIntention)
			{
			case MoveIntention.PushForward:
				MatchCurrentTarget();
				break;
			case MoveIntention.WaitActively:
				WaitActively();
				break;
			case MoveIntention.Stop:
				Stop();
				break;
			case MoveIntention.Transfer:
				Transfer();
				break;
			case MoveIntention.Fail:
				logger.InfoFormat("Locomotion cycle couldn't avoid blocked path for '{0}' at '{1}' going towards '{2}'.", base.name, base.trf.position, base.Entering);
				Locomotion.PathBlockedAt(base.trf.position);
				break;
			}
			BlockedCount = ((moveIntention == MoveIntention.WaitActively) ? (BlockedCount + 1) : 0);
		}
	}

	protected MoveIntention TryPushingForward()
	{
		if (!base.PathIterator.HasCurrent)
		{
			return MoveIntention.Stop;
		}
		VoxelPresence blocker = GetBlocker();
		Porte door = null;
		bool flag = IsBlockedByDynamicObstacle(out door);
		if (blocker == null && !flag)
		{
			if (door != null)
			{
				door.PassageEngage.Invoke(Locomotion.presence);
			}
			if (IsWalkable())
			{
				return MoveIntention.PushForward;
			}
			if (IsNavigatable())
			{
				return MoveIntention.Stop;
			}
		}
		if (Locomotion.RepathMoveNext())
		{
			IsBlockedByDynamicObstacle(out door);
			if (door != null)
			{
				door.PassageEngage.Invoke(Locomotion.presence);
			}
			if (IsWalkable())
			{
				return MoveIntention.PushForward;
			}
			return MoveIntention.Stop;
		}
		if (flag)
		{
			return MoveIntention.Fail;
		}
		if (blocker == null)
		{
			return MoveIntention.Fail;
		}
		if (!IsNavigatable())
		{
			return MoveIntention.Fail;
		}
		if (CanPassThroughBlocker(blocker))
		{
			if (IsWalkable())
			{
				return MoveIntention.PushForward;
			}
			return MoveIntention.Stop;
		}
		if (IsIdleBlocker(blocker))
		{
			return MoveIntention.Fail;
		}
		if (!IsWalkable())
		{
			return MoveIntention.Stop;
		}
		if ((float)BlockedCount >= BlockedMaxCount)
		{
			return MoveIntention.Fail;
		}
		return MoveIntention.WaitActively;
	}

	protected void MatchCurrentTarget()
	{
		Vector3 currentTarget = GetCurrentTarget();
		MatchTargetWithSpeed(currentTarget, SpeedMperS, TurningTopSpeed);
	}

	protected void MatchTargetWithSpeed(Vector3 position, float speed, float turningTopSpeed)
	{
		Vector3 a = position - base.trf.position;
		Quaternion quaternion = ((a.x == 0f && a.z == 0f) ? base.trf.rotation : Quaternion.LookRotation(a.Flatten()));
		float num = a.magnitude / speed;
		float rotationTime = Mathf.Min(num, Quaternion.Angle(base.trf.rotation, quaternion) / (turningTopSpeed * 360f));
		OverrideMatchTarget(num, rotationTime, position, quaternion);
	}

	protected void WaitActively()
	{
		OverrideMatchTarget(BlockedDelay, Vector3.Lerp(base.trf.position, Calcule.RoundVecteur(base.trf.position), BlockedMoveFactor), base.trf.rotation);
		BlockedTimeout = base.Time + BlockedDelay;
	}

	protected Vector3 GetCurrentTarget()
	{
		XYZ xYZ = XYZ.FromVector3(base.trf.position);
		XYZ xYZ2 = XYZ.FromVector3(base.PathIterator.Current.Position);
		Vector3 edgePoint = GetEdgePoint(xYZ, xYZ2);
		WalkNode peek = base.PathIterator.Peek;
		if (!IsWalkable(xYZ2, peek))
		{
			return Vector3.Lerp(edgePoint, xYZ2.ToVector3(), 0.1f);
		}
		XYZ xYZ3 = XYZ.FromVector3(peek.Position);
		if (IsCorner(xYZ, xYZ2, xYZ3) && IsSteep(xYZ, xYZ2, xYZ3))
		{
			return edgePoint + -(xYZ3 - xYZ2).Flatten().ToVector3() * 0.3f + (xYZ2 - xYZ).ToVector3() * 0.1f;
		}
		Vector3 edgePoint2 = GetEdgePoint(xYZ2, xYZ3);
		return Vector3.Lerp(edgePoint, edgePoint2, 0.5f);
	}

	private bool IsSteep(XYZ a, XYZ b, XYZ c)
	{
		return a.y != b.y && b.y != c.y && c.y != a.y;
	}

	private bool IsCorner(XYZ a, XYZ b, XYZ c)
	{
		return a.x != b.x == (b.z != c.z) && a.z != b.z == (b.x != c.x);
	}

	protected Vector3 GetEdgePoint(XYZ a, XYZ b)
	{
		return (a + b).ToVector3() / 2f;
	}

	protected void Transfer()
	{
		VoxelPresence blocker = GetBlocker();
		Locomotion.Labor.navigation.AssignBlocker(blocker);
	}

	protected abstract void Stop();
}
