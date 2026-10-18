using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class ReachSpaceLocation : SpaceLocation
{
	private readonly HashSet<XYZ> _positionSet;

	public ReachSpaceLocation(Vector3 pos, int reach = 1, bool excludeInside = true, bool findWalkable = true)
		: base(pos)
	{
		WalkablePathNode walkablePathNode = ((!findWalkable) ? PathNodeRegistry.At(XYZ.FromVector3(Pos)) : PathNodeRegistry.WalkableAt(XYZ.FromVector3(Pos)));
		if (walkablePathNode == null)
		{
			_positionSet = Brix.Utils.Empty.HashSet<XYZ>();
			return;
		}
		switch (reach)
		{
		case 0:
			_positionSet = GetReach0(walkablePathNode);
			return;
		case 1:
			_positionSet = GetReach1(walkablePathNode);
			if (!excludeInside)
			{
				_positionSet.Add(XYZ.FromVector3(Pos));
			}
			return;
		}
		HashSet<XYZ> x = GetReach0(walkablePathNode);
		HashSet<XYZ> y = CollectionPool.RequestHashSet<XYZ>();
		for (int i = 0; i < reach; i++)
		{
			Donnees.Swap(ref x, ref y);
			y.Slinq().AddTo(x);
			y.Slinq().SelectMany(PathNodeRegistry.GetConnectedNodes).AddTo(x);
		}
		if (excludeInside)
		{
			x.ExceptWith(y);
		}
		y.Dispose();
		_positionSet = x;
	}

	private HashSet<XYZ> GetReach0(WalkablePathNode start)
	{
		HashSet<XYZ> hashSet = new HashSet<XYZ>();
		hashSet.Add(start.RepresentativePosition);
		return hashSet;
	}

	private HashSet<XYZ> GetReach1(WalkablePathNode start)
	{
		HashSet<XYZ> hashSet = new HashSet<XYZ>();
		foreach (PathBranch branch in PathNodeRegistry.GetBranches(start))
		{
			hashSet.Add(branch.Node.RepresentativePosition);
		}
		return hashSet;
	}

	public override bool TargetReached(GameObject source, Vector3 position)
	{
		return _positionSet.Contains(XYZ.FromVector3(position));
	}
}
