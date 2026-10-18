using System;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Locomotion;

[Serializable]
public class Striking : LocomotionFightingState
{
	[DisplayRules(DisplayFlags.ReadOnly)]
	public MeleeAttack SwordStrike;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int StrikeVariation;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Vector3 StrikeEndPosition;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool IsCranking;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool IsSwinging;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public bool IsReacting;

	private const float SHORT_STRIKE_DISTANCE = 1.4f;

	private const float FAR_STRIKE_DISTANCE = 2f;

	public void Strike(IDamageReceiver strikeTarget, Vector3 endPosition)
	{
		StrikeEndPosition = endPosition;
		SwordStrike = MeleeWeapon.CreateAttack(base.gameObject, strikeTarget);
		SwordStrike.CurrentPhase = MeleeAttack.Phase.Startup;
	}

	private void UpdateAttackReaction()
	{
		base.controller.AttackWillHit.Set(b: true);
		base.controller.AttackIsBlocked.Set(SwordStrike.GetTargetReaction() == DamageReaction.Block);
	}

	public bool IsStrikeTargetValid()
	{
		return SwordStrike != null && !SwordStrike.Target.IsNullOrReleased() && Vector3.Distance(SwordStrike.Target.GameObject.transform.position, base.trf.position) <= 9.5f;
	}

	public void StartCranking()
	{
		IsCranking = true;
		base.Blocking.BlockingDuringStrike = false;
		if (IsStrikeTargetValid())
		{
			base.controller.AttackWillHit.Set(b: false);
			base.controller.AttackIsBlocked.Set(b: false);
			float f = Mathf.InverseLerp(1.4f, 2f, Vector3.Distance(base.trf.position.Flatten(), SwordStrike.Target.GameObject.transform.position.Flatten()));
			base.controller.StrikeDistance.Set(f);
			base.controller.AttackVariation.Set((float)SwordStrike.Variation);
			Vector3 vector = SwordStrike.Target.GameObject.transform.position.Flatten() + new Vector3(0f, StrikeEndPosition.y, 0f);
			Quaternion orientation = Quaternion.LookRotation((vector - StrikeEndPosition).normalized, Vector3.up);
			MatchTarget(base.controller.CombatCrank, StrikeEndPosition, orientation, 0.1f, 0.9f);
		}
	}

	public void EndCranking()
	{
		IsCranking = false;
	}

	public void UpdateCranking()
	{
		if (!IsStrikeTargetValid())
		{
			base.controller.IsAbortingStrike.Set(b: true);
		}
		else if (WantsStop(StrikeEndPosition, SwordStrike.Target.GameObject))
		{
			StrikeEndPosition = Locomotion.PositionOccupied.ToVector3();
			OverrideMatchTarget(Vector3.Distance(StrikeEndPosition, base.trf.position) / Locomotion.Fighting.Stepping.CombatStepSpeed, StrikeEndPosition, base.trf.rotation);
		}
	}

	public void StartSwinging()
	{
		IsSwinging = true;
		if (IsStrikeTargetValid())
		{
			SwordStrike.CurrentPhase = MeleeAttack.Phase.Anticipation;
			UpdateAttackReaction();
			Vector3 vector = SwordStrike.Target.GameObject.transform.position.Flatten() + new Vector3(0f, StrikeEndPosition.y, 0f);
			Quaternion orientation = Quaternion.LookRotation((vector - StrikeEndPosition).normalized, Vector3.up);
			MatchTarget(base.controller.CombatSwing, StrikeEndPosition, orientation, 0.1f, 0.9f);
		}
	}

	public void EndSwinging()
	{
		IsSwinging = false;
	}

	public void UpdateSwinging()
	{
		if (!IsStrikeTargetValid())
		{
			base.controller.IsAbortingStrike.Set(b: true);
			return;
		}
		if (WantsStop(StrikeEndPosition, SwordStrike.Target.GameObject))
		{
			StrikeEndPosition = Locomotion.PositionOccupied.ToVector3();
			OverrideMatchTarget(Vector3.Distance(StrikeEndPosition, base.trf.position) / Locomotion.Fighting.Stepping.CombatStepSpeed, StrikeEndPosition, base.trf.rotation);
		}
		UpdateAttackReaction();
	}

	public void StartReacting()
	{
		IsReacting = true;
		if (SwordStrike != null && !SwordStrike.Target.IsNullOrReleased())
		{
			SwordStrike.CurrentPhase = MeleeAttack.Phase.Resolution;
			SwordStrike.Apply(SwordStrike.Target);
		}
		if (SwordStrike != null)
		{
			SwordStrike.CurrentPhase = MeleeAttack.Phase.Finished;
			SwordStrike.Target = null;
			SwordStrike = null;
		}
	}

	public void EndReacting()
	{
		IsReacting = false;
	}

	public void StartAbortingStrike()
	{
		Locomotion.AttackFailed.Invoke();
		if (SwordStrike != null)
		{
			SwordStrike.CurrentPhase = MeleeAttack.Phase.Finished;
			SwordStrike.Target = null;
			SwordStrike = null;
		}
		if (base.Blocking.BlockingDuringStrike)
		{
			base.Blocking.BlockingDuringStrike = false;
			base.Blocking.EnableShieldBlock(base.Blocking.BlockingDuration);
		}
	}
}
