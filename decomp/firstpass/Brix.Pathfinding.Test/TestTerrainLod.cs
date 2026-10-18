using System.Collections.Generic;
using System.Linq;
using Brix.Engine;

namespace Brix.Pathfinding.Test;

public class TestTerrainLod : PathLowerLod
{
	public class BranchIterator : PathBranchIterator
	{
		public class SiblingsEnumerator : PathBranchEnumerator
		{
			public BranchIterator Iterator;

			private int i = -1;

			public override PathBranch Current => Iterator.Node.siblings[i];

			public SiblingsEnumerator(BranchIterator iterator)
			{
				Iterator = iterator;
			}

			public override bool MoveNext()
			{
				return ++i < Iterator.Node.siblings.Length;
			}

			public override void Reset()
			{
				i = -1;
			}
		}

		public TestTerrainLod PathLod;

		public TerrainPathNode Node;

		public SiblingsEnumerator Siblings;

		public BranchIterator(TestTerrainLod pathLod)
		{
			PathLod = pathLod;
			Siblings = new SiblingsEnumerator(this);
		}

		public SiblingsEnumerator GetSiblings(TerrainPathNode node)
		{
			Node = node;
			return Siblings;
		}

		public override PathBranchEnumerator GetChildren(PathNode node)
		{
			return PathBranchEnumerator.Empty;
		}

		public override PathBranchEnumerator GetSiblings(PathNode node)
		{
			return (!(node is TerrainPathNode node2)) ? ((PathBranchEnumerator)PathBranchEnumerator.Empty) : ((PathBranchEnumerator)GetSiblings(node2));
		}
	}

	public Dictionary<XYZ, TerrainPathNode> pathNodes;

	public override IEnumerable<PathDirectNode> PathDirectNodes => pathNodes.Values.Cast<PathDirectNode>();

	public override PathNode GetPathNode(XYZ p)
	{
		return (PathNode)(((object)pathNodes.GetValueOrDefault(p)) ?? ((object)PathNode.Empty));
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
