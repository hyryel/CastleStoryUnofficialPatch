using System;
using Brix.Audio;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Audio;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Flailing : LocomotionCommonState
{
	private static BrixLogger logger = new BrixLogger(typeof(Flailing).FullName);

	public float FlailingDefaultTorque = 3f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float StunTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float FlailingStartTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Locomotion4.Impact FlailingSmashImpact;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 FlailingThrowVelocity;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 FlailingThrowTorque;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 LastObservedPosition;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float LastObservedTime;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float ObservedVelocity;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float BodySurfingStartTime;

	private const float StandUpVelocityThreshold = 0.5f;

	private const float BodySurfingMaxDuration = 1f;

	public Transform ParentAtCreation => Locomotion.ParentAtCreation;

	public GameObject FlailingCollider => Locomotion.FlailingCollider;

	public GameObject RegularCollider => Locomotion.RegularCollider;

	public Rigidbody FlailingColliderRigidbody
	{
		get
		{
			return Locomotion.FlailingColliderRigidbody;
		}
		set
		{
			Locomotion.FlailingColliderRigidbody = value;
		}
	}

	public void GetSmashed(Locomotion4.Impact i)
	{
		FlailingSmashImpact = i;
		Flail();
	}

	public void Flail()
	{
		base.controller.IsFlailing.Set(b: true);
		ResetStun();
		if (FlailingSmashImpact.Force != Vector3.zero)
		{
			StartStun(FlailingSmashImpact.Force);
		}
		else if (FlailingThrowVelocity != Vector3.zero)
		{
			StartStun(FlailingThrowVelocity);
		}
	}

	public void Thrown(Vector3 v)
	{
		FlailingThrowVelocity = v;
		FlailingThrowTorque = RandomTorque();
		Flail();
	}

	public override void Enter()
	{
		RegularCollider.SetActive(value: false);
		FlailingCollider.SetActive(value: true);
		SwitchColliderAndCharacter();
		FlailingColliderRigidbody = FlailingCollider.GetComponent<Rigidbody>();
		FlailingColliderRigidbody.isKinematic = false;
		FlailingColliderRigidbody.useGravity = true;
		FlailingStartTime = base.Time;
		BodySurfingStartTime = 0f;
		Locomotion.AutoSnapToFloor = false;
		if (Locomotion.Common.Grounded.UngroundedTime > 0f)
		{
			FlailingColliderRigidbody.velocity = Physics.gravity * Locomotion.Common.Grounded.UngroundedTime;
			logger.Info(FlailingColliderRigidbody.velocity);
		}
		if (FlailingSmashImpact.Force != Vector3.zero)
		{
			SoundEngine.Play(Locomotion.Sounds.Flying, base.gameObject);
			ReceiveImpulseFromSmash();
		}
		else if (FlailingThrowVelocity != Vector3.zero)
		{
			SoundEngine.Play(Locomotion.Sounds.Flying, base.gameObject);
			ReceiveImpulseFromThrow();
		}
		else
		{
			SoundEngine.Play(Locomotion.Sounds.Falls, base.gameObject);
			ReceiveDefaultImpulse();
		}
	}

	public override void Exit()
	{
		SanitizeFlailingColliderHierarchy();
		SwitchColliderAndCharacter();
		FlailingCollider.SetActive(value: false);
		FlailingColliderRigidbody.isKinematic = true;
		RegularCollider.SetActive(value: true);
		base.controller.IsStunned.Set(b: false);
		FlailingSmashImpact = default(Locomotion4.Impact);
		FlailingThrowVelocity = default(Vector3);
	}

	public override void Update()
	{
		SanitizeFlailingColliderHierarchy();
		float value = Vector3.Angle(Vector3.up, base.trf.up);
		base.controller.FlailingVertical.Set(Map(value, 0f, 180f, 1f, -1f));
		float value2 = Vector3.Angle(Vector3.up, base.trf.forward);
		base.controller.FlailingHorizontal.Set(Map(value2, 0f, 180f, 1f, -1f));
		if (StunTime <= base.Time + 0.5f)
		{
			base.controller.IsStunned.Set(b: false);
		}
		base.Common.Grounded.UngroundedLastTime = base.Time;
		ObserveVelocity();
		TrackCharacter();
		if (IsReadyToStabilize())
		{
			StopFalling();
		}
		else
		{
			UpdateFreeFalling();
		}
	}

	private void SanitizeFlailingColliderHierarchy()
	{
		if (FlailingCollider.transform.parent != base.trf && base.trf.transform.parent != FlailingCollider.transform)
		{
			FlailingCollider.GetComponent<FlailingColliderMonitor>().ReparentToCharacterParent();
		}
	}

	private void SwitchColliderAndCharacter()
	{
		if (FlailingCollider.transform.parent == base.trf)
		{
			FlailingCollider.transform.SetParent(base.trf.parent);
			base.trf.SetParent(FlailingCollider.transform);
		}
		else if (base.trf.transform.parent == FlailingCollider.transform)
		{
			base.trf.SetParent(FlailingCollider.transform.parent);
			FlailingCollider.transform.SetParent(base.trf);
		}
		else
		{
			logger.Warn("Character transform parenting is messed up, defaulting");
			base.trf.SetParent(ParentAtCreation);
			FlailingCollider.transform.SetParent(base.trf);
		}
	}

	private void TrackCharacter()
	{
		Vector3 position = base.trf.position;
		FlailingCollider.transform.position = position;
		base.trf.position = position;
	}

	private bool IsReadyToStabilize()
	{
		if (StunTime > base.Time)
		{
			return false;
		}
		if (Locomotion.IsInsideGround())
		{
			return true;
		}
		if (IsBodySurfing())
		{
			return true;
		}
		if (FloorPicker.IsGrounded(base.trf.position))
		{
			return true;
		}
		return false;
	}

	private bool IsBodySurfing()
	{
		if (ObservedVelocity >= 0.5f)
		{
			BodySurfingStartTime = 0f;
			return false;
		}
		if (BodySurfingStartTime <= 0f)
		{
			BodySurfingStartTime = base.Time;
		}
		return base.Time >= BodySurfingStartTime + 1f;
	}

	private bool IsAboveCharacter()
	{
		return VolumeRegistry.HasVolumesWithPropertyAtExcluding(Locomotion.PositionOccupied, VProperty.character, Locomotion.volume) || VolumeRegistry.HasVolumesWithPropertyAtExcluding(Locomotion.PositionOccupied - XYZ.up, VProperty.character, Locomotion.volume);
	}

	private void UpdateFreeFalling()
	{
		float num = base.Time - FlailingStartTime;
		Vector3 vector = base.trf.position.Flatten((float)Mathf.FloorToInt(base.trf.position.y) - 0.1f);
		float magnitude = (vector - base.trf.position).magnitude;
		float v = Physics.gravity.y * num;
		float num2 = TimeToDistance(0f - magnitude, v, Physics.gravity.y);
		if (num2 > 0f)
		{
			OverrideMatchTarget(num2, vector, base.trf.rotation);
		}
	}

	private void StopFalling()
	{
		base.controller.IsFlailing.Set(b: false);
		FlailingImpactAudio component = FlailingCollider.GetComponent<FlailingImpactAudio>();
		if (component != null)
		{
			component.PlayImpactSoundEvent(Locomotion, ObservedVelocity);
		}
	}

	public static float TimeToDistance(float d, float v0)
	{
		return (!(v0 > 0f)) ? 0f : (d / v0);
	}

	public static float TimeToDistance(float d, float v0, float a)
	{
		float num = v0 * v0 + d * a * 2f;
		if (num <= 0f)
		{
			return 0f;
		}
		return (0f - v0 - Mathf.Sqrt(num)) / a;
	}

	private void ObserveVelocity()
	{
		ObservedVelocity = ((base.trf.position - LastObservedPosition) / (base.Time - LastObservedTime)).magnitude;
		LastObservedPosition = base.trf.position;
		LastObservedTime = base.Time;
	}

	private void ReceiveImpulseFromSmash()
	{
		FlailingCollider.GetComponent<Rigidbody>().AddForceAtPosition(FlailingSmashImpact.Force, FlailingSmashImpact.Origin, ForceMode.Impulse);
		if (FlailingSmashImpact.UpwardsEffect > 0f)
		{
			FlailingCollider.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * FlailingSmashImpact.Force.magnitude * FlailingSmashImpact.UpwardsEffect, ForceMode.Impulse);
		}
	}

	private void ReceiveImpulseFromThrow()
	{
		FlailingCollider.GetComponent<Rigidbody>().AddForce(FlailingThrowVelocity, ForceMode.Impulse);
		FlailingCollider.GetComponent<Rigidbody>().AddTorque(FlailingThrowTorque, ForceMode.Impulse);
	}

	private void ReceiveDefaultImpulse()
	{
		if (FlailingDefaultTorque > 0f)
		{
			FlailingCollider.GetComponent<Rigidbody>().AddTorque(RandomTorque(FlailingDefaultTorque), ForceMode.Impulse);
		}
	}

	private void ResetStun()
	{
		StunTime = 0f;
		base.controller.IsStunned.Set(b: false);
	}

	private void StartStun(Vector3 force)
	{
		StunTime = base.Time + 1f;
		base.controller.IsStunned.Set(b: true);
		float num = Map(Vector3.Angle(base.trf.forward, force), 0f, 180f, 0f, 1f);
		if (Vector3.Angle(base.trf.right, force) < 90f)
		{
			num = 0f - num;
		}
		base.controller.StunDirection.Set(num);
	}

	public Vector3 RandomTorque(float amount = 10f)
	{
		return UnityEngine.Random.insideUnitSphere * amount;
	}

	public float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
	{
		return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
	}
}
