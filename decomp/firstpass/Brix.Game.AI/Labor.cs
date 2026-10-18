using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Brix.Game.AI;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Behaviors;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Audio;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Legacy;
using Brix.Lua;
using Brix.Pathfinding;
using Brix.UI.GameSelector;
using Brix.UI.Icons;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Motus.Behavior;
using Newtonsoft.Json;
using Smooth.Collections;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

[RequireComponent(typeof(Toolbag))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(AffiliationComponent))]
[RequireComponent(typeof(CharacterParentTracker))]
[SelectionBase]
[RequireComponent(typeof(Navigation))]
public class Labor : BaseLabor
{
	public enum DecisionSource
	{
		Unspecified,
		User,
		Project,
		Character
	}

	private static BrixLogger logger = new BrixLogger(typeof(Labor).FullName);

	public static bool showCombatDebug;

	public Signal StartSignal = new Signal();

	public Navigation navigation;

	public Interaction interaction;

	public Transaction transaction;

	public GarnotteEater eater;

	[SerializeField]
	private Thinker thinker;

	[SerializeField]
	private CharacterParentTracker parentTracker;

	[SerializeField]
	public EnemyTracker enemyTracker;

	[SerializeField]
	private CharacterState state;

	[SerializeField]
	private LaborInstinct instinct;

	[SerializeField]
	public Toolbag toolbag;

	public string StringInstanceID;

	private long decisionFrame;

	public VoxelPresence presence;

	public AnimationEventBoard animationEventBoard;

	public bool CheckForConstructions;

	[JsonProperty]
	[LuaDrivable("OriginalSpawnCrystal")]
	public SpawnerCristal OriginalSpawnCrystal;

	public Func<bool> IsProjectLeader = () => false;

	private Node ProcessProjectWarningMessages;

	public Profile PersonalInterest;

	public Signal<Labor> DestroyedSignal = new Signal<Labor>();

	public static GameSignals.GameSignal<Labor> LaborStarted = GameSignals.CreateStaticSignal<Labor>();

	public static GameSignals.GameSignal<Labor> LaborDeleted = GameSignals.CreateStaticSignal<Labor>();

	public static GameSignals.GameSignal<Labor> OccupationChanged = GameSignals.CreateStaticSignal<Labor>();

	private Signal<Labor> projectChanged = new Signal<Labor>();

	public Activity Activity;

	private Occupation _occupation;

	public Transform PhasedSubObject;

	private bool _isPhaseOut;

	private AutonomyStatus _exCrewInfoC;

	public bool beingDestroyed;

	private float autonomyExpiration;

	[LuaDrivable("autonomyExpiration")]
	public bool AutonomyExpires = true;

	public HashSet<Factory.AssetKey> ProjectResourceNeeds = new HashSet<Factory.AssetKey>();

	public readonly Var<Goal> ResultGoal = new Var<Goal>();

	private readonly Var<OrderPackage<Labor>> _projectInstruction = new Var<OrderPackage<Labor>>();

	private static int _offsetWaitTime = 0;

	private static float _lastQuery = 0f;

	private static int _maxConcurrentBricktrons = 8;

	private static int _tickOffset = 2;

	private IBindableCache<Labor, Ressource, GameObject> resourceCache = new UnbindedCache<Labor, Ressource, GameObject>((Labor l) => l.decisionFrame, (Labor l, Ressource r) => l.FindOneResource(r));

	private IBindableCache<Labor, IGameComponent, IGameComponent> bestRecepteurCache = new UnbindedCache<Labor, IGameComponent, IGameComponent>((Labor l) => l.decisionFrame, (Labor l, IGameComponent go) => Knowledge.Instance.ClosestStorageWithRoomFor(l, go, l.faction));

	public AttackInfo DebugAttack = new AttackInfo();

	public DigGoal hackedDigGoal;

	public Signal<Labor> WasPhasedOut = new Signal<Labor>();

	public Signal<IconKey> AddedToInstructionQueue = new Signal<IconKey>();

	[LuaDrivable("hk_OnEnterCombat")]
	private static LuaHook<Labor> lua_OnEnterCombat = new LuaHook<Labor>(DangerStarted);

	[LuaDrivable("hk_OnExitCombat")]
	private static LuaHook<Labor> lua_OnExitCombat = new LuaHook<Labor>(DangerEnded);

	public bool IsInDanger => instinct.IsInDanger;

	public bool Defending => Autonomy.IsDefensive;

	public Goal Goal
	{
		get
		{
			if (CurrentTask == null || CurrentTask.Giver == null)
			{
				return null;
			}
			return CurrentTask.Giver.GetComponent<Goal>();
		}
	}

	public Project Project => parentTracker.Project;

	public IOperable<Labor> Operable
	{
		get
		{
			return state.Operable;
		}
		set
		{
			if (state.Operable != value)
			{
				Order.Move.HaltWhatever.Send(this);
				if (state.Operable != null && Operable.IsStillAlive)
				{
					ProjectDatabase.For(base.faction).RemoveWorker(state.Operable);
				}
				if (value == null)
				{
					Autonomy = Autonomy.OnFiredFromOperable;
				}
				else
				{
					ProjectDatabase.For(base.faction).AddWorker(value);
				}
				state.Operable = value;
			}
		}
	}

	[LuaDrivable("UnitAggro")]
	private float UnitAggro => Profession.RaidProjectAggro;

	public ConflictResolver ConflictResolver
	{
		get
		{
			if (Operable != null && Operable.IsStillAlive && Operable.ConflictResolver != null)
			{
				return Operable.ConflictResolver;
			}
			return (!(Project != null)) ? null : Project.ConflictResolver;
		}
	}

	public IReadOnlySignal<GameObject> SelfLaborDeleted => base.OnReleaseOrDestroySignal;

	public static GameSignals.GameSignal<Labor> DangerStarted => LaborInstinct.DangerStarted;

	public static GameSignals.GameSignal<Labor> DangerEnded => LaborInstinct.DangerEnded;

	public IReadOnlySignal OnDangerStartedSignal => instinct.OnDangerStarted;

	public IReadOnlySignal OnDangerEndedSignal => instinct.OnDangerEnded;

	public IReadOnlySignal<Labor> ProjectChanged => projectChanged;

	public ProjectileAiming Aiming => GetComponent<Locomotion4>().Aiming;

	public Profession Profession => (!(Occupation != null)) ? null : Occupation.Profession;

	public Occupation Occupation
	{
		get
		{
			if (_occupation == null)
			{
				_occupation = GetComponent<Occupation>();
			}
			return _occupation;
		}
	}

	public BricktronSoundEffects Sounds => Occupation.GetProfession().Sounds;

	public bool IsPhasedOut => _isPhaseOut;

	public IReadOnlySignal StateUpdated => state.NetworkUpdate;

	public Thought PreviousThought => thinker.CurrentThought;

	public Thought Thought
	{
		get
		{
			return state.Thought;
		}
		set
		{
			thinker.SetThought(value);
		}
	}

	public AutonomyStatus Autonomy
	{
		get
		{
			return state.Autonomy;
		}
		set
		{
			state.Autonomy = ((!value.ValidFor(this)) ? value.OnInvalid : value);
			autonomyExpiration = value.ExpirationTime;
		}
	}

	private static int OffsetWaitTime
	{
		get
		{
			if (Time.fixedTime - _lastQuery > Time.fixedDeltaTime * (float)_tickOffset)
			{
				_offsetWaitTime = 0;
			}
			else
			{
				_offsetWaitTime++;
			}
			_lastQuery = Time.fixedTime;
			return _offsetWaitTime % _maxConcurrentBricktrons * _tickOffset + 1;
		}
	}

	public void LeaveOperation()
	{
		if (Operable != null && Operable.IsStillAlive)
		{
			Operable.Fire(this);
		}
	}

	public Sprite GetAssignationIcon()
	{
		if (Operable != null)
		{
			return Operable.Icon.Get32();
		}
		return Project.Balise.Meta.sprite;
	}

	protected override void Awake()
	{
		base.Awake();
		StringInstanceID = $"Labor:{GetInstanceID()}";
	}

	protected override void Start()
	{
		base.Start();
		StartSignal.Invoke();
		if (!state.Initialized)
		{
			Autonomy = AutonomyStatus.FreeAgent;
		}
		if (IsPhasedOut)
		{
			PhaseOut();
		}
		if (Project == null)
		{
			AssignToIdle();
		}
		navigation.PathStarted.Connect(delegate(Labor self)
		{
			self.StopThinking();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		HasFinishedWork.Connect(delegate(Node n, Labor self)
		{
			self.CurrentIconKey = null;
		}, this);
		HookInsructionSignals();
		state.NetworkUpdate.Connect(delegate(Labor self)
		{
			self.SetCrewInfoC();
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		ResetAI();
		GameSignals.Invoke(LaborStarted, this);
	}

	private void SetCrewInfoC()
	{
		if (_exCrewInfoC != Autonomy)
		{
			_exCrewInfoC = Autonomy;
			LuaHookBase.InvokeFromCLR(Project.lua_OnSetCrewInfoC, Project);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		parentTracker.ObjectTrackedByNewParentSignal.Connect(delegate
		{
			projectChanged.Invoke(this);
		});
		navigation = GetComponent<Navigation>();
		interaction = GetComponent<Interaction>();
		transaction = GetComponent<Transaction>();
		presence = GetComponent<VoxelPresence>();
		animationEventBoard = GetComponent<AnimationEventBoard>();
		eater = GetComponent<GarnotteEater>();
		AddOrSetSequence(ProcessCurrentWorkSequence(), 1, "CurrentWork Sequence");
		AddOrSetSequence(ProcessPendingWorkSequence(), 1, "PendingWork Sequence");
		AddOrSetSequence(LookForWorkSequence(), 10, "LookForWork Sequence");
		AddOrSetSequence(AutonomySequence(), 1, "Check Autonomy");
		AddOrSetSequence(VoxelPresenceMaintenanceSequence(), 0, "VoxelPresence Maintenance Sequence");
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (NetworkServer.active)
		{
			base.affiliation.OnFactionChanged.Connect(delegate(Faction f1, Faction f2, Labor self)
			{
				self.AssignToIdle(f2);
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
			base.OnReleaseOrDestroySignal.ConnectOnce(delegate(GameObject go, float aggro, Labor self)
			{
				self.RegisterDeathAggro(aggro);
			}, Profession.RaidProjectAggro, this);
		}
		else
		{
			base.affiliation.OnFactionChanged.Connect(delegate(Faction f1, Faction f2, Labor self)
			{
				self.AssignToIdleIfMine(f2);
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
	}

	public void AssignToIdle()
	{
		if (this != null && base.faction != null)
		{
			IdleProject matchingProject = IdleProject.GetMatchingProject(base.faction);
			if (matchingProject != null)
			{
				matchingProject.Hire(this);
			}
		}
	}

	private void AssignToIdle(Faction f2)
	{
		if (Project == null || (Project != null && Project.faction != f2))
		{
			IdleProject matchingProject = IdleProject.GetMatchingProject(f2);
			if (matchingProject != null)
			{
				matchingProject.Hire(this);
			}
		}
	}

	private void AssignToIdleIfMine(Faction f2)
	{
		if (Affiliation.BelongsToMe(base.gameObject))
		{
			AssignToIdle(f2);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		base.AbortWorkSignal.Clear();
	}

	public void RegisterDeathAggro(float aggro)
	{
		if (!Project.IsNullOrReleased() && !(Project is IdleProject))
		{
			Project.DeathCount += aggro;
		}
	}

	public void OnOccupationChange()
	{
		if (Occupation.GetProfession().IsWorker())
		{
			base.recepteur.ClearExclusions();
		}
		else
		{
			base.recepteur.DoExcludeAll();
		}
		Autonomy = Autonomy.OnChangeClass(Occupation);
		GameSignals.Invoke(OccupationChanged, this);
		LuaHookBase.InvokeFromCLR(Project.lua_OnSetCrewInfoC, Project);
	}

	protected override void OnDestroy()
	{
		beingDestroyed = true;
		base.OnDestroy();
		if (!NetworkServer.active)
		{
			AssignToIdle();
		}
		GameSignals.Invoke(LaborDeleted, this);
		DestroyedSignal.Invoke(this);
	}

	public bool HasGoal()
	{
		return CurrentTask != null && CurrentTask.Giver != null;
	}

	public bool NoQueuedWork()
	{
		bool flag = PendingTask == null;
		bool flag2 = base.DelayBeforeAvailable == 0f;
		return flag && flag2 && !IsPhasedOut;
	}

	public void SetTemporaryThought(float seconds, Thought thoughtType)
	{
		thinker.SetThoughtFor(seconds, thoughtType);
	}

	public void ResolveThought()
	{
		thinker.ShowResolutionThought();
	}

	public void StopThinking()
	{
		thinker.StopThinking();
	}

	public bool HasThought()
	{
		return thinker.HasThought();
	}

	public GameObject SpawnTool(Accessoire accessoire)
	{
		GameObject gameObject = Toolbag.SpawnTool(accessoire);
		if (gameObject != null)
		{
			this.DropUnrelatedTools(gameObject);
			Profession.toolbag.EquipTool(gameObject);
			return gameObject;
		}
		return null;
	}

	private StaticNode AutonomySequence()
	{
		return Node.Selector.If((Labor self) => self.Autonomy.ExpiresFor(self) && self.AutonomyExpires && Time.time > self.autonomyExpiration && self.IsAvailable(), this).Do((Labor self) => self.Autonomy = self.Autonomy.OnExpiration, this);
	}

	private StaticNode LookForWorkSequence()
	{
		return Node.Selector.When((Labor self) => self.HasToFight(), this).Label("HasToFight").Do(CheckInstinctForWork())
			.Label("CheckInstinctForWork")
			.Do(Node.WaitFor.Ticks(10))
			.Label("Wait 10")
			.When((Labor self) => self.LookingForWork(), this)
			.Label("LookingForWork")
			.Do((Labor self) => self.CheckProjectForWork(), this)
			.Label("CheckProjectForWork")
			.Do(Node.WaitFor.Ticks(10))
			.Label("Wait 10")
			.Else()
			.Label("nothing to do")
			.Do(Node.WaitFor.Ticks(1))
			.Label("Wait 1")
			.Finally(delegate(Labor self)
			{
				self.decisionFrame++;
			}, this)
			.ToNode;
	}

	private bool LookingForWork()
	{
		return Activity == Activity.Chilling || (Activity == Activity.Idle && IsAvailable() && InstructionQueue.Count == 0);
	}

	private bool HasToFight()
	{
		return IsInDanger && Activity != Activity.Fighting;
	}

	private StaticNode CheckInstinctForWork()
	{
		return Node.Sequence.Do((Labor self) => self.instinct.GetInstruction(self), this).Label("instinct GetInstruction").Process((OrderPackage<Labor> package, Labor self) => self.AssignInstruction(package), this)
			.Label("AssignInstruction");
	}

	private Node CheckProjectForWork()
	{
		if (!Autonomy.TakeOrdersFromAnyOperable && !Autonomy.TakeOrdersCurrentOperable && !Autonomy.TakeOrdersFromIdleProject)
		{
			return Node.Empty;
		}
		List<IOperable<Labor>> list = CollectionPool.RequestList<IOperable<Labor>>();
		RefreshAvailableProjects(list);
		Var<int> var = Var<int>.Request();
		Var<FailException> var2 = Var<FailException>.Request();
		Looper.Builder builder = Node.Sequence.Do((List<IOperable<Labor>> ps, Labor self, Var<int> i, Var<FailException> e) => self.TryWorkInProject(ps, i, e), list, this, var, var2).Loop().Finally(delegate(List<IOperable<Labor>> ps, Var<int> i, Var<FailException> e)
		{
			ps.Dispose();
			i.Dispose();
			e.Dispose();
		}, list, var, var2);
		return (StaticNode)builder;
	}

	private void RefreshAvailableProjects(List<IOperable<Labor>> projects)
	{
		if (Autonomy.TakeOrdersCurrentOperable && !(Project is IdleProject))
		{
			projects.Add(Project);
		}
		if (Autonomy.TakeOrdersCurrentOperable && Operable != null && Operable.IsStillAlive)
		{
			projects.Add(Operable);
		}
		if (Autonomy.TakeOrdersFromAnyOperable)
		{
			ProjectDatabase.For(base.faction).AvailableProjects(projects, this);
			projects.SortBy((IOperable<Labor> p, Labor self) => 0f - p.PriorityInput(self) * p.PriorityWeights, this);
		}
		if (Autonomy.TakeOrdersFromIdleProject)
		{
			projects.Add(IdleProject.GetMatchingProject(base.faction));
		}
	}

	private Node TryWorkInProject(List<IOperable<Labor>> projects, Var<int> index, Var<FailException> error)
	{
		if (index.Get() >= projects.Count)
		{
			return (error.Get() == null) ? Node.Break() : Node.Fail(error.Get());
		}
		int num = index.Get();
		index.Set(num + 1);
		IOperable<Labor> operable = projects[num];
		if (operable == null || !operable.IsStillAlive || !operable.HasWorkFor(this))
		{
			return Node.Empty;
		}
		return (StaticNode)Node.Catcher.Do((IOperable<Labor> project, Labor labor) => labor.SendProjectInstruction(project), operable, this).Label("SendProjectInstruction").Do(delegate(List<IOperable<Labor>> ps, Var<FailException> e)
		{
			ps.Clear();
			e.Set(null);
		}, projects, error)
			.Catch(delegate(FailException fail, Var<FailException> e)
			{
				e.Set(fail);
			}, error);
	}

	private StaticNode SendProjectInstruction(IOperable<Labor> project)
	{
		return Node.Selector.When((IOperable<Labor> _project) => _project == null || !_project.IsStillAlive, project).Label("If project is null").Do(() => Node.Fail(FailException.Generic))
			.Label("Fail because projet is null")
			.Else()
			.Label("If project is ok")
			.Do(Node.WaitFor.Ticks(OffsetWaitTime))
			.Label("Offsetted wait")
			.Do((Labor self, IOperable<Labor> p) => p.VerifyWorkFor(self), this, project)
			.Label("project.HasWorkFor")
			.Do((Labor self, IOperable<Labor> p) => p.GetInstruction(self, self._projectInstruction), this, project)
			.Label("project GetInstruction")
			.Do((Labor self) => self.AssignInstruction(self._projectInstruction.Get()), this)
			.Label("AssignInstruction");
	}

	private Node AssignInstruction(OrderPackage<Labor> package)
	{
		return Node.Selector.If((OrderPackage<Labor> _package) => _package != null, package).Do((Labor self, OrderPackage<Labor> _package) => _package.Prepare(self), this, package).Label("Prepare")
			.Do(delegate(Labor self, OrderPackage<Labor> _package)
			{
				_package.Send(self);
			}, this, package)
			.Label("Send")
			.Else()
			.Do(Node.Fail(FailException.Generic))
			.ToNode;
	}

	private StaticNode VoxelPresenceMaintenanceSequence()
	{
		return Node.Sequence.Do(delegate(Labor self)
		{
			self.MaintainVoxelPresence();
		}, this).Label("MaintainVoxelPresence").Do(Node.WaitForTicks(5))
			.Label("Wait 5")
			.Loop();
	}

	private void MaintainVoxelPresence()
	{
		Unstack();
		UnstackFromNature();
		if (!IsAvailable())
		{
			LastWorkEndTime = Time.time;
		}
	}

	public GameObject Find(Profile profile)
	{
		return base.recepteur.FirstObjectOf(profile) ?? ClosestResource(profile.description);
	}

	public GameObject Find(Description desc)
	{
		return base.recepteur.FirstObjectOf(desc) ?? ClosestResource(desc);
	}

	public GameObject Find(Recepteur target)
	{
		return base.recepteur.FirstThatFitsIn(target) ?? ClosestResource(target.CurrentCapacity);
	}

	public GameObject FindComplete(Description desc)
	{
		Ressource ressource = base.recepteur.ContentDescription.GetRessources().FirstOrDefault((Ressource adjectif, Description d) => d.Value(adjectif) != 0 && adjectif.valeur >= d.Value(adjectif), desc);
		if (ressource != null)
		{
			return base.recepteur.FirstObjectOf(ressource);
		}
		return ClosestResource(desc);
	}

	private GameObject ClosestResource(Description capacity)
	{
		GameObject closest;
		int qty;
		return (!ClosestResource(out closest, out qty, capacity)) ? null : closest;
	}

	private bool ClosestResource(out GameObject closest, out int qty, Description ingredients)
	{
		resourceCache = resourceCache.Bind(this);
		resourceCache.RefreshIfNeeded();
		closest = null;
		qty = 0;
		foreach (Ressource item in ingredients.GetRessources().NoLeak())
		{
			qty = item.quantifiable.valeur;
			if (qty != 0)
			{
				closest = resourceCache.GetUnsafe(item);
				if (!closest.IsNullOrReleased() && base.recepteur.HasRoomFor(closest))
				{
					return true;
				}
			}
		}
		return false;
	}

	private GameObject FindOneResource(Ressource resource)
	{
		IGameComponent gameComponent = Knowledge.Instance.ClosestObjectToPickup(this, resource, base.faction);
		return gameComponent.WithRecepteur((Recepteur r, Ressource res) => r.FirstObjectOf(res), gameComponent?.GameObject, resource);
	}

	public GameObject BestRecepteurToStore(GameObject go)
	{
		return BestRecepteurToStore(go.GetComponent<IGameComponent>());
	}

	public GameObject BestRecepteurToStore(IGameComponent gc)
	{
		bestRecepteurCache = bestRecepteurCache.Bind(this);
		return bestRecepteurCache.Get(gc)?.GameObject;
	}

	public GameObject BestRecepteurToStoreInventory()
	{
		if (base.recepteur.IsEmpty())
		{
			return null;
		}
		return BestRecepteurToStore(base.recepteur.StoredItems.Slinq().First().GameComponent);
	}

	public GameObject BestToolToPickup(Accessoire toolType)
	{
		return Knowledge.Instance.ClosestToolFrom(this, toolType, base.faction)?.GameObject;
	}

	public GameObject BestToolRackToStore(Accessoire toolType)
	{
		return Knowledge.Instance.ClosestToolRackWithSpace(this, toolType, base.faction)?.GameObject;
	}

	public bool IsCloseEnemy(GameObject DestinationTarget)
	{
		return enemyTracker.IsAttackableEnemy(DestinationTarget) && enemyTracker.IsVisibleFor(DestinationTarget, base.faction) && (DestinationTarget.transform.position - enemyTracker.transform.position).sqrMagnitude <= Calcule.Sqr(Profession.AggroDistance);
	}

	public int GetAggroDistance(bool applyInstinctConstraint)
	{
		if (applyInstinctConstraint && !Autonomy.ListenToRangeInstinct)
		{
			return 3;
		}
		return (int)Profession.AggroDistance;
	}

	public Slinq<GameObject, LinkedContext<float, GameObject>> GetClosestEnemies(int vigilanceRadius)
	{
		TrackingLayer layers = ((Occupation.CurrentOccupation == Occupation.Type.Archer) ? TrackingLayer.Character : (TrackingLayer.Character | TrackingLayer.Structure | TrackingLayer.Crystal));
		return enemyTracker.AllVisibleForByDistance(layers, vigilanceRadius);
	}

	public void TemporaryFocusOnAttacker(GameObject enemy, float duration)
	{
		instinct.TemporaryFocusOnAttacker(enemy, duration);
	}

	public bool CanInteract(MonoBehaviour m)
	{
		return m != null && CanInteract(m.gameObject);
	}

	public bool CanInteract(GameObject o)
	{
		if (o.IsNullOrReleased())
		{
			return false;
		}
		GameComponent component = o.GetComponent<GameComponent>();
		return (!(component != null)) ? (Vector3.Distance(base.transform.position, o.transform.position) <= 2f) : CanInteract(component);
	}

	public bool CanInteract(GameComponent b)
	{
		if (b.IsNullOrReleased())
		{
			return false;
		}
		if ((bool)b.GetComponent<Garnotte>())
		{
			return true;
		}
		return b.volume?.HasPropertyAt(VProperty.access, base.voxel.position) ?? false;
	}

	private void LineDebug(GameObject target, bool Add)
	{
	}

	public StaticNode GoAttack(GameObject target)
	{
		return DebugAttack.GoAttack(this, target);
	}

	public StaticNode GoAttackWithPriority(GameObject target)
	{
		return DebugAttack.GoAttackWithPriority(this, target);
	}

	public StaticNode GoAttackDirectly(GameObject target)
	{
		return DebugAttack.GoAttackDirectly(this, target);
	}

	public StaticNode HoldPositionAndAttackForever()
	{
		return DebugAttack.HoldPositionAndAttackForever(this);
	}

	public StaticNode HoldPositionAndAttack()
	{
		return DebugAttack.HoldPositionAndAttack(this);
	}

	public StaticNode HoldPositionAndAttackDirectly(GameObject target)
	{
		return DebugAttack.HoldPositionAndAttackDirectly(this, target);
	}

	public StaticNode HoldPositionAndAttackWithPriority(GameObject target)
	{
		return DebugAttack.HoldPositionAndAttackWithPriority(this, target);
	}

	public StaticNode MoveAttack(Location location)
	{
		return DebugAttack.MoveAttack(this, location);
	}

	public StaticNode WalkMoveAttack(Location location)
	{
		return DebugAttack.WalkMoveAttack(this, location);
	}

	public StaticNode HaltWhatever()
	{
		return Node.Sequence.Do(InterruptMovement()).Do((Labor self) => UtilityNodes.WaitForTimeOrCondition((Labor _self) => _self.navigation.Locomotion.IsInPlace(), self, 1f, failOnDelay: true), this).Label("Wait for Locomotion.InInPlace()");
	}

	public StaticNode InterruptMovement()
	{
		return Node.Sequence.Do((Labor _labor) => _labor.FreezeAt(_labor.transform.position, 1), this).ToNode;
	}

	public bool HasPriorityOver(GameObject o)
	{
		return o == null || GetInstanceID() > o.GetInstanceID();
	}

	public override Node GetReadyForNextTask()
	{
		Locomotion4 component = GetComponent<Locomotion4>();
		return Node.Sequence.Do(delegate(Locomotion4 l)
		{
			l.GetReadyToMove();
		}, component).Do(Node.WaitForCondition((Locomotion4 l) => l.CanMove(), component)).ToNode;
	}

	private bool ShouldUnstack()
	{
		bool flag = base.CurrentWork == null && PendingTask == null;
		bool flag2 = Activity == Activity.Fighting;
		bool flag3 = Activity == Activity.Chilling;
		bool flag4 = !hackedDigGoal.IsNullOrReleased() && Occupation.CurrentJob != Occupation.Job.Builder;
		return flag || flag2 || flag3 || flag4;
	}

	private void Unstack()
	{
		if (!ShouldUnstack())
		{
			return;
		}
		DestinationIterator pathIterator = navigation.Locomotion.PathIterator;
		if (pathIterator.HasCurrent)
		{
			return;
		}
		VoxelPresence stackedPresence = presence.GetStackedPresence();
		if (stackedPresence == null)
		{
			return;
		}
		Func<XYZ, bool> func = presence.BuildPresenceTest();
		if (func(presence.CurrentVoxel))
		{
			return;
		}
		Labor component = stackedPresence.GetComponent<Labor>();
		if (!component.IsNullOrReleased())
		{
			DestinationIterator pathIterator2 = component.navigation.Locomotion.PathIterator;
			if (component.Profession.CanPassThrough() && pathIterator2.HasCurrent)
			{
				return;
			}
		}
		if (HasPriorityOver(stackedPresence.gameObject))
		{
			Func<XYZ, bool> func2 = stackedPresence.BuildPresenceTest();
			if (!func2(presence.CurrentVoxel))
			{
				return;
			}
		}
		if (TryGetStep(out var step))
		{
			logger.InfoFormat("Unstacking {0} at {1} towards {2}", base.name, base.transform.position, step);
			Order.Move.UnstackStep.Send(this, step);
		}
	}

	private bool TryGetStep(out PathBranch step)
	{
		List<PathBranch> list = navigation.GetSurroundingSurfaces(PathBranch.BranchType.Climbing, ignoreCharacters: true).Slinq().ToPooledList();
		step = list.Random().ValueOr(PathBranch.Empty);
		list.Dispose();
		return !step.IsEmpty;
	}

	private void UnstackFromNature()
	{
		if (IsAvailable() && TreeRegistry.Instance.IsNearATree(presence.CurrentVoxel) && TryGetStep(out var step))
		{
			logger.InfoFormat("UnstackingFromForest {0} at {1} towards {2}", base.name, base.transform.position, step);
			Order.Move.UnstackStep.Send(this, step);
		}
	}

	public override Node HandleAIError(AIError error)
	{
		if (error.thought != null)
		{
			SetTemporaryThought(2f, error.thought);
		}
		return Node.Empty;
	}

	public StaticNode WaitForever()
	{
		return Node.WaitFor.Ever();
	}

	public void ResetAI()
	{
		StopThinking();
		eater.Starve();
	}

	public void PhaseOut()
	{
		if (NetworkServer.active)
		{
			GetComponent<Locomotion4>().DoPhaseOut();
			GetComponent<CharacterVolume>().Disappear();
			GetComponent<BaseLabor>().enabled = false;
			GetComponent<CapsuleCollider>().enabled = false;
			UIGameSelector.Unselect(base.gameObject);
			PhasedSubObject.gameObject.SetActive(value: false);
			_isPhaseOut = true;
			WasPhasedOut.Invoke(this);
		}
	}

	public void PhaseIn(Location l)
	{
		if (NetworkServer.active)
		{
			GetComponent<Locomotion4>().DoPhaseIn(l);
			GetComponent<CharacterVolume>().Reappear();
			GetComponent<BaseLabor>().enabled = true;
			GetComponent<CapsuleCollider>().enabled = true;
			PhasedSubObject.gameObject.SetActive(value: true);
			_isPhaseOut = false;
			LocalFactionVision component = GetComponent<LocalFactionVision>();
			if ((bool)component)
			{
				component.UpdateStoredGameObjectVision.Invoke();
			}
		}
	}

	public Node GetSmartTransferNode(GameObject o, bool alternateAction)
	{
		GameComponent component = o.GetComponent<GameComponent>();
		if (!alternateAction)
		{
			return tryGetGoStoreNode(component) ?? tryGetGoPickUpNode(component) ?? this.RunToExactly(Location.Of(component));
		}
		return tryGetGoPickUpNode(component) ?? tryGetGoStoreNode(component) ?? this.RunToExactly(Location.Of(component));
	}

	private Node tryGetGoStoreNode(GameComponent interactionItem)
	{
		if ((bool)interactionItem.recepteur && !base.recepteur.IsEmpty() && interactionItem.recepteur.HasRoomForSomeOf(base.gameObject))
		{
			return this.GoStoreIn(interactionItem);
		}
		return null;
	}

	private Node tryGetGoPickUpNode(GameComponent interactionItem)
	{
		if (Recepteur.CharactersCouldGrabFrom(interactionItem.GameObject) && (base.recepteur.HasRoomForSomeOf(interactionItem.GameObject) || base.recepteur.HasRoomFor(interactionItem)))
		{
			return this.GoPickUp(interactionItem, -1);
		}
		return null;
	}

	private void HookInsructionSignals()
	{
		base.AbortWorkSignal.Connect(delegate
		{
			PendingTask = null;
		});
	}

	private Node BaseHandleWorkException(Exceptions.WorkFailedException exception)
	{
		return base.HandleWorkException(exception);
	}

	public override Node HandleWorkException(Exceptions.WorkFailedException exception)
	{
		return (StaticNode)Node.Sequence.Do((Labor self, Exceptions.WorkFailedException e) => self.BaseHandleWorkException(e), this, exception).Do(delegate(Labor self, Exceptions.WorkFailedException e)
		{
			self.Project.OnLaborError(self, e);
		}, this, exception);
	}

	public override Node HandlePathFail(Exceptions.PathException e)
	{
		if (!this.IsNullOrReleased() && e.SpreadExclusions && e.Location != null && !e.Location.Object.IsNullOrReleased())
		{
			VProperty accessProperty = ((e.Location is OperableLocation) ? VProperty.operation : VProperty.access);
			return Knowledge.Instance.SignalOutOfReach(base.gameObject, e.Location.Object, accessProperty);
		}
		return base.HandlePathFail(e);
	}

	public override void SendOrder(Task task, DecisionSource source)
	{
		InstructionQueue.Clear();
		IconQueue.Clear();
		if (source == DecisionSource.User)
		{
			Autonomy = Autonomy.OnPlayerOrder;
			LeaveOperation();
		}
		task.TaskEnded.ConnectOnce(delegate(TaskResult t, Labor self, Task _task)
		{
			self.OnTaskEnded(_task);
		}, this, task);
		base.SendOrder(task, source);
	}

	private void OnTaskEnded(Task t)
	{
		Activity = Activity.Idle;
		if (PendingTask == null)
		{
			Autonomy = Autonomy.OnTaskEnded;
		}
	}

	[LuaDrivable("SetAutonomyFreeAgent")]
	private static DynValue lua_AutonomyFreeAgent(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject laborGO = args.AsGameObject(0, "SetAutonomyFreeAgent");
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSetAutonomy(laborGO, (int)AutonomyStatus.FreeAgent.Id);
		return DynValue.Void;
	}

	[LuaDrivable("SetAutonomyInstinct")]
	private static DynValue lua_AutonomyInstinct(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject laborGO = args.AsGameObject(0, "SetAutonomyInstinct");
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSetAutonomy(laborGO, (int)AutonomyStatus.InstinctOnly.Id);
		return DynValue.Void;
	}

	[LuaDrivable("SetAutonomyHoldPosition")]
	private static DynValue lua_AutonomyHoldPosition(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject laborGO = args.AsGameObject(0, "SetAutonomyHoldPosition");
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdSetAutonomy(laborGO, (int)AutonomyStatus.HoldingPosition.Id);
		return DynValue.Void;
	}
}
