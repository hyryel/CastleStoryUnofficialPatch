using System;
using System.Collections.Generic;
using System.Reflection;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Behaviors;
using Brix.Game.AI.Damage;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Audio;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using FSM;
using MoonSharp.Interpreter;
using StateChart;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Locomotion4 : SignalStateMachineComponent
{
	[Serializable]
	public class StateUpdaters
	{
		public Movement Movement = new Movement();

		public Fighting Fighting = new Fighting();

		public Archery Archery = new Archery();

		public Brix.Game.Locomotion.Common Common = new Brix.Game.Locomotion.Common();
	}

	public struct Impact
	{
		public Vector3 Origin;

		public Vector3 Force;

		public float UpwardsEffect;
	}

	public enum State
	{
		InPlace,
		Trap,
		GettingTrapped,
		Trapped,
		LeavingTrap,
		WalkCycle,
		RunCycle,
		CloseStepStart,
		CloseStepEnd,
		Halting,
		TurnOnSpot,
		ClimbUp,
		StepDown,
		Ladder,
		JumpingOff,
		JumpingOffJumping,
		JumpingOffFalling,
		JumpingOffLanding,
		Stunned,
		Flailing,
		Stabilizing,
		Combat,
		CombatIdle,
		CombatStep,
		CombatTurn,
		CombatCrank,
		CombatSwing,
		CombatHit,
		CombatMissed,
		CombatBlocked,
		CombatAbortStrike,
		Archery,
		ArcheryInPlaceToIdle,
		ArcheryIdle,
		ArcheryIdleToAiming,
		ArcheryAiming,
		ArcheryCooldown,
		ArcheryShooting,
		ArcheryTurning,
		ShieldBlocking,
		Take,
		CutWood,
		HitBrick,
		UsePick,
		Sit,
		Unburrow,
		Work
	}

	private static BrixLogger logger;

	private static readonly UpdatableNetworkProfiler<Locomotion4> UpdatableProfiler;

	public Signal HasArrived = new Signal();

	public Signal<Vector3> PathBlocked = new Signal<Vector3>();

	public Signal AttackFailed = new Signal();

	public Signal HasFiredArrow = new Signal();

	[LuaDrivable("hk_OnFiredArrow")]
	private LuaHook lua_HasFiredArrow;

	[NonSerialized]
	public Transform trf;

	[NonSerialized]
	public Animator animator;

	[NonSerialized]
	public TargetMatcher targetMatcher;

	[NonSerialized]
	public AnimatorLayerActivator layerActivator;

	[NonSerialized]
	public VoxelPresence presence;

	[NonSerialized]
	public GridLocomotionControllerInterface controller;

	[NonSerialized]
	private Transform cameraTransform;

	[NonSerialized]
	private TrackableVoxelTransform VoxelTransform;

	[NonSerialized]
	public Occupation Occupation;

	[NonSerialized]
	public Labor Labor;

	[NonSerialized]
	private Transform moveTo;

	[NonSerialized]
	public DestinationIterator PathIterator;

	[NonSerialized]
	public DestinationResolver PathResolver;

	[NonSerialized]
	public Interaction Interaction;

	[NonSerialized]
	public Transform ParentAtCreation;

	[NonSerialized]
	public Rigidbody FlailingColliderRigidbody;

	[NonSerialized]
	public PopOuterOfObstacle PopOuterOfObstacle;

	[NonSerialized]
	public bool IsUsingLadder;

	[NonSerialized]
	public bool IsJumpingOff;

	[NonSerialized]
	public bool IsAirborne;

	[NonSerialized]
	public bool IsGrabbed;

	[NonSerialized]
	public bool IsDead;

	[NonSerialized]
	public bool InPlace;

	[NonSerialized]
	public bool TurningOnSpot;

	[NonSerialized]
	public bool Walking;

	[NonSerialized]
	public bool Running;

	[NonSerialized]
	public bool IsHalting;

	[NonSerialized]
	public bool IsInArcheryStance;

	[NonSerialized]
	public bool IsReadyToShoot;

	[NonSerialized]
	public bool InCombatStance;

	[NonSerialized]
	public bool IsReadyToStrike;

	[NonSerialized]
	public Vector3 PivotConserveHack;

	[NonSerialized]
	public bool AutoSnapToFloor;

	[NonSerialized]
	public GameObject FlailingCollider;

	[LuaDrivable("isCorpse")]
	private bool IsCorpse;

	public Factory.AssetKey FlailingColliderAssetKey;

	public Factory.AssetKey CorpseAssetKey;

	public GameObject RegularCollider;

	public bool NoRun;

	public float FloorOffset = 0.05f;

	public StateUpdaters States = new StateUpdaters();

	private Signal<Query<bool>> CanMoveQuery = new Signal<Query<bool>>();

	private bool _isInStrikingPath;

	private Signal<IPath> FollowPath = new Signal<IPath>();

	private Signal<Location> CombatTurnTo = new Signal<Location>();

	private Signal<Location> CombatStepSignal = new Signal<Location>();

	private Signal<Location> ClimbUpSignal = new Signal<Location>();

	private Signal<Location> Halted = new Signal<Location>();

	private Signal<Location> SoftHalted = new Signal<Location>();

	private Signal<Location, float> GetTrapped = new Signal<Location, float>();

	private Signal LeaveTrap = new Signal();

	private Renderer _skinRenderer;

	private Signal AnimatorMove = new Signal();

	protected Signal<int> AnimatorIK = new Signal<int>();

	private Signal AnimatorUpdate = new Signal();

	private Signal PhaseOut = new Signal();

	private Signal<Location> PhaseIn = new Signal<Location>();

	private Signal<Impact> GetKilled = new Signal<Impact>();

	private Signal<Impact> HurtSmall = new Signal<Impact>();

	private Signal<Impact> HurtBig = new Signal<Impact>();

	private Signal<Impact> GetSmashed = new Signal<Impact>();

	private Signal JumpOffMessage = new Signal();

	private Signal DoStandUp = new Signal();

	private Signal<Query<bool>> IsInPlaceMessage = new Signal<Query<bool>>();

	private Signal<Query<bool>> IsClimbingMessage = new Signal<Query<bool>>();

	private Signal GetReadyToMoveMessage = new Signal();

	private Signal<Query<bool>> IsReadyToWieldMessage = new Signal<Query<bool>>();

	private Signal<Tool> WieldMessage = new Signal<Tool>();

	private Signal<ProjectileWeapon> WieldBowMessage = new Signal<ProjectileWeapon>();

	private Signal<Location, float, float> ShootAtMessage = new Signal<Location, float, float>();

	private Signal StopArcheryStanceMessage = new Signal();

	private Signal StopCombatStanceMessage = new Signal();

	private Signal<IDamageReceiver, Vector3> StrikeMessage = new Signal<IDamageReceiver, Vector3>();

	private Signal<float> ShieldBlockMessage = new Signal<float>();

	private Signal<float> StopBlockingMessage = new Signal<float>();

	private Signal<Query<bool>> IsBlockingQuery = new Signal<Query<bool>>();

	private Signal<Query<bool>> CanBlockQuery = new Signal<Query<bool>>();

	private Signal<Query<bool>> IsCrankingQuery = new Signal<Query<bool>>();

	private Signal<ObjectLocation> Grab = new Signal<ObjectLocation>();

	private Signal Sit = new Signal();

	private Signal<Working.WorkStates, float> Work = new Signal<Working.WorkStates, float>();

	private Signal<Transform> GrabbedMessage = new Signal<Transform>();

	private Signal<Vector3> Thrown = new Signal<Vector3>();

	private Signal Dropped = new Signal();

	public State state;

	public AnimatorStateInfo stateInfo;

	private bool PhasedOut;

	private Occupation.Type occupationOnDeath;

	public bool ShowFloorPick;

	public bool DrawGizmos;

	public float PathAnticipationTime = 1f;

	private static int kRpcRpcDie;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public Movement Movement => States.Movement;

	public Fighting Fighting => States.Fighting;

	public Archery Archery => States.Archery;

	public Brix.Game.Locomotion.Common Common => States.Common;

	public BricktronSoundEffects Sounds => GetComponent<Labor>().Sounds;

	public ProjectileAiming Aiming => Archery.Aiming;

	public Transform ArcheryStartPoint => Archery.ArcheryStartPoint;

	public Transform RangeAttacksTargetPoint => Archery.RangeAttacksTargetPoint;

	public Vector3 ProjectileStartingPosition => Archery.ProjectileStartingPosition;

	public Vector3 ProjectileAimingPosition => Archery.ProjectileAimingPosition;

	public WalkNode RunningCurrent => Movement.Running.RunningCurrent;

	public float RunSpeedMperS
	{
		get
		{
			return Movement.Running.RunSpeedMperS;
		}
		set
		{
			Movement.Running.RunSpeedMperS = value;
		}
	}

	public WalkNode WalkingCurrent => Movement.Walking.WalkingCurrent;

	public float WalkSpeedMperS
	{
		get
		{
			return Movement.Walking.WalkSpeedMperS;
		}
		set
		{
			Movement.Walking.WalkSpeedMperS = value;
		}
	}

	private WalkablePathNode CurrentPathNode => presence.CurrentPathNode;

	public SpaceLocation CurrentHaltLocation => new SpaceLocation(trf.position.Rounded(), Quaternion.LookRotation(trf.forward.Flatten(), Vector3.up));

	public XYZ PositionOccupied => XYZ.FromVector3(trf.position);

	public XYZ PositionEntering
	{
		get
		{
			WalkNode currentActive = PathIterator.CurrentActive;
			return (currentActive == null || !(currentActive.Voxel != PositionOccupied)) ? XYZ.zero : currentActive.Voxel;
		}
	}

	public XYZ PositionNext
	{
		get
		{
			WalkNode peek = PathIterator.Peek;
			return (peek == null || !(peek.Voxel != PositionOccupied)) ? XYZ.zero : peek.Voxel;
		}
	}

	public bool IsInStrikingPath => _isInStrikingPath;

	public bool CanRun => !NoRun && Interaction.CanRun() && Occupation.Profession.CanRun() && (!Labor.Defending || Occupation.Profession.CanRunWhileDefensive());

	public bool CanClimb => Interaction.CanClimb();

	public bool IsReadyToBlock()
	{
		return CanBlock();
	}

	public bool IsWalking()
	{
		return Walking;
	}

	public void ChangeArchetype(int archetype)
	{
		controller.Archetype.Set(archetype);
	}

	public Vector3 GetProjectedPosition(float projectedTime)
	{
		Vector3 position = (Archery.RangeAttacksTargetPoint ?? trf).position;
		float num = projectedTime * (Running ? Movement.Running.RunSpeedMperS : ((!Walking) ? 0f : Movement.Walking.WalkSpeedMperS));
		if (PositionNext != XYZ.zero)
		{
			Vector3 vector = (PositionNext - PositionOccupied).ToVector3();
			float num2 = Mathf.Max(vector.Flatten().magnitude, 0.1f);
			return position + vector / num2 * num;
		}
		if (PositionEntering != XYZ.zero)
		{
			Vector3 vector2 = (PositionEntering - PositionOccupied).ToVector3();
			float num3 = Mathf.Max(vector2.Flatten().magnitude, 0.1f);
			return position + vector2 / num3 * num;
		}
		return PositionOccupied.ToVector3().Flatten(position);
	}

	public bool CanMove()
	{
		return InPlace || IsHalting || Walking || Running || TurningOnSpot;
	}

	public void DoFollowPath(IPath path)
	{
		_isInStrikingPath = false;
		FollowPath.Invoke(path);
	}

	public void DoFollowStrikePath(IPath path)
	{
		_isInStrikingPath = true;
		FollowPath.Invoke(path);
	}

	public void TurnTo(Vector3 pos)
	{
		if (CurrentPathNode != null)
		{
			WalkNode item = new WalkNode(CurrentPathNode, Quaternion.LookRotation((pos - trf.position).Flatten()));
			FollowPath.Invoke(new Path(trf.position, new List<WalkNode> { item }));
		}
	}

	public void DoStepTo(WalkNode location)
	{
		FollowPath.Invoke(new Path(trf.position, new List<WalkNode> { location }));
	}

	public void DoCombatTurnTo(Location dest)
	{
		CombatTurnTo.Invoke(dest);
	}

	public void DoCombatStep(Location dest)
	{
		CombatStepSignal.Invoke(dest);
	}

	public void ClimbTo(Location location)
	{
		ClimbUpSignal.Invoke(location);
	}

	public void DoHalt(Location dest = null)
	{
		Halted.Invoke(dest ?? CurrentHaltLocation);
	}

	public void DoSoftHalt(Location dest)
	{
		SoftHalted.Invoke(dest);
	}

	public bool NoHalt()
	{
		return true;
	}

	public bool ForcedPosition()
	{
		return !AutoSnapToFloor;
	}

	public bool IsInGroundedState()
	{
		return !IsAirborne && !IsGrabbed && !IsDead;
	}

	public void DoGetTrapped(Location location, float time)
	{
		GetTrapped.Invoke(location, time);
	}

	public void DoLeaveTrap()
	{
		LeaveTrap.Invoke();
	}

	private bool IsVisible()
	{
		if (_skinRenderer == null)
		{
			_skinRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (_skinRenderer != null && !_skinRenderer.isVisible)
		{
			return false;
		}
		return true;
	}

	private bool IsCloseToCamera()
	{
		return (trf.position - cameraTransform.position).sqrMagnitude <= 10000f;
	}

	private void OnAnimatorIK(int layerIndex)
	{
		AnimatorIK.Invoke(layerIndex);
	}

	protected override void BrixUpdate()
	{
		if (PhasedOut)
		{
			return;
		}
		float num = Time.deltaTime;
		targetMatcher.Time = Time.time;
		AnimatorUpdate.Invoke();
		while (targetMatcher.OverrideTimeOfEnd >= Time.time && targetMatcher.OverrideTimeOfEnd < Time.time + num)
		{
			float num2 = Mathf.Clamp(targetMatcher.OverrideTimeOfEnd - Time.time, 0.001f, num);
			ManualUpdate(num2);
			num -= num2;
			targetMatcher.Time += num2;
			if (num > 0f)
			{
				AnimatorUpdate.Invoke();
			}
			if (num2 <= 0.001f)
			{
				break;
			}
		}
		if (num > 0f)
		{
			ManualUpdate(num);
		}
		if ((Labor.Profession.CanBlock() && Labor.Defending) != Fighting.Blocking.IsSoftBlocking)
		{
			if (Labor.Profession.CanBlock() && Labor.Defending)
			{
				Fighting.Blocking.EnableSoftShieldBlock();
			}
			else
			{
				Fighting.Blocking.DisableSoftShieldBlock();
			}
		}
	}

	private void ManualUpdate(float deltaTime)
	{
		targetMatcher.AnimatorUpdate(deltaTime);
		layerActivator.AnimatorUpdate(deltaTime);
		AnimatorMove.Invoke();
	}

	public void DoPhaseOut()
	{
		PhaseOut.Invoke();
	}

	public void DoPhaseIn(Location l)
	{
		PhaseIn.Invoke(l);
	}

	public void Kill(Vector3 origin, Vector3 force, float upwardsEffect = 1f)
	{
		GetKilled.Invoke(new Impact
		{
			Origin = origin,
			Force = force,
			UpwardsEffect = upwardsEffect
		});
	}

	public void SmallHurt(Vector3 origin, Vector3 force)
	{
		HurtSmall.Invoke(new Impact
		{
			Origin = origin,
			Force = force,
			UpwardsEffect = 0f
		});
	}

	public void BigHurt(Vector3 origin, Vector3 force)
	{
		HurtBig.Invoke(new Impact
		{
			Origin = origin,
			Force = force,
			UpwardsEffect = 0f
		});
	}

	public void Smash(Vector3 origin, Vector3 force, float upwardsEffect = 1f)
	{
		GetSmashed.Invoke(new Impact
		{
			Origin = origin,
			Force = force,
			UpwardsEffect = upwardsEffect
		});
	}

	public void JumpOff()
	{
		JumpOffMessage.Invoke();
	}

	public void StandUp()
	{
		DoStandUp.Invoke();
	}

	public bool IsInPlace()
	{
		Query<bool> query = new Query<bool>(t: false);
		IsInPlaceMessage.Invoke(query);
		return query.Result && Common.Grounded.IsGrounded();
	}

	public bool IsClimbing()
	{
		Query<bool> query = new Query<bool>(t: false);
		IsClimbingMessage.Invoke(query);
		return query.Result;
	}

	public void GetReadyToMove()
	{
		GetReadyToMoveMessage.Invoke();
	}

	public bool IsReadyToWield()
	{
		Query<bool> query = new Query<bool>(t: false);
		IsReadyToWieldMessage.Invoke(query);
		return query.Result;
	}

	public void Wield(Tool weapon)
	{
		WieldMessage.Invoke(weapon);
	}

	public void WieldBow(ProjectileWeapon weapon)
	{
		WieldBowMessage.Invoke(weapon);
	}

	public void ShootAt(Location location, float aimtime, float inaccuracy)
	{
		ShootAtMessage.Invoke(location, aimtime, inaccuracy);
	}

	public void StopArcheryStance()
	{
		StopArcheryStanceMessage.Invoke();
	}

	public void StopCombatStance()
	{
		StopCombatStanceMessage.Invoke();
	}

	public MeleeAttack DoStrike(IDamageReceiver target, Vector3 endPosition)
	{
		StrikeMessage.Invoke(target, endPosition);
		return Fighting.Striking.SwordStrike;
	}

	public void DoStartBlocking(float maxDuration)
	{
		ShieldBlockMessage.Invoke(maxDuration);
	}

	public void DoStopBlocking(float maxDelay = 0f)
	{
		StopBlockingMessage.Invoke(maxDelay);
	}

	public bool IsBlocking()
	{
		Query<bool> query = new Query<bool>(t: false);
		IsBlockingQuery.Invoke(query);
		return query.Result;
	}

	public bool CanBlock()
	{
		Query<bool> query = new Query<bool>(t: false);
		CanBlockQuery.Invoke(query);
		return query.Result;
	}

	public bool IsCranking()
	{
		Query<bool> query = new Query<bool>(t: false);
		IsCrankingQuery.Invoke(query);
		return query.Result;
	}

	public void DoGrab(ObjectLocation target)
	{
		Grab.Invoke(target);
	}

	public void DoSit()
	{
		Sit.Invoke();
	}

	public void DoWork(Working.WorkStates state, Location location)
	{
		Work.Invoke(state, Mathf.Round(location.Position.y - (base.transform.position.y + 1f)));
	}

	public void StopWork()
	{
		GetReadyToMoveMessage.Invoke();
	}

	public void DoUnburrow()
	{
		controller.IsUnburrowing.Set(b: true);
	}

	public GameObject IsGrabbing()
	{
		return (!Common.Grabbing.GrabbedTarget) ? null : Common.Grabbing.GrabbedTarget.gameObject;
	}

	public void DoGetGrabbed(Transform GrabHand)
	{
		GrabbedMessage.Invoke(GrabHand);
	}

	public void DoGetThrown(Vector3 velocity)
	{
		Thrown.Invoke(velocity);
	}

	public void DoGetDropped()
	{
		Dropped.Invoke();
	}

	public void SetParent(Transform parent)
	{
		if (!(base.transform.parent == parent) && (!(FlailingCollider != null) || !(FlailingCollider.transform.parent == parent)))
		{
			if (FlailingCollider != null && base.transform.transform.parent == FlailingCollider.transform)
			{
				FlailingCollider.transform.SetParent(parent, worldPositionStays: true);
			}
			else
			{
				base.transform.SetParent(parent, worldPositionStays: true);
			}
		}
	}

	private void OnSignalGoTo(Signal<AnimatorStateInfo> controllerSignal, State state)
	{
		controllerSignal.Connect(delegate(AnimatorStateInfo stateInfo)
		{
			this.state = state;
			this.stateInfo = stateInfo;
		});
	}

	private void InitStateTransitions()
	{
		OnSignalGoTo(controller.InPlace.StateEnter, State.InPlace);
		OnSignalGoTo(controller.TurnOnSpot.StateEnter, State.TurnOnSpot);
		OnSignalGoTo(controller.ClimbUp.StateEnter, State.ClimbUp);
		OnSignalGoTo(controller.StepDown.StateEnter, State.StepDown);
		OnSignalGoTo(controller.WalkCycle.StateEnter, State.WalkCycle);
		OnSignalGoTo(controller.CloseStepStart.StateEnter, State.CloseStepStart);
		OnSignalGoTo(controller.CloseStepEnd.StateEnter, State.CloseStepEnd);
		OnSignalGoTo(controller.RunCycle.StateEnter, State.RunCycle);
		OnSignalGoTo(controller.Combat.StateEnter, State.Combat);
		OnSignalGoTo(controller.Halting.StateEnter, State.Halting);
		OnSignalGoTo(controller.Trap.StateEnter, State.Trap);
		OnSignalGoTo(controller.GettingTrapped.StateEnter, State.GettingTrapped);
		OnSignalGoTo(controller.Trapped.StateEnter, State.Trapped);
		OnSignalGoTo(controller.LeavingTrap.StateEnter, State.LeavingTrap);
		OnSignalGoTo(controller.Stunned.StateEnter, State.Stunned);
		OnSignalGoTo(controller.Stabilizing.StateEnter, State.Stabilizing);
		OnSignalGoTo(controller.Flailing.StateEnter, State.Flailing);
		OnSignalGoTo(controller.Ladder.StateEnter, State.Ladder);
		OnSignalGoTo(controller.Take.StateEnter, State.Take);
		OnSignalGoTo(controller.Sitting.StateEnter, State.Sit);
		OnSignalGoTo(controller.WorkingEntering.StateEnter, State.Work);
		OnSignalGoTo(controller.Unburrow.StateEnter, State.Sit);
		OnSignalGoTo(controller.JumpingOff.StateEnter, State.JumpingOff);
		OnSignalGoTo(controller.JumpingOffJumping.StateEnter, State.JumpingOffJumping);
		OnSignalGoTo(controller.JumpingOffFalling.StateEnter, State.JumpingOffFalling);
		OnSignalGoTo(controller.JumpingOffLanding.StateEnter, State.JumpingOffLanding);
		OnSignalGoTo(controller.CombatIdle.StateEnter, State.CombatIdle);
		OnSignalGoTo(controller.CombatCrank.StateEnter, State.CombatCrank);
		OnSignalGoTo(controller.CombatSwing.StateEnter, State.CombatSwing);
		OnSignalGoTo(controller.ShieldBlocking.StateEnter, State.ShieldBlocking);
		OnSignalGoTo(controller.CombatHit.StateEnter, State.CombatHit);
		OnSignalGoTo(controller.CombatMissed.StateEnter, State.CombatMissed);
		OnSignalGoTo(controller.CombatBlocked.StateEnter, State.CombatBlocked);
		OnSignalGoTo(controller.CombatStep.StateEnter, State.CombatStep);
		OnSignalGoTo(controller.CombatTurn.StateEnter, State.CombatTurn);
		OnSignalGoTo(controller.CombatAbortStrike.StateEnter, State.CombatAbortStrike);
		OnSignalGoTo(controller.Archery.StateEnter, State.Archery);
		OnSignalGoTo(controller.ArcheryInPlaceToIdle.StateEnter, State.ArcheryInPlaceToIdle);
		OnSignalGoTo(controller.ArcheryIdle.StateEnter, State.ArcheryIdle);
		OnSignalGoTo(controller.ArcheryIdleToAiming.StateEnter, State.ArcheryIdleToAiming);
		OnSignalGoTo(controller.ArcheryAiming.StateEnter, State.ArcheryAiming);
		OnSignalGoTo(controller.ArcheryCooldown.StateEnter, State.ArcheryCooldown);
		OnSignalGoTo(controller.ArcheryShooting.StateEnter, State.ArcheryShooting);
		OnSignalGoTo(controller.ArcheryTurning.StateEnter, State.ArcheryTurning);
		OnSignalGoTo(controller.CutWood.StateEnter, State.CutWood);
		OnSignalGoTo(controller.HitBrick.StateEnter, State.HitBrick);
		OnSignalGoTo(controller.UsePick.StateEnter, State.UsePick);
	}

	[ServerCallback]
	protected sealed override SignalState CreateFSM()
	{
		if (!NetworkServer.active)
		{
			return null;
		}
		InitializeLocomotion();
		InitStateTransitions();
		return SignalStateMachine.Begin("Locomotion").On(GetSmashed).Do<Impact>(Common.Flailing.GetSmashed)
			.On(PhaseOut)
			.Do(StartPhaseOut)
			.On(PhaseIn)
			.Do<Location>(EndPhaseOut)
			.On(GetKilled)
			.GoTo("Dead")
			.PostChildren()
			.On(GrabbedMessage)
			.GoTo("Grabbed")
			.Begin("Grounded")
			.BeginRegion("Movement")
			.On(AnimatorUpdate)
			.Do(delegate
			{
				Common.Grounded.Update();
			})
			.PostChildren()
			.On(AnimatorMove)
			.Do(ApplyAnimatorMotion)
			.On(FollowPath)
			.Do(delegate(IPath p)
			{
				SetPath(p);
			})
			.On(Halted)
			.Do<Location>(Movement.Halting.Halt)
			.On(SoftHalted)
			.Do<Location>(Movement.Halting.SoftHalt)
			.On(GetTrapped)
			.Do<Location, float>(Common.Trapped.Trap)
			.On(StopCombatStanceMessage)
			.Do(delegate
			{
				controller.IsTakingCombatStance.Set(b: false);
			})
			.On(WieldMessage)
			.Do<Tool>(Fighting.StartFighting)
			.On(controller.InPlace.StateEnter)
			.GoTo("InPlace")
			.On(controller.TurnOnSpot.StateEnter)
			.GoTo("TurnOnSpot")
			.On(controller.ClimbUp.StateEnter)
			.GoTo("ClimbUp")
			.On(controller.StepDown.StateEnter)
			.GoTo("StepDown")
			.On(controller.WalkCycle.StateEnter)
			.GoTo("Walking")
			.On(controller.CloseStepStart.StateEnter)
			.GoTo("CloseStep")
			.On(controller.RunCycle.StateEnter)
			.GoTo("Running")
			.On(controller.Combat.StateEnter)
			.GoTo("Fighting")
			.On(controller.Archery.StateEnter)
			.GoTo("Archery")
			.On(controller.JumpingOffJumping.StateEnter)
			.GoTo("JumpingOff")
			.On(controller.Halting.StateEnter)
			.GoTo("Halting")
			.On(controller.Trapped.StateEnter)
			.GoTo("Trapped")
			.On(controller.Stunned.StateEnter)
			.GoTo("Airborne")
			.On(controller.Flailing.StateEnter)
			.GoTo("Airborne")
			.On(controller.Ladder.StateEnter)
			.GoTo("Ladder")
			.On(controller.CutWood.StateEnter)
			.GoTo("CutWood")
			.On(controller.HitBrick.StateEnter)
			.GoTo("HitBrick")
			.On(controller.UsePick.StateEnter)
			.GoTo("UsePick")
			.On(controller.Take.StateEnter)
			.GoTo("Take")
			.On(controller.Sitting.StateEnter)
			.GoTo("Sitting")
			.On(controller.WorkingEntering.StateEnter)
			.GoTo("Working")
			.On(controller.Unburrow.StateEnter)
			.GoTo("Unburrow")
			.Entry(controller.Combat.StateEnter, controller.Combat.StateEnter.Invoke)
			.Begin("InPlace")
			.Entry(Movement.InPlace.Enter)
			.Exit(Movement.InPlace.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.InPlace.Update)
			.On(IsInPlaceMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(IsReadyToWieldMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(CanMoveQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(WieldBowMessage)
			.Do<ProjectileWeapon>(Archery.WieldBow)
			.On(JumpOffMessage)
			.Do(delegate
			{
				controller.IsJumpingOff.Set(b: true);
			})
			.On(ClimbUpSignal)
			.Do(delegate(Location l)
			{
				Movement.Ladder.UseLadder(l.Position);
			})
			.On(Grab)
			.Do<ObjectLocation>(Common.Grabbing.Grab)
			.On(Sit)
			.Do(Common.Sitting.DoSit)
			.On(Work)
			.Do(delegate(Working.WorkStates s, float f)
			{
				Common.Working.WorkToward(s, f);
			})
			.End()
			.Begin("Sitting")
			.Exit(Common.Sitting.Exit)
			.On(GetReadyToMoveMessage)
			.Do(Common.Sitting.EndSit)
			.End()
			.Begin("Working")
			.Exit(Common.Working.Exit)
			.On(GetReadyToMoveMessage)
			.Do(Common.Working.EndWork)
			.End()
			.Begin("Take")
			.Entry(Common.Grabbing.Enter)
			.Exit(Common.Grabbing.Exit)
			.On(AnimatorUpdate)
			.Do(Common.Grabbing.Update)
			.On(HurtSmall)
			.Do<Impact>(Common.Grabbing.OnHurt)
			.On(HurtBig)
			.Do<Impact>(Common.Grabbing.OnHurt)
			.End()
			.Begin("TurnOnSpot")
			.Entry(Movement.TurnOnSpot.Enter)
			.Exit(Movement.TurnOnSpot.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.TurnOnSpot.Update)
			.On(CanMoveQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("CloseStep")
			.Entry(Movement.CloseStep.Enter)
			.On(controller.CloseStepEnd.StateEnter)
			.GoTo("CloseStepEnd")
			.On(CanMoveQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.Begin("CloseStepStart")
			.Entry(Movement.CloseStepStart.Enter)
			.End()
			.Begin("CloseStepEnd")
			.Entry(Movement.CloseStepEnd.Enter)
			.End()
			.End()
			.Begin("Walking")
			.Entry(Movement.Walking.Enter)
			.Exit(Movement.Walking.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.Walking.Update)
			.On(CanMoveQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("Running")
			.Entry(Movement.Running.Enter)
			.Exit(Movement.Running.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.Running.Update)
			.On(CanMoveQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("Ladder")
			.Entry(Movement.Ladder.Enter)
			.Exit(Movement.Ladder.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.Ladder.Update)
			.End()
			.Begin("Halting")
			.Entry(Movement.Halting.Enter)
			.Exit(Movement.Halting.Exit)
			.On(CanMoveQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("HardHurting")
			.Entry(delegate
			{
				AutoSnapToFloor = true;
			})
			.End()
			.Begin("ClimbUp")
			.Entry(Movement.ClimbUp.Enter)
			.Exit(Movement.ClimbUp.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.ClimbUp.Update)
			.On(IsClimbingMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("StepDown")
			.Entry(Movement.StepDown.Enter)
			.Exit(Movement.StepDown.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.StepDown.Update)
			.On(IsClimbingMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("JumpingOff")
			.On(controller.JumpingOffFalling.StateEnter)
			.GoTo("JumpingOffFalling")
			.On(controller.JumpingOffLanding.StateEnter)
			.GoTo("JumpingOffLanding")
			.Begin("JumpingOffJumping")
			.Entry(Movement.JumpingOff.StartJumping)
			.End()
			.Begin("JumpingOffFalling")
			.Entry(Movement.JumpingOff.StartFalling)
			.Exit(Movement.JumpingOff.EndFalling)
			.On(AnimatorUpdate)
			.Do(Movement.JumpingOff.UpdateFalling)
			.End()
			.Begin("JumpingOffLanding")
			.Entry(Movement.JumpingOff.StartLanding)
			.Exit(Movement.JumpingOff.EndLanding)
			.End()
			.End()
			.Begin("Unburrow")
			.End()
			.Begin("CutWood")
			.End()
			.Begin("HitBrick")
			.Entry(Movement.HitBrick.Enter)
			.Exit(Movement.HitBrick.Exit)
			.On(AnimatorUpdate)
			.Do(Movement.HitBrick.Update)
			.End()
			.Begin("UsePick")
			.End()
			.Begin("Fighting")
			.Entry(Fighting.Enter)
			.Exit(Fighting.Exit)
			.On(StopCombatStanceMessage)
			.Do(Fighting.StopFighting)
			.On(GetReadyToMoveMessage)
			.Do(Fighting.StopFighting)
			.On(IsInPlaceMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(CombatStepSignal)
			.Do<Location>(Fighting.Stepping.CombatStep)
			.On(FollowPath)
			.Do<IPath>(Fighting.FollowPath)
			.On(AnimatorUpdate)
			.Do(Fighting.Update)
			.On(controller.CombatIdle.StateEnter)
			.GoTo("ReadyToStrike")
			.On(controller.CombatCrank.StateEnter)
			.GoTo("Cranking")
			.On(controller.CombatSwing.StateEnter)
			.GoTo("Swinging")
			.On(controller.ShieldBlocking.StateEnter)
			.GoTo("Blocking")
			.On(controller.CombatHit.StateEnter)
			.GoTo("Reacting")
			.On(controller.CombatMissed.StateEnter)
			.GoTo("Reacting")
			.On(controller.CombatBlocked.StateEnter)
			.GoTo("Reacting")
			.On(controller.CombatStep.StateEnter)
			.GoTo("Stepping")
			.On(controller.CombatTurn.StateEnter)
			.GoTo("TurningTo")
			.On(controller.CombatAbortStrike.StateEnter)
			.GoTo("AbortingStrike")
			.Begin("ReadyToStrike")
			.Entry(Fighting.ReadyToStrike.Enter)
			.Exit(Fighting.ReadyToStrike.Exit)
			.On(AnimatorUpdate)
			.Do(Fighting.ReadyToStrike.Update)
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlock)
			.On(CombatTurnTo)
			.Do<Location>(Fighting.TurningTo.CombatTurnTo)
			.On(StrikeMessage)
			.Do<IDamageReceiver, Vector3>(Fighting.Striking.Strike)
			.End()
			.Begin("TurningTo")
			.Entry(Fighting.TurningTo.Enter)
			.Exit(Fighting.TurningTo.Exit)
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlock)
			.On(IsInPlaceMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = false;
			})
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("Stepping")
			.Entry(Fighting.Stepping.Enter)
			.Exit(Fighting.Stepping.Exit)
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlock)
			.On(IsInPlaceMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = false;
			})
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(AnimatorUpdate)
			.Do(Fighting.Stepping.Update)
			.End()
			.Begin("Cranking")
			.Entry(Fighting.Striking.StartCranking)
			.Exit(Fighting.Striking.EndCranking)
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlockDuringStrike)
			.On(IsCrankingQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(AnimatorUpdate)
			.Do(Fighting.Striking.UpdateCranking)
			.End()
			.Begin("Swinging")
			.Entry(Fighting.Striking.StartSwinging)
			.Exit(Fighting.Striking.EndSwinging)
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlockDuringStrike)
			.On(AnimatorUpdate)
			.Do(Fighting.Striking.UpdateSwinging)
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("Reacting")
			.Entry(Fighting.Striking.StartReacting)
			.Exit(Fighting.Striking.EndReacting)
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlockDuringStrike)
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("AbortingStrike")
			.Entry(Fighting.Striking.StartAbortingStrike)
			.On(ShieldBlockMessage)
			.Do<float>(Fighting.Blocking.EnableShieldBlockDuringStrike)
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.End()
			.Begin("Blocking")
			.Entry(Fighting.Blocking.Enter)
			.Exit(Fighting.Blocking.Exit)
			.On(StopBlockingMessage)
			.Do<float>(Fighting.Blocking.DisableShieldBlock)
			.On(IsBlockingQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(CanBlockQuery)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.On(AnimatorUpdate)
			.Do(Fighting.Blocking.Update)
			.End()
			.End()
			.Begin("Archery")
			.Entry(Archery.Enter)
			.Exit(Archery.Exit)
			.On(AnimatorUpdate)
			.Do(Archery.Update)
			.On(controller.ArcheryIdle.StateEnter)
			.GoTo("IdleArchery")
			.On(controller.ArcheryAiming.StateEnter)
			.GoTo("Aiming")
			.On(controller.ArcheryTurning.StateEnter)
			.GoTo("PivotingAim")
			.On(controller.ArcheryShooting.StateEnter)
			.GoTo("Shooting")
			.On(controller.ArcheryCooldown.StateEnter)
			.GoTo("Cooldown")
			.On(controller.ArcheryInPlaceToIdle.StateEnter)
			.GoTo("ToArcheryIdle")
			.On(controller.ArcheryIdleToAiming.StateEnter)
			.GoTo("IdleToAiming")
			.On(StopArcheryStanceMessage)
			.Do(Archery.LeaveArcheryStance)
			.On(GetReadyToMoveMessage)
			.Do(Archery.LeaveArcheryStance)
			.On(IsInPlaceMessage)
			.Do(delegate(Query<bool> q)
			{
				q.Result = true;
			})
			.Begin("ToArcheryIdle")
			.End()
			.Begin("IdleArchery")
			.Entry(Archery.StartIdleArchery)
			.Exit(Archery.EndIdleArchery)
			.On(AnimatorUpdate)
			.Do(Archery.UpdateIdleArchery)
			.On(ShootAtMessage)
			.Do<Location, float, float>(Archery.ShootAt)
			.End()
			.Begin("IdleToAiming")
			.On(AnimatorUpdate)
			.Do(Archery.UpdateIdleToAiming)
			.End()
			.Begin("Aiming")
			.Entry(Archery.StartAiming)
			.On(AnimatorUpdate)
			.Do(Archery.UpdateAiming)
			.On(ShootAtMessage)
			.Do<Location, float, float>(Archery.ShootAt)
			.On(HurtBig)
			.Do((Action<Impact>)delegate
			{
				Archery.AbortAimingOnHurt();
			})
			.End()
			.Begin("PivotingAim")
			.Entry(Archery.StartPivotingAim)
			.On(AnimatorUpdate)
			.Do(Archery.UpdatePivotingAim)
			.End()
			.Begin("Shooting")
			.Entry(Archery.StartShooting)
			.On(AnimatorUpdate)
			.Do(Archery.UpdateShooting)
			.End()
			.Begin("Cooldown")
			.Entry(Archery.StartCooldown)
			.End()
			.Begin("Loading")
			.End()
			.End()
			.Begin("Trapped")
			.Entry(Common.Trapped.Enter)
			.On(AnimatorUpdate)
			.Do(Common.Trapped.Update)
			.On(LeaveTrap)
			.Do(Common.Trapped.LeaveTrap)
			.End()
			.End()
			.BeginRegion("PartialHurt")
			.On(HurtSmall)
			.GoTo("Hurting")
			.On(HurtBig)
			.GoTo("Hurting")
			.Begin("NotHurting")
			.End()
			.Begin("Hurting")
			.Entry(HurtSmall, Common.Hurting.SoakSmallHurt)
			.Entry(HurtBig, Common.Hurting.StartHardHurt)
			.Exit(Common.Hurting.EndSmallHurt)
			.On(AnimatorUpdate)
			.GoTo("NotHurting")
			.If(Common.Hurting.IsReadyToEnd)
			.End()
			.End()
			.BeginRegion("LookAt")
			.On(AnimatorIK)
			.Do<int>(Common.LookAt.UpdateIK)
			.End()
			.End()
			.Begin("Airborne")
			.Entry(Common.Airborne.Enter)
			.Exit(Common.Airborne.Exit)
			.On(controller.Stabilizing.StateEnter)
			.GoTo("Stabilizing")
			.On(controller.StandingUp.StateEnter)
			.GoTo("StandingUp")
			.On(controller.InPlace.StateEnter)
			.GoTo("Grounded")
			.On(controller.Combat.StateEnter)
			.GoTo("Grounded")
			.Begin("Flailing")
			.Entry(Common.Flailing.Enter)
			.Exit(Common.Flailing.Exit)
			.On(AnimatorUpdate)
			.Do(Common.Flailing.Update)
			.End()
			.Begin("Stabilizing")
			.Entry(Common.Airborne.StartStabilizing)
			.On(controller.Stunned.StateEnter)
			.GoTo("Flailing")
			.On(controller.Flailing.StateEnter)
			.GoTo("Flailing")
			.On(AnimatorMove)
			.Do(ApplyAnimatorMotion)
			.End()
			.Begin("StandingUp")
			.Entry(Common.Airborne.StartStandingUp)
			.On(controller.Stunned.StateEnter)
			.GoTo("Flailing")
			.On(controller.Flailing.StateEnter)
			.GoTo("Flailing")
			.On(AnimatorMove)
			.Do(ApplyAnimatorMotion)
			.End()
			.End()
			.Begin("Dead")
			.Entry(GetKilled, Die)
			.Entry(delegate
			{
				IsDead = true;
			})
			.Exit(delegate
			{
				IsDead = false;
			})
			.End()
			.Begin("Grabbed")
			.Entry(GrabbedMessage, Common.Grabbing.StartGrabbedOnGrabbed)
			.Entry(Common.Grabbing.StartGrabbed)
			.Exit(Common.Grabbing.EndGrabbed)
			.On(AnimatorUpdate)
			.Do(Common.Grabbing.UpdateGrabbed)
			.On(controller.Stunned.StateEnter)
			.GoTo("Airborne")
			.On(controller.Flailing.StateEnter)
			.GoTo("Airborne")
			.On(Dropped)
			.Do(Common.Flailing.Flail)
			.On(Thrown)
			.Do<Vector3>(Common.Flailing.Thrown)
			.End()
			.End();
	}

	public void PathBlockedAt(Vector3 position)
	{
		if (IsInGroundedState())
		{
			Movement.Halting.SoftHalt(CurrentHaltLocation);
		}
		PathBlocked.Invoke(position);
	}

	public void SetPath(IPath path)
	{
		PathIterator.Replace(path);
	}

	public void SetForcedDirectPath(WalkNode destination)
	{
		Path path = new Path(trf.position, new List<WalkNode> { destination });
		PathIterator.Replace(path);
	}

	public void AdvancePath()
	{
		PathIterator.MoveNext();
	}

	public void ClearPath()
	{
		PathIterator.Clear();
	}

	public bool RepathMoveNext()
	{
		if (!Labor.Profession.CanPassThrough())
		{
			if (!PathResolver.RepathMoveNext(tryPassThrough: false, canClimb: true))
			{
				return false;
			}
		}
		else if (!PathResolver.RepathMoveNext(tryPassThrough: false, canClimb: false) && !PathResolver.RepathMoveNext(tryPassThrough: true, canClimb: true))
		{
			return false;
		}
		PathIterator.Replace(PathResolver.Result);
		if (XYZ.FromVector3(trf.position) == XYZ.FromVector3(PathIterator.Current.Position))
		{
			PathIterator.MoveNext();
		}
		return true;
	}

	private void StartPhaseOut()
	{
		RegularCollider.SetActive(value: false);
		PhasedOut = true;
	}

	private void EndPhaseOut()
	{
		RegularCollider.SetActive(value: true);
		PhasedOut = false;
	}

	private void Die(Impact i)
	{
		occupationOnDeath = Occupation.CurrentOccupation;
		CallRpcDie(i);
	}

	[ClientRpc]
	public void RpcDie(Impact i)
	{
		if (!IsCorpse)
		{
			IsCorpse = true;
			Corpse corpse = Corpse.Spawn(CorpseAssetKey, occupationOnDeath, base.gameObject, i);
			SetLevelUpMaterial(corpse);
			if (FlailingCollider != null && base.transform.IsChildOf(FlailingCollider.transform))
			{
				ObjectPoolSingleton.Release(base.gameObject);
				ObjectPoolSingleton.Release(FlailingCollider);
			}
			else
			{
				ObjectPoolSingleton.Release(FlailingCollider);
				ObjectPoolSingleton.Release(base.gameObject);
			}
			Corpse.PostSpawn(corpse);
		}
	}

	public void SetLevelUpMaterial(Corpse corpse)
	{
		SkinnedMeshRenderer componentInChildren = GetComponentInChildren<SkinnedMeshRenderer>();
		SkinnedMeshRenderer componentInChildren2 = corpse.GetComponentInChildren<SkinnedMeshRenderer>();
		if ((bool)componentInChildren && (bool)componentInChildren2)
		{
			componentInChildren2.material = componentInChildren.material;
		}
	}

	public void MaintainDirection()
	{
		if (PathIterator.HasCurrent)
		{
			Vector3 forward = (PathIterator.Current.Position - trf.position).Flatten();
			if (!(forward.sqrMagnitude < 0.01f))
			{
				trf.rotation = Quaternion.LookRotation(forward);
			}
		}
	}

	private void InitializeLocomotion()
	{
		lua_HasFiredArrow = new LuaHook(HasFiredArrow);
		trf = base.transform;
		InitAnimator();
		InitSceneComponents();
		InitGameComponents();
		InitPathPredictors();
		InitFlailingCollider();
		InitStateUpdaters(States);
	}

	protected override void InitClientAnimator()
	{
		trf = base.transform;
		InitAnimator();
		InitSceneComponents();
		InitGameComponents();
		InitFlailingCollider();
		animator = GetComponent<Animator>();
		controller = GetComponent<GridLocomotionControllerInterface>();
		controller.AttackWillHit.Set(b: true);
	}

	protected override void InitAnimator()
	{
		animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.applyRootMotion = false;
		}
		AnimationEventBoard component = GetComponent<AnimationEventBoard>();
		if (component != null)
		{
			component.OnEvent.Connect(HandleEvent);
		}
		controller = GetComponent<GridLocomotionControllerInterface>();
		if (controller == null)
		{
			controller = base.gameObject.AddComponent<GridLocomotionControllerInterface>();
		}
		layerActivator = GetComponent<AnimatorLayerActivator>();
		layerActivator.Deactivate(1);
		layerActivator.Deactivate(2);
		targetMatcher = GetComponent<TargetMatcher>();
	}

	private void InitSceneComponents()
	{
		if (Camera.main != null)
		{
			cameraTransform = Camera.main.transform;
		}
	}

	private void InitGameComponents()
	{
		Labor = GetComponent<Labor>();
		presence = GetComponent<VoxelPresence>();
		Interaction = GetComponent<Interaction>();
		Occupation = GetComponent<Occupation>();
		PopOuterOfObstacle = GetComponent<PopOuterOfObstacle>();
		VoxelTransform = GetComponent<TrackableVoxelTransform>();
	}

	private void InitPathPredictors()
	{
		PathIterator = new DestinationIterator();
		PathResolver = new DestinationResolver(this);
		moveTo = new GameObject("MoveTo").transform;
		moveTo.SetParent(trf);
		moveTo.localPosition = Vector3.zero;
		moveTo.localRotation = Quaternion.identity;
		moveTo.gameObject.SetActive(value: false);
	}

	private void InitFlailingCollider()
	{
		if (FlailingCollider != null && FlailingCollider == trf.parent)
		{
			logger.InfoFormat("InitFlailingCollider: '{0}' was already parented to its flailing collider ({1})", base.name, FlailingCollider);
			ParentAtCreation = trf.parent.parent;
			trf.SetParent(ParentAtCreation);
			return;
		}
		if (trf.parent.GetComponent<Rigidbody>() != null)
		{
			trf.SetParent(trf.parent.parent);
		}
		ParentAtCreation = trf.parent;
		try
		{
			ObjectPoolSingleton.Request(out FlailingCollider, FlailingColliderAssetKey);
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "InitFlailingCollider: '{0}' couldn't get FlailingCollider from assetKey '{1}'.", base.name, FlailingColliderAssetKey);
		}
		FlailingCollider.GetComponent<FlailingColliderMonitor>().characterTransform = base.transform;
		if (FlailingCollider != null)
		{
			FlailingCollider.SetActive(value: false);
			FlailingCollider.transform.parent = trf;
			FlailingCollider.transform.localPosition = Vector3.zero;
			FlailingCollider.transform.localRotation = Quaternion.identity;
		}
	}

	private void InitStateUpdaters(object root)
	{
		FieldInfo[] fields = root.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (typeof(LocomotionStateUpdater).IsAssignableFrom(fieldInfo.FieldType))
			{
				LocomotionStateUpdater locomotionStateUpdater = (LocomotionStateUpdater)fieldInfo.GetValue(root);
				locomotionStateUpdater.Init(this);
				InitStateUpdaters(locomotionStateUpdater);
			}
		}
	}

	public void SetPosition(Vector3 position, bool forced)
	{
		if (!(trf.position == position) && !(position == Vector3.zero))
		{
			XYZ xYZ = XYZ.FromVector3(trf.position);
			XYZ xYZ2 = XYZ.FromVector3(position);
			if (xYZ == xYZ2)
			{
				VoxelTransform.MoveTransform(position);
			}
			else if (!forced && !ForcedPosition() && XYZ.MaxMagnitude(xYZ - xYZ2) > 1)
			{
				logger.InfoFormat("Locomotion4.SetPosition failed because {0} is moving multiple voxels at once, from {1} to {2}", base.name, trf.position.ToPreciseString(), position.ToPreciseString());
			}
			else
			{
				VoxelTransform.MoveTransform(position);
			}
		}
	}

	public void SetPosition(Vector3 position, Quaternion rotation)
	{
		trf.rotation = rotation;
		SetPosition(position, forced: false);
	}

	public bool GetFloorHeight(Vector3 pos, out float result)
	{
		result = pos.y;
		WalkablePathNode walkablePathNode = PathNodeRegistry.WalkableAt(XYZ.FromVector3(pos));
		if (walkablePathNode == null || walkablePathNode.IsEmpty)
		{
			return false;
		}
		XYZ representativePosition = walkablePathNode.RepresentativePosition;
		Vector3 vector = pos - representativePosition.ToVector3();
		float surfaceHeightDelta = walkablePathNode.GetSurfaceHeightDelta(vector.x, vector.z);
		result = FloorPicker.GetAdjustedHeight((float)representativePosition.y + surfaceHeightDelta, FloorOffset, representativePosition.y);
		return true;
	}

	public Vector3 GetNearestFreePosition(Vector3 pos)
	{
		return FloorPicker.GetNearestFreePosition(base.gameObject, pos, FloorOffset, avoidCharacterCollisions: true, 5);
	}

	public Vector3 GetNearestAligned(Vector3 pos)
	{
		FloorPicker.GetNearestAligned(pos, FloorOffset, out var aligned);
		return aligned;
	}

	private void ApplyAnimatorMotion()
	{
		ComputeAnimatorMotion();
		SetPosition(moveTo.position, moveTo.rotation);
	}

	private void ComputeAnimatorMotion()
	{
		moveTo.position = targetMatcher.ConsumePosition();
		moveTo.rotation = targetMatcher.ConsumeOrientation();
		CheckSnapToFloor(moveTo);
	}

	private void CheckSnapToFloor(Transform moveTrf)
	{
		if (AutoSnapToFloor)
		{
			if (!SnapToFloor(moveTrf))
			{
				logger.InfoFormat("ApplyAnimatorMotion failed because SnapToFloor failed: {0} from {1} to {2}", base.name, trf.position.ToPreciseString(), moveTrf.position.ToPreciseString());
				Vector3 position = moveTrf.position;
				PathBlockedAt(position);
			}
		}
		else
		{
			PopOutOfGround(moveTrf);
		}
	}

	private bool SnapToFloor(Transform moveTrf)
	{
		bool result = false;
		moveTrf.rotation = Quaternion.LookRotation(moveTrf.forward - Vector3.Project(moveTrf.forward, Vector3.up), Vector3.up);
		if (Common.Grounded.IsGrounded())
		{
			float result2;
			bool floorHeight = GetFloorHeight(moveTrf.position, out result2);
			if (floorHeight && IsSnappable(moveTrf.position.y, result2))
			{
				result = true;
			}
			else if (floorHeight)
			{
				logger.InfoFormat("SnapToFloor would make {0} teleport from {1} to {2}. Aborting.", base.name, moveTrf.position.ToPreciseString(), new Vector3(moveTrf.position.x, result2, moveTrf.position.z).ToPreciseString());
			}
			moveTrf.position = new Vector3(moveTrf.position.x, result2, moveTrf.position.z);
		}
		return result;
	}

	private bool IsSnappable(float yOrigin, float floorHeight)
	{
		return Mathf.RoundToInt(floorHeight) == Mathf.RoundToInt(yOrigin) || Mathf.Abs(floorHeight - yOrigin) <= 1.5f;
	}

	private void PopOutOfGround(Transform moveTrf)
	{
		if (FloorPicker.IsOccupiedOrInsideGround(base.gameObject, moveTrf.position, avoidCharacterCollisions: false))
		{
			float result;
			bool floorHeight = GetFloorHeight(moveTrf.position, out result);
			moveTrf.position = new Vector3(moveTrf.position.x, result, moveTrf.position.z);
			if (floorHeight && FloorPicker.IsOccupiedOrInsideGround(base.gameObject, moveTrf.position, avoidCharacterCollisions: false))
			{
				logger.WarnFormat("ApplyAnimatorMotion failed because floor is occupied: {0} from {1} to {2}", base.name, trf.position.ToPreciseString(), moveTrf.position.ToPreciseString());
				Vector3 position = moveTrf.position;
				moveTrf.position = trf.position;
				PathBlockedAt(position);
			}
		}
	}

	public bool CheckGrounded()
	{
		Vector3 position = trf.position;
		XYZ xYZ = XYZ.FromVector3(position);
		if (Voxel.IsNonEmpty(xYZ) || Voxel.IsNonEmpty(xYZ + XYZ.up))
		{
			return true;
		}
		if (CurrentPathNode != null)
		{
			return true;
		}
		float result;
		bool floorHeight = GetFloorHeight(position, out result);
		if (floorHeight && IsSnappable(position.y, result))
		{
			return true;
		}
		if (!floorHeight && Voxel.IsNonEmpty(xYZ - XYZ.up) && !NatureRegistry.HasNatureBlockerAt(xYZ - XYZ.up))
		{
			return true;
		}
		return false;
	}

	public bool IsInsideGround()
	{
		if (!CheckGrounded())
		{
			return false;
		}
		if (!GetFloorHeight(trf.position, out var result))
		{
			return false;
		}
		return trf.position.y <= result + FloorOffset;
	}

	public bool CheckObstacle()
	{
		return PopOuterOfObstacle != null && PopOuterOfObstacle.CheckObstacle();
	}

	public bool IsFlailing()
	{
		return base.transform.parent == FlailingCollider.transform;
	}

	private void HandleEvent(Brix.Game.AI.Behaviors.AnimationEvent ev)
	{
		if (!string.IsNullOrEmpty(ev.SoundEvent) && IsVisible() && IsCloseToCamera())
		{
			if (ev.IsSwitchingOnSurface)
			{
				SetSurfaceSwitch(ev);
			}
			else if (ev.SoundSwitchName != null)
			{
				SoundEngine.SetSwitch(ev.SoundSwitchName, ev.SoundSwitchValue, base.gameObject);
			}
			string text = ev.SoundEvent;
			if (ev.IsBricktronSoundEffect)
			{
				text = Sounds.SoundsByName[ev.SoundEvent];
			}
			SoundEngine.Play(text, base.gameObject);
		}
	}

	private void SetSurfaceSwitch(Brix.Game.AI.Behaviors.AnimationEvent ev)
	{
		if (ev.SoundSwitchName == null)
		{
			logger.WarnFormat("AnimationEvent {0} is using IsSwitchingOnSurface but has no SoundSwitchName set, ignoring. Please fix.", ev.name);
			return;
		}
		XYZ position = XYZ.FromVector3(trf.position);
		if (Voxel.IsEmpty(position))
		{
			position.y--;
			if (Voxel.IsEmpty(position))
			{
				return;
			}
		}
		string surfaceName = Voxel.GetSurfaceName(position);
		SoundEngine.SetSwitch(ev.SoundSwitchName, surfaceName, base.gameObject);
	}

	private void OnDrawGizmos()
	{
		if (DrawGizmos && PathIterator != null && PathIterator.HasCurrent)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(PathIterator.Current.Position + new Vector3(0f, 0.3f, 0f), 0.2f);
			Gizmos.DrawLine(PathIterator.Current.Position, PathIterator.Current.Position + PathIterator.Current.Orientation * Vector3.forward);
			if (PathAnticipationTime > 0f)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawCube(PathIterator.GetPositionInTimeForSpeed(trf.position, PathAnticipationTime, Movement.Running.RunSpeedMperS), Vector3.one * 0.5f);
			}
		}
	}

	[LuaDrivable("RandomTreeAroundVoxel")]
	public static DynValue lua_RandomTreeAroundVoxel(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "RandomTreeAroundVoxel");
		Vector3 position = gameObject.transform.position;
		float radius = 50f;
		Func<XYZ, bool> acceptFunc = (XYZ p) => TreeRegistry.Instance.IsNearATree(p);
		Location location = Location.RandomAroundObjectWithPredicate(XYZ.FromVector3(position), radius, PathBranch.BranchType.Forest, acceptFunc);
		DynValue value = DynValue.NewNumber(location.Position.x);
		DynValue value2 = DynValue.NewNumber(location.Position.y);
		DynValue value3 = DynValue.NewNumber(location.Position.z);
		return DynValue.NewTuple(value, value2, value3);
	}

	static Locomotion4()
	{
		logger = new BrixLogger(typeof(Locomotion4).FullName);
		UpdatableProfiler = new UpdatableNetworkProfiler<Locomotion4>();
		kRpcRpcDie = 1924097307;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Locomotion4), kRpcRpcDie, InvokeRpcRpcDie);
		NetworkCRC.RegisterBehaviour("Locomotion4", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcDie(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDie called on server.");
		}
		else
		{
			((Locomotion4)obj).RpcDie(GeneratedNetworkCode._ReadImpact_Locomotion4(reader));
		}
	}

	public void CallRpcDie(Impact i)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDie called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDie);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteImpact_Locomotion4(networkWriter, i);
		SendRPCInternal(networkWriter, 0, "RpcDie");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
