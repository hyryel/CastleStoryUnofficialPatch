using Brix.Audio;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Input;
using UnityEngine;

namespace Brix.Game.AI;

public static class DirectOrderProject
{
	public static bool CheckLeftShift()
	{
		return PickingUtility.Player.GetButton("cam_mouse_alt");
	}

	public static void AddTurnToGoal(Labor labor, Location location, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdTurnTo(labor.gameObject, location.Position);
	}

	public static void AddMoveToGoal(Labor labor, Vector3 position, bool drop = false, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddMoveToGoal(labor.gameObject, position, drop, queued);
	}

	public static void AddMoveToVolumeGoal(Labor labor, IVolume toObject, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddMoveToVolumeGoal(labor.gameObject, toObject.GameObject, queued);
	}

	public static void ShootLocation(Labor labor, Location location, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		if (location.Object.IsNullOrReleased())
		{
			User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdFireAt(labor.gameObject, location.Position);
		}
		else
		{
			User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdFireAtObject(labor.gameObject, location.Object);
		}
	}

	public static void TransformLog(Labor labor, GameObject objectToTransform)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdTransformLog(labor.gameObject, objectToTransform);
	}

	public static void JumpOff(Labor labor, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdJumpOff(labor.gameObject, queued);
	}

	public static void GoAndJumpTo(Labor labor, Vector3 position)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdGoAndJumpTo(labor.gameObject, position);
	}

	public static void Stop(Labor labor)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdStop(labor.gameObject);
	}

	public static void AddEmergencyRecallGoal(Labor labor, GameObject go)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddEmergencyRecallGoal(labor.gameObject, go);
	}

	public static void AddCallToArmsGoal(Labor labor, GameObject go)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddCallToArmsGoal(labor.gameObject, go);
	}

	public static void AddBackToworkGoal(Labor labor)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddBackToWorkGoal(labor.gameObject);
	}

	public static void AddCleanGoal(Labor labor, Vector3 position, GameObject onlyObjectsLikeThisOne, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.ActionClean, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddCleanGoal(labor.gameObject, position, onlyObjectsLikeThisOne, queued);
	}

	public static void AddNatureHarvestGoal(Labor labor, int index)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddNatureHarvestGoal(labor.gameObject, index);
	}

	public static void GoDetonate(Labor labor, GameObject barel)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdGoDetonate(labor.gameObject, barel);
	}

	public static void AddGoDropInto(Labor labor, GameObject target)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdGoStoreInto(labor.gameObject, target);
	}

	public static void AddGoDropAt(Labor labor, Vector3 target)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdGoDropAt(labor.gameObject, target);
	}

	public static void AddSmartTransferGoal(Labor labor, GameObject target, bool queued = false)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.Go, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddSmartTransferGoal(labor.gameObject, target, queued);
	}

	public static void AddBreakVoxelGoal(Labor labor, int index)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.ActionMine, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAddBreakVoxelGoal(labor.gameObject, index);
	}

	public static void AddBreakObjectGoal(Labor labor, GameObject go)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.ActionAttack, labor.gameObject);
		if (labor.faction.IsAlliedTo(go))
		{
			User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdBreakObject(labor.gameObject, go);
		}
		else
		{
			User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdAttack(labor.gameObject, go);
		}
	}

	public static void RepairBlock(Labor labor, int index)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.ActionRepair, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdRepair(labor.gameObject, index);
	}

	public static void AddMoveAttackGoal(Labor labor, Location destination)
	{
		labor.AssignToIdle();
		SoundEngine.Play(labor.Sounds.ActionAttack, labor.gameObject);
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdMoveAttack(labor.gameObject, destination.Position);
	}

	public static void ChangeProject(Labor labor, Project project, bool withSound = false)
	{
		if (withSound)
		{
			SoundEngine.Play(project.GetAssignationSound(labor), labor.gameObject);
		}
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdChangeProject(labor.gameObject, project.gameObject);
	}

	public static void PickUp(Labor labor, GameObject objectToPickFrom)
	{
		labor.AssignToIdle();
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdPickup(labor.gameObject, objectToPickFrom);
	}
}
