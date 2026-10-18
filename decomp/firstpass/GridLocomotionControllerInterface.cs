using UnityEngine;

public class GridLocomotionControllerInterface : MonoBehaviour
{
	public FloatParameter Archetype;

	public BoolParameter StepWithLeftFoot;

	public FloatParameter TurnDirection;

	public FloatParameter CombatStepDirection;

	public FloatParameter CloseStepStartDirection;

	public FloatParameter ArrivalDirection;

	public FloatParameter WalkVariation;

	public FloatParameter RunVariation;

	public FloatParameter FacingUp;

	public FloatParameter HurtLatteralFactor;

	public FloatParameter HurtForwardFactor;

	public BoolParameter AttackWillHit;

	public BoolParameter AttackIsBlocked;

	public FloatParameter AttackVariation;

	public FloatParameter ArcheryPitch;

	public FloatParameter ArcheryYaw;

	public FloatParameter StrikeDistance;

	public TriggerParameter DoRunningStrike;

	public FloatParameter FlailingVertical;

	public FloatParameter FlailingHorizontal;

	public FloatParameter StunDirection;

	public BoolParameter IsStunned;

	public FloatParameter CarryMode;

	public BoolParameter IsSitting;

	public IntParameter WorkState;

	public FloatParameter WorkHeight;

	public BoolParameter IsInPlace;

	public BoolParameter IsTurningOnSpot;

	public BoolParameter IsClosingStepStart;

	public BoolParameter IsClosingStepEnd;

	public BoolParameter IsClosingStepEndLeftFoot;

	public BoolParameter IsClosingStepEndRightFoot;

	public BoolParameter IsRunCycling;

	public BoolParameter IsWalkCycling;

	public BoolParameter IsHalting;

	public BoolParameter IsClimbingUp;

	public BoolParameter IsSteppingDown;

	public BoolParameter IsFlailing;

	public BoolParameter IsStabilizing;

	public BoolParameter IsSoftHurt;

	public BoolParameter IsHardHurt;

	public BoolParameter IsTaking;

	public BoolParameter IsUnburrowing;

	public BoolParameter IsGrabbingLadderFromBottom;

	public BoolParameter IsGrabbingLadderFromTop;

	public BoolParameter IsTakingCombatStance;

	public BoolParameter IsAttacking;

	public BoolParameter IsTakingArcheryStance;

	public BoolParameter IsAiming;

	public BoolParameter IsShooting;

	public BoolParameter IsAbortingStrike;

	public BoolParameter IsRunStriking;

	public BoolParameter IsJumpingOff;

	public BoolParameter IsGettingTrapped;

	public BoolParameter IsLeavingTrap;

	public BoolParameter IsLandingOnFeet;

	public BoolParameter IsShieldBlocking;

	public BoolParameter IsTakingOneHand;

	public BoolParameter IsTurningOnSpotArchery;

	public BoolParameter IsTurningOnSpotCombat;

	public BoolParameter IsCombatStepping;

	public BoolParameter IsBeingGrabbed;

	public BoolParameter IsPlantingInGround;

	public BoolParameter SoftShieldBlock;

	public BoolParameter HittingBrick;

	public AnimatorStateProgress InPlace;

	public AnimatorStateProgress TurnOnSpot;

	public AnimatorStateProgress CloseStepStart;

	public AnimatorStateProgress CloseStepEnd;

	public AnimatorStateProgress CloseStepEndLeftFoot;

	public AnimatorStateProgress CloseStepEndRightFoot;

	public AnimatorStateProgress WalkCycle;

	public AnimatorStateProgress RunCycle;

	public AnimatorStateProgress Halting;

	public AnimatorStateProgress ClimbUp;

	public AnimatorStateProgress StepDown;

	public AnimatorStateProgress Flailing;

	public AnimatorStateProgress Stabilizing;

	public AnimatorStateProgress StandingUp;

	public AnimatorStateProgress SoftHurt;

	public AnimatorStateProgress Hurting;

	public AnimatorStateProgress Trap;

	public AnimatorStateProgress Take;

	public AnimatorStateProgress Stunned;

	public AnimatorStateProgress Sitting;

	public AnimatorStateProgress Unburrow;

	public AnimatorStateProgress WorkingEntering;

	public AnimatorStateProgress WorkingLeaving;

	public AnimatorStateProgress GettingTrapped;

	public AnimatorStateProgress Trapped;

	public AnimatorStateProgress LeavingTrap;

	public AnimatorStateProgress JumpingOff;

	public AnimatorStateProgress JumpingOffJumping;

	public AnimatorStateProgress JumpingOffFalling;

	public AnimatorStateProgress JumpingOffLanding;

	public AnimatorStateProgress Ladder;

	public AnimatorStateProgress LadderGoingDownStart;

	public AnimatorStateProgress LadderGoingDownCycle;

	public AnimatorStateProgress LadderGoingDownEnd;

	public AnimatorStateProgress LadderGoingUpStart;

	public AnimatorStateProgress LadderGoingUpCycle;

	public AnimatorStateProgress LadderGoingUpEnd;

	public AnimatorStateProgress Combat;

	public AnimatorStateProgress CombatCrank;

	public AnimatorStateProgress CombatIdle;

	public AnimatorStateProgress CombatSwing;

	public AnimatorStateProgress CombatMissed;

	public AnimatorStateProgress CombatHit;

	public AnimatorStateProgress CombatBlocked;

	public AnimatorStateProgress CombatTurn;

	public AnimatorStateProgress CombatStep;

	public AnimatorStateProgress ShieldBlocking;

	public AnimatorStateProgress RunningStrike;

	public AnimatorStateProgress CombatAbortStrike;

	public AnimatorStateProgress Archery;

	public AnimatorStateProgress ArcheryIdle;

	public AnimatorStateProgress ArcheryTurning;

	public AnimatorStateProgress ArcheryShooting;

	public AnimatorStateProgress ArcheryAiming;

	public AnimatorStateProgress ArcheryCooldown;

	public AnimatorStateProgress ArcheryInPlaceToIdle;

	public AnimatorStateProgress ArcheryIdleToAiming;

	public AnimatorStateProgress TakingOneHand;

	public AnimatorStateProgress TakingTwoHands;

	public AnimatorStateProgress TakingOnBack;

	public AnimatorStateProgress TakingInBag;

	public AnimatorStateProgress UsePick;

	public AnimatorStateProgress HitBrick;

	public AnimatorStateProgress CutWood;

	public void Awake()
	{
		Animator component = GetComponent<Animator>();
		Archetype = new FloatParameter(component, "Archetype", sendOnNetwork: true);
		TurnDirection = new FloatParameter(component, "TurnDirection", sendOnNetwork: true);
		CloseStepStartDirection = new FloatParameter(component, "CloseStepStartDirection");
		CombatStepDirection = new FloatParameter(component, "CombatStepDirection");
		ArrivalDirection = new FloatParameter(component, "ArrivalDirection");
		FacingUp = new FloatParameter(component, "FacingUp");
		HurtLatteralFactor = new FloatParameter(component, "HurtLatteralFactor", sendOnNetwork: true);
		HurtForwardFactor = new FloatParameter(component, "HurtForwardFactor", sendOnNetwork: true);
		WalkVariation = new FloatParameter(component, "WalkVariation");
		RunVariation = new FloatParameter(component, "RunVariation");
		DoRunningStrike = new TriggerParameter(component, "DoRunningStrike");
		AttackVariation = new FloatParameter(component, "AttackVariation");
		ArcheryPitch = new FloatParameter(component, "ArcheryPitch", sendOnNetwork: true);
		ArcheryYaw = new FloatParameter(component, "ArcheryYaw", sendOnNetwork: true);
		StrikeDistance = new FloatParameter(component, "StrikeDistance");
		FlailingVertical = new FloatParameter(component, "FlailingVertical", sendOnNetwork: true);
		FlailingHorizontal = new FloatParameter(component, "FlailingHorizontal", sendOnNetwork: true);
		StunDirection = new FloatParameter(component, "StunDirection");
		CarryMode = new FloatParameter(component, "CarryMode", sendOnNetwork: true);
		WorkState = new IntParameter(component, "WorkState", sendOnNetwork: true);
		WorkHeight = new FloatParameter(component, "WorkHeight", sendOnNetwork: true);
		AnimatorTracker component2 = GetComponent<AnimatorTracker>();
		int num = 0;
		IsInPlace = new BoolParameter(num++, component, "IsInPlace", sendOnNetwork: true);
		IsTurningOnSpot = new BoolParameter(num++, component, "IsTurningOnSpot", sendOnNetwork: true);
		IsClosingStepStart = new BoolParameter(num++, component, "IsClosingStepStart", sendOnNetwork: true);
		IsClosingStepEnd = new BoolParameter(num++, component, "IsClosingStepEnd", sendOnNetwork: true);
		IsClosingStepEndLeftFoot = new BoolParameter(num++, component, "IsClosingStepEndLeftFoot", sendOnNetwork: true);
		IsClosingStepEndRightFoot = new BoolParameter(num++, component, "IsClosingStepEndRightFoot", sendOnNetwork: true);
		IsRunCycling = new BoolParameter(num++, component, "IsRunCycling", sendOnNetwork: true);
		IsWalkCycling = new BoolParameter(num++, component, "IsWalkCycling", sendOnNetwork: true);
		IsHalting = new BoolParameter(num++, component, "IsHalting", sendOnNetwork: true);
		IsClimbingUp = new BoolParameter(num++, component, "IsClimbingUp", sendOnNetwork: true);
		IsSteppingDown = new BoolParameter(num++, component, "IsSteppingDown", sendOnNetwork: true);
		IsFlailing = new BoolParameter(num++, component, "IsFlailing", sendOnNetwork: true);
		IsStabilizing = new BoolParameter(num++, component, "IsStabilizing", sendOnNetwork: true);
		IsSitting = new BoolParameter(num++, component, "IsSitting", sendOnNetwork: true);
		IsSoftHurt = new BoolParameter(num++, component, "IsSoftHurt", sendOnNetwork: true);
		IsHardHurt = new BoolParameter(num++, component, "IsHardHurt", sendOnNetwork: true);
		IsTaking = new BoolParameter(num++, component, "IsTaking", sendOnNetwork: true);
		IsUnburrowing = new BoolParameter(num++, component, "IsUnburrowing", sendOnNetwork: true);
		IsShieldBlocking = new BoolParameter(num++, component, "IsShieldBlocking", sendOnNetwork: true);
		IsGrabbingLadderFromBottom = new BoolParameter(num++, component, "IsGrabbingLadderFromBottom", sendOnNetwork: true);
		IsGrabbingLadderFromTop = new BoolParameter(num++, component, "IsGrabbingLadderFromTop", sendOnNetwork: true);
		IsTakingCombatStance = new BoolParameter(num++, component, "IsTakingCombatStance", sendOnNetwork: true);
		IsAttacking = new BoolParameter(num++, component, "IsAttacking", sendOnNetwork: true);
		IsTakingArcheryStance = new BoolParameter(num++, component, "IsTakingArcheryStance", sendOnNetwork: true);
		IsAiming = new BoolParameter(num++, component, "IsAiming", sendOnNetwork: true);
		IsShooting = new BoolParameter(num++, component, "IsShooting", sendOnNetwork: true);
		IsAbortingStrike = new BoolParameter(num++, component, "IsAbortingStrike", sendOnNetwork: true);
		IsRunStriking = new BoolParameter(num++, component, "IsRunStriking", sendOnNetwork: true);
		IsJumpingOff = new BoolParameter(num++, component, "IsJumpingOff", sendOnNetwork: true);
		IsGettingTrapped = new BoolParameter(num++, component, "IsGettingTrapped", sendOnNetwork: true);
		IsLeavingTrap = new BoolParameter(num++, component, "IsLeavingTrap", sendOnNetwork: true);
		IsLandingOnFeet = new BoolParameter(num++, component, "IsLandingOnFeet", sendOnNetwork: true);
		IsTakingOneHand = new BoolParameter(num++, component, "IsTakingOneHand", sendOnNetwork: true);
		IsTurningOnSpotArchery = new BoolParameter(num++, component, "IsTurningOnSpotArchery", sendOnNetwork: true);
		IsTurningOnSpotCombat = new BoolParameter(num++, component, "IsTurningOnSpotCombat", sendOnNetwork: true);
		IsCombatStepping = new BoolParameter(num++, component, "IsCombatStepping", sendOnNetwork: true);
		IsBeingGrabbed = new BoolParameter(num++, component, "IsBeingGrabbed", sendOnNetwork: true);
		IsTurningOnSpotCombat = new BoolParameter(num++, component, "IsTurningOnSpotCombat", sendOnNetwork: true);
		IsTurningOnSpotArchery = new BoolParameter(num++, component, "IsTurningOnSpotArchery", sendOnNetwork: true);
		StepWithLeftFoot = new BoolParameter(num++, component, "StepWithLeftFoot");
		AttackWillHit = new BoolParameter(num++, component, "AttackWillHit");
		AttackIsBlocked = new BoolParameter(num++, component, "AttackIsBlocked");
		IsStunned = new BoolParameter(num++, component, "IsStunned", sendOnNetwork: true);
		SoftShieldBlock = new BoolParameter(num++, component, "SoftShieldBlock", sendOnNetwork: true);
		HittingBrick = new BoolParameter(num++, component, "HittingBrick", sendOnNetwork: true);
		InPlace = new AnimatorStateProgress(component, 0, "Locomotion.InPlace", component2, IsInPlace);
		TurnOnSpot = new AnimatorStateProgress(component, 0, "Locomotion.TurnOnSpot", component2, IsTurningOnSpot);
		CloseStepStart = new AnimatorStateProgress(component, 0, "Locomotion.CloseStepStart", component2, IsClosingStepStart);
		CloseStepEnd = new AnimatorStateProgress(component, 0, "Locomotion.CloseStepEnd", component2, IsClosingStepEnd);
		CloseStepEndLeftFoot = new AnimatorStateProgress(component, 0, "Locomotion.CloseStepEnd.LeftFoot", component2, IsClosingStepEndLeftFoot);
		CloseStepEndRightFoot = new AnimatorStateProgress(component, 0, "Locomotion.CloseStepEnd.RightFoot", component2, IsClosingStepEndRightFoot);
		RunCycle = new AnimatorStateProgress(component, 0, "Locomotion.RunCycle", component2, IsRunCycling);
		WalkCycle = new AnimatorStateProgress(component, 0, "Locomotion.WalkCycle", component2, IsWalkCycling);
		Halting = new AnimatorStateProgress(component, 0, "Locomotion.Halting", component2, IsHalting);
		ClimbUp = new AnimatorStateProgress(component, 0, "Locomotion.ClimbUp", component2, IsClimbingUp);
		StepDown = new AnimatorStateProgress(component, 0, "Locomotion.StepDown", component2, IsSteppingDown);
		Flailing = new AnimatorStateProgress(component, 0, "Locomotion.Flailing", component2, IsFlailing);
		Stabilizing = new AnimatorStateProgress(component, 0, "Locomotion.Stabilizing", component2, IsStabilizing);
		StandingUp = new AnimatorStateProgress(component, 0, "Locomotion.StandingUp", component2);
		SoftHurt = new AnimatorStateProgress(component, 2, "Spine.SoftHurt", component2, IsSoftHurt);
		Hurting = new AnimatorStateProgress(component, 0, "Locomotion.Hurting", component2, IsHardHurt);
		Trap = new AnimatorStateProgress(component, 0, "Locomotion.Trap", component2, IsLeavingTrap);
		Take = new AnimatorStateProgress(component, 0, "Locomotion.Take", component2, IsTaking);
		Stunned = new AnimatorStateProgress(component, 0, "Locomotion.Stunned", component2, IsStunned);
		Sitting = new AnimatorStateProgress(component, 0, "Locomotion.Sitting", component2, IsSitting);
		WorkingEntering = new AnimatorStateProgress(component, 0, "Locomotion.Work.Entering", component2);
		WorkingLeaving = new AnimatorStateProgress(component, 0, "Locomotion.Work.Leaving", component2);
		Unburrow = new AnimatorStateProgress(component, 0, "Locomotion.Unburrow", component2, IsUnburrowing);
		GettingTrapped = new AnimatorStateProgress(component, 0, "Locomotion.Trap.GettingTrapped", component2, IsGettingTrapped);
		Trapped = new AnimatorStateProgress(component, 0, "Locomotion.Trap.Trapped", component2);
		LeavingTrap = new AnimatorStateProgress(component, 0, "Locomotion.Trap.LeavingTrap", component2, IsLeavingTrap);
		JumpingOff = new AnimatorStateProgress(component, 0, "Locomotion.JumpingOff", component2, IsJumpingOff);
		JumpingOffJumping = new AnimatorStateProgress(component, 0, "Locomotion.JumpingOff.Jumping", component2);
		JumpingOffFalling = new AnimatorStateProgress(component, 0, "Locomotion.JumpingOff.Falling", component2);
		JumpingOffLanding = new AnimatorStateProgress(component, 0, "Locomotion.JumpingOff.Landing", component2);
		Ladder = new AnimatorStateProgress(component, 0, "Locomotion.Ladder", component2);
		LadderGoingDownStart = new AnimatorStateProgress(component, 0, "Locomotion.Ladder.GrabbingLadderFromBottom", component2);
		LadderGoingDownCycle = new AnimatorStateProgress(component, 0, "Locomotion.Ladder.GoingUpLadder", component2);
		LadderGoingDownEnd = new AnimatorStateProgress(component, 0, "Locomotion.Ladder.LeavingLadderFromTop", component2);
		LadderGoingUpStart = new AnimatorStateProgress(component, 0, "Locomotion.Ladder.GrabbingLadderFromTop", component2);
		LadderGoingUpCycle = new AnimatorStateProgress(component, 0, "Locomotion.Ladder.GoingDownLadder", component2);
		LadderGoingUpEnd = new AnimatorStateProgress(component, 0, "Locomotion.Ladder.LeavingLadderFromBottom", component2);
		Combat = new AnimatorStateProgress(component, 0, "Locomotion.Combat", component2);
		CombatIdle = new AnimatorStateProgress(component, 0, "Locomotion.Combat.CombatIdle", component2);
		CombatCrank = new AnimatorStateProgress(component, 0, "Locomotion.Combat.Crank", component2, IsAttacking);
		CombatSwing = new AnimatorStateProgress(component, 0, "Locomotion.Combat.Swing", component2);
		CombatMissed = new AnimatorStateProgress(component, 0, "Locomotion.Combat.Missed", component2);
		CombatHit = new AnimatorStateProgress(component, 0, "Locomotion.Combat.Hit", component2);
		CombatBlocked = new AnimatorStateProgress(component, 0, "Locomotion.Combat.Blocked", component2);
		CombatTurn = new AnimatorStateProgress(component, 0, "Locomotion.Combat.CombatTurn", component2, IsTurningOnSpotCombat);
		CombatStep = new AnimatorStateProgress(component, 0, "Locomotion.Combat.CombatStep", component2, IsCombatStepping);
		ShieldBlocking = new AnimatorStateProgress(component, 0, "Locomotion.Combat.ShieldBlocking", component2, IsShieldBlocking);
		RunningStrike = new AnimatorStateProgress(component, 0, "Locomotion.RunningStrike", component2);
		CombatAbortStrike = new AnimatorStateProgress(component, 0, "Locomotion.Combat.CombatAbortStrike", component2, IsAbortingStrike);
		Archery = new AnimatorStateProgress(component, 0, "Locomotion.Archery", component2);
		ArcheryIdle = new AnimatorStateProgress(component, 0, "Locomotion.Archery.Idle", component2);
		ArcheryShooting = new AnimatorStateProgress(component, 0, "Locomotion.Archery.Shooting", component2, IsShooting);
		ArcheryAiming = new AnimatorStateProgress(component, 0, "Locomotion.Archery.Aiming", component2);
		ArcheryTurning = new AnimatorStateProgress(component, 0, "Locomotion.Archery.Turning", component2, IsTurningOnSpotArchery);
		ArcheryCooldown = new AnimatorStateProgress(component, 0, "Locomotion.Archery.Cooldown", component2);
		ArcheryInPlaceToIdle = new AnimatorStateProgress(component, 0, "Locomotion.Archery.ToArcheryIdle", component2);
		ArcheryIdleToAiming = new AnimatorStateProgress(component, 0, "Locomotion.Archery.Idle_To_Aiming", component2, IsAiming);
		TakingOneHand = new AnimatorStateProgress(component, 0, "Locomotion.Take.TakingOneHand", component2, IsTakingOneHand);
		TakingTwoHands = new AnimatorStateProgress(component, 0, "Locomotion.Take.TakingTwoHands", component2);
		TakingOnBack = new AnimatorStateProgress(component, 0, "Locomotion.Take.TakingOnBack", component2);
		TakingInBag = new AnimatorStateProgress(component, 0, "Locomotion.Take.TakingInBag", component2);
		UsePick = new AnimatorStateProgress(component, 0, "Locomotion.UsePick", component2);
		HitBrick = new AnimatorStateProgress(component, 0, "Locomotion.HitBrick", component2, HittingBrick);
		CutWood = new AnimatorStateProgress(component, 0, "Locomotion.CutWood", component2);
	}
}
