using System;
using Brix.Engine;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Blocking : LocomotionFightingState
{
	public float SoftBlockingSpeed = 10f;

	public float SoftBlockingLimit = 0.9f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool BlockingDuringStrike;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float BlockingEndTime;

	public bool IsSoftBlocking => base.controller.SoftShieldBlock.Get();

	public float BlockingDuration
	{
		get
		{
			return BlockingEndTime - base.Time;
		}
		set
		{
			BlockingEndTime = base.Time + value;
		}
	}

	public void EnableShieldBlock(float maxDuration)
	{
		BlockingDuration = maxDuration;
		base.controller.IsShieldBlocking.Set(b: true);
	}

	public void DisableShieldBlock(float maxDelay)
	{
		BlockingDuration = Mathf.Min(BlockingDuration, maxDelay);
	}

	public void EnableShieldBlockDuringStrike(float maxDuration)
	{
		BlockingDuration = maxDuration;
		base.controller.AttackVariation.Set(1f);
	}

	public void FinishStrikeThenEnableShieldBlock(float maxDuration)
	{
		BlockingDuringStrike = true;
		BlockingDuration = maxDuration;
	}

	public void AbortStrikeThenEnableShieldBlock(float maxDuration)
	{
		BlockingDuringStrike = true;
		BlockingDuration = maxDuration;
		base.controller.IsAbortingStrike.Set(b: true);
	}

	public void EnableSoftShieldBlock(float maxDuration)
	{
		EnableSoftShieldBlock();
		BlockingDuration = maxDuration;
		Neo.Relais(delegate
		{
			if (BlockingDuration < 0.1f)
			{
				DisableSoftShieldBlock();
			}
		}, maxDuration, "shield block");
	}

	public void EnableSoftShieldBlock()
	{
		base.layerActivator.SetLimit(2, SoftBlockingLimit);
		base.layerActivator.SetSpeed(2, SoftBlockingSpeed);
		base.layerActivator.Activate(2);
		base.controller.SoftShieldBlock.Set(b: true);
	}

	public void DisableSoftShieldBlock()
	{
		base.layerActivator.ResetLimit(2);
		base.layerActivator.ResetSpeed(2);
		base.layerActivator.Deactivate(2);
		base.controller.SoftShieldBlock.Set(b: false);
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
		base.controller.IsShieldBlocking.Set(b: false);
	}

	public override void Update()
	{
		if (BlockingDuration <= 0f)
		{
			base.controller.IsShieldBlocking.Set(b: false);
		}
	}
}
