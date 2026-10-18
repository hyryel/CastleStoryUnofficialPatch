using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI.Pathfinding;

public class PathNodeRegistry : MonoBehaviour
{
	public static PathLod TerrainLod;

	private static PathBranchIterator Iterator;

	private static PathBranchIterator Iterator2;

	private static readonly Func<XYZ, bool> DefaultTestFunc = (XYZ p) => true;

	private void Start()
	{
		TerrainLod = PathLod.Empty;
		Iterator = TerrainLod.GetIterator();
		Iterator2 = TerrainLod.GetIterator();
		GameSignals.LateConnectOnce(TestPathfinding.InitializedSignal, Init);
	}

	private static void Init()
	{
		TerrainLod = TestPathfinding.Instance.terrainLod;
		Iterator = TerrainLod.GetIterator();
		Iterator2 = TerrainLod.GetIterator();
	}

	public static WalkablePathNode At(XYZ voxel)
	{
		WalkablePathNode walkablePathNode = TerrainLod.GetPathNode(voxel) as WalkablePathNode;
		if ((walkablePathNode != null && walkablePathNode.IsEmpty) || Voxel.IsFull(voxel))
		{
			walkablePathNode = null;
		}
		return walkablePathNode;
	}

	public static WalkablePathNode WalkableAt(XYZ voxel)
	{
		return At(voxel) ?? At(voxel + XYZ.up) ?? At(voxel - XYZ.up);
	}

	public static WalkablePathNode WalkableAt(XYZ voxel, int maxDist)
	{
		for (int num = 0; num <= maxDist; num = -num + ((num <= 0) ? 1 : 0))
		{
			WalkablePathNode walkablePathNode = At(voxel + XYZ.up * num);
			if (walkablePathNode != null)
			{
				return walkablePathNode;
			}
		}
		return null;
	}

	public static WalkablePathNode WalkableAtOrUnder(XYZ voxel, int maxDist)
	{
		for (int i = 0; i <= maxDist; i++)
		{
			WalkablePathNode walkablePathNode = At(voxel - XYZ.up * i);
			if (walkablePathNode != null)
			{
				return walkablePathNode;
			}
		}
		return null;
	}

	private static bool CanCross(PathBranch branch, PathBranch.BranchType permission)
	{
		return branch.Type.CompareTo(permission) >= 0;
	}

	public static bool CheckPathNode(XYZ voxel)
	{
		return !TerrainLod.GetPathNode(voxel).IsEmpty;
	}

	public static PathBranchEnumerator GetBranches(PathNode node)
	{
		return Iterator.GetSiblings(node);
	}

	public static PathBranchEnumerator GetBranches2(PathNode node)
	{
		return Iterator2.GetSiblings(node);
	}

	public static PathBranchEnumerator GetBranches(XYZ voxel)
	{
		return Iterator.GetSiblings(TerrainLod.GetPathNode(voxel));
	}

	public static PathBranch.BranchType PermissionVers(XYZ voxel, XYZ liberte)
	{
		PathNode pathNode = TerrainLod.GetPathNode(voxel);
		foreach (PathBranch sibling in Iterator.GetSiblings(pathNode))
		{
			if (sibling.Node.RepresentativePosition == liberte)
			{
				return sibling.Type;
			}
		}
		return PathBranch.BranchType.Empty;
	}

	public static bool HasHashLiberte(XYZ voxel, PathBranch.BranchType permission, XYZ liberte)
	{
		PathNode pathNode = TerrainLod.GetPathNode(voxel);
		foreach (PathBranch sibling in Iterator.GetSiblings(pathNode))
		{
			if (CanCross(sibling, permission) && sibling.Node.RepresentativePosition == liberte)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasHashLibertes(XYZ voxel, PathBranch.BranchType permission, XYZ liberteA, XYZ liberteB)
	{
		PathNode pathNode = TerrainLod.GetPathNode(voxel);
		foreach (PathBranch sibling in Iterator.GetSiblings(pathNode))
		{
			if ((CanCross(sibling, permission) && sibling.Node.RepresentativePosition == liberteA) || sibling.Node.RepresentativePosition == liberteB)
			{
				return true;
			}
		}
		return false;
	}

	public static int CountMutualLibertes(XYZ voxelA, XYZ voxelB)
	{
		return CountMutualLibertes(voxelA, voxelB, PathBranch.BranchType.Full);
	}

	public static int CountMutualLibertes(XYZ voxelA, XYZ voxelB, PathBranch.BranchType permission, Func<XYZ, bool> test = null)
	{
		return CountMutualLibertes(voxelA, voxelB, PathBranch.BranchType.Full, (XYZ p, Func<XYZ, bool> t) => t(p), test ?? DefaultTestFunc);
	}

	public static int CountMutualLibertes<T>(XYZ voxelA, XYZ voxelB, PathBranch.BranchType permission, Func<XYZ, T, bool> test = null, T arg = default(T))
	{
		PathNode pathNode = TerrainLod.GetPathNode(voxelB);
		if (pathNode.IsEmpty)
		{
			return 0;
		}
		PathNode pathNode2 = TerrainLod.GetPathNode(voxelA);
		if (pathNode2.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		test = test ?? ((Func<XYZ, T, bool>)((XYZ p, T t) => true));
		foreach (PathBranch sibling in Iterator.GetSiblings(pathNode2))
		{
			if (!CanCross(sibling, permission))
			{
				continue;
			}
			foreach (PathBranch sibling2 in Iterator2.GetSiblings(pathNode))
			{
				if (permission.CompareTo(sibling2.Type) <= 0 && sibling.Node == sibling2.Node && test(sibling.Node.RepresentativePosition, arg))
				{
					num++;
				}
			}
		}
		return num;
	}

	public static IEnumerable<XYZ> GetAdjacentPositions(XYZ voxel)
	{
		return GetAdjacentPositions(voxel, PathBranch.BranchType.Full);
	}

	public static IEnumerable<XYZ> GetAdjacentPositions(XYZ voxel, PathBranch.BranchType permission, Func<XYZ, bool> validate = null)
	{
		validate = validate ?? DefaultTestFunc;
		PathNode nodeA = TerrainLod.GetPathNode(voxel);
		foreach (PathBranch branchA in Iterator.GetSiblings(nodeA))
		{
			if (CanCross(branchA, permission) && validate(branchA.Node.RepresentativePosition))
			{
				yield return branchA.Node.RepresentativePosition;
			}
		}
	}

	public static IEnumerable<PathBranch> GetAdjacentBranches(XYZ voxel, PathBranch.BranchType permission, Func<XYZ, bool> validate = null)
	{
		validate = validate ?? DefaultTestFunc;
		PathNode nodeA = TerrainLod.GetPathNode(voxel);
		foreach (PathBranch branchA in Iterator.GetSiblings(nodeA))
		{
			if (CanCross(branchA, permission) && validate(branchA.Node.RepresentativePosition))
			{
				yield return branchA;
			}
		}
	}

	public static IEnumerable<XYZ> GetPositionInReach(XYZ voxel)
	{
		return GetPositionInReach(voxel, PathBranch.BranchType.Full);
	}

	public static IEnumerable<XYZ> GetPositionInReach(XYZ voxel, PathBranch.BranchType permission, Func<XYZ, bool> validate = null)
	{
		CollectionPool.Request<PathNode>(out HashSet<PathNode> hash);
		validate = validate ?? DefaultTestFunc;
		PathNode nodeA = TerrainLod.GetPathNode(voxel);
		foreach (PathBranch sibling in Iterator.GetSiblings(nodeA))
		{
			if (!CanCross(sibling, permission) || !validate(sibling.Node.RepresentativePosition))
			{
				continue;
			}
			hash.Add(sibling.Node);
			PathNode node = sibling.Node;
			foreach (PathBranch sibling2 in Iterator2.GetSiblings(node))
			{
				if (CanCross(sibling2, permission) && !hash.Contains(sibling2.Node) && validate(sibling2.Node.RepresentativePosition))
				{
					hash.Add(sibling2.Node);
				}
			}
		}
		foreach (PathNode node2 in hash)
		{
			yield return node2.RepresentativePosition;
		}
		hash.Dispose();
	}

	public static Slinq<XYZ, SelectContext<XYZ, PathBranch, IEnumerableContext<PathBranch>>> GetConnectedNodes(XYZ voxel)
	{
		return from branch in Iterator.GetSiblings(TerrainLod.GetPathNode(voxel)).Slinq()
			select branch.Node.RepresentativePosition;
	}

	public static Slinq<XYZ, SelectContext<XYZ, PathBranch, PredicateContext<PathBranch, IEnumerableContext<PathBranch>, PathBranch.BranchType>>> GetConnectedNodes(XYZ voxel, PathBranch.BranchType permission)
	{
		return from branch in Iterator.GetSiblings(TerrainLod.GetPathNode(voxel)).Slinq().Where(CanCross, permission)
			select branch.Node.RepresentativePosition;
	}

	public static Slinq<PathBranch, PredicateContext<PathBranch, IEnumerableContext<PathBranch>, PathBranch.BranchType>> GetConnectedLibertes(XYZ voxel, PathBranch.BranchType permission)
	{
		return Iterator.GetSiblings(TerrainLod.GetPathNode(voxel)).Slinq().Where(CanCross, permission);
	}
}
