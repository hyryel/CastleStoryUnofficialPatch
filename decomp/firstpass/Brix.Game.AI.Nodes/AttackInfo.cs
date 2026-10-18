using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Game.Components.Volumes;
using Brix.Game.Components.Volumes.Requests;
using Brix.Game.Rules;
using Brix.Legacy;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

[Serializable]
public class AttackInfo : IPoolable, IResetable, IDisposable
{
	public enum AttackDestinationBehaviour
	{
		DontPrioritizeDestination,
		PrioritizeDestination,
		MustAttackDestination
	}

	private static class Utility<T>
	{
		public static StaticNode ResetThenWork(AttackInfo info, Labor attacker, T arg, Action<AttackInfo, T> init, Action<AttackInfo> cleanup)
		{
			return Node.Sequence.Do(delegate(AttackInfo self, Labor _attacker)
			{
				self.Reset(_attacker);
			}, info, attacker).Do((AttackInfo self, T _args, Action<AttackInfo, T> _init, Action<AttackInfo> _cleanup) => self.Work(_args, _init, _cleanup), info, arg, init, cleanup).Finally(delegate(AttackInfo self)
			{
				self.Dispose();
			}, info);
		}

		public static Node Work(AttackInfo info, T args, Action<AttackInfo, T> init, Action<AttackInfo> cleanup)
		{
			return (StaticNode)Node.Sequence.Do(init, info, args).Do((AttackInfo self) => self.MainLoop(), info).Finally(delegate(AttackInfo self, Action<AttackInfo> _cleanup)
			{
				_cleanup(self);
			}, info, cleanup);
		}
	}

	public class SearchMeleeTarget : IPoolable, IResetable, IDisposable
	{
		public PathQueue PathQueue;

		public MaxHorizontalDistanceBranchEvaluator PathEvaluator;

		public PathBuilder PathBuilder;

		public EnemyAccepter PathAccepter;

		public PathSearch PathSearch;

		public PathLink PathStart;

		public void Init(AttackInfo attack)
		{
			int distance = ((!attack.CanMove) ? 2 : attack.MeleeSearchRadius);
			if (PathSearch == null)
			{
				PathStart = new PathLink(attack.Presence.CurrentPathNode);
				PathQueue = new PathBreadthFirstQueue();
				PathEvaluator = new MaxHorizontalDistanceBranchEvaluator(attack.Presence.PositionOccupied, distance, attack.PresenceTestFunc);
				PathBuilder = new PathDirectBuilder(TestPathfinding.Instance.terrainLod, PathEvaluator);
				PathAccepter = new EnemyAccepter(attack);
				PathSearch = new PathSearch(PathQueue, PathBuilder, PathAccepter);
			}
			else
			{
				PathStart.Reset(attack.Presence.CurrentPathNode);
				PathQueue.Clear();
				PathEvaluator.Reset(attack.Presence.PositionOccupied, distance, attack.PresenceTestFunc);
				PathAccepter.Reset(attack);
				PathSearch.Clear();
			}
		}

		public static SearchMeleeTarget Get(AttackInfo attack)
		{
			SearchMeleeTarget searchMeleeTarget = ObjectPool<SearchMeleeTarget>.Reserve();
			searchMeleeTarget.Init(attack);
			return searchMeleeTarget;
		}

		public void Dispose()
		{
			ObjectPool<SearchMeleeTarget>.Release(this);
		}

		public void ResetObject()
		{
		}
	}

	public class EnemyAccepter : PathAccepter
	{
		public AttackInfo Attack;

		public IVolume Enemy;

		public PathLink Result;

		public XYZ BestTree;

		public EnemyAccepter(AttackInfo attack)
		{
			Attack = attack;
		}

		public void Reset(AttackInfo attack)
		{
			Attack = attack;
			Enemy = null;
			Result = null;
		}

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			bool result = false;
			if (Attack.HasEnemiesAt(link.head.Node.RepresentativePosition))
			{
				MultiPropertyRequest<AttackInfo>.Enumerator enumerator = Attack.GetEnemiesAt(link.head.Node.RepresentativePosition).GetEnumerator();
				while (enumerator.MoveNext())
				{
					IVolume current = enumerator.Current;
					if (_Visit(link, current))
					{
						result = true;
						break;
					}
				}
				enumerator.Dispose();
			}
			return result;
		}

		private bool _Visit(PathLink link, IVolume v)
		{
			if (v == null || v.GameObject == null || IsInvincible(v.GameObject))
			{
				return false;
			}
			if (v.GameObject == Attack.DestinationTarget)
			{
				Enemy = v;
				Result = link;
				return true;
			}
			if (Attack.AttackDestination == AttackDestinationBehaviour.MustAttackDestination)
			{
				Enemy = null;
				Result = null;
				return false;
			}
			CombatPriorityType combatPriority = v.DamageReceiver.CombatPriority;
			if (combatPriority >= CombatPriorityType.AttackOnSight)
			{
				Enemy = v;
				Result = link;
				return true;
			}
			if (combatPriority >= CombatPriorityType.AttackHighPriority && (Enemy == null || Enemy.DamageReceiver == null || combatPriority > Enemy.DamageReceiver.CombatPriority))
			{
				Enemy = v;
				Result = link;
				return false;
			}
			if (IsBlockingPath(v))
			{
				Enemy = v;
				Result = link;
				return false;
			}
			if (Attack.AttackDestination == AttackDestinationBehaviour.PrioritizeDestination && Attack.DestinationTarget != null)
			{
				return false;
			}
			if (combatPriority >= CombatPriorityType.AttackLowPriority && Enemy == null && LocalFactionVision.ObjectIsVisibleForFaction(v.GameObject, Attack.Attacker.faction))
			{
				Enemy = v;
				Result = link;
				return false;
			}
			return false;
		}

		private bool IsBlockingPath(IVolume v)
		{
			if (v.PassThrough.CanSearchThrough(Attack.Attacker.GameObject) && !v.PassThrough.CanPassThrough(Attack.Attacker.GameObject))
			{
				return v.HasPropertyAt(VProperty.access, Attack.Attacker.presence.PositionOccupied);
			}
			return false;
		}
	}

	public class MaxHorizontalDistanceBranchEvaluator : SearchPathRequest.BranchEvaluator
	{
		public XYZ Origin;

		public int Distance2;

		public MaxHorizontalDistanceBranchEvaluator(XYZ origin, int distance, Func<XYZ, bool> presenceTest, bool canClimb = true, bool canWalkInForest = false, bool canBreakVoxels = false)
			: base(presenceTest, canClimb, canWalkInForest, canBreakVoxels)
		{
			Distance2 = distance * distance;
			Origin = origin;
		}

		public void Reset(XYZ origin, int distance, Func<XYZ, bool> presenceTest, bool canClimb = true, bool canWalkInForest = false, bool canBreakVoxels = false)
		{
			Reset(presenceTest, canClimb, canWalkInForest, canBreakVoxels);
			Distance2 = distance * distance;
			Origin = origin;
		}

		public override bool CanCross(PathBranch branch)
		{
			XYZ representativePosition = branch.Node.RepresentativePosition;
			int num = representativePosition.x - Origin.x;
			int num2 = representativePosition.z - Origin.z;
			return num * num + num2 * num2 <= Distance2 && base.CanCross(branch);
		}
	}

	public bool CanMove = true;

	public bool CanAttackMelee = true;

	public bool CanAttackRange = true;

	public bool CanMoveAttack = true;

	public bool CanRun = true;

	public bool KeepWaiting;

	public bool PreciseSearch = true;

	public AttackDestinationBehaviour AttackDestination;

	protected int MeleeSearchRadius = 3;

	protected int MeleeSearchLimit = 30;

	public Labor Attacker;

	public Location Destination;

	public int CurrentFrame;

	public int NextAttackFrame;

	public int NextMeleeTargetFrame;

	public int NextRangedTargetFrame;

	public int NoActionEndFrame;

	public GameObject MeleeTarget;

	public GameObject RangedTarget;

	public bool Active;

	public PathBranch StepBranch;

	public PathBranch NextBranch;

	public PathBranch ProjectedBranch;

	public PathBranch NextProjectedBranch;

	public WalkNode StepLocation;

	public WalkNode NextLocation;

	public WalkNode ProjectedLocation;

	public WalkNode NextProjectedLocation;

	public XYZ Step;

	public XYZ NextStep;

	public XYZ ProjectedStep;

	public XYZ NextProjectedStep;

	public bool WantStep;

	public bool WantStrike;

	public bool WantRanged;

	public XYZ WantStrikeFrom;

	public XYZ WantStrikeTo;

	public int IsPreparingToStrike;

	public bool Stepping;

	public bool Turning;

	public bool Halting;

	public bool Striking;

	public bool Shooting;

	public bool Waiting;

	public bool Fighting;

	public bool IsGrounded;

	public bool InCombatStance;

	public bool IsReadyToStrike;

	public bool InArcheryStance;

	public bool IsReadyToShoot;

	public bool RunningToDestination;

	protected readonly ObjectLocation TargetObjectLocation = new ObjectLocation();

	protected readonly CharacterLocation TargetCharacterLocation = new CharacterLocation();

	protected readonly Func<XYZ, bool> PresenceTestFunc;

	protected readonly Path SteppingPath = new Path(new List<WalkNode>());

	private const int RangedTargetRefreshDelay = 100;

	private const int RangedNoTargetRefreshDelay = 10;

	private const int MeleeTargetRefreshDelay = 5;

	private const int MeleeNoTargetRefreshDelay = 5;

	public GameObject GameObject => Attacker.gameObject;

	public Transform Transform => Attacker.transform;

	public Navigation Navigation => Attacker.navigation;

	public Locomotion4 Locomotion => Navigation.Locomotion;

	public Profession Profession => Attacker.Occupation.Profession;

	public AffiliationComponent Affiliation => Attacker.affiliation;

	public EnemyTracker EnemyTracker => Attacker.GetComponent<EnemyTracker>();

	public VoxelPresence Presence => Attacker.presence;

	public Animator Animator => Locomotion.animator;

	public Signal<GameObject> ChangeTarget => Attacker.signal.ChangeTarget;

	public ProjectileWeapon ProjectileWeapon
	{
		get
		{
			Profession.ProjectileAttackAvailable(out var weapon);
			return weapon;
		}
	}

	public GameObject DestinationTarget => (Destination == null) ? null : Destination.Object;

	public XYZ PositionEnteringOrOccupied => GetMeleeEnteringPosition(Presence, CanMove && CanMoveAttack);

	public XYZ TargetEnteringOrOccupied
	{
		get
		{
			VoxelPresence component = MeleeTarget.GetComponent<VoxelPresence>();
			return GetEnteringOrOccupied(component);
		}
	}

	public AttackInfo()
	{
		PresenceTestFunc = PresenceTest;
	}

	private XYZ GetEnteringOrOccupied(VoxelPresence presence)
	{
		XYZ positionEntering = presence.PositionEntering;
		if (positionEntering != XYZ.zero)
		{
			return positionEntering;
		}
		return presence.PositionOccupied;
	}

	private XYZ GetMeleeEnteringPosition(VoxelPresence presence, bool canMove)
	{
		XYZ positionOccupied = presence.PositionOccupied;
		if (!canMove)
		{
			return positionOccupied;
		}
		XYZ enteringOrOccupied = GetEnteringOrOccupied(presence);
		if (enteringOrOccupied == positionOccupied)
		{
			return positionOccupied;
		}
		if (!IsEnterableInMelee(positionOccupied, enteringOrOccupied))
		{
			return positionOccupied;
		}
		return enteringOrOccupied;
	}

	private bool IsEnterableInMelee(XYZ occupied, XYZ entering)
	{
		return PathNodeRegistry.GetBranches(occupied).Slinq().Any((PathBranch branch, XYZ _entering) => branch.Node.RepresentativePosition == _entering && branch.IsFull, entering);
	}

	private Location GetTemporaryLocationOf(GameObject target)
	{
		Labor component = target.GetComponent<Labor>();
		if (component != null)
		{
			TargetCharacterLocation.Reset(component);
			return TargetCharacterLocation;
		}
		TargetObjectLocation.Object = MeleeTarget;
		return TargetObjectLocation;
	}

	public void ResetObject()
	{
		CanMove = true;
		CanAttackMelee = true;
		CanAttackRange = true;
		CanMoveAttack = true;
		CanRun = true;
		KeepWaiting = false;
		PreciseSearch = true;
		AttackDestination = AttackDestinationBehaviour.DontPrioritizeDestination;
		MeleeSearchRadius = 3;
		MeleeSearchLimit = 30;
		Attacker = null;
		Destination = null;
		CurrentFrame = 0;
		NextAttackFrame = 0;
		NextMeleeTargetFrame = 0;
		NextRangedTargetFrame = 0;
		NoActionEndFrame = 0;
		MeleeTarget = null;
		RangedTarget = null;
		Active = false;
		Stepping = (Turning = (Halting = (Striking = (Shooting = false))));
		Waiting = (Fighting = (IsGrounded = (InCombatStance = false)));
		IsReadyToStrike = (InArcheryStance = (IsReadyToShoot = (RunningToDestination = false)));
	}

	public void Dispose()
	{
		ResetObject();
	}

	public StaticNode GoAttack(Labor labor, GameObject target)
	{
		return ResetThenWork(labor, target, delegate(AttackInfo self, GameObject _target)
		{
			if (self.Attacker.Profession.CanMakeWay())
			{
				self.PreciseSearch = false;
			}
			self.Destination = Location.Of(_target);
		}, delegate
		{
		});
	}

	public StaticNode GoAttackWithPriority(Labor labor, GameObject target)
	{
		return ResetThenWork(labor, target, delegate(AttackInfo self, GameObject _target)
		{
			self.Destination = Location.Of(_target);
			self.AttackDestination = AttackDestinationBehaviour.PrioritizeDestination;
		}, delegate
		{
		});
	}

	public StaticNode GoAttackDirectly(Labor labor, GameObject target)
	{
		return ResetThenWork(labor, target, delegate(AttackInfo self, GameObject _target)
		{
			self.Destination = Location.Of(_target);
			self.AttackDestination = AttackDestinationBehaviour.MustAttackDestination;
			self.ChangeTarget.Invoke(_target);
		}, delegate(AttackInfo self)
		{
			self.ChangeTarget.Invoke(null);
		});
	}

	public StaticNode HoldPositionAndAttackForever(Labor labor)
	{
		return ResetThenWork(labor, delegate(AttackInfo self)
		{
			self.CanMove = false;
			self.KeepWaiting = true;
		}, delegate
		{
		});
	}

	public StaticNode HoldPositionAndAttack(Labor labor)
	{
		return ResetThenWork(labor, delegate(AttackInfo self)
		{
			self.CanMove = false;
		}, delegate
		{
		});
	}

	public StaticNode HoldPositionAndAttackDirectly(Labor labor, GameObject target)
	{
		return ResetThenWork(labor, target, delegate(AttackInfo self, GameObject _target)
		{
			self.Destination = Location.Of(_target);
			self.CanMove = false;
			self.AttackDestination = AttackDestinationBehaviour.MustAttackDestination;
			self.ChangeTarget.Invoke(_target);
		}, delegate(AttackInfo self)
		{
			self.ChangeTarget.Invoke(null);
		});
	}

	public StaticNode HoldPositionAndAttackWithPriority(Labor labor, GameObject target)
	{
		return ResetThenWork(labor, target, delegate(AttackInfo self, GameObject _target)
		{
			self.Destination = Location.Of(_target);
			self.CanMove = false;
			self.AttackDestination = AttackDestinationBehaviour.PrioritizeDestination;
			self.ChangeTarget.Invoke(_target);
		}, delegate(AttackInfo self)
		{
			self.ChangeTarget.Invoke(null);
		});
	}

	public StaticNode MoveAttack(Labor labor, Location location)
	{
		return ResetThenWork(labor, location, delegate(AttackInfo self, Location _location)
		{
			self.Destination = _location;
			self.MeleeSearchRadius = 10;
			self.MeleeSearchLimit = 200;
		}, delegate
		{
		});
	}

	public StaticNode WalkMoveAttack(Labor labor, Location location)
	{
		return ResetThenWork(labor, location, delegate(AttackInfo self, Location _location)
		{
			self.Destination = _location;
			self.CanRun = false;
			self.MeleeSearchRadius = 10;
			self.MeleeSearchLimit = 200;
		}, delegate
		{
		});
	}

	private void Reset(Labor attacker)
	{
		ResetObject();
		Attacker = attacker;
		attacker.Activity = Activity.Fighting;
	}

	private void SpotEnemy()
	{
		if (Destination is CharacterLocation)
		{
			SoundEngine.Play(Attacker.Sounds.SpotEnemy, Destination.Object);
		}
	}

	private StaticNode ResetThenWork(Labor attacker, Action<AttackInfo> init, Action<AttackInfo> cleanup)
	{
		return Node.Sequence.Do(delegate(AttackInfo self, Labor _attacker)
		{
			self.Reset(_attacker);
		}, this, attacker).Do((AttackInfo self, Action<AttackInfo> _init, Action<AttackInfo> _cleanup) => self.Work(_init, _cleanup), this, init, cleanup).Finally(delegate(AttackInfo self)
		{
			self.Dispose();
		}, this);
	}

	private StaticNode ResetThenWork<T>(Labor attacker, T arg, Action<AttackInfo, T> init, Action<AttackInfo> cleanup)
	{
		return Utility<T>.ResetThenWork(this, attacker, arg, init, cleanup);
	}

	private Node Work(Action<AttackInfo> init, Action<AttackInfo> cleanup)
	{
		return (StaticNode)Node.Sequence.Do(init, this).Do((AttackInfo self) => self.MainLoop(), this).Finally(delegate(AttackInfo self, Action<AttackInfo> _cleanup)
		{
			_cleanup(self);
		}, this, cleanup);
	}

	private Node Work<T>(T args, Action<AttackInfo, T> init, Action<AttackInfo> cleanup)
	{
		return Utility<T>.Work(this, args, init, cleanup);
	}

	private Node MainLoop()
	{
		return (StaticNode)Node.Sequence.Do((AttackInfo self) => self.Active = true, this).Do(Node.Selector.When((AttackInfo self) => self.Active, this).Do((AttackInfo self) => self.Attacker.Drop(), this).Do(delegate(AttackInfo self)
		{
			self.SpotEnemy();
		}, this)
			.Do(delegate(AttackInfo self)
			{
				self.FindBestTarget(force: true);
			}, this)
			.Label("FindBestTarget")
			.Do(Node.Parallel.Do((AttackInfo self) => self.ActionLoop(), this).Label("ActionLoop").Do((AttackInfo self) => self.ObservationLoop(), this)
				.Label("ObservationLoop"))
			.When(() => true)
			.Do(delegate
			{
			}));
	}

	private Node ActionLoop()
	{
		return (StaticNode)Node.Selector.When((AttackInfo self) => self.HasCombatAction(), this).Do((AttackInfo self) => self.DoCombatAction(), this).Label("DoCombatAction")
			.When((AttackInfo self) => self.HasRunDestination(), this)
			.Do(delegate(AttackInfo self)
			{
				self.StopInactiveActions();
			}, this)
			.Label("StopInactiveActions")
			.Do((AttackInfo self) => self.RunTowardDestination(), this)
			.Label("RunTowardDestination")
			.Do(Node.WaitForTicks(50))
			.Label("Wait for at least 1 second")
			.Do((AttackInfo self) => self.WaitForever(), this)
			.Label("WaitForever")
			.Do((AttackInfo self) => self.Active = false, this)
			.Label("Active = false")
			.When(() => true)
			.Do((AttackInfo self) => self.WaitForever(), this)
			.Label("WaitForever")
			.Do((AttackInfo self) => self.Active = false, this)
			.Label("Active = false")
			.Loop();
	}

	private Node ObservationLoop()
	{
		return (StaticNode)Node.Sequence.Do(delegate(AttackInfo self)
		{
			self.FindBestTarget(force: false);
		}, this).Label("FindBestTarget").Do(delegate(AttackInfo self)
		{
			self.NextFrame();
		}, this)
			.Label("NextFrame")
			.Loop();
	}

	private Node WaitForever()
	{
		return Node.Sequence.Do((AttackInfo self) => self.Waiting = true, this).Do(Node.WaitForCondition((AttackInfo self) => !self.MustWait(), this)).Label("Wait forever")
			.Finally(delegate(AttackInfo self)
			{
				self.Waiting = false;
			}, this)
			.ToNode;
	}

	private bool MustWait()
	{
		return KeepWaiting || (CanMove && AttackDestination == AttackDestinationBehaviour.MustAttackDestination && DestinationTarget != null);
	}

	private bool HasCombatAction()
	{
		if (MeleeTarget != null || RangedTarget != null)
		{
			NoActionEndFrame = 0;
			return true;
		}
		if (NoActionEndFrame == 0)
		{
			NoActionEndFrame = CurrentFrame + Mathf.Max(5, 10) + 1;
		}
		return CurrentFrame < NoActionEndFrame;
	}

	private bool HasRunDestination()
	{
		if (!CanMove)
		{
			return false;
		}
		if (Destination == null)
		{
			return false;
		}
		if (Destination is SpaceLocation)
		{
			return true;
		}
		if (Destination.Object == null)
		{
			return false;
		}
		if (Attacker.Autonomy.ListenToRangeInstinct)
		{
			return true;
		}
		if (AttackDestination != AttackDestinationBehaviour.MustAttackDestination)
		{
			return false;
		}
		return true;
	}

	private bool CanCombatStep()
	{
		if (!CanMove)
		{
			return false;
		}
		if (Attacker.Autonomy.ListenToRangeInstinct)
		{
			return true;
		}
		if (Destination == null || Destination is SpaceLocation)
		{
			return true;
		}
		if (AttackDestination != AttackDestinationBehaviour.MustAttackDestination)
		{
			return false;
		}
		return true;
	}

	private Node DoCombatAction()
	{
		return (StaticNode)Node.Sequence.Do((AttackInfo self) => self.Fighting = true, this).Do((AttackInfo self) => self.CombatActionLoop(), this).Label("CombatActionLoop")
			.Finally(delegate(AttackInfo self)
			{
				self.Fighting = false;
			}, this);
	}

	private Node CombatActionLoop()
	{
		return (StaticNode)Node.Selector.If((AttackInfo self) => self.MeleeTarget != null && (self.RangedTarget == null || self.PrefersMeleeAction()), this).Do(delegate(AttackInfo self)
		{
			self.FindBestMeleeAction();
		}, this).Label("FindBestMeleeAction")
			.Do(delegate(AttackInfo self)
			{
				self.StopInactiveRangedAction();
			}, this)
			.Label("StopInactiveRangedAction")
			.Do(delegate(AttackInfo self)
			{
				self.DoMeleeAction();
			}, this)
			.Label("DoMeleeAction")
			.If((AttackInfo self) => self.RangedTarget != null, this)
			.Do(delegate(AttackInfo self)
			{
				self.StopInactiveMeleeAction();
			}, this)
			.Label("StopInactiveMeleeAction")
			.Do((AttackInfo self) => self.DoRangedAction(), this)
			.Label("DoRangedAction")
			.Loop();
	}

	private void StopInactiveActions()
	{
		if (!Fighting)
		{
			StopInactiveMeleeAction();
			StopInactiveRangedAction();
		}
	}

	private bool PrefersMeleeAction()
	{
		FindBestMeleeAction();
		return WantStrike;
	}

	private void FindBestMeleeAction()
	{
		WantStep = (WantStrike = false);
		if (MeleeTarget == null)
		{
			return;
		}
		XYZ positionEnteringOrOccupied = PositionEnteringOrOccupied;
		Location temporaryLocationOf = GetTemporaryLocationOf(MeleeTarget);
		if (temporaryLocationOf is CharacterLocation characterLocation)
		{
			XYZ targetEnteringOrOccupied = TargetEnteringOrOccupied;
			if (characterLocation.TargetReachedOrAcceptable(GameObject, Presence.PositionOccupied, targetEnteringOrOccupied))
			{
				WantStrike = true;
				WantStrikeFrom = Presence.PositionOccupied;
				WantStrikeTo = targetEnteringOrOccupied;
			}
			else if (characterLocation.TargetReachedOrAcceptable(GameObject, positionEnteringOrOccupied, targetEnteringOrOccupied))
			{
				WantStrike = true;
				WantStrikeFrom = positionEnteringOrOccupied;
				WantStrikeTo = targetEnteringOrOccupied;
			}
			else if (!StepBranch.IsEmpty && !StepBranch.Node.IsEmpty && StepBranch.Node.RepresentativePosition != positionEnteringOrOccupied)
			{
				WantStep = true;
			}
		}
		else if (temporaryLocationOf.TargetReached(GameObject, positionEnteringOrOccupied.ToVector3()))
		{
			WantStrike = true;
			WantStrikeFrom = positionEnteringOrOccupied;
			WantStrikeTo = XYZ.FromVector3(temporaryLocationOf.Position);
		}
		else if (!StepBranch.IsEmpty && !StepBranch.Node.IsEmpty && StepBranch.Node.RepresentativePosition != positionEnteringOrOccupied)
		{
			WantStep = true;
		}
	}

	private void DoMeleeAction()
	{
		if (MeleeTarget == null)
		{
			return;
		}
		IsGrounded = Locomotion.IsInGroundedState();
		InCombatStance = Locomotion.InCombatStance;
		IsReadyToStrike = Locomotion.IsReadyToStrike;
		Halting = Locomotion.IsHalting;
		Turning = Locomotion.Fighting.TurningTo.IsTurning;
		Striking = Locomotion.Fighting.Striking.IsCranking || Locomotion.Fighting.Striking.IsSwinging || Locomotion.Fighting.Striking.IsReacting;
		if (Striking)
		{
			IsPreparingToStrike = 0;
		}
		else if (IsPreparingToStrike > 0)
		{
			IsPreparingToStrike--;
		}
		if (!IsGrounded)
		{
			return;
		}
		if (WantStrike || Striking || IsPreparingToStrike > 0)
		{
			if (IsReadyToStrike && CurrentFrame >= NextAttackFrame)
			{
				IsPreparingToStrike = 10;
				XYZ at = (WantStrikeFrom + WantStrikeTo) / 2;
				if (at.Flatten() == WantStrikeFrom.Flatten() || at.Flatten() == WantStrikeTo.Flatten() || PassThroughDoor(at))
				{
					Strike();
				}
			}
			else if (!InCombatStance)
			{
				IsPreparingToStrike = 10;
				Locomotion.Wield(null);
				Locomotion.ClearPath();
			}
		}
		else
		{
			if (!WantStep)
			{
				return;
			}
			if (!CanCombatStep())
			{
				if (!InCombatStance)
				{
					Locomotion.Wield(null);
				}
				else if (!Striking && !IsInPreferredOrientation(Location.Of(MeleeTarget)))
				{
					Locomotion.DoCombatTurnTo(Location.Of(MeleeTarget));
				}
			}
			else if (InCombatStance)
			{
				Locomotion.GetReadyToMove();
			}
			else if (StepLocation != null)
			{
				List<WalkNode> nodes = SteppingPath.nodes;
				nodes.Clear();
				if (StepLocation != null)
				{
					nodes.Add(StepLocation);
				}
				if (NextLocation != null)
				{
					nodes.Add(NextLocation);
				}
				if (ProjectedLocation != null)
				{
					nodes.Add(ProjectedLocation);
				}
				if (NextProjectedLocation != null)
				{
					nodes.Add(NextProjectedLocation);
				}
				Locomotion.DoFollowStrikePath(SteppingPath);
			}
		}
	}

	private bool PassThroughDoor(XYZ at)
	{
		if (Attacker.Defending)
		{
			return !IsClosedDoor(at);
		}
		return OpenDoor(at);
	}

	private bool IsClosedDoor(XYZ at)
	{
		IVolume volume = VolumeRegistry.VolumeWithPropertyAt(at, VProperty.obstacle, (IVolume v) => v.PassThrough is Porte);
		if (volume != null)
		{
			Porte porte = volume.PassThrough as Porte;
			return porte.IsClosed();
		}
		return false;
	}

	private bool OpenDoor(XYZ at)
	{
		IVolume volume = VolumeRegistry.VolumeWithPropertyAt(at, VProperty.obstacle, (IVolume v) => v.PassThrough is Porte);
		if (volume != null)
		{
			Porte porte = volume.PassThrough as Porte;
			if (porte.CanPassThrough(GameObject))
			{
				porte.PassageEngage.Invoke(Attacker);
				return true;
			}
			return false;
		}
		return true;
	}

	private void StopInactiveMeleeAction()
	{
		InCombatStance = Locomotion.InCombatStance;
		if (InCombatStance)
		{
			Locomotion.StopCombatStance();
		}
	}

	private IEnumerator<ActivityProgress> DoRangedAction()
	{
		if (RangedTarget == null)
		{
			yield return ActivityProgress.Fail(SharedWorkException<Exceptions.CombatTargetIsDead>.Get);
		}
		IsGrounded = Locomotion.IsInGroundedState();
		InArcheryStance = Locomotion.IsInArcheryStance;
		IsReadyToShoot = Locomotion.IsReadyToShoot;
		if (!IsGrounded)
		{
			yield return ActivityProgress.Fail(SharedWorkException<Exceptions.CombatNotGrounded>.Get);
		}
		if (!InArcheryStance)
		{
			InCombatStance = Locomotion.InCombatStance;
			if (InCombatStance)
			{
				Locomotion.StopCombatStance();
			}
			else if (!IsInPreferredOrientation(Location.Of(RangedTarget)))
			{
				Locomotion.TurnTo(RangedTarget.transform.position);
			}
			else if (Locomotion.PathIterator.Active)
			{
				Locomotion.ClearPath();
			}
			else
			{
				Locomotion.WieldBow(ProjectileWeapon);
			}
		}
		else if (IsReadyToShoot && CurrentFrame >= NextAttackFrame)
		{
			Location location = Location.Of(RangedTarget);
			if (!Locomotion.GetComponent<Profession>().CanFireAt(location))
			{
				yield return ActivityProgress.Fail(SharedWorkException<Exceptions.CombatTargetIsTooFarAway>.Get);
			}
			Locomotion.Aiming.SetupFire(location, ProjectileWeapon, Profession.ProjectileSpeedMultiplier, Profession.AvoidsFriendlyFire);
			Locomotion.ShootAt(location, CombatRules.ArrowAimTime, CombatRules.ArrowInaccuracy);
			NextAttackFrame = CurrentFrame + 10;
		}
	}

	private void StopInactiveRangedAction()
	{
		InArcheryStance = Locomotion.IsInArcheryStance;
		if (InArcheryStance)
		{
			Locomotion.StopArcheryStance();
		}
	}

	private void NextFrame()
	{
		CurrentFrame++;
	}

	private void Strike()
	{
		Locomotion.DoStrike(MeleeTarget.GetComponent<DamageReceiver>(), WantStrikeFrom.ToVector3());
		NextAttackFrame = CurrentFrame + Attacker.Profession.RandomizedAttackCooldown;
	}

	private Node RunTowardDestination()
	{
		return (StaticNode)Node.Sequence.Do((AttackInfo self) => self.RunningToDestination = true, this).Do((AttackInfo self) => (!self.CanRun) ? self.Attacker.WalkToward(self.Destination, self.PreciseSearch, self.AttackDestination != AttackDestinationBehaviour.MustAttackDestination) : self.Attacker.RunToward(self.Destination, self.PreciseSearch, self.AttackDestination != AttackDestinationBehaviour.MustAttackDestination), this).Label("Navigate to target")
			.Finally(delegate(AttackInfo self)
			{
				self.RunningToDestination = false;
			}, this);
	}

	private void FindBestTarget(bool force)
	{
		if (Active)
		{
			CanAttackMelee = Profession.HasMeleeAttack() && Locomotion.Interaction.CanStrike();
			CanAttackRange = Profession.ProjectileAttackAvailable();
			CanMoveAttack = Profession.CanMoveAttack();
			if (!CanAttackMelee)
			{
				MeleeTarget = null;
			}
			else if ((CurrentFrame >= NextMeleeTargetFrame || force) && (!(MeleeTarget != null) || !WantStrike || !(MeleeTarget.GetComponent<Labor>() != null)))
			{
				FindBestMeleeTarget();
				int num = ((!(MeleeTarget != null)) ? 5 : 5);
				NextMeleeTargetFrame = CurrentFrame + num;
			}
			if (!CanAttackRange)
			{
				RangedTarget = null;
			}
			else if (CurrentFrame >= NextRangedTargetFrame || force)
			{
				FindBestRangedTarget();
				int num2 = ((RangedTarget != null) ? 100 : ((AttackDestination != AttackDestinationBehaviour.MustAttackDestination) ? 10 : 0));
				NextRangedTargetFrame = CurrentFrame + num2;
			}
		}
	}

	private void FindBestRangedTarget()
	{
		RangedTarget = null;
		if (DestinationTarget != null && Attacker.IsCloseEnemy(DestinationTarget) && CanFireAt(DestinationTarget))
		{
			RangedTarget = DestinationTarget;
		}
		else
		{
			if (AttackDestination == AttackDestinationBehaviour.MustAttackDestination)
			{
				return;
			}
			int aggroDistance = Attacker.GetAggroDistance(applyInstinctConstraint: false);
			IGameComponent closestEnemy = Attacker.enemyTracker.GetClosestEnemy((float)aggroDistance / 4f, (IGameComponent c, AttackInfo self) => self.CanFireAt(c.GameObject), this, TrackingLayer.Character);
			if (closestEnemy == null)
			{
				closestEnemy = Attacker.enemyTracker.GetClosestEnemy((float)aggroDistance / 2f, (IGameComponent c, AttackInfo self) => self.CanFireAt(c.GameObject), this, TrackingLayer.Character);
				if (closestEnemy == null)
				{
					closestEnemy = Attacker.enemyTracker.GetClosestEnemy(aggroDistance, (IGameComponent c, AttackInfo self) => self.CanFireAt(c.GameObject), this, TrackingLayer.Character);
					if (closestEnemy == null)
					{
						closestEnemy = Attacker.enemyTracker.GetClosestEnemy((float)aggroDistance / 2f, (IGameComponent c, AttackInfo self) => self.CanFireAt(c.GameObject), this, TrackingLayer.Crystal);
						if (closestEnemy == null)
						{
							return;
						}
					}
				}
			}
			RangedTarget = closestEnemy.GameObject;
		}
	}

	private bool CanFireAt(GameObject enemy)
	{
		if (enemy.IsNullOrReleased())
		{
			return false;
		}
		if (IsInvincible(enemy))
		{
			return false;
		}
		Labor component = enemy.GetComponent<Labor>();
		if (component != null)
		{
			TargetCharacterLocation.Reset(component);
			return Profession.CanFireAt(TargetCharacterLocation);
		}
		TargetObjectLocation.Object = enemy;
		return Profession.CanFireAt(TargetObjectLocation);
	}

	private bool IsInPreferredOrientation(Location location)
	{
		return Vector3.Angle(Transform.forward.Flatten(), (location.Position - Transform.position).Flatten()) < 45f;
	}

	private void FindBestMeleeTarget()
	{
		SearchMeleeTarget searchMeleeTarget = SearchMeleeTarget.Get(this);
		FindBestMeleeTarget(searchMeleeTarget);
		searchMeleeTarget.Dispose();
	}

	private void FindBestMeleeTarget(SearchMeleeTarget search)
	{
		NextProjectedBranch = (ProjectedBranch = (NextBranch = (StepBranch = default(PathBranch))));
		NextProjectedLocation = (ProjectedLocation = (NextLocation = (StepLocation = null)));
		NextProjectedStep = (ProjectedStep = (NextStep = (Step = XYZ.zero)));
		MeleeTarget = null;
		PathLink pathStart = search.PathStart;
		PathSearch pathSearch = search.PathSearch;
		EnemyAccepter pathAccepter = search.PathAccepter;
		if (pathStart.head.Node.IsEmpty)
		{
			return;
		}
		pathSearch.Start(pathStart);
		for (int i = 0; i < MeleeSearchLimit; i++)
		{
			if (!pathSearch.MoveNext())
			{
				break;
			}
		}
		MeleeTarget = ((pathAccepter.Enemy == null) ? null : pathAccepter.Enemy.GameObject);
		if (MeleeTarget == null)
		{
			return;
		}
		if (!CanMove)
		{
			Location temporaryLocationOf = GetTemporaryLocationOf(MeleeTarget);
			if (!temporaryLocationOf.TargetReached(GameObject, PositionEnteringOrOccupied.ToVector3()))
			{
				MeleeTarget = null;
				return;
			}
		}
		PathLink pathLink = pathAccepter.Result;
		while (pathLink.tail != null)
		{
			NextProjectedBranch = ProjectedBranch;
			ProjectedBranch = NextBranch;
			NextBranch = StepBranch;
			StepBranch = pathLink.head;
			pathLink = pathLink.tail;
		}
		GetSteppingLocation(StepBranch, Transform.position, Transform.rotation, ref StepLocation, ref Step);
		GetSteppingLocation(NextBranch, StepLocation, ref NextLocation, ref NextStep);
		GetSteppingLocation(ProjectedBranch, NextLocation, ref ProjectedLocation, ref ProjectedStep);
		GetSteppingLocation(NextProjectedBranch, ProjectedLocation, ref NextProjectedLocation, ref NextProjectedStep);
	}

	private bool HasPotentialMeleeTarget(out bool onlyStructureInRange)
	{
		onlyStructureInRange = false;
		if (EnemyTracker.AllVisibleFor(TrackingLayer.Character | TrackingLayer.Crystal, (int)Attacker.Profession.AggroDistance).Any((GameObject e, AttackInfo self) => self.IsValidTarget(e.gameObject), this))
		{
			return true;
		}
		if (EnemyTracker.AllVisibleFor(TrackingLayer.Structure, (int)Attacker.Profession.AggroDistance).Any((GameObject e, AttackInfo self) => self.IsValidTarget(e.gameObject), this))
		{
			onlyStructureInRange = true;
			return true;
		}
		return false;
	}

	private static void GetSteppingLocation(PathBranch stepBranch, WalkNode previousLocation, ref WalkNode stepLocation, ref XYZ stepVoxel)
	{
		if (previousLocation != null)
		{
			GetSteppingLocation(stepBranch, previousLocation.Position, previousLocation.Orientation, ref stepLocation, ref stepVoxel);
		}
	}

	private static void GetSteppingLocation(PathBranch stepBranch, Vector3 previousPosition, Quaternion previousOrientation, ref WalkNode stepLocation, ref XYZ stepVoxel)
	{
		if (!stepBranch.IsEmpty && !stepBranch.Node.IsEmpty)
		{
			stepVoxel = stepBranch.Node.RepresentativePosition;
			Vector3 forward = (stepVoxel.ToVector3() - previousPosition).Flatten();
			if (forward.magnitude < 0.1f)
			{
				forward = previousOrientation * Vector3.forward;
			}
			stepLocation = new WalkNode(stepBranch, Quaternion.LookRotation(forward));
		}
	}

	private static bool IsInvincible(GameObject enemy)
	{
		return enemy.GetComponent<IDamageReceiver>()?.IsInvincible ?? true;
	}

	private bool IsValidTarget(GameObject enemy)
	{
		return enemy != null && !IsInvincible(enemy) && LocalFactionVision.ObjectIsVisibleForFaction(enemy, Attacker.faction);
	}

	private bool PresenceTest(XYZ p)
	{
		if (BlockedByObstacle(p))
		{
			return false;
		}
		if (BlockedByAlly(p))
		{
			return false;
		}
		return true;
	}

	private bool BlockedByObstacle(XYZ p)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.obstacle, (IVolume v, AttackInfo attack) => !v.PassThrough.CanPassThrough(attack.GameObject), this);
	}

	private bool BlockedByAlly(XYZ p)
	{
		if (!VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.character))
		{
			return false;
		}
		return true;
	}

	private bool IsEnemy(IVolume v)
	{
		if (v == null)
		{
			return false;
		}
		return Brix.Game.Affiliation.IsEnemy(Attacker.faction, v.Faction);
	}

	private bool HasEnemiesAt(XYZ p)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(p, VPropertyMask.access | VPropertyMask.characterAttackAccess, (IVolume v, AttackInfo self) => self.IsEnemy(v), this);
	}

	private MultiPropertyRequest<AttackInfo> GetEnemiesAt(XYZ p)
	{
		return VolumeRegistry.VolumesWithPropertyAt(p, VPropertyMask.access | VPropertyMask.characterAttackAccess, (IVolume v, AttackInfo self) => self.IsEnemy(v), this);
	}

	private IVolume GetEnemyAt(XYZ p)
	{
		return VolumeRegistry.VolumeWithPropertyAt(p, VPropertyMask.access | VPropertyMask.characterAttackAccess, (IVolume v, AttackInfo self) => self.IsEnemy(v), this);
	}
}
