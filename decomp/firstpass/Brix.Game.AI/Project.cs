using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Game.Components.Tunnel;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Game.Storage;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Motus.Behavior;
using Newtonsoft.Json;
using Smooth.Algebraics;
using Smooth.Delegates;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public abstract class Project : MultiBehavior, PieMenuable, IOperable<Labor>
{
	public enum ShowInTaskBarWhen
	{
		Never,
		HasWorker,
		Always
	}

	private class Utility<T, TSlinq> where T : Goal
	{
		public static Slinq<Goal, PredicateContext<Goal, SelectContext<Goal, T, TSlinq>, Tuple<Project, Labor>>> GoalsNeedsEvaluation(Project project, Slinq<T, TSlinq> goals, Labor labor)
		{
			return goals.Select((DelegateFunc<T, Goal>)((T g) => g)).Where((Goal g, Tuple<Project, Labor> t) => t._1.GoalNeedsEvaluation(g, t._2), Tuple.Create(project, labor));
		}
	}

	public enum CommitStatus
	{
		commited,
		partial,
		uncommited,
		invalid
	}

	public struct PooledListEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly List<T> _pooledList;

		private List<T>.Enumerator _enumerator;

		object IEnumerator.Current => Current;

		public T Current => _enumerator.Current;

		private PooledListEnumerator(List<T> pooledList)
		{
			_pooledList = pooledList;
			_enumerator = _pooledList.GetEnumerator();
		}

		public static PooledListEnumerator<T> Request<TEnumerator>(TEnumerator e) where TEnumerator : IEnumerator<T>
		{
			List<T> list = CollectionPool.RequestList<T>();
			while (e.MoveNext())
			{
				list.Add(e.Current);
			}
			return new PooledListEnumerator<T>(list);
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
			_pooledList.Dispose();
			_enumerator.Dispose();
		}
	}

	public enum CommitStatusAction
	{
		Ok,
		Apply,
		Cancel,
		Delete
	}

	[LuaDrivable("maxSize")]
	public int MaxSize = 16;

	[LuaDrivable("isBeingPlaced")]
	public bool IsBeingPlaced;

	public bool HasWorkStarted;

	protected List<IGoalProvider> GoalProviders = new List<IGoalProvider>();

	[JsonProperty]
	public ProjectData ProjectData;

	private static BrixLogger logger = new BrixLogger(typeof(Project).FullName);

	private bool shouldBeInvisible;

	[SerializeField]
	public ShowInTaskBarWhen showInTaskBarWhen;

	[SerializeField]
	private GameObject _goalParent;

	[SerializeField]
	public bool CanBeDeleted = true;

	[SerializeField]
	public bool LocateStorage;

	[SerializeField]
	public bool OverridesWorkerStorage;

	[SerializeField]
	[LuaDrivable("AutoDeleteDisabled")]
	public bool AutoDeleteDisabled;

	[LuaDrivable("CrewAggro")]
	public float CrewAggro;

	[SerializeField]
	[JsonProperty]
	public bool isForAI;

	public Signal projectDataStarted = new Signal();

	private float _deathCount;

	protected float LastDeathCountChange;

	[SerializeField]
	protected WorkForceProvider workforceProvider;

	[SerializeField]
	public ProjectState State;

	protected AIErrorsStore errorStore;

	private ConflictResolver conflictResolver;

	protected GoalSelector goalSelector;

	public bool CheckForGoals = true;

	public int DelayOnFailure = 1;

	protected ProjectPriority priorityWeights;

	public static int projectCap = 16;

	public static GameSignals.GameSignal<Project> ProjectCreated = GameSignals.CreateStaticSignal<Project>();

	public static GameSignals.GameSignal<Project> ProjectStarted = GameSignals.CreateStaticSignal<Project>();

	public static GameSignals.GameSignal<Project> ProjectDeleted = GameSignals.CreateStaticSignal<Project>();

	public static GameSignals.GameSignal<Project> OnSetCrewCountSignal = new GameSignals.GameSignal<Project>();

	public static GameSignals.GameSignal<Project> OnSetGoalCountSignal = new GameSignals.GameSignal<Project>();

	public static GameSignals.GameSignal<Project> OnSetStorageCountSignal = new GameSignals.GameSignal<Project>();

	public Signal DestroyedSignal = new Signal();

	public Signal<Labor, Goal> GoalMatchedSignal = new Signal<Labor, Goal>();

	protected Func<Labor, string> AssignationSoundFunc = (Labor l) => l.Sounds.ActionBuild;

	private int givenRefreshedFrame;

	private HashSet<Factory.AssetKey> uniqueItemsGiven = new HashSet<Factory.AssetKey>();

	private int neededRefreshedFrame;

	private HashSet<Factory.AssetKey> uniqueItemsNeeded = new HashSet<Factory.AssetKey>();

	private readonly Signal<Labor> _populationIncreased = new Signal<Labor>();

	private readonly Signal<Labor> _populationDecreased = new Signal<Labor>();

	public Beacon _balise;

	public static LuaEnumDescriptor<CommitStatusAction> descCommitStatusAction = new LuaEnumDescriptor<CommitStatusAction>();

	public static LuaEnumDescriptor<ProjectPriority.Bias> descPriority = new LuaEnumDescriptor<ProjectPriority.Bias>();

	public static LuaEnumDescriptor<RaidType> descRaidType = new LuaEnumDescriptor<RaidType>();

	private static LuaHook<Project> lua_Selected = new LuaHook<Project>();

	private static LuaHook<Project> lua_Unselected = new LuaHook<Project>();

	private static LuaHook<Project> lua_InventoryChanged = new LuaHook<Project>();

	private static LuaHook<Project> lua_OnIconChanged = new LuaHook<Project>();

	private static LuaHook<Project> lua_OnSetCommitStatus = new LuaHook<Project>();

	public static LuaHook<Project> lua_OnSetCrewInfoA = new LuaHook<Project>();

	public static LuaHook<Project> lua_OnSetCrewInfoB = new LuaHook<Project>();

	public static LuaHook<Project> lua_OnSetCrewInfoC = new LuaHook<Project>();

	public static LuaHook<Project> lua_OnSetCrewInfoPriority = new LuaHook<Project>();

	public static LuaHook<Project> lua_OnSetSelectedCrewCount = new LuaHook<Project>();

	[LuaDrivable("hk_OnAddLabor")]
	private LuaHook<Labor> lua_onAddLabor;

	[LuaDrivable("hk_OnRemoveLabor")]
	private LuaHook<Labor> lua_OnRemoveLabor;

	private static LuaHook<Project> lua_onAddLabor2 = new LuaHook<Project>();

	private static LuaHook<Project> lua_OnRemoveLabor2 = new LuaHook<Project>();

	public static LuaHookCollapsed<Project> lua_OnSetCrewInfo;

	private static readonly Occupation.Job[] Jobs = (Occupation.Job[])Enum.GetValues(typeof(Occupation.Job));

	public IconKey Icon => IconKeys.Missing;

	[LuaDrivable("beaconSpriteOverride")]
	private Sprite LuaBeaconSpriteOverride => BeaconSpriteOverride;

	protected virtual Sprite BeaconSpriteOverride => null;

	public GameObject goalParent
	{
		get
		{
			if (_goalParent == null)
			{
				_goalParent = new GameObject("Goals");
				_goalParent.transform.SetParent(base.transform);
			}
			return _goalParent;
		}
	}

	public float DeathCount
	{
		get
		{
			return _deathCount;
		}
		set
		{
			_deathCount = value;
			LastDeathCountChange = Time.time;
		}
	}

	protected virtual WorkForce Workforce => workforceProvider.GetAs<WorkForce>();

	public ProjectDatabase projectDatabase => ProjectDatabase.For(base.faction);

	public Beacon Beacon => GetComponentInChildren<Beacon>();

	public ConflictResolver ConflictResolver => conflictResolver;

	public bool ShowAllPoints
	{
		get
		{
			if (ProjectData == null)
			{
				return false;
			}
			return ProjectData.ShowAllPoints;
		}
		set
		{
			if (!(ProjectData == null))
			{
				ProjectData.ShowAllPoints = value;
			}
		}
	}

	public IReadOnlySignal<Labor> PopulationIncreased => _populationIncreased;

	public IReadOnlySignal<Labor> PopulationDecreased => _populationDecreased;

	public bool IsStillAlive => !this.IsNullOrReleased();

	public ProjectPriority PriorityWeights => priorityWeights;

	public ProjectPriority.Bias PriorityBias
	{
		get
		{
			return State.Bias;
		}
		set
		{
			State.Bias = value;
		}
	}

	protected ConflictResolver.ResolutionModel ConflictResolutionModel
	{
		set
		{
			conflictResolver.ResolutionMode = value;
		}
	}

	public Beacon Balise
	{
		get
		{
			if (this != null && _balise == null)
			{
				_balise = GetComponentInChildren<Beacon>();
			}
			return _balise;
		}
		set
		{
			_balise = value;
		}
	}

	private bool commited
	{
		get
		{
			return State.Commited;
		}
		set
		{
			State.Commited = value;
			SignalCommitmentChange();
		}
	}

	public CommitStatus Commited
	{
		get
		{
			if (!commited)
			{
				return CommitStatus.uncommited;
			}
			if (HasUncommitedGoals())
			{
				return CommitStatus.partial;
			}
			return CommitStatus.commited;
		}
	}

	private IRecepteur Storage
	{
		get
		{
			if (ProjectData == null)
			{
				return null;
			}
			return ProjectData.Storage;
		}
	}

	public int MaximumCrewSize
	{
		get
		{
			return workforceProvider.MaxPopulation;
		}
		set
		{
			workforceProvider.MaxPopulation = Mathf.Clamp(value, 0, 8);
		}
	}

	[LuaDrivable("freeAgentCount")]
	private DynValue lua_FreeAgentCount => DynValue.NewNumber(workforceProvider.FreeAgentCount());

	[LuaDrivable("maxCrewCount")]
	private DynValue lua_MaximumCrewSize
	{
		get
		{
			return DynValue.NewNumber(MaximumCrewSize);
		}
		set
		{
			MaximumCrewSize = (int)value.Number;
		}
	}

	[LuaDrivable("assignedCrewCount")]
	private DynValue lua_AssignedCrewCount => DynValue.NewNumber(workforceProvider.AssignedCount());

	public Labor Operator
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		priorityWeights = ProjectPriority.WeightsFor(this);
		workforceProvider.CreateWorkForce();
		conflictResolver = new ConflictResolver(this);
		errorStore = new AIErrorsStore();
		goalSelector = GoalSelector.GetSelectorFromType(GoalSelector.Type.First, errorStore);
		GameSignals.Invoke(ProjectCreated, this);
		if (LocateStorage)
		{
			InitProjectData();
		}
		lua_onAddLabor = new LuaHook<Labor>(PopulationIncreased);
		lua_OnRemoveLabor = new LuaHook<Labor>(PopulationDecreased);
		PopulationIncreased.Connect(delegate(Labor aLabor, LuaHook<Project> aHook, Project aProject)
		{
			LuaHookBase.InvokeFromCLR(aHook, aProject);
		}, lua_onAddLabor2, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		PopulationDecreased.Connect(delegate(Labor aLabor, LuaHook<Project> aHook, Project aProject)
		{
			LuaHookBase.InvokeFromCLR(aHook, aProject);
		}, lua_OnRemoveLabor2, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		State.NetworkUpdate.Connect(delegate(Project self)
		{
			LuaHookBase.InvokeFromCLR(lua_OnSetCrewInfoPriority, self);
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	public virtual void OnSelect()
	{
		LuaHookBase.InvokeFromCLR(lua_Selected, this);
	}

	public virtual void OnUnselect()
	{
		LuaHookBase.InvokeFromCLR(lua_Unselected, this);
	}

	public void OnIconChanged()
	{
		LuaHookBase.InvokeFromCLR(lua_OnIconChanged, this);
	}

	protected override void Start()
	{
		base.Start();
		projectDatabase.Register(this);
		if (base.faction == User.LocalUser.faction)
		{
			GameSignals.Invoke(ProjectStarted, this);
		}
	}

	public void InitProjectData()
	{
		BeaconDriver component = GetComponent<BeaconDriver>();
		if (component.Beacon == null)
		{
			component.BaliseCreated.ConnectOnce(delegate
			{
				InitProjectData();
			});
			return;
		}
		if (ProjectData == null)
		{
			ProjectData = Factory.Instantiate(Test.ProjectData).GetComponent<ProjectData>();
		}
		ProjectData.transform.SetParent(base.transform);
		ProjectData.ReferencePoint = component.Beacon.gameObject;
		ProjectData.Project = this;
		ProjectData.LocateStorage = LocateStorage;
		projectDataStarted.Invoke();
		ProjectData.Storage.Changed.Connect(delegate(Project self)
		{
			LuaProjectStorage.CallStorageSignals(self);
		}, this);
		ProjectData.BlueprinRecepteurCompound.Recepteur.Changed.Connect(delegate(Project self)
		{
			LuaProjectStorage.CallStorageSignals(self);
		}, this);
	}

	public virtual void OnMaxPopulationChanged(int value)
	{
		LuaHookBase.InvokeFromCLR(lua_OnSetCrewInfoB, this);
	}

	public virtual void OnPopulationChanged()
	{
		LuaHookBase.InvokeFromCLR(lua_OnSetCrewInfoB, this);
	}

	public virtual void OnBecameEmpty()
	{
	}

	protected void AddProvider(IGoalProvider goalProvider)
	{
		GoalProviders.Add(goalProvider);
	}

	protected virtual void InitGoalProviders()
	{
		GoalProviders = new List<IGoalProvider>();
	}

	public Beacon GetPoint(Beacon.BalisePointsType type)
	{
		return ProjectData.GetBalisePoint(type);
	}

	public void SetPoint(Beacon.BalisePointsType type, Beacon beacon)
	{
		ProjectData.SetBalisePoint(type, beacon);
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (IGoalProvider goalProvider in GetGoalProviders())
		{
			goalProvider.UniqueItemsGiven(ref keys);
		}
	}

	public void UniqueRequiredResource(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (IGoalProvider goalProvider in GetGoalProviders())
		{
			goalProvider.UniqueItemsNeeded(ref keys);
		}
	}

	public virtual bool HasRequiredResources()
	{
		HashSet<Factory.AssetKey> keys = CollectionPool.RequestHashSet<Factory.AssetKey>();
		HashSet<Recepteur> hashset = CollectionPool.RequestHashSet<Recepteur>();
		UniqueRequiredResource(ref keys);
		bool result = CanFindSome(keys);
		hashset.Dispose();
		keys.Dispose();
		return result;
	}

	private bool CanFindSome(HashSet<Factory.AssetKey> keys)
	{
		if (keys.Count == 0)
		{
			return true;
		}
		HashSet<Recepteur> recepteurs = CollectionPool.RequestHashSet<Recepteur>();
		ProjectData.Storage.RecepteursWithOneOf(keys, ref recepteurs);
		bool result = recepteurs.Count > 0;
		recepteurs.Dispose();
		return result;
	}

	public HashSet<Factory.AssetKey> UniqueItemsGiven()
	{
		if (Time.frameCount > givenRefreshedFrame)
		{
			uniqueItemsGiven.Clear();
			UniqueItemsGiven(ref uniqueItemsGiven);
			givenRefreshedFrame = Time.frameCount;
		}
		return uniqueItemsGiven;
	}

	public HashSet<Factory.AssetKey> UniqueItemsNeeded()
	{
		if (Time.frameCount > neededRefreshedFrame)
		{
			uniqueItemsNeeded.Clear();
			UniqueRequiredResource(ref uniqueItemsNeeded);
			neededRefreshedFrame = Time.frameCount;
		}
		return uniqueItemsNeeded;
	}

	protected virtual List<IGoalProvider> GetGoalProviders()
	{
		return GoalProviders;
	}

	public bool HasNonStorableGoals()
	{
		return GetGoalProviders().Slinq().Any((IGoalProvider provider) => provider.HasNonStorable());
	}

	public void RequestDelete()
	{
		UIGameSelector.Unselect(base.gameObject);
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdDelete(base.gameObject);
	}

	protected Slinq<Goal, PredicateContext<Goal, SelectContext<Goal, T, TSlinq>, Tuple<Project, Labor>>> GoalsNeedingEvaluation<T, TSlinq>(Slinq<T, TSlinq> goals, Labor labor) where T : Goal
	{
		return Utility<T, TSlinq>.GoalsNeedsEvaluation(this, goals, labor);
	}

	protected bool GoalNeedsEvaluation(Goal goal, Labor labor)
	{
		if (goal.IsNullOrReleased())
		{
			return false;
		}
		if (!goal.IsValid())
		{
			return false;
		}
		return true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ReleaseGoals();
		if (base.faction == User.LocalUser.faction && NetworkClient.active)
		{
			GameSignals.Invoke(ProjectDeleted, this);
		}
		DestroyedSignal.Invoke();
	}

	private void ReleaseGoals()
	{
		(from goal in GetComponentsInChildren<Goal>(includeInactive: true).Slinq()
			where goal != null
			select goal).ForEach(delegate(Goal g)
		{
			ObjectPoolSingleton.Release(g.gameObject);
		});
	}

	public virtual int GoalCounts()
	{
		return GetGoalProviders().Slinq().Aggregate(0, (int current, IGoalProvider goalProvider) => current + goalProvider.GoalCount());
	}

	public bool HasGoals()
	{
		return GoalCounts() > 0;
	}

	public string GetAssignationSound(Labor labor)
	{
		return AssignationSoundFunc(labor);
	}

	public int CrewCount()
	{
		return Workforce.Count;
	}

	public bool CanHire(Labor labor)
	{
		Thought thought;
		return CanHire(labor, out thought);
	}

	public virtual bool HasWorkFor(Labor labor)
	{
		return HasGoals() && HasRoomFor(labor);
	}

	public virtual Node VerifyWorkFor(Labor labor)
	{
		Var<bool> var = Var<bool>.Request(x: false);
		StaticNode staticNode = ((!NeedsToStore()) ? Node.Empty : GetVerifyStorageNode(labor, var));
		StaticNode staticNode2 = ((!NeedsToFetch()) ? Node.Empty : GetVerifyResourcesNode(labor, var));
		if (staticNode == Node.Empty && staticNode2 == Node.Empty)
		{
			return Node.Empty;
		}
		return (StaticNode)Node.Sequence.Do(staticNode).Label("Evaluate project storing needs").Do(staticNode2)
			.Label("Evaluate project resource needs")
			.Do((Var<bool> _result) => (!_result.Get()) ? Node.FailWork<Exceptions.NoCorrespondingStorage>() : Node.Empty, var)
			.Finally(delegate(Var<bool> _result)
			{
				_result.Dispose();
			}, var);
	}

	protected StaticNode GetVerifyResourcesNode(Labor labor, Var<bool> result)
	{
		return Node.ForEach(delegate(List<Factory.AssetKey> list, HashSet<Factory.AssetKey> set)
		{
			set.Slinq().AddTo(list);
		}, UniqueItemsNeeded()).Process((Factory.AssetKey key, Labor _labor, Var<bool> _result) => VerifyResourceAccessFor(_labor, key, _result), labor, result);
	}

	protected StaticNode GetVerifyAllStorageNode(Labor labor, Var<bool> result)
	{
		return Node.ForEach(delegate(List<Factory.AssetKey> list, HashSet<Factory.AssetKey> set)
		{
			set.Slinq().AddTo(list);
		}, ItemsRequiringStorage()).Process((Factory.AssetKey key, Labor _labor, Var<bool> _result) => VerifyStorageForAll(_labor, key, _result), labor, result);
	}

	protected StaticNode GetVerifyStorageNode(Labor labor, Var<bool> result)
	{
		return Node.ForEach(delegate(List<Factory.AssetKey> list, HashSet<Factory.AssetKey> set)
		{
			set.Slinq().AddTo(list);
		}, ItemsRequiringStorage()).Process((Factory.AssetKey key, Labor _labor, Var<bool> _result) => VerifyOneStorageFor(_labor, key, _result), labor, result);
	}

	protected virtual bool NeedsToStore()
	{
		return UniqueItemsGiven().Count != 0;
	}

	protected virtual HashSet<Factory.AssetKey> ItemsRequiringStorage()
	{
		return UniqueItemsGiven();
	}

	protected bool NeedsToFetch()
	{
		return UniqueItemsNeeded().Count != 0;
	}

	private static Node VerifyOneStorageFor(Labor labor, Factory.AssetKey key, Var<bool> result)
	{
		if (key.IsNullOrInvalid())
		{
			return Node.Empty;
		}
		if ((bool)labor.BestRecepteurToStore(Factory.Peek(key)))
		{
			result.Set(x: true);
			return Node.WaitForTicks(1);
		}
		return Node.WaitForTicks(1);
	}

	private static Node VerifyStorageForAll(Labor labor, Factory.AssetKey key, Var<bool> result)
	{
		if (key.IsNullOrInvalid())
		{
			return Node.Empty;
		}
		if (!labor.BestRecepteurToStore(Factory.Peek(key)))
		{
			result.Set(x: false);
			return Node.WaitForTicks(1);
		}
		return Node.WaitForTicks(1);
	}

	private static Node VerifyResourceAccessFor(Labor labor, Factory.AssetKey key, Var<bool> result)
	{
		if (key.IsNullOrInvalid())
		{
			return Node.Empty;
		}
		Description description = Factory.Peek(key).GetComponent<Apparence>().Description;
		if ((bool)labor.Find(description))
		{
			result.Set(x: true);
			return Node.WaitForTicks(1);
		}
		return Node.WaitForTicks(1);
	}

	protected bool HasRoomFor(Labor labor)
	{
		if (workforceProvider.IsOverPopulated())
		{
			return false;
		}
		if (workforceProvider.IsFull())
		{
			return labor.Project == this;
		}
		return true;
	}

	public bool IsFull()
	{
		return workforceProvider.IsFull();
	}

	public virtual bool HasCleanup()
	{
		return false;
	}

	public virtual bool HasConnectedOrigins(Labor labor)
	{
		return false;
	}

	public ProjectPriority PriorityInput(Labor labor)
	{
		return new ProjectPriority
		{
			bias = 1f,
			crewSize = CrewCount(),
			remainingCrewSpace = MaximumCrewSize - CrewCount(),
			globalCrewSize = GlobalCrewCount(),
			isCurrent = ((labor.IsNullOrReleased() || !(labor.Project == this)) ? 0f : 1f),
			goalCount = GoalCounts(),
			distance = DistanceIncrement(this, labor),
			deathCount = DeathCount,
			crewAggro = CrewAggro,
			hasCleanup = ((!HasCleanup()) ? 0f : 1f),
			manualBias = (float)PriorityBias,
			connectedOrigins = ((!HasConnectedOrigins(labor)) ? 0f : 1f)
		};
	}

	public static float DistanceIncrement(MonoBehaviour op, Labor labor)
	{
		if (labor.IsNullOrReleased())
		{
			return 0f;
		}
		float num = Vector3.Distance(labor.transform.position, op.gameObject.transform.position);
		num = Mathf.Log(Mathf.Max(num * 0.05f, 1f), 2f);
		return Mathf.FloorToInt(num);
	}

	public static bool ProjectUnderCap()
	{
		return UIGameObserver.projects.all.Slinq().Count() < projectCap;
	}

	public virtual bool CanHire(Labor labor, out Thought thought)
	{
		thought = null;
		if (labor == null)
		{
			return false;
		}
		if (!workforceProvider.Accepts(labor))
		{
			thought = Exceptions.CharacterCantJoinProject.Thought;
			return false;
		}
		if (workforceProvider.IsFull() && labor.Project != this)
		{
			thought = Exceptions.ProjectFullWorker.Thought;
			return false;
		}
		return true;
	}

	public virtual bool Hire(Labor labor)
	{
		if (!(this is IdleProject) || (this is IdleProject && labor.Autonomy != AutonomyStatus.LockedToOperable))
		{
			labor.LeaveOperation();
		}
		if (labor.Project == this)
		{
			return true;
		}
		if (!CanHire(labor, out var thought))
		{
			labor.SetTemporaryThought(2f, thought);
			return false;
		}
		Project project = labor.Project;
		workforceProvider.AddObject(labor.gameObject);
		LuaHookBase.InvokeFromCLR(lua_OnSetCrewInfoA, project);
		LuaHookBase.InvokeFromCLR(lua_OnSetCrewInfoA, this);
		return true;
	}

	public void Fire(Labor labor)
	{
	}

	public virtual void OnHire(Labor labor)
	{
		projectDatabase.AddWorker(this);
		CrewAggro += labor.Profession.RaidProjectAggro;
		OnPopulationChanged();
		_populationIncreased.Invoke(labor);
	}

	public virtual void OnFire(Labor labor)
	{
		if (!Architecte.termine)
		{
			projectDatabase.RemoveWorker(this);
			CrewAggro -= labor.Profession.RaidProjectAggro;
			OnPopulationChanged();
			_populationDecreased.Invoke(labor);
			if (Workforce.Count == 0)
			{
				OnBecameEmpty();
			}
		}
	}

	public void FireAll()
	{
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdRemoveAllLabor(base.gameObject);
	}

	public ICollection<Labor> Crew()
	{
		return Workforce;
	}

	protected void SelfDestructWhen(IReadOnlySignal signal)
	{
		signal.ConnectOnce(delegate(Project project)
		{
			project.SelfDestruct();
		}, this);
	}

	protected void SelfDestruct()
	{
		if (!this.IsNullOrReleased() && !AutoDeleteDisabled)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	protected void SetGoalSelectionModel(GoalSelector.Type type, bool chooseWithAccess = false)
	{
		goalSelector = GoalSelector.GetSelectorFromType(type, errorStore, chooseWithAccess);
	}

	protected Node SelectBestGoal<TEnumerator>(TEnumerator goals, Labor labor, Var<Goal> result) where TEnumerator : IEnumerator<Goal>
	{
		return goalSelector.SelectBestGoal(labor, goals, this, result);
	}

	protected Node SelectBestGoalPooledList<TEnumerator>(TEnumerator goalsRaw, Labor labor, Var<Goal> result) where TEnumerator : IEnumerator<Goal>
	{
		PooledListEnumerator<Goal> goals = PooledListEnumerator<Goal>.Request(goalsRaw);
		return goalSelector.SelectBestGoal(labor, goals, this, result);
	}

	protected abstract Node SelectBestGoal(Labor labor, Var<Goal> result);

	public void StopAllMovement()
	{
		foreach (Labor item in Workforce)
		{
			if (!item.IsNullOrReleased())
			{
				Order.Move.CompleteStop.Send(item, 0f);
			}
		}
	}

	public void CancelAllWork()
	{
		foreach (Labor item in Workforce)
		{
			item.AbortWorkSignal.Invoke(TaskResult.ProjectCancelledTask);
		}
	}

	public override bool CheckTag(string tag)
	{
		if (this is BuildProject)
		{
			return "Blocs".Contains(tag);
		}
		return base.CheckTag(tag);
	}

	public virtual bool ReadyToAssignGoals()
	{
		return !Editing() && Commited != CommitStatus.uncommited;
	}

	public virtual bool Editing()
	{
		return (bool)Balise && Balise.Editing;
	}

	public void SignalCommitmentChange()
	{
		LuaHookBase.InvokeFromCLR(lua_OnSetCommitStatus, this);
	}

	public virtual void Commit()
	{
		commited = true;
	}

	public virtual void CancelCommit()
	{
	}

	public virtual bool HasUncommitedGoals()
	{
		return false;
	}

	public virtual Node GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		Sequencer.Builder n = Node.Sequence.Do((Project self, Labor _labor) => self.SelectBestGoal(_labor, _labor.ResultGoal), this, labor).Do((Var<Goal> goal, Project self, Var<OrderPackage<Labor>> instruction) => Node.Selector.If((Var<Goal> g) => g.Get().IsNullOrReleased(), goal).Do(Node.Fail(FailException.Generic)).Else()
			.Do(delegate(Var<Goal> g, Project project, Var<OrderPackage<Labor>> i)
			{
				project._GetInstruction(g.Get(), i);
			}, goal, self, instruction), labor.ResultGoal, this, result);
		return (StaticNode)Node.Selector.If((Project self) => self.ReadyToAssignGoals(), this).Do(n).Else()
			.Do(Node.Fail(FailException.Generic));
	}

	private void _GetInstruction(Goal goal, Var<OrderPackage<Labor>> result)
	{
		OrderPackage<Labor> orderPackage = goal.Package(Labor.DecisionSource.Project);
		orderPackage.SetPrepare(GetPrepareFunction(goal));
		result.Set(orderPackage);
	}

	public Func<BaseLabor, bool> GetPrepareFunction(Goal goal)
	{
		return delegate(BaseLabor labor)
		{
			goal.ConnectProject(this);
			return Hire(labor as Labor);
		};
	}

	public GameObject GetGameObject()
	{
		return (!this.IsNullOrReleased()) ? base.gameObject : null;
	}

	public int GlobalCrewCount()
	{
		return projectDatabase.WorkerInTaskSimilarTasksTo(this);
	}

	public void RaisePriority()
	{
		if (PriorityBias == ProjectPriority.Bias.Locked)
		{
			PriorityBias = ProjectPriority.Bias.Low;
		}
		else if (PriorityBias == ProjectPriority.Bias.Low)
		{
			PriorityBias = ProjectPriority.Bias.Medium;
		}
		else if (PriorityBias == ProjectPriority.Bias.Medium)
		{
			PriorityBias = ProjectPriority.Bias.High;
		}
	}

	public void LowerPriority()
	{
		if (PriorityBias == ProjectPriority.Bias.High)
		{
			PriorityBias = ProjectPriority.Bias.Medium;
		}
		else if (PriorityBias == ProjectPriority.Bias.Medium)
		{
			PriorityBias = ProjectPriority.Bias.Low;
		}
		else if (PriorityBias == ProjectPriority.Bias.Low)
		{
			PriorityBias = ProjectPriority.Bias.Locked;
		}
	}

	public ProjectPriority.Bias GetPriority()
	{
		return PriorityBias;
	}

	public virtual void OnLaborError(Labor labor, Exceptions.WorkFailedException e)
	{
		logger.InfoFormat(e, "Labor '{0}' failed with error '{1}'", labor, e.Problem.thought);
	}

	public static void RegisterClass(Script script)
	{
		if (lua_OnSetCrewInfo == null)
		{
			lua_OnSetCrewInfo = new LuaHookCollapsed<Project>(PersistentCoroutineRunner.UpdateSignal, new LuaHook<Project>[5] { lua_OnSetCrewInfoA, lua_OnSetCrewInfoB, lua_OnSetCrewInfoC, lua_OnSetCrewInfoPriority, lua_OnSetSelectedCrewCount }, noDoubles: true);
		}
		DynValue dynValue = DynValue.NewTable(script);
		dynValue.Table.Set("hk_OnProjectStarted", UserData.CreateWithDescriptor(new LuaHook<Project>(ProjectStarted), LuaHookBase.desc));
		dynValue.Table.Set("hk_OnProjectDeleted", UserData.CreateWithDescriptor(new LuaHook<Project>(ProjectDeleted), LuaHookBase.desc));
		dynValue.Table.Set("hk_OnProjectSelected", UserData.CreateWithDescriptor(lua_Selected, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnProjectUnselected", UserData.CreateWithDescriptor(lua_Unselected, LuaHookBase.desc));
		dynValue.Table.Set("hk_InventoryChanged", UserData.CreateWithDescriptor(lua_InventoryChanged, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnIconChanged", UserData.CreateWithDescriptor(lua_OnIconChanged, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetCrewInfoA", UserData.CreateWithDescriptor(lua_OnSetCrewInfoA, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetCrewInfoB", UserData.CreateWithDescriptor(lua_OnSetCrewInfoB, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetCrewInfoC", UserData.CreateWithDescriptor(lua_OnSetCrewInfoC, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetCrewInfo", UserData.CreateWithDescriptor(lua_OnSetCrewInfo, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetCrewInfoPriority", UserData.CreateWithDescriptor(lua_OnSetCrewInfoPriority, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetSelectedCrewCount", UserData.CreateWithDescriptor(lua_OnSetSelectedCrewCount, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetGoalCount", UserData.CreateWithDescriptor(new LuaHook<Project>(OnSetGoalCountSignal), LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetCrewCount", UserData.CreateWithDescriptor(new LuaHook<Project>(OnSetCrewCountSignal), LuaHookBase.desc));
		dynValue.Table.Set("hk_OnSetStorageCount", UserData.CreateWithDescriptor(new LuaHook<Project>(OnSetStorageCountSignal), LuaHookBase.desc));
		dynValue.Table.Set("hk_OnAddLabor2", UserData.CreateWithDescriptor(lua_onAddLabor2, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnRemoveLabor2", UserData.CreateWithDescriptor(lua_OnRemoveLabor2, LuaHookBase.desc));
		dynValue.Table.Set("hk_OnTunnelStairChanged", UserData.CreateWithDescriptor(new LuaHook(TunnelVolume.ExpansionTunnelTypeChanged), LuaHookBase.desc));
		dynValue.Table.Set("GetSelected", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetSelected(exec, args)));
		dynValue.Table.Set("GetProjectCap", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetProjectCap(exec, args)));
		dynValue.Table.Set("GetAll", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetAll(exec, args)));
		dynValue.Table.Set("ProjectUnderCap", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_ProjectUnderCap(exec, args)));
		dynValue.Table.Set("hk_OnSetStatus", UserData.CreateWithDescriptor(lua_OnSetCommitStatus, LuaHookBase.desc));
		dynValue.Table.Set("StatusAction", descCommitStatusAction.CreateDynValue());
		dynValue.Table.Set("GetGoalCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetGoalCount(exec, args)));
		dynValue.Table.Set("ShowStatusAction", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_ShowStatusAction(exec, args)));
		dynValue.Table.Set("HasStatusAction", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_HasStatusAction(exec, args)));
		dynValue.Table.Set("SetStatusAction", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SetStatusAction(exec, args)));
		dynValue.Table.Set("SelectMeleeUnits", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SelectMeleeUnits(exec, args)));
		dynValue.Table.Set("SelectRangedUnits", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SelectRangedUnits(exec, args)));
		dynValue.Table.Set("Priority", descPriority.CreateDynValue());
		dynValue.Table.Set("GetPriority", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetPriority(exec, args)));
		dynValue.Table.Set("AssignSelection", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_AssignSelection(exec, args)));
		dynValue.Table.Set("UnassignSelection", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_UnassignSelection(exec, args)));
		dynValue.Table.Set("SelectWorkers", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SelectWorkers(exec, args)));
		dynValue.Table.Set("LowerPriority", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_LowerPriority(exec, args)));
		dynValue.Table.Set("RaisePriority", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_RaisePriority(exec, args)));
		dynValue.Table.Set("GetMaxCrewCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetMaxCrewCount(exec, args)));
		dynValue.Table.Set("GetFreeAgentCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetFreeAgentCount(exec, args)));
		dynValue.Table.Set("GetSelectedCrewCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetSelectedCrewCount(exec, args)));
		dynValue.Table.Set("GetAssignedCrewCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetAssignedCrewCount(exec, args)));
		dynValue.Table.Set("IsBeingPlaced", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_IsBeingPlaced(exec, args)));
		dynValue.Table.Set("GetGlobalFreeAgentCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetGlobalFreeAgentCount(exec, args)));
		dynValue.Table.Set("AssignLabor", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_AssignLabor(exec, args)));
		dynValue.Table.Set("Select", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_Select(exec, args)));
		dynValue.Table.Set("Deselect", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_Deselect(exec, args)));
		dynValue.Table.Set("GetCrew", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetCrew(exec, args)));
		dynValue.Table.Set("GetCrewCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetCrewCount(exec, args)));
		dynValue.Table.Set("ToggleProjectInvisibility", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_ToggleProjectInvisibility(exec, args)));
		dynValue.Table.Set("GetFightersInfo", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetFightersInfo(exec, args)));
		dynValue.Table.Set("GetIdlersInfo", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetIdlersInfo(exec, args)));
		dynValue.Table.Set("GetBuildersInfo", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetBuildersInfo(exec, args)));
		dynValue.Table.Set("raidTypeEnum", descRaidType.CreateDynValue());
		LuaProjectStorage.AddToLuaTable(script, dynValue);
		script.Globals.Set("Project", dynValue);
	}

	private static DynValue lua_GetGlobalFreeAgentCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		return DynValue.NewNumber((from b in UIGameObserver.bricktrons.CheckCreation().Slinq()
			where b.Autonomy == AutonomyStatus.FreeAgent
			select b).Count());
	}

	private static DynValue lua_GetSelected(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		return DynValue.FromObject(executionContext.OwnerScript, UIGameObserver.projects.all.Slinq().FirstOrDefault((Project project) => project.selection));
	}

	private static DynValue lua_GetProjectCap(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		return DynValue.NewNumber(projectCap);
	}

	private static DynValue lua_GetAll(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		return DynValue.FromObject(executionContext.OwnerScript, UIGameObserver.projects.all.Slinq().ToList());
	}

	private static DynValue lua_ProjectUnderCap(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		return DynValue.NewBoolean(UIGameObserver.projects.all.Slinq().Count() < projectCap);
	}

	[LuaDrivable("GetFightersInfo")]
	private static DynValue lua_GetFightersInfo(ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = new Table(context.OwnerScript);
		Dictionary<Occupation.Job, int> dictionary = CollectionPool.RequestDictionary<Occupation.Job, int>();
		Dictionary<Occupation.Job, int> dictionary2 = CollectionPool.RequestDictionary<Occupation.Job, int>();
		Occupation.Job[] jobs = Jobs;
		foreach (Occupation.Job key in jobs)
		{
			dictionary.Add(key, 0);
			dictionary2.Add(key, 0);
		}
		int num = 0;
		foreach (Labor item in UIGameObserver.bricktrons.CheckCreation())
		{
			dictionary[item.Occupation.CurrentJob]++;
			num++;
		}
		foreach (Labor item2 in UIGameObserver.bricktrons.CheckSelection())
		{
			dictionary2[item2.Occupation.CurrentJob]++;
		}
		foreach (KeyValuePair<Occupation.Job, int> item3 in dictionary)
		{
			Table table2 = new Table(context.OwnerScript);
			table2.Set("count", DynValue.NewNumber(item3.Value));
			table2.Set("selectedCount", DynValue.NewNumber(dictionary2[item3.Key]));
			table.Set(DynValue.FromObject(context.OwnerScript, item3.Key), DynValue.NewTable(table2));
		}
		table.Set("count", DynValue.FromObject(context.OwnerScript, DynValue.NewNumber(num)));
		dictionary.Dispose();
		dictionary2.Dispose();
		return DynValue.NewTable(table);
	}

	[LuaDrivable("GetIdlersInfo")]
	private static DynValue lua_GetIdlersInfo(ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = new Table(context.OwnerScript);
		List<LuaDriver> list = CollectionPool.RequestList<LuaDriver>();
		List<LuaDriver> list2 = CollectionPool.RequestList<LuaDriver>();
		List<LuaDriver> list3 = CollectionPool.RequestList<LuaDriver>();
		foreach (Labor item in UIGameObserver.bricktrons.CheckCreation())
		{
			list.Add(item.luaDriver);
			if (item.Occupation.CurrentJob != Occupation.Job.Builder)
			{
				list2.Add(item.luaDriver);
			}
			else if (item.selection)
			{
				list3.Add(item.luaDriver);
			}
		}
		table.Set("all", DynValue.FromObject(context.OwnerScript, list));
		table.Set("busy", DynValue.FromObject(context.OwnerScript, list2));
		table.Set("selected", DynValue.FromObject(context.OwnerScript, list3));
		list.Dispose();
		list2.Dispose();
		list3.Dispose();
		return DynValue.NewTable(table);
	}

	[LuaDrivable("GetBuildersInfo")]
	private static DynValue lua_GetBuildersInfo(ScriptExecutionContext context, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetBuildersInfo");
		Table table = new Table(context.OwnerScript);
		List<LuaDriver> list = CollectionPool.RequestList<LuaDriver>();
		List<LuaDriver> list2 = CollectionPool.RequestList<LuaDriver>();
		foreach (Labor item in project.Crew().Slinq().NoLeak())
		{
			if (item.Occupation.CurrentJob == Occupation.Job.Builder)
			{
				list.Add(item.luaDriver);
				if (item.selection)
				{
					list2.Add(item.luaDriver);
				}
			}
		}
		table.Set("all", DynValue.FromObject(context.OwnerScript, list));
		table.Set("selected", DynValue.FromObject(context.OwnerScript, list2));
		list.Dispose();
		list2.Dispose();
		return DynValue.NewTable(table);
	}

	private static DynValue lua_GetSelectedCrewCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetSelectedCrewCount");
		int num = 0;
		foreach (Labor item in project.Crew().Slinq().NoLeak())
		{
			if (item.selection)
			{
				num++;
			}
		}
		return DynValue.NewNumber(num);
	}

	protected static DynValue lua_ShowStatusAction(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "ShowCommitStatusAction");
		return args.AsEnum<CommitStatusAction>(1, "ShowCommitStatusAction") switch
		{
			CommitStatusAction.Ok => (!ShowOk(project)) ? DynValue.False : DynValue.True, 
			CommitStatusAction.Apply => (!ShowApply(project)) ? DynValue.False : DynValue.True, 
			CommitStatusAction.Cancel => (!ShowCancel(project)) ? DynValue.False : DynValue.True, 
			CommitStatusAction.Delete => (!ShowDelete(project)) ? DynValue.False : DynValue.True, 
			_ => DynValue.Void, 
		};
	}

	protected static DynValue lua_HasStatusAction(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "HasCommitStatusAction");
		return args.AsEnum<CommitStatusAction>(1, "HasCommitStatusAction") switch
		{
			CommitStatusAction.Ok => (!HasOk(project)) ? DynValue.False : DynValue.True, 
			CommitStatusAction.Apply => (!HasApply(project)) ? DynValue.False : DynValue.True, 
			CommitStatusAction.Cancel => (!HasCancel(project)) ? DynValue.False : DynValue.True, 
			CommitStatusAction.Delete => (!HasDelete(project)) ? DynValue.False : DynValue.True, 
			_ => DynValue.False, 
		};
	}

	protected static DynValue lua_SetStatusAction(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "SetCommitStatusAction");
		switch (args.AsEnum<CommitStatusAction>(1, "SetCommitStatusAction"))
		{
		case CommitStatusAction.Ok:
			SetOk(project);
			break;
		case CommitStatusAction.Apply:
			SetApply(project);
			break;
		case CommitStatusAction.Cancel:
			SetCancel(project);
			break;
		case CommitStatusAction.Delete:
			SetDelete(project);
			break;
		}
		return DynValue.Void;
	}

	private static bool ShowOk(Project project)
	{
		return true;
	}

	private static bool ShowApply(Project project)
	{
		return true;
	}

	private static bool ShowCancel(Project project)
	{
		return project is BuildProject;
	}

	private static bool ShowDelete(Project project)
	{
		return true;
	}

	private static bool HasOk(Project project)
	{
		return LuaProjectStorage.IsStorageManagementReady(project);
	}

	private static bool HasApply(Project project)
	{
		if (project is BuildProject)
		{
			return project.HasUncommitedGoals();
		}
		return !project.commited;
	}

	private static bool HasCancel(Project project)
	{
		if (project is BuildProject)
		{
			return project.Commited == CommitStatus.partial;
		}
		return !project.commited;
	}

	private static bool HasDelete(Project project)
	{
		return true;
	}

	private static void SetOk(Project project)
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdCommit(project.gameObject);
		UIGameActor.SelectIdleProject();
	}

	private static void SetApply(Project project)
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdCommit(project.gameObject);
	}

	private static void SetCancel(Project project)
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdCancelCommit(project.gameObject);
	}

	private static void SetDelete(Project project)
	{
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdDelete(project.gameObject);
	}

	private static DynValue lua_GetGoalCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetGoalCount");
		return DynValue.NewNumber(project.GoalCounts());
	}

	protected static DynValue lua_AssignLabor(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "AssignLabor");
		Labor labor = args.AsComponent<Labor>(1, "AssignLabor");
		if (labor == null)
		{
			return DynValue.Void;
		}
		if (labor.faction != project.faction)
		{
			logger.WarnFormat("Labor {0} is of incompatible faction {1}, project is {2}.", labor, labor.faction, project.faction);
			return DynValue.Void;
		}
		DirectOrderProject.ChangeProject(labor, project);
		return DynValue.Void;
	}

	protected static DynValue lua_SelectWorkers(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "SelectWorkers");
		bool includeFixedAgents = args.AsBool(1, "includeFixedAgents");
		bool includeFreeAgents = args.AsBool(2, "includeFreeAgents");
		UIGameActor.SelectIdleProject();
		UIGameActor.SelectProjectMember(project, includeFixedAgents, includeFreeAgents);
		return DynValue.Void;
	}

	protected static DynValue lua_SelectMeleeUnits(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Occupation.Type occupation = Occupation.Type.Knight;
		Faction faction = args.AsComponent<Faction>(0, "SelectMeleeUnits");
		UIGameActor.SelectIdleProject();
		UIGameActor.SelectByOccupation(occupation, faction);
		return DynValue.Void;
	}

	protected static DynValue lua_SelectRangedUnits(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Occupation.Type occupation = Occupation.Type.Archer;
		Faction faction = args.AsComponent<Faction>(0, "SelectRangedUnits");
		UIGameActor.SelectIdleProject();
		UIGameActor.SelectByOccupation(occupation, faction);
		return DynValue.Void;
	}

	protected static DynValue lua_AssignSelection(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "AssignSelection");
		UIGameActor.AssignSelectionToProject(project);
		return DynValue.Void;
	}

	protected static DynValue lua_UnassignSelection(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "UnassignSelection");
		if (project is IdleProject)
		{
			UIGameActor.MakeSelectionFreeAgents();
			return DynValue.Void;
		}
		UIGameActor.AssignSelectionToProject(IdleProject.GetAlliedProject());
		return DynValue.Void;
	}

	protected static DynValue lua_Select(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "Select");
		UIGameSelector.Select(obj);
		return DynValue.Void;
	}

	protected static DynValue lua_Deselect(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		UIGameActor.SelectIdleProject();
		return DynValue.Void;
	}

	protected static DynValue lua_GetCrew(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetCrew");
		return DynValue.FromObject(executionContext.OwnerScript, (from l in project.Crew().Slinq()
			where !l.IsNullOrReleased()
			select l).ToList());
	}

	protected static DynValue lua_GetCrewCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetCrewCount");
		return DynValue.NewNumber(project.CrewCount());
	}

	private static DynValue lua_AddFreeAgentSlot(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject instance = args.AsGameObject(0, "AddFreeAgentSlot");
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdAddProjectCapacity(instance);
		return DynValue.Void;
	}

	private static DynValue lua_RemoveFreeAgentSlot(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject instance = args.AsGameObject(0, "RemoveFreeAgentSlot");
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdRemoveProjectCapacity(instance);
		return DynValue.Void;
	}

	private static DynValue lua_IsBeingPlaced(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "IsBeingPlaced");
		return DynValue.NewBoolean(project.IsBeingPlaced);
	}

	private static DynValue lua_GetAssignedCrewCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetAssignedCrewCount");
		return DynValue.NewNumber(project.workforceProvider.AssignedCount());
	}

	private static DynValue lua_GetFreeAgentCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetFreeAgentCount");
		return DynValue.NewNumber(project.workforceProvider.FreeAgentCount());
	}

	private static DynValue lua_GetMaxCrewCount(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetMaxCrewCount");
		return DynValue.NewNumber(project.MaximumCrewSize);
	}

	private static DynValue lua_RaisePriority(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject instance = args.AsGameObject(0, "RaisePriority");
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdRaisePriority(instance);
		return DynValue.Void;
	}

	private static DynValue lua_LowerPriority(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject instance = args.AsGameObject(0, "LowerPriority");
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdLowerPriority(instance);
		return DynValue.Void;
	}

	private static DynValue lua_GetPriority(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetPriority");
		return UserData.CreateWithDescriptor(new LuaEnumProxy<ProjectPriority.Bias>(project.GetPriority()), descPriority);
	}

	public static DynValue lua_ToggleProjectInvisibility(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "ToggleProjectInvisibility");
		project.shouldBeInvisible = !project.shouldBeInvisible;
		return DynValue.Void;
	}

	public void MakeInvisible()
	{
		if ((bool)GetComponentInChildren<BaliseOverheadDisplayDriver>(includeInactive: true))
		{
			GetComponentInChildren<BaliseOverheadDisplayDriver>(includeInactive: true).enabled = false;
		}
		Component componentInChildren = GetComponentInChildren<BaliseVisuals>(includeInactive: true);
		componentInChildren.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (shouldBeInvisible)
		{
			MakeInvisible();
		}
	}

	IReadOnlySignal<GameObject> IOperable<Labor>.get_OnReleaseOrDestroySignal()
	{
		return base.OnReleaseOrDestroySignal;
	}

	GameObject IOperable<Labor>.get_gameObject()
	{
		return base.gameObject;
	}
}
