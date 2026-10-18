using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Pathfinding.Map;

namespace Brix.Pathfinding;

public class PathTerrainLod : PathLowerLod
{
	public class BranchIterator : PathBranchIterator
	{
		public class SiblingsEnumerator : PathBranchEnumerator
		{
			public BranchIterator Iterator;

			private PathBranch current;

			private int i = -1;

			public override PathBranch Current => current;

			public SiblingsEnumerator(BranchIterator iterator)
			{
				Iterator = iterator;
			}

			public override bool MoveNext()
			{
				if (++i >= PathTerrainNode.neighbors.Length)
				{
					return false;
				}
				PathTerrainNode.BranchType branchType = Iterator.Node.branches[i];
				if (branchType != PathTerrainNode.BranchType.None)
				{
					PathNode pathNode = Iterator.PathLod.GetPathNode(Iterator.Node.LowResPosition + Iterator.Node.GetRelativeNeighbor(branchType) + PathTerrainNode.neighbors[i]);
					if (!pathNode.IsEmpty)
					{
						if (branchType == PathTerrainNode.BranchType.UpClimbing || branchType == PathTerrainNode.BranchType.SameClimbing)
						{
							current = new PathBranch(pathNode, PathBranch.BranchType.Climbing, 1);
						}
						else
						{
							current = new PathBranch(pathNode, 1);
						}
						return true;
					}
				}
				return MoveNext();
			}

			public override void Reset()
			{
				i = -1;
			}
		}

		public PathTerrainLod PathLod;

		public PathTerrainNode Node;

		public SiblingsEnumerator Siblings;

		public BranchIterator(PathTerrainLod pathLod)
		{
			PathLod = pathLod;
			Siblings = new SiblingsEnumerator(this);
		}

		public SiblingsEnumerator GetSiblings(PathTerrainNode node)
		{
			Node = node;
			Siblings.Reset();
			return Siblings;
		}

		public override PathBranchEnumerator GetChildren(PathNode node)
		{
			return PathBranchEnumerator.Empty;
		}

		public override PathBranchEnumerator GetSiblings(PathNode node)
		{
			return GetSiblings((PathTerrainNode)node);
		}
	}

	public Dictionary<XYZ, PathTerrainNode> pathNodes;

	public Brix.Pathfinding.Map.Map map;

	public IEnumerable<PathTerrainNode> PathTerrainNodes => pathNodes.Values;

	public override IEnumerable<PathDirectNode> PathDirectNodes => PathTerrainNodes.Cast<PathDirectNode>();

	public void Init(Brix.Pathfinding.Map.Map map, PathLod coarserLod)
	{
		this.map = map;
		base.coarserLod = coarserLod;
		HashSet<XYZ> source = new HashSet<XYZ>(map.SurfaceVoxels.Where((XYZ p) => !map.Contains(p + XYZ.up)));
		pathNodes = source.ToDictionary((XYZ p) => p, (XYZ p) => new PathTerrainNode());
		foreach (KeyValuePair<XYZ, PathTerrainNode> pathNode in pathNodes)
		{
			pathNode.Value.Init(pathNode.Key, this);
		}
	}

	public override PathNode GetPathNode(XYZ lowResPosition)
	{
		pathNodes.TryGetValue(lowResPosition, out var value);
		return (PathNode)(((object)value) ?? ((object)PathNode.Empty));
	}

	public bool WantsNodeAt(XYZ p)
	{
		return map.Contains(p) && map.Contains(p - XYZ.up) && (!map.Contains(p + XYZ.right) || !map.Contains(p - XYZ.right) || !map.Contains(p + XYZ.forward) || !map.Contains(p - XYZ.forward) || !map.Contains(p + XYZ.up));
	}

	public BranchIterator GetTerrainIterator()
	{
		return new BranchIterator(this);
	}

	public override PathBranchIterator GetIterator()
	{
		return GetTerrainIterator();
	}
}
