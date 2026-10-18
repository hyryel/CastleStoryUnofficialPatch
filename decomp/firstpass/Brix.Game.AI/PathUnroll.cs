using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.AI;

public class PathUnroll : IPath
{
	private class PathEnumerator : IEnumerator<WalkNode>, IDisposable, IEnumerator
	{
		private readonly PathUnroll _path;

		private List<WalkNode> _resultLocations;

		private List<WalkNode>.Enumerator _resultEnumerator;

		object IEnumerator.Current => _resultEnumerator.Current;

		WalkNode IEnumerator<WalkNode>.Current => _resultEnumerator.Current;

		public PathEnumerator(PathUnroll path)
		{
			_path = path;
			Reset();
		}

		public bool MoveNext()
		{
			return _resultLocations != null && _resultEnumerator.MoveNext();
		}

		public void Reset()
		{
			_resultLocations = CollectionPool.RequestList<WalkNode>();
			List<PathBranch> preprocessed = _path._preprocessed;
			_path.ConvertVoxelListToPath(preprocessed, _resultLocations);
			_resultEnumerator = _resultLocations.GetEnumerator();
		}

		public void Dispose()
		{
			if (_resultLocations != null)
			{
				_resultEnumerator.Dispose();
				for (int i = 0; i < _resultLocations.Count; i++)
				{
					ObjectPool<WalkNode>.Release(_resultLocations[i]);
				}
				_resultLocations.Dispose();
				_resultLocations = null;
			}
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(PathUnroll).FullName);

	protected static Brix.Pathfinding.Pathfinding Pathfinding;

	protected static TestPathfinding TestPathfinding;

	public TestTerrainLod terrainLod;

	public IPathUnroller unroller;

	public Location destinationLocation;

	public PathBranchEvaluator branchEvaluator;

	public Func<XYZ, bool> presenceTest;

	public TestTerrainLod.BranchIterator iterator;

	public XYZ initialPosition;

	public XYZ enteringPosition;

	public Transform transform;

	public int processed;

	public int skipped;

	public List<Location> resultLocations;

	public bool debug;

	private List<PathBranch> _preprocessed;

	private PathEnumerator _pathEnumerator;

	public bool IsEmpty => !unroller.PathExists;

	public Func<XYZ, bool> PresenceTest => presenceTest;

	public XYZ Start => initialPosition;

	public XYZ Entering => enteringPosition;

	public Location DestinationLocation => destinationLocation;

	public IEnumerator<WalkNode> NodesEnumerator
	{
		get
		{
			if (_pathEnumerator == null)
			{
				_pathEnumerator = new PathEnumerator(this);
			}
			else
			{
				_pathEnumerator.Reset();
			}
			return _pathEnumerator;
		}
	}

	public PathUnroll(TestTerrainLod terrainLod, IPathUnroller unroller, Location destinationLocation, PathBranchEvaluator branchEvaluator, Func<XYZ, bool> presenceTest, XYZ initialPosition, XYZ enteringPosition, Transform transform)
	{
		this.terrainLod = terrainLod;
		this.unroller = unroller;
		this.destinationLocation = destinationLocation;
		this.branchEvaluator = branchEvaluator;
		this.presenceTest = presenceTest;
		this.initialPosition = initialPosition;
		this.enteringPosition = enteringPosition;
		this.transform = transform;
		iterator = terrainLod.GetTerrainIterator();
		processed = 0;
		skipped = 0;
		resultLocations = null;
		PreProcessNodes();
	}

	public void Reset(TestTerrainLod terrainLod, IPathUnroller unroller, Location destinationLocation, PathBranchEvaluator branchEvaluator, Func<XYZ, bool> presenceTest, XYZ initialPosition, XYZ enteringPosition, Transform transform)
	{
		this.terrainLod = terrainLod;
		this.unroller = unroller;
		this.destinationLocation = destinationLocation;
		this.branchEvaluator = branchEvaluator;
		this.presenceTest = presenceTest;
		this.initialPosition = initialPosition;
		this.enteringPosition = enteringPosition;
		this.transform = transform;
		processed = 0;
		skipped = 0;
		resultLocations = null;
		PreProcessNodes();
	}

	public void GetPathLocations(List<WalkNode> result)
	{
		ConvertVoxelListToPath(_preprocessed, result);
	}

	private void PreProcessNodes()
	{
		if (Pathfinding == null)
		{
			Pathfinding = UnityEngine.Object.FindObjectOfType<Brix.Pathfinding.Pathfinding>();
		}
		if (TestPathfinding == null)
		{
			TestPathfinding = UnityEngine.Object.FindObjectOfType<TestPathfinding>();
		}
		VoxelPresence component = transform.GetComponent<VoxelPresence>();
		XYZ valueOrDefault = TestPathfinding.VoxelPresences.GetValueOrDefault(component);
		PathNode walkable = Pathfinding.GetWalkable(valueOrDefault, 0);
		bool flag = walkable.ObstacleCount > 0;
		if (flag)
		{
			walkable.ObstacleCount--;
		}
		List<PathBranch> x = CollectionPool.RequestList<PathBranch>();
		List<PathBranch> y = CollectionPool.RequestList<PathBranch>();
		unroller.GetResultBranches(x);
		Donnees.Swap(ref x, ref y);
		x.Clear();
		RemoveInvalidNodes(y, x);
		if (_preprocessed == null)
		{
			_preprocessed = new List<PathBranch>();
		}
		else
		{
			_preprocessed.Clear();
		}
		_preprocessed.AddRange(x);
		x.Dispose();
		y.Dispose();
		if (flag)
		{
			walkable.ObstacleCount++;
		}
	}

	private Vector3 GetGroundPosition(PathNode node)
	{
		XYZ representativePosition = node.RepresentativePosition;
		if (Voxel.IsFull(representativePosition))
		{
			representativePosition += XYZ.up;
		}
		return representativePosition.ToVector3();
	}

	private void ConvertVoxelListToPath(List<PathBranch> branches, List<WalkNode> outLocations)
	{
		List<PathBranch>.Enumerator enumerator = branches.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		PathBranch current = enumerator.Current;
		Vector3 vector = GetGroundPosition(current.Node);
		WalkNode walkNode;
		Vector3 forward;
		if (!enumerator.MoveNext())
		{
			walkNode = ConvertBranch(current);
			forward = (destinationLocation.Position - vector).Flatten();
			if (forward.sqrMagnitude >= 0.010000001f)
			{
				walkNode.Orientation = Quaternion.LookRotation(forward);
			}
			else if (destinationLocation.HasSpecificOrientation)
			{
				walkNode.Orientation = destinationLocation.Orientation;
			}
			outLocations.Add(walkNode);
			return;
		}
		PathBranch current2 = enumerator.Current;
		Vector3 groundPosition = GetGroundPosition(current2.Node);
		walkNode = ConvertBranch(current);
		forward = (groundPosition - vector).Flatten();
		if (forward.sqrMagnitude >= 0.010000001f)
		{
			walkNode.Orientation = Quaternion.LookRotation(forward);
		}
		outLocations.Add(walkNode);
		while (enumerator.MoveNext())
		{
			walkNode = ConvertBranch(current2);
			forward = (groundPosition - vector).Flatten();
			if (forward.sqrMagnitude >= 0.010000001f)
			{
				walkNode.Orientation = Quaternion.LookRotation(forward);
			}
			outLocations.Add(walkNode);
			vector = groundPosition;
			current2 = enumerator.Current;
			groundPosition = GetGroundPosition(current2.Node);
			if (walkNode.MustFail())
			{
				return;
			}
		}
		walkNode = ConvertBranch(current2);
		forward = (destinationLocation.Position - groundPosition).Flatten();
		if (forward.sqrMagnitude >= 0.010000001f)
		{
			walkNode.Orientation = Quaternion.LookRotation(forward);
		}
		else if (destinationLocation.HasSpecificOrientation)
		{
			walkNode.Orientation = destinationLocation.Orientation;
		}
		outLocations.Add(walkNode);
	}

	private WalkNode ConvertBranch(PathBranch current)
	{
		XYZ representativePosition = current.Node.RepresentativePosition;
		WalkNode walkNode = ObjectPool<WalkNode>.Reserve();
		walkNode.Reset(current);
		return walkNode;
	}

	private void RemoveInvalidNodes(List<PathBranch> branches, List<PathBranch> outBranches)
	{
		List<PathBranch>.Enumerator enumerator = branches.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PathBranch item = enumerator.Current;
			bool flag = false;
			if (item.Node.Lod != terrainLod.Lod)
			{
				logger.DebugFormat("PathUnroll for {0} at {1} processed a node with the wrong LOD: {2} at {3}", transform.name, transform.position, item.Node.Lod, item.Node.RepresentativePosition);
				flag = true;
			}
			else if (item.Node.IsEmpty || !(item.Node is WalkablePathNode) || item.Node != terrainLod.GetPathNode(item.Node.RepresentativePosition))
			{
				flag = true;
			}
			if (flag)
			{
				if (outBranches.Count > 0)
				{
					item = new PathBranch(outBranches[outBranches.Count - 1].Node, PathBranch.BranchType.Empty, item.weight);
				}
				break;
			}
			outBranches.Add(item);
		}
	}
}
