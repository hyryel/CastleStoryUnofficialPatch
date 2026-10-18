using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components.Volumes;
using Brix.Pathfinding;

namespace Brix.Input.ContextualOrders;

public class FormationSearch
{
	private class PathAccepter : Brix.Pathfinding.PathAccepter
	{
		private HashSet<PathNode> _destinationNodes;

		private HashSet<PathNode> _neighborNodes;

		private int _count;

		public void Init(HashSet<PathNode> destinationNodes, HashSet<PathNode> neighborNodes, int count)
		{
			_destinationNodes = destinationNodes;
			_neighborNodes = neighborNodes;
			_count = count;
		}

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			if (_destinationNodes.Contains(link.head.Node) || _neighborNodes.Contains(link.head.Node))
			{
				return false;
			}
			_destinationNodes.Add(link.head.Node);
			foreach (PathBranch branch in PathNodeRegistry.GetBranches(link.head.Node))
			{
				_neighborNodes.Add(branch.Node);
				foreach (PathBranch item in PathNodeRegistry.GetBranches2(branch.Node))
				{
					_neighborNodes.Add(item.Node);
				}
			}
			return _destinationNodes.Count >= _count;
		}
	}

	private readonly PathSearch _pathSearch;

	private readonly PathLink _pathStart;

	private readonly PathAccepter _pathAccepter;

	private readonly PathQueue _pathQueue;

	public FormationSearch()
	{
		_pathAccepter = new PathAccepter();
		_pathQueue = new PathLightFirstQueue();
		_pathStart = new PathLink();
		_pathSearch = new PathSearch(_pathQueue, new PathDirectBuilder(PathNodeRegistry.TerrainLod, new SearchPathRequest.BranchEvaluator(PresenceTest)), _pathAccepter);
	}

	private bool PresenceTest(XYZ p)
	{
		return !VolumeRegistry.HasVolumesWithPropertyAt(p, VProperty.obstacle);
	}

	public void Search(HashSet<PathNode> destinationNodes, HashSet<PathNode> neighborNodes, PathNode destinationNode, int count)
	{
		_pathAccepter.Init(destinationNodes, neighborNodes, count);
		_pathStart.Reset(destinationNode);
		_pathSearch.Start(_pathStart);
		for (int i = 0; i < count * 8; i++)
		{
			if (!_pathSearch.MoveNext())
			{
				break;
			}
		}
		_pathSearch.Clear();
		_pathQueue.Clear();
	}
}
