using System;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class JumpingOff : LocomotionMovementState
{
	public float JumpSmashingFactor = 1f;

	public float JumpHorizontalSpeed = 5f;

	public float JumpMaxDistanceBeforeFlail = 20f;

	public float JumpMaxLandingDistance = 0.5f;

	public float JumpLaunchingMatchFraction = 0.75f;

	public float JumpLaunchingMatchEndFraction = 1f;

	public float JumpLandingMatchFraction = 0.25f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 JumpOrigin;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 JumpDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool JumpFreeFalling;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float JumpFallingStartTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float JumpDurationToHorizontalTarget;

	private Quaternion JumpOrientation => Quaternion.LookRotation((JumpDestination - JumpOrigin).Flatten());

	public void StartJumping()
	{
		Locomotion.AutoSnapToFloor = false;
		JumpFreeFalling = false;
		base.controller.IsLandingOnFeet.Set(b: false);
		JumpOrigin = base.trf.position;
		Vector3 origin = Calcule.RoundVecteur(base.trf.position) + Calcule.GetDiscreteVector(base.trf.forward.Flatten());
		if (!FloorPicker.GetNearestAlignedUnder(origin, Locomotion.FloorOffset, 20, out JumpDestination))
		{
			JumpDestination.y = origin.y - 20f;
		}
		MatchTarget(base.controller.JumpingOffJumping, JumpDestination.Flatten(JumpOrigin), JumpLaunchingMatchFraction, JumpLaunchingMatchEndFraction);
	}

	public void StartFalling()
	{
		JumpFallingStartTime = 0f;
		JumpDurationToHorizontalTarget = (JumpDestination - JumpOrigin).Flatten().magnitude / JumpHorizontalSpeed;
		MatchTargetFalling();
	}

	public void EndFalling()
	{
	}

	public void StartLanding()
	{
		base.controller.IsLandingOnFeet.Set(b: false);
	}

	public void EndLanding()
	{
		if (Locomotion.IsInPlace())
		{
			MakeStompedEnemiesFlail();
			Locomotion.SetPosition(JumpDestination, forced: false);
		}
	}

	public void UpdateFalling()
	{
		bool flag = MatchTargetFalling();
		MakeStompedEnemiesFlail();
		if ((base.trf.position - JumpDestination).Flatten().magnitude <= JumpMaxLandingDistance && FloorPicker.IsGrounded(base.trf.position))
		{
			base.controller.IsLandingOnFeet.Set(b: true);
		}
		else if (JumpOrigin.y - base.trf.position.y >= JumpMaxDistanceBeforeFlail || !flag)
		{
			JumpFreeFalling = true;
			Locomotion.Common.Grounded.GroundedLastTime = JumpFallingStartTime;
			Locomotion.Common.Grounded.UngroundedLastTime = base.Time;
			base.Common.Flailing.Flail();
		}
	}

	private bool MatchTargetFalling()
	{
		if (JumpFallingStartTime <= 0f)
		{
			JumpFallingStartTime = base.Time - JumpDurationToHorizontalTarget / 2f;
		}
		float num = base.Time - JumpFallingStartTime;
		Vector3 projectedFallingPosition = GetProjectedFallingPosition(num);
		Vector3 vector = JumpDestination;
		vector.y = Mathf.Max(vector.y, (float)Mathf.FloorToInt(projectedFallingPosition.y) - 0.1f);
		if (FloorPicker.IsGrounded(vector))
		{
			vector = Locomotion.GetNearestAligned(vector);
		}
		Vector3 a = vector - projectedFallingPosition;
		Vector3 vector2 = a.Flatten();
		float num2 = 0f - a.y;
		float magnitude = vector2.magnitude;
		float v = Physics.gravity.y * num;
		float num3 = Flailing.TimeToDistance(0f - num2, v, Physics.gravity.y);
		if (num3 <= 0f)
		{
			return false;
		}
		if (magnitude > num3 * JumpHorizontalSpeed)
		{
			float t = num3 * JumpHorizontalSpeed / magnitude;
			Vector3 vector3 = Vector3.Lerp(projectedFallingPosition, vector, t);
			vector = new Vector3(vector3.x, vector.y, vector3.z);
		}
		OverrideMatchTarget(num3, vector, JumpOrientation);
		return true;
	}

	private Vector3 GetProjectedFallingPosition(float ungroundedTime)
	{
		return JumpDestination.Flatten(JumpOrigin) + Physics.gravity * (ungroundedTime * ungroundedTime / 2f);
	}

	private Vector3 GetProjectedPosition(float ungroundedTime)
	{
		Vector3 a = JumpDestination - JumpOrigin;
		Vector3 vector = a.Flatten();
		return JumpOrigin + vector.normalized * Mathf.Min(JumpHorizontalSpeed * ungroundedTime, vector.magnitude) + Physics.gravity * (ungroundedTime * ungroundedTime / 2f);
	}

	private void MakeStompedEnemiesFlail()
	{
		Slinq<GameObject, PredicateContext<GameObject, SelectContext<GameObject, IVolume, IListContext<IVolume>>>> slinq = from v in VolumeRegistry.VolumesWithPropertyAt(base.voxel.position - XYZ.up, VProperty.character).Slinq()
			select v.GameObject into o
			where o != base.gameObject
			select o;
		bool flag = false;
		foreach (GameObject item in slinq.NoLeak())
		{
			Labor component = item.GetComponent<Labor>();
			if (component != null && component.affiliation.IsEnemy(Locomotion))
			{
				Stomp(component);
				flag = true;
			}
			else if (component != null)
			{
				base.Common.Flailing.Flail();
			}
		}
		if (!flag && !JumpFreeFalling)
		{
		}
	}

	private void Stomp(Labor enemy)
	{
		Vector3 position = base.trf.position;
		Vector3 normalized = (enemy.transform.position - base.trf.position).normalized;
		float num = Mathf.Max(0f, JumpOrigin.y - base.trf.position.y);
		enemy.navigation.Locomotion.Smash(position, normalized * num * JumpSmashingFactor);
	}
}
