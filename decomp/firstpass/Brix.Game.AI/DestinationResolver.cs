using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI.Pathfinding;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class DestinationResolver
{
	private static BrixLogger logger = new BrixLogger(typeof(DestinationResolver).FullName);

	private static readonly List<WalkNode> LocationPool = new List<WalkNode>();

	private readonly Locomotion4 _locomotion;

	private readonly Path _result = new Path(new List<WalkNode>());

	public IPath Result => _result;

	private string Name => _locomotion.name;

	private DestinationIterator PathIterator => _locomotion.PathIterator;

	protected XYZ Occupied => _locomotion.PositionOccupied;

	protected XYZ Entering => _locomotion.PositionEntering;

	protected XYZ Next => _locomotion.PositionNext;

	public DestinationResolver(Locomotion4 locomotion)
	{
		_locomotion = locomotion;
	}

	public bool RepathMoveNext(bool tryPassThrough, bool canClimb)
	{
		XYZ repathDestination = GetRepathDestination(tryPassThrough);
		DestinationRepathSearch destinationRepathSearch = ObjectPool<DestinationRepathSearch>.Reserve().Init(_locomotion, repathDestination, tryPassThrough, canClimb);
		List<PathBranch> list = CollectionPool.RequestList<PathBranch>();
		Dictionary<XYZ, int> dictionary = CollectionPool.RequestDictionary<XYZ, int>();
		List<WalkNode> list2 = CollectionPool.RequestList<WalkNode>();
		bool result = MergePaths(destinationRepathSearch, list, dictionary, list2);
		ObjectPool<DestinationRepathSearch>.Release(destinationRepathSearch);
		list.Dispose();
		dictionary.Dispose();
		list2.Dispose();
		return result;
	}

	private XYZ GetRepathDestination(bool acceptEnteringPosition)
	{
		XYZ xYZ = Next;
		if (acceptEnteringPosition && xYZ == XYZ.zero)
		{
			xYZ = Entering;
		}
		if (PathIterator.Destination == null || xYZ == XYZ.zero)
		{
			logger.InfoFormat("RepathMoveNext {0}: PathIterator doesn't have a destination. Occupied={1}, Entering={2}, Next={3}", Name, Occupied, Entering, Next);
			return XYZ.zero;
		}
		WalkNode walkNode = PathIterator.GetNth(PathIterator.CurrentIndex + 3) ?? PathIterator.Destination;
		return XYZ.FromVector3(walkNode.Position);
	}

	private bool MergePaths(DestinationRepathSearch search, List<PathBranch> branches, Dictionary<XYZ, int> exPathPositions, List<WalkNode> locations)
	{
		if (!search.Search())
		{
			return false;
		}
		search.Result.ReversedBranches.Slinq().AddTo(branches);
		for (int i = 0; PathIterator.GetNth(i) != null; i++)
		{
			exPathPositions[XYZ.FromVector3(PathIterator.GetNth(i).Position)] = i;
		}
		if (!exPathPositions.ContainsKey(search.Destination))
		{
			logger.WarnFormat("RepathMoveNext {0}: Old path doesn't contain destination position {1}.", Name, search.Destination);
			return false;
		}
		int num = 0;
		int num2 = 0;
		for (int j = 0; j < branches.Count; j++)
		{
			PathBranch pathBranch = branches[j];
			if (pathBranch.Node.IsEmpty || !(pathBranch.Node is WalkablePathNode))
			{
				logger.WarnFormat("RepathMoveNext {0}: Old path contains empty or invalid branch at {1}.", Name, pathBranch.Node.RepresentativePosition);
				return false;
			}
			WalkNode walkNode = ((LocationPool.Count <= 0) ? new WalkNode() : LocationPool.Pop());
			walkNode.Reset(pathBranch);
			if (j < branches.Count - 1 && branches[j + 1].Node.RepresentativePosition.Flatten() != branches[j].Node.RepresentativePosition.Flatten())
			{
				walkNode.Orientation = Quaternion.LookRotation((branches[j + 1].Node.RepresentativePosition - branches[j].Node.RepresentativePosition).Flatten().ToVector3());
			}
			else if (PathIterator.Path != null && PathIterator.Path.DestinationLocation != null && XYZ.FromVector3(PathIterator.Path.DestinationLocation.Position).Flatten() != branches[j].Node.RepresentativePosition.Flatten())
			{
				walkNode.Orientation = Quaternion.LookRotation((PathIterator.Path.DestinationLocation.Position - branches[j].Node.RepresentativePosition.ToVector3()).Flatten());
			}
			locations.Add(walkNode);
			int valueOrDefault = exPathPositions.GetValueOrDefault(branches[j].Node.RepresentativePosition);
			if (valueOrDefault > num)
			{
				num = valueOrDefault;
				num2 = j + 1;
			}
		}
		if (num2 < locations.Count)
		{
			for (int k = num2; k < locations.Count; k++)
			{
				LocationPool.Add(locations[k]);
			}
			locations.RemoveRange(num2, locations.Count - num2);
		}
		if (locations.Count == 0)
		{
			logger.WarnFormat("RepathMoveNext {0}: New path doesn't contain nodes, where it should have at least one at contact point.", Name);
			return false;
		}
		XYZ xYZ = XYZ.FromVector3(locations[locations.Count - 1].Position);
		if (!exPathPositions.ContainsKey(xYZ))
		{
			logger.WarnFormat("RepathMoveNext {0}: Old path doesn't connect to new path at {1}.", Name, xYZ);
			return false;
		}
		int num3 = -1;
		for (int l = 0; PathIterator.GetNth(l) != null; l++)
		{
			if (XYZ.FromVector3(PathIterator.GetNth(l).Position) == xYZ)
			{
				num3 = l;
				break;
			}
		}
		if (num3 == -1)
		{
			logger.WarnFormat("RepathMoveNext {0}: Old path doesn't connect to new path at {1}.", Name, xYZ);
			return false;
		}
		for (int m = num3 + 1; PathIterator.GetNth(m) != null; m++)
		{
			WalkNode walkNode2 = ((LocationPool.Count <= 0) ? new WalkNode() : LocationPool.Pop());
			WalkNode nth = PathIterator.GetNth(m);
			WalkNode walkNode3 = nth;
			if (walkNode3 != null)
			{
				walkNode2.Reset(walkNode3);
			}
			else
			{
				walkNode2.Reset(new PathBranch(PathNodeRegistry.WalkableAt(XYZ.FromVector3(nth.Position)), PathBranch.BranchType.Full));
			}
			locations.Add(walkNode2);
		}
		_result.presenceTest = PathIterator.Path.PresenceTest;
		_result.destinationLocation = PathIterator.Path.DestinationLocation;
		foreach (WalkNode node in _result.nodes)
		{
			LocationPool.Add(node);
		}
		_result.nodes.Clear();
		locations.Slinq().AddTo(_result.nodes);
		return true;
	}
}
