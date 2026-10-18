using System;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class LookAt : LocomotionCommonState
{
	public bool EnableLookAt;

	public GameObject LookAtTarget;

	public float LookAtHeadFactor = 0.8f;

	public float LookAtBodyFactor = 0.3f;

	public float LookAtActivationSpeed = 2f;

	private float LookAtWeight;

	public void UpdateIK(int ignored)
	{
		if (!LookAtTarget && EnableLookAt)
		{
			EnableLookAt = false;
		}
		if (EnableLookAt)
		{
			base.animator.SetLookAtPosition(LookAtTarget.transform.position);
			LookAtWeight = Mathf.Min(1f, LookAtWeight + LookAtActivationSpeed * UnityEngine.Time.deltaTime);
		}
		else
		{
			LookAtWeight = Mathf.Max(0f, LookAtWeight - LookAtActivationSpeed * UnityEngine.Time.deltaTime);
		}
		base.animator.SetLookAtWeight(LookAtWeight, LookAtBodyFactor, LookAtHeadFactor);
	}
}
