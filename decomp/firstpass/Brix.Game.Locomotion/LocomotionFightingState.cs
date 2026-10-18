using System;
using Brix.Engine;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class LocomotionFightingState : LocomotionStateUpdater
{
	protected ReadyToStrike ReadyToStrike => base.Fighting.ReadyToStrike;

	protected TurningTo TurningTo => base.Fighting.TurningTo;

	protected Stepping Stepping => base.Fighting.Stepping;

	protected Striking Striking => base.Fighting.Striking;

	protected Blocking Blocking => base.Fighting.Blocking;

	protected bool WantsStop(Vector3 destination, GameObject strikeTarget)
	{
		XYZ xYZ = XYZ.FromVector3(base.trf.position);
		XYZ xYZ2 = XYZ.FromVector3(destination);
		if (xYZ == xYZ2)
		{
			return false;
		}
		if (!LocomotionStateUpdater.IsAdjacent(xYZ, xYZ2))
		{
			return true;
		}
		if (VoxelPresence.HasPresenceAt(xYZ2))
		{
			return true;
		}
		if (strikeTarget != null && LocomotionStateUpdater.IsAdjacent(xYZ2, XYZ.FromVector3(strikeTarget.transform.position)) && Location.Of(strikeTarget).TargetReached(Locomotion.gameObject, xYZ.ToVector3()))
		{
			return true;
		}
		return false;
	}
}
