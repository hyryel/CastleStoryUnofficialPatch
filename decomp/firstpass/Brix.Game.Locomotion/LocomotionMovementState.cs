using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Pathfinding;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class LocomotionMovementState : LocomotionStateUpdater
{
	private static Porte _door;

	protected InPlace InPlace => base.Movement.InPlace;

	protected Walking Walking => base.Movement.Walking;

	protected Running Running => base.Movement.Running;

	protected Halting Halting => base.Movement.Halting;

	protected ClimbUp ClimbUp => base.Movement.ClimbUp;

	protected StepDown StepDown => base.Movement.StepDown;

	protected Ladder Ladder => base.Movement.Ladder;

	protected JumpingOff JumpingOff => base.Movement.JumpingOff;

	protected TurnOnSpot TurnOnSpot => base.Movement.TurnOnSpot;

	protected CloseStep CloseStep => base.Movement.CloseStep;

	protected CloseStepStart CloseStepStart => base.Movement.CloseStepStart;

	protected CloseStepEnd CloseStepEnd => base.Movement.CloseStepEnd;

	protected XYZ Occupied => Locomotion.PositionOccupied;

	protected XYZ Entering => Locomotion.PositionEntering;

	protected XYZ Next => Locomotion.PositionNext;

	protected void DoWalk()
	{
		base.controller.IsWalkCycling.Set(b: true);
	}

	protected void DoCloseStep()
	{
		base.controller.IsClosingStepStart.Set(b: true);
	}

	protected void DoRun()
	{
		base.controller.IsRunCycling.Set(b: true);
	}

	protected void DoClimbUp()
	{
		base.controller.IsClimbingUp.Set(b: true);
	}

	protected void DoStepDown()
	{
		base.controller.IsSteppingDown.Set(b: true);
	}

	protected bool IsBlockedByCharacterOrObstacle(bool allowPassThrough)
	{
		VoxelPresence blocker = GetBlocker();
		if ((blocker == null || (allowPassThrough && CanPassThroughBlocker(blocker))) && !IsBlockedByDynamicObstacle())
		{
			return false;
		}
		return true;
	}

	protected bool IsBlockedByTerrainOrBlock()
	{
		return Voxel.IsFull(Entering);
	}

	protected bool IsBlockedByDynamicObstacle()
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(Entering, VProperty.obstacle, (IVolume v, GameObject _gameObject) => !v.PassThrough.CanPassThrough(_gameObject), Locomotion.gameObject);
	}

	protected bool IsBlockedByDynamicObstacle(out Porte door)
	{
		_door = null;
		bool result = VolumeRegistry.HasVolumesWithPropertyAt(Entering, VProperty.obstacle, delegate(IVolume v, GameObject _gameObject)
		{
			IPassThrough passThrough = v.PassThrough;
			if (passThrough is Porte)
			{
				_door = passThrough as Porte;
			}
			return !passThrough.CanPassThrough(_gameObject);
		}, Locomotion.gameObject);
		door = _door;
		return result;
	}

	protected bool IsWalkable(XYZ origin, WalkNode destination)
	{
		if (destination == null)
		{
			return false;
		}
		if (destination.IsWalkable())
		{
			return true;
		}
		PathBranch.BranchType branchType = PathNodeRegistry.PermissionVers(origin, destination.Voxel);
		return branchType == PathBranch.BranchType.Full || branchType == PathBranch.BranchType.Forest;
	}

	protected bool IsWalkable()
	{
		PathBranch.BranchType branchType = PathNodeRegistry.PermissionVers(Occupied, Entering);
		return branchType == PathBranch.BranchType.Full || branchType == PathBranch.BranchType.Forest;
	}

	protected bool IsNavigatable(XYZ origin, WalkNode destination)
	{
		if (destination == null)
		{
			return false;
		}
		if (!destination.MustFail())
		{
			return true;
		}
		PathBranch.BranchType branchType = PathNodeRegistry.PermissionVers(origin, destination.Voxel);
		return branchType != PathBranch.BranchType.Empty;
	}

	protected bool IsNavigatable()
	{
		PathBranch.BranchType branchType = PathNodeRegistry.PermissionVers(Occupied, Entering);
		return branchType != PathBranch.BranchType.Empty;
	}

	protected bool IsBlockedByTree()
	{
		PathBranch.BranchType branchType = PathNodeRegistry.PermissionVers(Occupied, Entering);
		return branchType == PathBranch.BranchType.Forest && !IsInForest();
	}

	protected bool IsInForest()
	{
		return TreeRegistry.Instance.IsNearATree(Occupied);
	}

	protected bool HasBlocker()
	{
		return VoxelPresence.HasPresenceAt(Entering);
	}

	protected VoxelPresence GetBlocker()
	{
		return VoxelPresence.GetPresenceAt(Entering).FirstOrDefault();
	}

	protected bool IsIdleBlocker(VoxelPresence blocker)
	{
		return blocker != null && !blocker.Locomotion.PathIterator.HasCurrent;
	}

	protected bool CanPassThroughBlocker(VoxelPresence blocker)
	{
		return blocker != null && Locomotion.Occupation.Profession.CanPassThrough() && Affiliation.IsAllied(Locomotion.faction, blocker.faction);
	}

	protected void FastForwardPath()
	{
		if (!base.PathIterator.HasCurrent)
		{
			return;
		}
		int currentIndex = base.PathIterator.CurrentIndex;
		XYZ a = XYZ.FromVector3(base.trf.position);
		for (int i = Mathf.Max(0, currentIndex - 1); base.PathIterator.GetNth(i) != null; i++)
		{
			if (LocomotionStateUpdater.IsSameVoxelOrAlmost(a, XYZ.FromVector3(base.PathIterator.GetNth(i).Position)))
			{
				if (i >= currentIndex)
				{
					base.PathIterator.MoveNext(i - currentIndex + 1);
				}
				break;
			}
		}
	}
}
