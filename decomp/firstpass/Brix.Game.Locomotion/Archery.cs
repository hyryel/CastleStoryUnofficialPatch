using System;
using System.Collections;
using Brix.Audio;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Archery : LocomotionStateUpdater
{
	public Transform ArcheryStartPoint;

	public Transform ArcheryAimPoint;

	public Transform ArcheryPitchPivot;

	public Transform ArcheryYawPivot;

	public Transform RangeAttacksTargetPoint;

	public ProjectileAiming Aiming;

	public float ObstructionTimeOut = 3f;

	public float AimingYawExtent = 45f;

	public float AimingPitchExtent = 45f;

	public float IdleTimeoutDelay = 10f;

	public float AimDampFactor = 0.5f;

	public float AimSpeedFactor = 3f;

	public float HurtStunDelay = 2f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float Inaccuracy;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ArcheryAimReadyTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ArcheryLastValidTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool Shooting;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool ProjectileFired;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float IdleStartingTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float AdjustLastTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float AdjustYawVelocity;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float AdjustPitchVelocity;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float CooldownEndTime;

	public Vector3 ProjectileStartingPosition => (!(ArcheryStartPoint != null)) ? Aiming.defaultStartPoint : ArcheryStartPoint.position;

	public Vector3 ProjectileAimingPosition => (!(ArcheryAimPoint != null)) ? Aiming.defaultStartPoint : ArcheryAimPoint.position;

	protected Profession Profession => Locomotion.Occupation.Profession;

	public override void Init(Locomotion4 locomotion)
	{
		base.Init(locomotion);
		Aiming = new ProjectileAiming(base.trf);
	}

	public override void Enter()
	{
		Locomotion.IsInArcheryStance = true;
		Locomotion.PivotConserveHack = base.trf.position;
	}

	public override void Exit()
	{
		Locomotion.IsInArcheryStance = false;
		if (Shooting && !ProjectileFired)
		{
			Locomotion.AttackFailed.Invoke();
			Shooting = false;
		}
	}

	public override void Update()
	{
	}

	public void WieldBow(ProjectileWeapon weapon)
	{
		base.controller.IsTakingArcheryStance.Set(b: true);
		base.controller.IsAbortingStrike.Set(b: false);
		base.controller.IsAiming.Set(b: false);
		base.controller.IsShooting.Set(b: false);
	}

	public void LeaveArcheryStance()
	{
		base.controller.IsTakingArcheryStance.Set(b: false);
		base.controller.IsAbortingStrike.Set(b: true);
	}

	public void AbortAimingOnHurt()
	{
		CooldownEndTime = base.Time + HurtStunDelay;
		SoundEngine.Play(base.Sounds.Fail, base.gameObject);
		base.controller.IsAbortingStrike.Set(b: true);
		Locomotion.AttackFailed.Invoke();
	}

	public void StartIdleArchery()
	{
		Locomotion.IsReadyToShoot = base.Time >= CooldownEndTime;
		IdleStartingTime = base.Time + IdleTimeoutDelay;
		base.controller.IsAbortingStrike.Set(b: false);
	}

	public void EndIdleArchery()
	{
		Locomotion.IsReadyToShoot = false;
	}

	public void UpdateIdleArchery()
	{
		if (IdleTimeoutDelay > 0f && base.Time >= IdleStartingTime)
		{
			LeaveArcheryStance();
		}
		if (CooldownEndTime > 0f && base.Time >= CooldownEndTime)
		{
			CooldownEndTime = 0f;
			Locomotion.IsReadyToShoot = true;
		}
	}

	public void UpdateIdleToAiming()
	{
		if (IsArcheryTargetMissing() || !IsArcheryAvailable())
		{
			AimingToIdleArchery();
		}
	}

	private bool IsArcheryTargetOutsideAimExtents()
	{
		return Vector3.Angle(base.trf.forward.Flatten(), (Aiming.Target.Position - base.trf.position).Flatten()) > AimingYawExtent;
	}

	public bool IsArcheryTargetMissing()
	{
		return Aiming.Target == null || (Aiming.Target is ObjectLocation && Aiming.Target.Object == null);
	}

	private bool IsArcheryTargetInvalid()
	{
		if (Profession.CanFireAt(Aiming.Target))
		{
			ArcheryLastValidTime = base.Time;
			return false;
		}
		return true;
	}

	private bool IsArcheryAvailable()
	{
		return Profession.ProjectileAttackAvailable();
	}

	public void StartAiming()
	{
		SoundEngine.Play(base.Sounds.LoadArrow, base.gameObject);
		AdjustLastTime = base.Time;
	}

	public void UpdateAiming()
	{
		if (base.Archery.IsAimingReadyForIdleArchery())
		{
			AimingToIdleArchery();
		}
		else if (base.Archery.IsAimingReadyForPivotingAim())
		{
			AimingToPivotingAim();
		}
		else if (base.Archery.IsAimingReadyForShooting())
		{
			AimingToShooting();
		}
		else
		{
			AdjustAim();
		}
	}

	public void EndAimingOnHurtSmall(Locomotion4.Impact impact)
	{
		SoundEngine.Play(base.Sounds.Fail, base.gameObject);
	}

	public void EndAimingOnHurtBig(Locomotion4.Impact impact)
	{
		SoundEngine.Play(base.Sounds.Fail, base.gameObject);
	}

	public void ShootAt(Location target, float aimtime, float inaccuracy)
	{
		SoundEngine.Play(base.Sounds.SpotEnemy, base.gameObject);
		ArcheryAimReadyTime = base.Time + aimtime;
		ArcheryLastValidTime = base.Time;
		Inaccuracy = inaccuracy;
		base.controller.IsAiming.Set(b: true);
		ProjectileFired = false;
	}

	private bool IsAimingReadyForPivotingAim()
	{
		return IsArcheryTargetOutsideAimExtents() && Aiming.IsArcheryTargetFarEnough();
	}

	private void AimingToPivotingAim()
	{
		TurnToArcheryTargetPosition();
	}

	private bool IsAimingReadyForShooting()
	{
		return base.Time >= ArcheryAimReadyTime && !IsArcheryTargetInvalid();
	}

	private void AimingToShooting()
	{
		base.controller.IsShooting.Set(b: true);
	}

	private bool IsAimingReadyForIdleArchery()
	{
		return base.Time >= ArcheryLastValidTime + ObstructionTimeOut || IsArcheryTargetMissing() || !Aiming.IsArcheryTargetFarEnough() || !IsArcheryAvailable();
	}

	private void AimingToIdleArchery()
	{
		SoundEngine.Play(base.Sounds.Impossible, base.gameObject);
		base.controller.IsAbortingStrike.Set(b: true);
		Locomotion.AttackFailed.Invoke();
	}

	public void StartPivotingAim()
	{
		AdjustLastTime = base.Time;
	}

	public void UpdatePivotingAim()
	{
		if (IsPivotingAimReadyForIdleArchery())
		{
			PivotingAimToIdleArchery();
		}
		else
		{
			AdjustAim();
		}
	}

	private bool IsPivotingAimReadyForIdleArchery()
	{
		return IsArcheryTargetMissing() || !IsArcheryAvailable();
	}

	private void PivotingAimToIdleArchery()
	{
		base.controller.IsAbortingStrike.Set(b: true);
		Locomotion.AttackFailed.Invoke();
	}

	private void TurnToArcheryTargetPosition()
	{
		if (!IsTowards(Aiming.Target) && Aiming.IsArcheryTargetFarEnough() && !base.animator.IsInTransition(0))
		{
			base.controller.TurnDirection.Set(Aiming.Target.GetDirectionToPosition(base.trf.position, base.trf.rotation));
			base.controller.IsTurningOnSpotArchery.Set(b: true);
			Vector3 normalized = (Aiming.Target.Position - base.trf.position).Flatten().normalized;
			Quaternion orientation = Quaternion.LookRotation(normalized, Vector3.up);
			MatchTarget(base.controller.ArcheryTurning, base.trf.position, orientation, 0.1f, 0.9f);
			base.controller.StartCoroutine(ShootIfMatchingTarget());
		}
	}

	private IEnumerator ShootIfMatchingTarget()
	{
		while (!base.TargetMatcher.Finished)
		{
			yield return GameClock.WaitForEndOfFrame;
		}
		if (!base.TargetMatcher.MatchFailed)
		{
			base.controller.IsAiming.Set(b: true);
		}
	}

	public void StartShooting()
	{
		Shooting = true;
		ProjectileFired = false;
		AdjustLastTime = base.Time;
	}

	public void UpdateShooting()
	{
		AdjustAim();
	}

	public void StartCooldown()
	{
		ProjectileFire();
	}

	private void ProjectileFire()
	{
		if (!IsArcheryTargetInvalid())
		{
			Vector3 anticipatedTargetPosition = Aiming.GetAnticipatedTargetPosition();
			Aiming.Weapon.Fire(ProjectileStartingPosition, ProjectileAimingPosition, anticipatedTargetPosition, base.gameObject, Aiming.Target, (!(base.Common.Grabbing.GrabbedTarget == null)) ? base.Common.Grabbing.GrabbedTarget.gameObject : null, Aiming.PowerMultiplier, Inaccuracy, Profession.RangeDamage);
			Locomotion.HasFiredArrow.Invoke();
			base.Common.Grabbing.GrabbedTarget = null;
			base.controller.CarryMode.Set(0f);
			ProjectileFired = true;
			Shooting = false;
			CooldownEndTime = base.Time + Locomotion.Occupation.Profession.JobRangedCooldown;
		}
	}

	private void AdjustAim()
	{
		if (Aiming.Target == null || ProjectileFired || Aiming.Target.Object.IsNullOrReleased())
		{
			return;
		}
		Vector3 anticipatedTargetPosition = Aiming.GetAnticipatedTargetPosition();
		if (Aiming.IsArcheryTargetFarEnough())
		{
			float num;
			if (ArcheryYawPivot != null)
			{
				ArcheryYawPivot.localRotation = Quaternion.identity;
				num = ComputeArcheryYaw(anticipatedTargetPosition - ArcheryYawPivot.position);
			}
			else
			{
				num = ComputeArcheryYaw(anticipatedTargetPosition - ProjectileStartingPosition);
			}
			float num2;
			if (ArcheryPitchPivot != null)
			{
				ArcheryPitchPivot.localRotation = Quaternion.identity;
				num2 = ComputeArcheryPitch(anticipatedTargetPosition - ArcheryPitchPivot.position);
			}
			else
			{
				num2 = ComputeArcheryPitch(anticipatedTargetPosition - ProjectileStartingPosition);
			}
			if (ArcheryYawPivot != null)
			{
				ArcheryYawPivot.localRotation *= Quaternion.Euler(0f, num, 0f);
			}
			if (ArcheryPitchPivot != null)
			{
				ArcheryPitchPivot.localRotation *= Quaternion.Euler(0f - num2, 0f, 0f);
			}
			float deltaTime = base.Time - AdjustLastTime;
			AdjustArcheryYaw(num / AimingYawExtent, deltaTime);
			AdjustArcheryPitch(num2 / AimingPitchExtent, deltaTime);
			AdjustLastTime = base.Time;
		}
	}

	public static float SafeClamp(float x, float min, float max)
	{
		if (x >= min)
		{
			return (!(x <= max)) ? max : x;
		}
		if (x <= min)
		{
			return min;
		}
		return (min + max) / 2f;
	}

	private float ComputeArcheryYaw(Vector3 relativeTargetPos)
	{
		Vector3 vector = relativeTargetPos.Flatten();
		float x = Vector3.Angle(base.trf.forward, vector) * Mathf.Sign(Vector3.Dot(base.trf.right, vector));
		return SafeClamp(x, 0f - AimingYawExtent, AimingYawExtent);
	}

	private float ComputeArcheryPitch(Vector3 relativeTargetPos)
	{
		float x = ProjectilePhysics.ComputeElevationAngle(Aiming.Power, relativeTargetPos) * 57.29578f;
		return SafeClamp(x, 0f - AimingPitchExtent, AimingPitchExtent);
	}

	private void AdjustArcheryYaw(float value, float deltaTime)
	{
		value = Mathf.SmoothDamp(base.controller.ArcheryYaw, value, ref AdjustYawVelocity, AimDampFactor, AimSpeedFactor, deltaTime);
		base.controller.ArcheryYaw.Set(value);
	}

	private void AdjustArcheryPitch(float value, float deltaTime)
	{
		value = Mathf.SmoothDamp(base.controller.ArcheryPitch, value, ref AdjustPitchVelocity, AimDampFactor, AimSpeedFactor, deltaTime);
		base.controller.ArcheryPitch.Set(value);
	}
}
