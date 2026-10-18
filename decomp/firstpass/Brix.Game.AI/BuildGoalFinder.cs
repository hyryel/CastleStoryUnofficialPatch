using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI;

public class BuildGoalFinder
{
	public class Builder : PathLodBuilder
	{
		public Builder(PathLod pathLod, PathBranchEvaluator evaluator, int lodCount = -1)
			: base(pathLod, evaluator, lodCount)
		{
		}

		protected override bool IsSuitableParent(PathNode parentNode)
		{
			return (parentNode.Lod == lodMin && !parentNode.IsEmpty) || (parentNode.Lod <= lodMax && parentNode.IsFull && parentNode.AccessCount == 0);
		}

		protected override bool IsSuitableBranch(PathBranch branch)
		{
			return branchEvaluator.CanCross(branch);
		}
	}

	public class Accepter : PathSimplifiedAccepter
	{
		public BuildGoalFinder Finder;

		public override bool Visit(PathNode node)
		{
			return Finder.Visit(node);
		}
	}

	public class BranchEvaluator : PathBranchEvaluator
	{
		public BuildGoalFinder Finder;

		public PathBranch.BranchType AcceptedType = PathBranch.BranchType.Climbing;

		public bool PresenceTest(XYZ p)
		{
			if (VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.obstacle, (IVolume v, GameObject c) => !v.PassThrough.CanPassThrough(c), Finder.Character.gameObject))
			{
				return false;
			}
			return Finder.Character.presence.DontCollideIfAbleToWork(p);
		}

		public override bool CanCross(PathBranch branch)
		{
			return _CanCross(branch);
		}

		private bool _CanCross(PathBranch branch)
		{
			if (!IsWithinBounds(branch.Node))
			{
				return false;
			}
			if (branch.Type.CompareTo(AcceptedType) < 0)
			{
				return false;
			}
			if (branch.Node.IsFull && branch.Node.AccessCount == 0)
			{
				return true;
			}
			if (branch.Node.Lod == 0)
			{
				return PresenceTest(branch.Node.RepresentativePosition);
			}
			return false;
		}

		private bool IsWithinBounds(PathNode node)
		{
			if (Finder.SearchBounds.extents == Vector3.zero)
			{
				return true;
			}
			Bounds bounds = new Bounds((node.LowResPosition.ToVector3() + Vector3.one / 2f) * (1 << node.Lod), Vector3.one * (1 << node.Lod));
			return Finder.SearchBounds.Intersects(bounds);
		}
	}

	public class SimplifiedQueue : PathSimplifiedQueue
	{
		private Queue<Tuple<PathNode, int>> queue = new Queue<Tuple<PathNode, int>>();

		public override bool Empty => queue.Count == 0;

		public override void Clear()
		{
			queue.Clear();
		}

		public override PathNode Dequeue(out int weight)
		{
			Tuple<PathNode, int> tuple = queue.Dequeue();
			weight = tuple._2;
			return tuple._1;
		}

		public override void Enqueue(PathNode node, int weight)
		{
			queue.Enqueue(Tuple.Create(node, weight));
		}
	}

	public PathNode FindAllOrigin;

	public SimplifiedQueue FindAllQueue;

	public PathBranchEvaluator FindAllEvaluator;

	public Builder FindAllBuilder;

	public Accepter FindAllAccepter;

	public PathSimplifiedSearch FindAllSearch;

	public int FindAllSearched;

	public HashSet<PathNode> FindAllDictionary;

	public Bounds SearchBounds;

	public Labor Character;

	public bool CanBreakVoxel;

	public HashSet<Factory.AssetKey> Fetchables;

	public Transform ProjecTransform;

	public HashSet<GameObject> VisitedVolumes;

	public Dictionary<Factory.AssetKey, bool> ApprovedGoalAssetKeys;

	public List<BuildGoal> Results;

	public const int SearchIterations = 10;

	public static readonly Vector3 SearchBoundsSizeIncrease = Vector3.one * 50f;

	public BuildGoalFinder()
	{
		FindAllQueue = new SimplifiedQueue();
		FindAllEvaluator = new BranchEvaluator
		{
			Finder = this
		};
		FindAllBuilder = new Builder(TestPathfinding.Instance.terrainLod, FindAllEvaluator);
		FindAllAccepter = new Accepter
		{
			Finder = this
		};
		FindAllDictionary = new HashSet<PathNode>();
		FindAllSearch = new PathSimplifiedSearch(FindAllQueue, FindAllBuilder, FindAllAccepter, FindAllDictionary);
		FindAllSearched = 0;
		SearchBounds = default(Bounds);
		Character = null;
		CanBreakVoxel = false;
		Fetchables = null;
		ProjecTransform = null;
		VisitedVolumes = new HashSet<GameObject>();
		ApprovedGoalAssetKeys = new Dictionary<Factory.AssetKey, bool>();
		Results = new List<BuildGoal>();
	}

	public void Destroy()
	{
		FindAllQueue = null;
		FindAllEvaluator = null;
		FindAllBuilder = null;
		FindAllAccepter = null;
		FindAllDictionary = null;
		FindAllSearch = null;
		SearchBounds = default(Bounds);
		Character = null;
		CanBreakVoxel = false;
		Fetchables = null;
		VisitedVolumes.Clear();
		VisitedVolumes = null;
		ProjecTransform = null;
		ApprovedGoalAssetKeys.Clear();
		ApprovedGoalAssetKeys = null;
		Results.Clear();
		Results = null;
	}

	public void Clear()
	{
		ClearEverythingExceptResults();
		Results.Clear();
	}

	private void ClearEverythingExceptResults()
	{
		FindAllSearch.Clear();
		FindAllQueue.Clear();
		FindAllBuilder.Reset(PathLod.Empty);
		FindAllDictionary.Clear();
		FindAllSearched = 0;
		VisitedVolumes.Clear();
		ApprovedGoalAssetKeys.Clear();
		Character = null;
		Fetchables = null;
		ProjecTransform = null;
	}

	public void PrepareStep(Labor character, HashSet<Factory.AssetKey> fetchables, Transform projectTransform)
	{
		SearchBounds = default(Bounds);
		VisitedVolumes.Clear();
		Results.Clear();
		ApprovedGoalAssetKeys.Clear();
		Character = character;
		CanBreakVoxel = Character.Profession.CanBreakVoxels();
		Fetchables = fetchables;
		ProjecTransform = projectTransform;
		TestTerrainLod terrainLod = TestPathfinding.Instance.terrainLod;
		FindAllSearch.Clear();
		FindAllSearched = 0;
		FindAllQueue.Clear();
		FindAllBuilder.Reset(terrainLod);
		FindAllSearch.Start(Character.presence.CurrentPathNode);
	}

	public bool SearchStep()
	{
		bool flag = false;
		for (int i = 0; i < 10; i++)
		{
			if (flag)
			{
				break;
			}
			flag = !FindAllSearch.MoveNext();
			FindAllSearched++;
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	public void ResultStep()
	{
		ClearEverythingExceptResults();
	}

	public List<BuildGoal> ComputeConnectedAccesses(Labor character, HashSet<Factory.AssetKey> fetchables, Transform projecTransform)
	{
		PrepareStep(character, fetchables, projecTransform);
		while (!SearchStep())
		{
		}
		ResultStep();
		return Results;
	}

	public StaticNode ComputeConnectedAccessesNode(Labor character, HashSet<Factory.AssetKey> fetchables, Transform projecTransform)
	{
		return Node.Sequence.Do(delegate(BuildGoalFinder self, Labor c, HashSet<Factory.AssetKey> f, Transform p)
		{
			self.PrepareStep(c, f, p);
		}, this, character, fetchables, projecTransform).Label("Prepare").Do(Node.Selector.When((BuildGoalFinder self) => self.SearchStep(), this).Label("Search").Do(Node.Empty)
			.Label("Stop")
			.Else()
			.Do(Node.WaitFor.Ever())
			.Label("Wait forever"))
			.Do(delegate(BuildGoalFinder self)
			{
				self.ResultStep();
			}, this)
			.Label("Result");
	}

	private bool Visit(PathNode node)
	{
		if (node.AccessCount > 0 && node.Lod == 0 && TestPathfinding.Instance.AccessPositions.TryGetValue(node.RepresentativePosition, out var value))
		{
			foreach (IVolume item in value)
			{
				Visit(item);
			}
		}
		return false;
	}

	private void Visit(IVolume v)
	{
		GameObject gameObject = v.GameObject;
		if (gameObject.IsNullOrReleased() || !VisitedVolumes.Add(gameObject))
		{
			return;
		}
		BuildGoal component = gameObject.GetComponent<BuildGoal>();
		if (component == null || !component.Blueprint.State.HasFlag(PlacementFlags.valid) || !component.Blueprint.State.HasFlag(PlacementFlags.buildable) || !component.Commited || !(component.transform.parent == ProjecTransform))
		{
			return;
		}
		bool value = component.IsAnti;
		if (!value || CanBreakVoxel)
		{
			if (!value && !ApprovedGoalAssetKeys.TryGetValue(component.AssetKey, out value))
			{
				value = component.recepteur.HasBaseCapacityForOneOf(Fetchables);
				ApprovedGoalAssetKeys.Add(component.AssetKey, value);
			}
			if (value)
			{
				Results.Add(component);
			}
		}
	}

	public void ConstrainBounds(Vector3 position)
	{
		if (SearchBounds.extents == Vector3.zero)
		{
			SearchBounds = new Bounds(position, SearchBoundsSizeIncrease);
		}
		else
		{
			SearchBounds.Encapsulate(new Bounds(position, SearchBoundsSizeIncrease));
		}
	}
}
