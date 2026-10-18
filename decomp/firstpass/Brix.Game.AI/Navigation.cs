using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.Nodes;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Lifecycle.Pooling;
using Brix.Pathfinding;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

[RequireComponent(typeof(Labor))]
[RequireComponent(typeof(VoxelPresence))]
public class Navigation : GameComponent
{
	public class NavigateOrderData : IPoolable, Brix.Utils.IResetable, IDisposable
	{
		public Labor blocker;

		public Func<XYZ, bool> presenceTest;

		public bool firstTime;

		public Vector3 targetInitialPosition;

		public bool invalidTransfer;

		public bool active;

		public bool done;

		public Navigation navigation;

		public Location location;

		public bool exact;

		public bool precise;

		public bool noHalt;

		public bool away;

		public bool aggressive;

		private SignalConnection _requestTransferConnection = SignalConnection.Empty;

		private readonly Action<Labor, Labor> _catchTransferAction;

		private readonly Action _listenToTransferAction;

		private readonly Action _onFinallyAction;

		private readonly Action _onSearchFinishedAction;

		private readonly Func<bool> _canMoveFunc;

		private SearchPathRequest _searchRequest;

		private Path _searchResult;

		public Path SearchResult => _searchResult;

		public NavigateOrderData()
		{
			_catchTransferAction = CatchTransfer;
			_listenToTransferAction = ListenToTransfers;
			_onFinallyAction = OnFinally;
			_onSearchFinishedAction = OnSearchFinished;
			_canMoveFunc = () => navigation.Locomotion.CanMove();
		}

		public NavigateOrderData Init(Navigation navigation, Location location, bool exact, bool precise, bool noHalt, bool away, bool aggressive)
		{
			this.navigation = navigation;
			this.location = location;
			this.exact = exact;
			this.precise = precise;
			this.noHalt = noHalt;
			this.away = away;
			this.aggressive = aggressive;
			invalidTransfer = false;
			active = false;
			done = false;
			blocker = null;
			bool tryPassThrough = navigation.Labor.Profession.CanPassThrough();
			presenceTest = navigation.Presence.BuildPresenceTest(tryPassThrough, aggressive);
			firstTime = true;
			targetInitialPosition = location.Position;
			return this;
		}

		public void ResetLookupValues()
		{
			firstTime = false;
			blocker = null;
			bool tryPassThrough = navigation.Labor.Profession.CanPassThrough();
			presenceTest = navigation.Presence.BuildPresenceTest(tryPassThrough, aggressive);
			targetInitialPosition = location.Position;
		}

		public void Dispose()
		{
			ResetObject();
		}

		public void ResetObject()
		{
			_requestTransferConnection.Disconnect();
		}

		public Node WrapWithTransferListener(StaticNode node)
		{
			return (StaticNode)Node.Sequence.Do(_listenToTransferAction).Do(node).Finally(_onFinallyAction);
		}

		private void ListenToTransfers()
		{
			_requestTransferConnection = navigation.RequestTransferSignal.Connect(_catchTransferAction);
		}

		private void CatchTransfer(Labor labor, Labor blocker)
		{
			this.blocker = blocker;
		}

		private void OnFinally()
		{
			_requestTransferConnection.Disconnect();
		}

		private void InitSearch()
		{
			if (_searchRequest == null || !_searchRequest.Initialized)
			{
				_searchRequest = SearchPathRequest.Make();
			}
			_searchRequest.InitNavigation(navigation);
			_searchRequest.From = navigation.Presence.CurrentVoxel;
			_searchRequest.Location = location;
			_searchRequest.Exact = exact;
			_searchRequest.Precise = precise;
			_searchRequest.PresenceTest = presenceTest;
			_searchRequest.CanClimb = navigation.Interaction.CanClimb();
			_searchRequest.CanWalkInForest = navigation.Labor.Profession.CanWalkInForest();
			_searchRequest.CanBreakVoxels = navigation.Labor.Profession.CanMakeWay();
			_searchRequest.SearchAway = away;
			_searchRequest.SearchAwayDistance = 15;
			_searchRequest.LodCount = ((!away) ? _searchRequest.Pathfinding.lodCount : 3);
			_searchRequest.SearchLimit = ((!away) ? (-1) : 100);
			_searchRequest.SearchPerUpdate = ((!navigation.faction.isAI) ? navigation.Labor.Autonomy.PathfindingBudget : GeneralRules.CorruptronSearchPathBudget);
		}

		private void OnSearchFinished()
		{
			if (_searchRequest.Search != null)
			{
				_searchRequest.Search.Clear();
			}
			_searchRequest.Dispose();
			_searchRequest = null;
		}

		private void CopyResultLocally()
		{
			PathUnroll resultPath = _searchRequest.ResultPath;
			if (_searchResult == null)
			{
				_searchResult = new Path(new List<WalkNode>());
			}
			else
			{
				for (int i = 0; i < _searchResult.nodes.Count; i++)
				{
					ObjectPool<WalkNode>.Release(_searchResult.nodes[i]);
				}
				_searchResult.nodes.Clear();
			}
			_searchResult.destinationLocation = resultPath.destinationLocation;
			_searchResult.presenceTest = resultPath.presenceTest;
			resultPath.GetPathLocations(_searchResult.nodes);
		}

		public StaticNode Validate(float delayUntilFailure = 1f)
		{
			return Node.Selector.If((NavigateOrderData self) => !self.navigation.Locomotion.CanMove(), this).Do((NavigateOrderData self, float _delay) => UtilityNodes.WaitForTimeOrCondition(self._canMoveFunc, _delay), this, delayUntilFailure).Label("Wait until can move")
				.Do(Node.Selector.If((NavigateOrderData self) => !self.navigation.Locomotion.CanMove(), this).Do(Node.FailPathTo<Exceptions.CantMove>(location)));
		}

		public StaticNode Search()
		{
			return Node.Sequence.Do(delegate(NavigateOrderData self)
			{
				self.InitSearch();
			}, this).Do((NavigateOrderData self) => self._searchRequest.ComputePath(), this).Label("ComputePath")
				.Do(Node.Selector.If((NavigateOrderData self) => self._searchRequest.Succeeded, this).Do(delegate(NavigateOrderData self)
				{
					self.CopyResultLocally();
				}, this).If((NavigateOrderData self) => self._searchRequest.VisitedInvalidNodes, this)
					.Do(Node.FailPathTo<Exceptions.PathInvalid>(location))
					.If((NavigateOrderData self) => self.firstTime, this)
					.Do(Node.FailPathTo<Exceptions.PathUnfindable>(location))
					.Else()
					.Do(Node.FailPathTo<Exceptions.PathBlockedSecondTime>(location)))
				.Finally(_onSearchFinishedAction);
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(Navigation).FullName);

	public Signal<Labor, Labor> RequestTransferSignal = new Signal<Labor, Labor>();

	public Locomotion4 Locomotion;

	public Interaction Interaction;

	public VoxelPresence Presence;

	public Labor Labor;

	public Signal Blocked = new Signal();

	public PathDrawer PathDrawer;

	public Signal<bool> PathFinished = new Signal<bool>();

	public Signal PathSucceeded = new Signal();

	public Signal PathStarted = new Signal();

	public Signal PathInterrupted = new Signal();

	public Signal InvalidTransfer = new Signal();

	public Signal PathInterruptedManually = new Signal();

	private Func<bool> IsInPlaceFunc;

	private Func<bool> IsHaltingFunc;

	public static bool DrawPathIfSelected = false;

	private IPath CurrentPath;

	public bool ShowDebugPath;

	public IPath DebugPath;

	private FollowPathRequest _followRequest;

	public bool EnableDebugLogs;

	private Vector3 OriginOfBlockedVoxel;

	private Vector3 BlockedVoxel;

	private bool IsFollowingPath => _followRequest != null && _followRequest.IsActive;

	public XYZ PathIndex
	{
		get
		{
			XYZ xYZ = XYZ.FromVector3(base.transform.position);
			bool success;
			XYZ walkableVoxel = VoxelPresence.GetWalkableVoxel(xYZ, out success);
			if (success && walkableVoxel != xYZ)
			{
				logger.Warn("The character is off his voxel by a few digits in height. Please tell that to the animation programer.");
			}
			return walkableVoxel;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		Locomotion = GetComponent<Locomotion4>();
		Interaction = GetComponent<Interaction>();
		Presence = GetComponent<VoxelPresence>();
		Labor = GetComponent<Labor>();
		IsInPlaceFunc = () => Locomotion.IsInPlace();
		IsHaltingFunc = () => Locomotion.IsHalting;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearFollowNode();
	}

	public Node NavigateTo(Location location, bool exact = true, bool precise = true, bool aggressive = false)
	{
		return NavigateToOrAway(location, exact, precise, nohalt: true, away: false, aggressive);
	}

	public Node NavigateAway(Location location)
	{
		return NavigateToOrAway(location, exact: false, precise: true, nohalt: true, away: true, aggressive: false);
	}

	private Node NavigateToOrAway(Location location, bool exact, bool precise, bool nohalt, bool away, bool aggressive)
	{
		if (location == null)
		{
			return Node.FailPathTo<Exceptions.PathInvalid>(null);
		}
		if (base.faction == User.LocalUser.faction && !(location is CharacterLocation) && Labor.Autonomy.ShowMoveBorne)
		{
			ObjectPoolSingleton.Request_Unsafe(Brix.External.Factories.UI.Borne, Architecte.SurfaceRaycast(location.Position), Quaternion.Euler(270f, 0f, 0f));
		}
		NavigateOrderData data = ObjectPool<NavigateOrderData>.Reserve().Init(this, location, exact, precise, nohalt, away, aggressive);
		Sequencer.Builder n = Node.Sequence.Do((Navigation self, NavigateOrderData _data) => self.SearchPathOrTransferTo(_data), this, data).Label("SearchPathOrTransferTo").Do(delegate(NavigateOrderData _data)
		{
			_data.ResetLookupValues();
		}, data)
			.Label("Reset lookup values")
			.Do((Navigation self) => self.TryFollowNode(), this)
			.Label("Navigating");
		Brancher.Builder builder = Node.Selector.When(PathSucceeded).Label("When PathSuceeded", base.gameObject).Do((Navigation self, NavigateOrderData _data) => (!_data.noHalt) ? self.Halt() : Node.Empty, this, data)
			.Label("Do HaltIfPossible", base.gameObject)
			.Do((NavigateOrderData _data) => _data.done = true, data)
			.Label("done")
			.When(PathInterrupted)
			.Label("When PathInterrupted", base.gameObject)
			.Do((Navigation self) => self.HaltAndWaitForInPlace(8f), this)
			.Label("Do Halt", base.gameObject)
			.Do((Navigation self, NavigateOrderData _data, bool _noHalt) => _data.noHalt = _noHalt, this, data, nohalt)
			.Label("reset halt")
			.Do((NavigateOrderData _data) => _data.active = false, data)
			.Label("reset active")
			.Do(Node.WaitForTicks(1))
			.Label("wait for selector")
			.When(RequestTransferSignal)
			.Label("When RequestTransfer signal received")
			.Do((Navigation self) => self.Halt(), this)
			.Label("RequestTransfer.Halt")
			.Do((Navigation self, NavigateOrderData _data, bool _noHalt) => _data.noHalt = _noHalt, this, data, nohalt)
			.Do((Navigation self, NavigateOrderData _data) => Node.WaitForTicks(self.GetTransferDelay(_data.blocker)), this, data)
			.Label("WaitFor Transfer Delay", base.gameObject)
			.Do((NavigateOrderData _data) => _data.active = false, data)
			.Label("reset active")
			.Do(Node.WaitForTicks(1))
			.Label("wait for selector")
			.When((Navigation self, NavigateOrderData _data) => self.NeedsRetargeting(_data), this, data)
			.Label("When Target has moved during path")
			.Do((NavigateOrderData _data) => _data.noHalt = true, data)
			.Label("TargetMoved.Halt")
			.Do((NavigateOrderData _data) => _data.active = false, data)
			.Label("reset active")
			.Do(Node.WaitForTicks(1))
			.Label("wait for selector")
			.When(() => !data.active && !data.done)
			.Label("When ready to search and move")
			.Do(() => data.active = true)
			.Label("set active")
			.Do(n)
			.Label("Search and move");
		return data.WrapWithTransferListener(builder);
	}

	private bool NeedsRetargeting(NavigateOrderData data)
	{
		return IsFollowingPath && data.location is ObjectLocation && data.location.Object != null && TargetMovedEnough(data.location.Position, data.targetInitialPosition, base.transform.position);
	}

	private bool TargetMovedEnough(Vector3 targetActualPosition, Vector3 targetLastPosition, Vector3 selfPosition)
	{
		float num = Vector3.Distance(targetActualPosition, targetLastPosition);
		float num2 = Vector3.Distance(targetActualPosition, selfPosition);
		int num3 = Mathf.Max(1, Mathf.CeilToInt((num2 - 2f) / 5f));
		return num >= (float)num3;
	}

	private Node SearchPathOrTransferTo(NavigateOrderData data)
	{
		return (StaticNode)Node.Catcher.Do((NavigateOrderData _data) => _data.Validate(), data).Label("Validate").Do((NavigateOrderData _data) => _data.Search(), data)
			.Label("SearchPathToOrAway")
			.Do(delegate(Navigation self, NavigateOrderData _data)
			{
				self.ProcessPathResult(_data.SearchResult, _data);
			}, this, data)
			.Label("Process SearchPathToOrAway result")
			.Catch((Exceptions.PathException e, Navigation self, NavigateOrderData _data) => self.ProcessPathFailure(e, _data), this, data);
	}

	private void ProcessPathResult(IPath p, NavigateOrderData data)
	{
		CurrentPath = p;
		data.blocker = null;
	}

	private Node ProcessPathFailure(Exceptions.PathException e, NavigateOrderData data)
	{
		if (e is Exceptions.PathInvalid)
		{
			return Node.FailPathTo<Exceptions.PathInvalid>(data.location);
		}
		if (e is Exceptions.PathBlockedSecondTime)
		{
			return Node.FailPathTo<Exceptions.PathBlockedSecondTime>(data.location);
		}
		if (data.blocker == null)
		{
			return Node.FailPathTo<Exceptions.PathUnfindable>(data.location);
		}
		data.blocker = null;
		return (StaticNode)Node.Sequence.Do(Node.WaitFor.Ticks(20)).Do(Node.FailPathTo<Exceptions.PathBlocked>(data.location));
	}

	private Node TryFollowNode()
	{
		return (StaticNode)Node.Sequence.Do(delegate
		{
			PathStarted.Invoke();
		}).Do(Node.Trier.Do(Node.Sequence.Do((Navigation self) => self.FollowNode(self.CurrentPath), this).Do(delegate(Navigation self)
		{
			self.PathSucceeded.Invoke();
		}, this).Do(() => Node.WaitForTicks(1))).Do(Node.Sequence.Do(delegate(Navigation self)
		{
			self.PathInterrupted.Invoke();
		}, this).Do(() => Node.WaitForTicks(1))));
	}

	private Node FollowNode(IPath p)
	{
		return (StaticNode)Node.Sequence.Do(delegate(Navigation self, IPath path)
		{
			self.FollowNodeStart(path);
		}, this, p).Do((Navigation self) => self._followRequest.FollowNode(), this).Finally(delegate(Navigation self)
		{
			self.ClearFollowNode();
		}, this);
	}

	private void FollowNodeStart(IPath p)
	{
		ClearFollowNode();
		_followRequest = FollowPathRequest.Make(this, p);
	}

	private void ClearFollowNode()
	{
		if (_followRequest != null)
		{
			_followRequest.Dispose();
			_followRequest = null;
		}
	}

	public StaticNode Halt()
	{
		return Node.Do(delegate(Navigation self)
		{
			self.Halt(self.transform.position, roundPosition: true);
		}, this);
	}

	public StaticNode HaltAndWaitForInPlace(float delayUntilFailure = 1f)
	{
		return Node.Sequence.Do(delegate(Navigation self)
		{
			self.Halt(self.transform.position, roundPosition: true);
		}, this).Label("Halt").Do((Navigation self, float _delay) => UtilityNodes.WaitForTimeOrCondition(self.IsHaltingFunc, 1f), this, delayUntilFailure)
			.Label("Wait to start Halting")
			.Do((Navigation self, float _delay) => UtilityNodes.WaitForTimeOrCondition(self.IsInPlaceFunc, _delay, failOnDelay: true), this, delayUntilFailure)
			.Label("Wait to become InPlace again");
	}

	private void Halt(Vector3 haltPosition, bool roundPosition)
	{
		if (roundPosition)
		{
			haltPosition = Calcule.RoundVecteur(haltPosition);
		}
		Locomotion.DoHalt(new SpaceLocation(haltPosition, Quaternion.LookRotation(base.transform.forward.Flatten(), Vector3.up)));
	}

	private void SoftHalt(Vector3 haltPosition, bool roundPosition)
	{
		if (roundPosition)
		{
			haltPosition = Calcule.RoundVecteur(haltPosition);
		}
		Locomotion.DoSoftHalt(new SpaceLocation(Calcule.RoundVecteur(haltPosition), Quaternion.LookRotation(base.transform.forward.Flatten(), Vector3.up)));
	}

	public void Fail()
	{
		Locomotion.PathBlockedAt(base.transform.position);
	}

	public void Freeze(Vector3 haltPosition)
	{
		PathInterrupted.Invoke();
		Halt(haltPosition, roundPosition: true);
	}

	public void GetTrapped(Vector3 haltPosition, float duration)
	{
		PathInterrupted.Invoke();
		SpaceLocation location = new SpaceLocation(haltPosition, Quaternion.LookRotation(base.transform.forward.Flatten(), Vector3.up));
		Locomotion.DoGetTrapped(location, duration);
	}

	public bool IsTurnedTowards(Vector3 position, float tolerance = 10f)
	{
		return Vector3.Angle((position - base.transform.position).Flatten(), base.transform.forward) <= tolerance;
	}

	public IEnumerator<ActivityProgress> TurnTowards(Location location)
	{
		return TurnTo(location.Position);
	}

	public IEnumerator<ActivityProgress> TurnTo(Vector3 position)
	{
		if (!IsTurnedTowards(position))
		{
			logger.InfoFormat("Bricktron '{0}' is Halting at '{1}' because of a Turn-Towards request", base.name, base.transform.position);
			Halt(base.transform.position, roundPosition: true);
			while (!Locomotion.CanMove())
			{
				yield return ActivityProgress.Wait();
			}
			Locomotion.TurnTo(position);
			yield return ActivityProgress.Sub(UtilityNodes.WaitForTimeOrSignal(Locomotion.HasArrived, 2f, failOnDelay: true));
			yield return ActivityProgress.Wait(20);
		}
	}

	private int GetTransferDelay(Labor blocker)
	{
		if (blocker != null && string.CompareOrdinal(blocker.name, base.name) < 0)
		{
			return 300;
		}
		return 200;
	}

	public void AssignBlocker(VoxelPresence v)
	{
		if (v != null)
		{
			Labor component = v.GetComponent<Labor>();
			RequestTransfer(Labor, component);
		}
	}

	private void RequestTransfer(Labor a, Labor b)
	{
		Labor.ConflictResolver?.OnLaborRequestTransfer(a, b);
		RequestTransferSignal.Invoke(a, b);
	}

	public Node UnstackStep(PathBranch destination)
	{
		WalkNode item = new WalkNode(destination);
		Path p = new Path(base.transform.position, new List<WalkNode> { item });
		return FollowNode(p);
	}

	public IEnumerable<PathBranch> GetSurroundingSurfaces(PathBranch.BranchType permission, bool ignoreCharacters = false)
	{
		return GetSurroundingSurfaces(PathIndex, permission, ignoreCharacters);
	}

	public static IEnumerable<PathBranch> GetSurroundingSurfaces(XYZ index, PathBranch.BranchType permission, bool ignoreCharacters = false)
	{
		return PathNodeRegistry.GetAdjacentBranches(index, permission, (XYZ liberte) => ignoreCharacters || !VoxelPresence.GetPresenceAt(liberte).Any());
	}

	public static XYZ SnapToWalkableSurface(XYZ voxel)
	{
		if (Voxel.IsFull(voxel))
		{
			voxel += XYZ.up;
		}
		if (!Voxel.IsFull(voxel - XYZ.up))
		{
			voxel -= XYZ.up;
		}
		return voxel;
	}

	private void OnDrawGizmos()
	{
		if (ShowDebugPath && DebugPath != null)
		{
			if (DebugPath is PathUnroll)
			{
				(DebugPath as PathUnroll).debug = true;
			}
			IEnumerator<WalkNode> nodesEnumerator = DebugPath.NodesEnumerator;
			while (nodesEnumerator.MoveNext())
			{
				WalkNode current = nodesEnumerator.Current;
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(current.Position + new Vector3(0f, 0.3f, 0f), 0.2f);
				Gizmos.DrawLine(current.Position, current.Position + current.Orientation * Vector3.forward);
			}
			nodesEnumerator.Dispose();
		}
	}
}
