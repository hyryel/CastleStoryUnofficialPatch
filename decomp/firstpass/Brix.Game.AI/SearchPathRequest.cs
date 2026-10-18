using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.Nature;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Logging;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

[Serializable]
public class SearchPathRequest : IPoolable, IResetable, IDisposable
{
	public class OptimisticBranchEvaluator : PathBranchEvaluator
	{
		public PathBranch.BranchType AcceptedType;

		public OptimisticBranchEvaluator(PathBranch.BranchType type)
		{
			AcceptedType = type;
		}

		public override bool CanCross(PathBranch branch)
		{
			return branch.Type.CompareTo(AcceptedType) >= 0;
		}
	}

	public class BranchEvaluator : PathBranchEvaluator
	{
		public Func<XYZ, bool> PresenceTest;

		public PathBranch.BranchType AcceptedType = PathBranch.BranchType.Empty;

		public BranchEvaluator(Func<XYZ, bool> presenceTest, bool canClimb = true, bool canWalkInForest = false, bool canBreakVoxels = false)
		{
			PresenceTest = presenceTest;
			AcceptedType = (canWalkInForest ? PathBranch.BranchType.Forest : (canBreakVoxels ? PathBranch.BranchType.Breaking : ((!canClimb) ? PathBranch.BranchType.StepDown : PathBranch.BranchType.Climbing)));
		}

		public void Reset(Func<XYZ, bool> presenceTest, bool canClimb = true, bool canWalkInForest = false, bool canBreakVoxels = false)
		{
			PresenceTest = presenceTest;
			AcceptedType = (canWalkInForest ? PathBranch.BranchType.Forest : (canBreakVoxels ? PathBranch.BranchType.Breaking : ((!canClimb) ? PathBranch.BranchType.StepDown : PathBranch.BranchType.Climbing)));
		}

		public override bool CanCross(PathBranch branch)
		{
			if (branch.Type.CompareTo(AcceptedType) < 0)
			{
				return false;
			}
			if (branch.Node.IsFull)
			{
				return true;
			}
			if (branch.Node.Lod == 0)
			{
				return PresenceTest(branch.Node.RepresentativePosition);
			}
			return false;
		}
	}

	public class AStarQueue : PathAStarQueue
	{
		public int Visited;

		public AStarQueue(XYZ goalPosition)
			: base(goalPosition)
		{
		}

		public override void Reset(XYZ goalPosition)
		{
			base.Reset(goalPosition);
			Visited = 0;
		}

		private static int Magnitude(XYZ d)
		{
			return (int)Math.Sqrt(XYZ.SqrMagnitude(d));
		}

		public override int Evaluate(PathLink link)
		{
			PathNode node = link.head.Node;
			XYZ d = node.RepresentativePosition - base.GoalPosition;
			int num = Magnitude(d);
			int length = link.length;
			return num * 45 + length * 10 + link.weight * 2 - (node.Surface >> node.Lod * 0);
		}
	}

	public class UnrollQueue : PathAStarQueue
	{
		public UnrollQueue(XYZ goalPosition)
			: base(goalPosition)
		{
		}

		private static int Magnitude(XYZ d)
		{
			return (int)Math.Sqrt(XYZ.SqrMagnitude(d));
		}

		public override int Evaluate(PathLink link)
		{
			XYZ d = link.head.Node.RepresentativePosition - goalPosition;
			int num = Magnitude(d);
			int length = link.length;
			return num * 45 + length * 10 + link.weight * 2;
		}
	}

	public class MaximizeDistanceQueue : PathAStarQueue
	{
		public MaximizeDistanceQueue(XYZ goalPosition)
			: base(goalPosition)
		{
		}

		private static int Magnitude(XYZ d)
		{
			return (int)Math.Sqrt(XYZ.SqrMagnitude(d));
		}

		public override int Evaluate(PathLink link)
		{
			PathNode node = link.head.Node;
			XYZ d = node.RepresentativePosition - base.GoalPosition;
			int num = Magnitude(d);
			int length = link.length;
			return -num * 45 + length * 10 + link.weight * 2;
		}
	}

	public abstract class ResultAccepter : PathAccepter
	{
		public PathLink Result;

		public PathLink BestVisited;

		public int BestDistance;
	}

	public class LocationAccepter : ResultAccepter
	{
		public GameObject Source;

		public Location Location;

		private bool Reached(PathNode node)
		{
			if (node.Lod != 0)
			{
				return false;
			}
			Vector3 position = node.RepresentativePosition.ToVector3();
			return Location.TargetReached(Source, position);
		}

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			if (Reached(link.head.Node))
			{
				Result = link;
				return true;
			}
			int num = XYZ.ManhattanMagnitude(link.head.Node.RepresentativePosition - XYZ.FromVector3(Location.Position));
			if (BestVisited == null || BestDistance > num)
			{
				BestVisited = link;
				BestDistance = num;
			}
			return false;
		}

		public void Reset()
		{
			Result = null;
			BestVisited = null;
			BestDistance = 0;
		}

		public void Reset(GameObject source, Location location)
		{
			Source = source;
			Location = location;
			Reset();
		}
	}

	public class MaximizeDistanceAccepter : ResultAccepter
	{
		public Location Location;

		public float AcceptedDistance;

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			if ((float)link.length >= AcceptedDistance)
			{
				Result = link;
				return true;
			}
			int num = XYZ.ManhattanMagnitude(link.head.Node.RepresentativePosition - XYZ.FromVector3(Location.Position));
			if (BestVisited == null || num > BestDistance)
			{
				BestVisited = link;
				BestDistance = num;
			}
			return false;
		}

		public void Reset(Location location, float acceptedDistance)
		{
			Location = location;
			AcceptedDistance = acceptedDistance;
			Result = null;
			BestVisited = null;
			BestDistance = 0;
		}
	}

	public class ConditionAccepter : PathAccepter
	{
		public Func<PathLink, PathLink, bool> Condition;

		public ConditionAccepter(Func<PathLink, PathLink, bool> condition)
		{
			Condition = condition;
		}

		public void Reset(Func<PathLink, PathLink, bool> condition)
		{
			Condition = condition;
		}

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			return Condition(link, fromSource);
		}
	}

	public class PathToPathAccepter : PathAccepter
	{
		public PathLink BestVisited;

		public int BestWeight;

		private Dictionary<PathNode, int> WeightToFinalDestination = new Dictionary<PathNode, int>();

		public PathLink Result => BestVisited;

		public PathToPathAccepter(PathLink successLink)
		{
			int num = 1;
			for (PathLink pathLink = successLink; pathLink != null; pathLink = pathLink.tail)
			{
				WeightToFinalDestination.Add(pathLink.head.Node, num);
				num += pathLink.head.Weight;
			}
		}

		public PathToPathAccepter(IEnumerable<PathBranch> branchesFromEndToStart)
		{
			int num = 1;
			foreach (PathBranch item in branchesFromEndToStart)
			{
				WeightToFinalDestination.Add(item.Node, num);
				num += item.Weight;
			}
		}

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			int valueOrDefault = WeightToFinalDestination.GetValueOrDefault(link.head.Node);
			if (valueOrDefault == 0)
			{
				return false;
			}
			valueOrDefault += link.weight;
			if (valueOrDefault < BestWeight)
			{
				BestWeight = valueOrDefault;
				BestVisited = link;
			}
			return false;
		}
	}

	public class LodBuilder : PathLodBuilder
	{
		protected XYZ DestinationMin;

		protected XYZ DestinationMax;

		public Func<PathBranch, PathLink, int, int> GetWeightFunc;

		public LodBuilder(PathLod pathLod, PathBranchEvaluator branchEvaluator, XYZ destinationMin, XYZ destinationMax, int lodCount)
			: base(pathLod, branchEvaluator, lodCount)
		{
			DestinationMin = destinationMin;
			DestinationMax = destinationMax;
		}

		public LodBuilder(PathLod pathLod, PathBranchEvaluator branchEvaluator, XYZ destination, float distance, int lodCount)
			: base(pathLod, branchEvaluator, lodCount)
		{
			DestinationMin = destination - XYZ.one * Mathf.FloorToInt(distance);
			DestinationMax = destination + XYZ.one * Mathf.FloorToInt(distance);
		}

		public void Reset(PathLod pathLod, PathBranchEvaluator branchEvaluator, XYZ destinationMin, XYZ destinationMax, int lodCount)
		{
			base.pathLod = pathLod;
			base.branchEvaluator = branchEvaluator;
			DestinationMin = destinationMin;
			DestinationMax = destinationMax;
			Reset(lodCount);
		}

		public void Reset(PathLod pathLod, PathBranchEvaluator branchEvaluator, XYZ destination, float distance, int lodCount)
		{
			base.pathLod = pathLod;
			base.branchEvaluator = branchEvaluator;
			DestinationMin = destination - XYZ.one * Mathf.FloorToInt(distance);
			DestinationMax = destination + XYZ.one * Mathf.FloorToInt(distance);
			Reset(lodCount);
		}

		protected override bool IsGoalParent(PathNode node)
		{
			return node.LowResPosition >= DestinationMin >> node.Lod && node.LowResPosition <= DestinationMax >> node.Lod;
		}

		public override int ComputePathWeight(PathBranch head, PathLink tail)
		{
			int num = base.ComputePathWeight(head, tail);
			if (GetWeightFunc != null)
			{
				return GetWeightFunc(head, tail, num);
			}
			return num;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(SearchPathRequest).FullName);

	[NonSerialized]
	public bool Initialized;

	public XYZ From;

	public Location Location;

	public bool FirstTime = true;

	public bool Exact = true;

	public Func<XYZ, bool> PresenceTest;

	public bool CareAboutResult = true;

	public bool CanClimb = true;

	public bool CanWalkInForest;

	public bool CanBreakVoxels;

	public bool Precise;

	public int SearchLimit = -1;

	public bool SearchAway;

	public int SearchAwayDistance = 10;

	public int LodCount = -1;

	public int SearchPerUpdate = 100;

	public string LastSearchedGameObject;

	public Vector3 LastSearchedPosition;

	public XYZ LastSearchResult;

	public Navigation Navigation;

	public VoxelPresence Presence;

	public Brix.Pathfinding.Pathfinding Pathfinding;

	public TestPathfinding TestPathfinding;

	public PathDrawer PathDrawer;

	public TestTerrainLod TerrainLod;

	public Func<XYZ, bool> CanCrossAtMethod;

	public PathBranchEvaluator Evaluator;

	public PathBuilder Builder;

	public PathAccepter Accepter;

	public PathLink StartLink;

	public PathSearch Search;

	public PathNode Origin;

	public XYZ Destination;

	public float GoalRefinementDistance;

	public int MaxSearched;

	public int Searched;

	public bool Done;

	public bool Succeeded;

	public PathLink SuccessLink;

	public PathUnroll ResultPath;

	public UnrollQueue UnrollerQueue;

	public PathLodUnroller PathUnroller;

	public int OptimisticLength;

	public int PessimisticLength;

	public XYZ SuccessDestination;

	public int LodMax;

	public int OptimisticLod;

	public AStarQueue ImproveQueue;

	public LodBuilder ImproveBuilder;

	public SpaceLocation ImproveAccepterLocation;

	public LocationAccepter ImproveAccepter;

	public PathLink ImproveStart;

	public PathSearch ImproveSearch;

	public int ImproveLength;

	public int ImproveSearched;

	public int ImproveLod;

	public int ImproveMaxSearch = 100;

	public BranchEvaluator DirectEvaluator;

	public LodBuilder DirectBuilder;

	public LocationAccepter DirectAccepter;

	public AStarQueue DirectQueue;

	public PathLink DirectStartLink;

	public PathSearch DirectSearch;

	public BranchEvaluator MaximizeEvaluator;

	public PathLodBuilder MaximizeBuilder;

	public MaximizeDistanceAccepter MaximizeAccepter;

	public MaximizeDistanceQueue MaximizeQueue;

	public PathLink MaximizeStartLink;

	public PathSearch MaximizeSearch;

	private Func<bool> _workLoopCondition;

	private Func<bool> _improveLoopCondition;

	public bool VisitedInvalidNodes => Search.Searched.Slinq().Any((KeyValuePair<PathNode, PathLink> kv) => kv.Value.head.Node.IsEmpty);

	public SearchPathRequest()
	{
		_workLoopCondition = () => !Work(SearchPerUpdate);
		_improveLoopCondition = () => !LoopImprovePath(SearchPerUpdate);
	}

	public static SearchPathRequest Make()
	{
		SearchPathRequest searchPathRequest = ObjectPool<SearchPathRequest>.Reserve();
		searchPathRequest.Init();
		return searchPathRequest;
	}

	public void Dispose()
	{
		ObjectPool<SearchPathRequest>.Release(this);
	}

	public void ResetObject()
	{
		Navigation = null;
		Presence = null;
		TestPathfinding = null;
		Pathfinding = null;
		TerrainLod = null;
		PathDrawer = null;
		Initialized = false;
	}

	private void Init()
	{
		TestPathfinding = TestPathfinding.Instance;
		Pathfinding = TestPathfinding.pathfinding;
		TerrainLod = TestPathfinding.terrainLod;
		CanCrossAtMethod = CanCrossAt;
		Initialized = true;
	}

	public void InitNavigation(Navigation navigation)
	{
		Navigation = navigation;
		Presence = Navigation.Presence;
		PathDrawer = Navigation.PathDrawer;
	}

	public StaticNode ComputePath()
	{
		return Node.Sequence.Do(InitOriginNode()).Do(delegate(SearchPathRequest self)
		{
			self.InitSearch();
		}, this).Do(WorkLoop())
			.Do(delegate(SearchPathRequest self)
			{
				self.InitImprove();
			}, this)
			.Do(ImproveLoop())
			.Do(delegate(SearchPathRequest self)
			{
				self.ConcludeSearch();
			}, this);
	}

	private StaticNode WorkLoop()
	{
		return Node.Sequence.Do(Node.WaitForTicks(1)).Loop().While(_workLoopCondition);
	}

	private StaticNode ImproveLoop()
	{
		return Node.Sequence.Do(Node.WaitForTicks(1)).Loop().While(_improveLoopCondition);
	}

	public static bool HasBlockerAt(GameObject gameObject, XYZ index)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(index, VProperty.obstacle, (IVolume v, GameObject _gameObject) => !v.PassThrough.CanSearchThrough(_gameObject), gameObject);
	}

	private static bool HasCharacterAt(XYZ index)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(index, VProperty.character);
	}

	private static bool HasNatureAt(XYZ index)
	{
		return TreeRegistry.Instance.IsNearATree(index) || NatureRegistry.HasNatureBlockerAt(index);
	}

	private static bool HasObstacleAt(XYZ index)
	{
		return VolumeRegistry.HasVolumesWithPropertyAt(index, VProperty.obstacle);
	}

	private bool CanCrossAt(XYZ voxel)
	{
		if (HasBlockerAt(Navigation.gameObject, voxel))
		{
			return false;
		}
		return PresenceTest(voxel);
	}

	private XYZ FindClosestWalkable(XYZ destination)
	{
		return destination;
	}

	private void Info(string msg, XYZ p)
	{
	}

	private void Warn(string msg)
	{
		logger.InfoFormat("SearchPathRequest: {0} from {1} to {2} currenly at {3}{4} -> {5}", (!(Navigation != null)) ? "null" : Navigation.name, (!(Navigation != null)) ? Vector3.zero : Navigation.transform.position, (!(Location is ObjectLocation)) ? "voxel" : ((!((Location as ObjectLocation).Object != null)) ? "null" : (Location as ObjectLocation).Object.name), (Location == null) ? Vector3.zero : Location.Position, (!Exact) ? string.Empty : " (exact)", msg);
	}

	private StaticNode InitOriginNode()
	{
		return Node.Selector.If((SearchPathRequest self) => !self.InitOrginAndDestination(), this).Do(Node.FailPathTo<Exceptions.PathInvalid>(null));
	}

	private bool InitOrginAndDestination()
	{
		GameSignals.Invoke(GameSignals.PathfindingRequest);
		Succeeded = false;
		LastSearchedGameObject = ((!(Location.Object != null)) ? "null" : Location.Object.name);
		LastSearchedPosition = Location.Position;
		LastSearchResult = XYZ.zero;
		if (Location is ObjectLocation && !(Location as ObjectLocation).IsValid())
		{
			return false;
		}
		if (!InitOrigin(From, out Origin))
		{
			return false;
		}
		if (!InitDestination(Location, out Destination, Exact))
		{
			return false;
		}
		return true;
	}

	private bool InitOrigin(XYZ from, out PathNode origin)
	{
		origin = TerrainLod.GetPathNode(from);
		if (origin.IsEmpty)
		{
			origin = TerrainLod.GetPathNode(from + XYZ.up);
			if (origin.IsEmpty)
			{
				origin = TerrainLod.GetPathNode(from - XYZ.up);
			}
			if (origin.IsEmpty)
			{
				Warn("invalid origin");
				return false;
			}
		}
		return true;
	}

	private bool InitDestination(Location location, out XYZ destination, bool exact)
	{
		destination = XYZ.FromVector3(location.Position);
		if (location is SpaceLocation && !(location is ReachSpaceLocation))
		{
			XYZ representativePosition = Pathfinding.GetWalkable(destination, 0).RepresentativePosition;
			if (representativePosition == XYZ.zero)
			{
				Info("destination is not walkable", destination);
				if (exact)
				{
					destination = XYZ.zero;
				}
			}
			else if (representativePosition != destination)
			{
				Info("changing destination to", representativePosition);
				destination = representativePosition;
				location.Position = destination.ToVector3();
			}
		}
		if (destination == XYZ.zero || destination.ToScalaire() == -1)
		{
			if (location.Position == Brix.Engine.Abstract.Voxel.GetVecteur(-1))
			{
				Warn("index=-1");
			}
			else if (destination == XYZ.zero)
			{
				Warn("invalid destination");
			}
			return false;
		}
		if (location is SpaceLocation && !(location is ReachSpaceLocation) && !CanCrossAt(XYZ.FromVector3(location.Position)))
		{
			if (exact)
			{
				Warn("invalid destination");
				return false;
			}
			Info("destination is not walkable", destination);
			destination = FindClosestWalkable(destination);
			location.Position = destination.ToVector3();
		}
		return true;
	}

	private void InitDirectSearch()
	{
		VoxelBounds voxelBounds = new VoxelBounds(Destination, Destination);
		if (Location is CharacterLocation)
		{
			voxelBounds = new VoxelBounds(Destination - XYZ.one * 2, Destination + XYZ.one * 2);
		}
		else if (Location is ObjectLocation)
		{
			voxelBounds = Location.Object.GetComponent<IVolume>()?.ComputeBounds() ?? new VoxelBounds(Destination - XYZ.one * 2, Destination + XYZ.one * 2);
		}
		else if (Location is ReachSpaceLocation)
		{
			voxelBounds = new VoxelBounds(Destination - XYZ.one, Destination + XYZ.one);
		}
		if (DirectSearch == null)
		{
			DirectEvaluator = new BranchEvaluator(CanCrossAtMethod, CanClimb, CanWalkInForest, CanBreakVoxels);
			DirectQueue = new AStarQueue(Destination);
			DirectBuilder = new LodBuilder(TerrainLod, DirectEvaluator, voxelBounds.Min, voxelBounds.Max, LodCount);
			DirectAccepter = new LocationAccepter
			{
				Source = Navigation.gameObject,
				Location = Location
			};
			DirectSearch = new PathSearch(DirectQueue, DirectBuilder, DirectAccepter);
			DirectStartLink = new PathLink(Origin);
		}
		else
		{
			DirectEvaluator.Reset(CanCrossAtMethod, CanClimb, CanWalkInForest, CanBreakVoxels);
			DirectQueue.Reset(Destination);
			DirectBuilder.Reset(TerrainLod, DirectEvaluator, voxelBounds.Min, voxelBounds.Max, LodCount);
			DirectAccepter.Reset(Navigation.gameObject, Location);
			DirectStartLink.Reset(Origin);
			DirectSearch.Clear();
		}
		Evaluator = DirectEvaluator;
		Builder = DirectBuilder;
		Accepter = DirectAccepter;
		StartLink = DirectStartLink;
		Search = DirectSearch;
	}

	private void InitMaximizeSearch()
	{
		if (MaximizeSearch == null)
		{
			MaximizeEvaluator = new BranchEvaluator(CanCrossAtMethod, CanClimb, CanWalkInForest, CanBreakVoxels);
			MaximizeQueue = new MaximizeDistanceQueue(Destination);
			MaximizeBuilder = new PathLodBuilder(TerrainLod, MaximizeEvaluator, LodCount);
			MaximizeAccepter = new MaximizeDistanceAccepter
			{
				Location = Location,
				AcceptedDistance = SearchAwayDistance
			};
			MaximizeStartLink = new PathLink(Origin);
			MaximizeSearch = new PathSearch(MaximizeQueue, MaximizeBuilder, MaximizeAccepter);
		}
		else
		{
			MaximizeEvaluator.Reset(CanCrossAtMethod, CanClimb, CanWalkInForest, CanBreakVoxels);
			MaximizeQueue.Reset(Destination);
			MaximizeBuilder.Reset(TerrainLod, LodCount);
			MaximizeAccepter.Reset(Location, SearchAwayDistance);
			MaximizeStartLink.Reset(Origin);
			MaximizeSearch.Clear();
		}
		Evaluator = MaximizeEvaluator;
		Builder = MaximizeBuilder;
		Accepter = MaximizeAccepter;
		StartLink = MaximizeStartLink;
		Search = MaximizeSearch;
	}

	private void InitSearch()
	{
		if (SearchAway)
		{
			InitMaximizeSearch();
		}
		else
		{
			InitDirectSearch();
		}
		Search.Flags = (CareAboutResult ? PathSearchFlags.AllowRequeueing : PathSearchFlags.None);
		Search.Start(StartLink);
		Searched = 0;
		MaxSearched = ((SearchLimit < 0) ? int.MaxValue : SearchLimit);
		ImproveMaxSearch = Mathf.Min(100, MaxSearched);
		Done = false;
	}

	private PathNode RemoveVolumeTemporarily()
	{
		PathNode walkable = Pathfinding.GetWalkable(TestPathfinding.VoxelPresences.GetValueOrDefault(Navigation.Presence), 0);
		if (walkable.ObstacleCount <= 0)
		{
			return PathNode.Empty;
		}
		walkable.ObstacleCount--;
		return walkable;
	}

	private void PutVolumeBack(PathNode volumeNode)
	{
		volumeNode.ObstacleCount++;
	}

	private bool Work(int count)
	{
		PathNode volumeNode = RemoveVolumeTemporarily();
		for (int i = 0; i < count; i++)
		{
			if (Done)
			{
				break;
			}
			bool flag = Search.MoveNext();
			Done = ++Searched >= MaxSearched || !flag;
		}
		PutVolumeBack(volumeNode);
		return Done;
	}

	private void InitImprove()
	{
		SuccessLink = ((ResultAccepter)Accepter).Result;
		if (!Exact)
		{
			AcceptInexactPath();
		}
		LastSearchResult = ((SuccessLink == null) ? XYZ.zero : SuccessLink.head.Node.RepresentativePosition);
		if (SuccessLink != null)
		{
			SuccessDestination = SuccessLink.head.Node.RepresentativePosition;
			TryImprovePath();
		}
	}

	private bool WrapUpSearch()
	{
		InitImprove();
		if (SuccessLink == null)
		{
			return false;
		}
		if (PathUnroller == null)
		{
			UnrollerQueue = new UnrollQueue(SuccessDestination);
			PathUnroller = new PathLodUnroller(SuccessLink, TerrainLod, Evaluator, UnrollerQueue);
			PathUnroller.Search.Flags = PathSearchFlags.AllowRequeueing;
			ResultPath = new PathUnroll(TerrainLod, PathUnroller, Location, Evaluator, CanCrossAtMethod, Presence.CurrentVoxel, Presence.PositionEntering, Navigation.transform);
		}
		else
		{
			UnrollerQueue.Reset(SuccessDestination);
			PathUnroller.Reset(SuccessLink, TerrainLod, Evaluator, UnrollerQueue);
			ResultPath.Reset(TerrainLod, PathUnroller, Location, Evaluator, CanCrossAtMethod, Presence.CurrentVoxel, Presence.PositionEntering, Navigation.transform);
		}
		return true;
	}

	private void ConcludeSearch()
	{
		Succeeded = WrapUpSearch();
		if (Succeeded)
		{
			DrawDebugPath();
			GameSignals.Invoke(GameSignals.PathfindingSolvedRequest);
		}
		else
		{
			GameSignals.Invoke(GameSignals.PathfindingUnsolvedRequest);
		}
	}

	private void AcceptInexactPath()
	{
		SuccessLink = SuccessLink ?? ((ResultAccepter)Accepter).BestVisited;
		while (SuccessLink != null && SuccessLink.head.Node.Lod > 0)
		{
			PathLodNode pathLodNode = (PathLodNode)SuccessLink.head.Node;
			PathNode representativeChild = pathLodNode.RepresentativeChild;
			PathBranch branch = new PathBranch(representativeChild, pathLodNode.GetWeightToChild(representativeChild));
			SuccessLink = Builder.GetPath(branch, SuccessLink);
		}
	}

	private bool LoopImprovePath(int searchBudget)
	{
		if (Done)
		{
			ImproveLod++;
			if (ImproveLod >= LodMax)
			{
				return true;
			}
			if (ImproveSearch == null)
			{
				ImproveQueue = new AStarQueue(SuccessDestination);
				ImproveAccepterLocation = new SpaceLocation(SuccessDestination.ToVector3());
				ImproveAccepter = new LocationAccepter
				{
					Source = Navigation.gameObject,
					Location = ImproveAccepterLocation
				};
				ImproveBuilder = new LodBuilder(TerrainLod, Evaluator, SuccessDestination, 0.1f, ImproveLod + 1);
				ImproveStart = new PathLink(Origin);
				ImproveSearch = new PathSearch(ImproveQueue, ImproveBuilder, ImproveAccepter);
			}
			else
			{
				ImproveQueue.Reset(SuccessDestination);
				ImproveAccepterLocation.SetPosition(SuccessDestination.ToVector3());
				ImproveAccepter.Reset(Navigation.gameObject, ImproveAccepterLocation);
				ImproveBuilder.Reset(TerrainLod, Evaluator, SuccessDestination, 0.1f, ImproveLod + 1);
				ImproveStart.Reset(Origin);
				ImproveSearch.Clear();
			}
			ImproveSearch.Start(ImproveStart);
			ImproveSearched = 0;
			Done = false;
		}
		else if (WorkImprovePath(searchBudget))
		{
			ImproveLength = 0;
			if (ImproveAccepter.Result != null)
			{
				SuccessLink = ImproveAccepter.Result;
				ImproveLength = SuccessLink.length;
				return true;
			}
		}
		return false;
	}

	private bool WorkImprovePath(int searchBudget)
	{
		PathNode volumeNode = RemoveVolumeTemporarily();
		for (int i = 0; i < searchBudget; i++)
		{
			if (Done)
			{
				break;
			}
			bool flag = ImproveSearch.MoveNext();
			Done = ++ImproveSearched >= ImproveMaxSearch || !flag;
		}
		PutVolumeBack(volumeNode);
		return Done;
	}

	private void TryImprovePath()
	{
		if (Precise)
		{
			OptimisticLength = XYZ.ManhattanMagnitude(Origin.RepresentativePosition - SuccessDestination);
			PessimisticLength = SuccessLink.length;
			LodMax = ((PathLodBuilder)Builder).lodMax;
			OptimisticLod = Mathf.Max(0, Mathf.RoundToInt(Mathf.Log(OptimisticLength + 1, 2f)) - 5);
			ImproveLod = OptimisticLod - 1;
			Done = true;
		}
		else
		{
			ImproveLod = LodMax;
			Done = true;
		}
	}

	private void DrawDebugPath()
	{
	}
}
