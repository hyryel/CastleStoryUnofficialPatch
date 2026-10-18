using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Util;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Halting : LocomotionMovementState
{
	private static BrixLogger logger = new BrixLogger(typeof(Halting).FullName);

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Location HaltDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float LastSoftHalt;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 LastSoftDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float LastWarning;

	public void Halt(Location haltDestination)
	{
		Init(haltDestination);
		ValidateDestination();
		if (Locomotion.NoHalt())
		{
			DoSoftHalt();
		}
		else
		{
			DoHardHalt();
		}
	}

	public void SoftHalt(Location haltDestination)
	{
		Init(haltDestination);
		DoSoftHalt();
	}

	private void Init(Location haltDestination)
	{
		HaltDestination = haltDestination;
	}

	private void DoSoftHalt()
	{
		if (base.Time >= LastSoftHalt + 0.1f)
		{
			ValidateDestination();
			LastSoftHalt = base.Time;
			LastSoftDestination = HaltDestination.Position;
			LastWarning = 0f;
			if (Locomotion.InPlace)
			{
				MatchTarget();
			}
			SetPath();
		}
		else if (base.Time >= LastWarning + 1f)
		{
			LastWarning = base.Time;
			logger.InfoFormat("Halting.SoftHalt has been called twice in a short laspse of time: name={0}, from={1}, to={2}, lastto={3}, time={4}, lasttime={5}", Locomotion.name, base.trf.position.ToPreciseString(), HaltDestination.Position.ToPreciseString(), LastSoftDestination.ToPreciseString(), base.Time, LastSoftHalt);
		}
	}

	private void DoHardHalt()
	{
		base.controller.IsHalting.Set(b: true);
		SetPath();
	}

	public override void Enter()
	{
		Locomotion.IsHalting = true;
		Locomotion.AutoSnapToFloor = true;
	}

	public override void Exit()
	{
		Locomotion.IsHalting = false;
	}

	private void MatchTarget(float time = 0.1f)
	{
		ValidateDestination();
		OverrideMatchTarget(time, HaltDestination.Position, HaltDestination.Orientation);
	}

	private void MatchNow()
	{
		ValidateDestination();
		Locomotion.SetPosition(HaltDestination.Position, HaltDestination.Orientation);
	}

	private void SetPath()
	{
		ValidateDestination();
		Locomotion.ClearPath();
	}

	private void ValidateDestination()
	{
		if (XYZ.FromVector3(HaltDestination.Position) != XYZ.FromVector3(base.trf.position))
		{
			logger.WarnFormat("Halting has an off-voxel destination: name={0}, from={1}, to={2}", Locomotion.name, base.trf.position.ToPreciseString(), HaltDestination.Position.ToPreciseString());
			HaltDestination = new SpaceLocation(Calcule.RoundVecteur(base.trf.position), HaltDestination.Orientation, HaltDestination.HasSpecificOrientation);
		}
	}
}
