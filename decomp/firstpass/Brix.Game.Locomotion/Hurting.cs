using System;
using Brix.Game.AI;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Hurting : LocomotionCommonState
{
	public const float MaxHurtingTime = 2f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 HurtDestination;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public float HurtingStartTime;

	public void StartHardHurt(Locomotion4.Impact i)
	{
		HurtDestination = Locomotion.GetNearestAligned(base.trf.position);
		base.controller.IsHardHurt.Set(b: true);
		MatchTarget(base.controller.Hurting, HurtDestination, 0f, 0.85f);
	}

	public void SoakSmallHurt(Locomotion4.Impact i)
	{
		HurtingStartTime = base.Time;
		base.layerActivator.SetSpeed(1, 100f);
		base.layerActivator.Activate(1);
		Vector3 vector = Vector3.Project(i.Force.normalized, base.trf.forward);
		base.controller.HurtForwardFactor.Set(vector.magnitude);
		if (Vector3.Angle(vector, base.trf.forward) > 90f)
		{
			base.controller.HurtForwardFactor.Set(0f - base.controller.HurtForwardFactor.Get());
		}
		Vector3 vector2 = Vector3.Project(i.Force.normalized, base.trf.right);
		base.controller.HurtLatteralFactor.Set(vector2.magnitude);
		if (Vector3.Angle(vector2, base.trf.right) > 90f)
		{
			base.controller.HurtLatteralFactor.Set(0f - base.controller.HurtLatteralFactor.Get());
		}
		base.controller.IsSoftHurt.Set(b: true);
	}

	public void EndSmallHurt()
	{
		base.layerActivator.Deactivate(1);
	}

	public bool IsReadyToEnd()
	{
		return base.controller.SoftHurt.Finished || base.Time > HurtingStartTime + 2f;
	}
}
