using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;

namespace Brix.Pathfinding;

public class PathHigherLod : PathLod
{
	public class BranchIterator : PathBranchIterator
	{
		public class ChildrenEnumerator : PathBranchEnumerator
		{
			public BranchIterator Iterator;

			private int i = -1;

			public override PathBranch Current
			{
				get
				{
					PathNode pathNode = Iterator.Node.Children[i];
					return new PathBranch(pathNode, Iterator.Node.GetWeightToChild(pathNode));
				}
			}

			public ChildrenEnumerator(BranchIterator iterator)
			{
				Iterator = iterator;
			}

			public override bool MoveNext()
			{
				return ++i < Iterator.Node.Children.Length;
			}

			public override void Reset()
			{
				i = -1;
			}
		}

		public class SiblingsEnumerator : PathBranchEnumerator
		{
			public BranchIterator Iterator;

			private int i = -1;

			public override PathBranch Current => Iterator.Node.Siblings[i];

			public SiblingsEnumerator(BranchIterator iterator)
			{
				Iterator = iterator;
			}

			public override bool MoveNext()
			{
				return ++i < Iterator.Node.Siblings.Length;
			}

			public override void Reset()
			{
				i = -1;
			}
		}

		public PathHigherLod PathLod;

		public PathLodNode Node;

		public ChildrenEnumerator Children;

		public SiblingsEnumerator Siblings;

		public BranchIterator(PathHigherLod pathLod)
		{
			PathLod = pathLod;
			Children = new ChildrenEnumerator(this);
			Siblings = new SiblingsEnumerator(this);
		}

		public ChildrenEnumerator GetChildren(PathLodNode node)
		{
			Node = node;
			Children.Reset();
			return Children;
		}

		public SiblingsEnumerator GetSiblings(PathLodNode node)
		{
			Node = node;
			Siblings.Reset();
			return Siblings;
		}

		public override PathBranchEnumerator GetChildren(PathNode node)
		{
			return GetChildren((PathLodNode)node);
		}

		public override PathBranchEnumerator GetSiblings(PathNode node)
		{
			return GetSiblings((PathLodNode)node);
		}
	}

	private static BrixLogger logger = new BrixLogger();

	public int lod;

	public PathLod finerLod = PathLod.Empty;

	public PathLod coarserLod = PathLod.Empty;

	public int fulls = 0;

	public int total = 0;

	public Dictionary<XYZ, List<PathLodNode>> pathNodes;

	private static readonly List<PathNode> _finerNodes = new List<PathNode>();

	private static readonly List<List<PathNode>> _linkedGroups = new List<List<PathNode>>();

	public override int Lod => lod;

	public override PathLod FinerLod => finerLod;

	public override PathLod CoarserLod => coarserLod;

	public override IEnumerable<PathNode> PathNodes => pathNodes.SelectMany((KeyValuePair<XYZ, List<PathLodNode>> kv) => kv.Value.Cast<PathNode>());

	public void Init(PathLod finerLod, PathLod coarserLod)
	{
		this.finerLod = finerLod;
		this.coarserLod = coarserLod;
		pathNodes = new Dictionary<XYZ, List<PathLodNode>>();
		Dictionary<XYZ, List<PathNode>> dictionary = new Dictionary<XYZ, List<PathNode>>();
		foreach (PathNode pathNode in finerLod.PathNodes)
		{
			XYZ key = pathNode.LowResPosition >> 1;
			if (!dictionary.TryGetValue(key, out var value))
			{
				dictionary.Add(key, value = new List<PathNode>());
			}
			value.Add(pathNode);
		}
		foreach (KeyValuePair<XYZ, List<PathNode>> item in dictionary)
		{
			List<PathLodNode> list = new List<PathLodNode>();
			CreatePathNodesFromFinerNodes(item.Value, list);
			pathNodes.Add(item.Key, list);
		}
		logger.Info("PathHigherLod.Init: " + Lod + ", " + pathNodes.Sum((KeyValuePair<XYZ, List<PathLodNode>> kv) => kv.Value.Count()));
		PathBranchIterator iterator = finerLod.GetIterator();
		foreach (KeyValuePair<XYZ, List<PathLodNode>> pathNode2 in pathNodes)
		{
			foreach (PathLodNode item2 in pathNode2.Value)
			{
				item2.InitChildrenPaths(finerLod, iterator);
			}
		}
		foreach (KeyValuePair<XYZ, List<PathLodNode>> pathNode3 in pathNodes)
		{
			foreach (PathLodNode item3 in pathNode3.Value)
			{
				item3.InitChildren(finerLod, iterator);
			}
		}
	}

	public IEnumerator Init_Coroutine(PathLod finerLod, PathLod coarserLod, int stepsPerFrame = 10000)
	{
		this.finerLod = finerLod;
		this.coarserLod = coarserLod;
		pathNodes = new Dictionary<XYZ, List<PathLodNode>>();
		int steps = 0;
		Dictionary<XYZ, List<PathNode>> lowerDico = new Dictionary<XYZ, List<PathNode>>();
		foreach (PathNode finerNode in finerLod.PathNodes)
		{
			XYZ p = finerNode.LowResPosition >> 1;
			if (!lowerDico.TryGetValue(p, out var list))
			{
				List<PathNode> value;
				list = (value = new List<PathNode>());
				lowerDico.Add(p, value);
			}
			list.Add(finerNode);
			steps++;
			if (steps == stepsPerFrame)
			{
				yield return null;
				steps = 0;
			}
			list = null;
		}
		foreach (KeyValuePair<XYZ, List<PathNode>> group in lowerDico)
		{
			List<PathLodNode> nodes = new List<PathLodNode>();
			CreatePathNodesFromFinerNodes(group.Value, nodes);
			pathNodes.Add(group.Key, nodes);
			steps++;
			if (steps == stepsPerFrame)
			{
				yield return null;
				steps = 0;
			}
		}
		PathBranchIterator iterator = finerLod.GetIterator();
		foreach (KeyValuePair<XYZ, List<PathLodNode>> pathNode in pathNodes)
		{
			foreach (PathLodNode node in pathNode.Value)
			{
				node.InitChildrenPaths(finerLod, iterator);
				steps++;
				if (steps == stepsPerFrame)
				{
					yield return null;
					steps = 0;
				}
			}
		}
		foreach (KeyValuePair<XYZ, List<PathLodNode>> pathNode2 in pathNodes)
		{
			foreach (PathLodNode node2 in pathNode2.Value)
			{
				node2.InitChildren(finerLod, iterator);
				steps++;
				if (steps == stepsPerFrame)
				{
					yield return null;
					steps = 0;
				}
			}
		}
		yield return null;
	}

	private void GetFinerNodesAt(XYZ lowResPosition, ICollection<PathNode> results)
	{
		foreach (XYZ item in Calcule.VolumeBetween(lowResPosition << 1, (lowResPosition << 1) + XYZ.one))
		{
			finerLod.GetAllPathNodesAt(item, results);
		}
	}

	public void RefreshAt(XYZ lowResPosition)
	{
		if (pathNodes.TryGetValue(lowResPosition, out var value))
		{
			value.Clear();
		}
		else
		{
			pathNodes.Add(lowResPosition, value = new List<PathLodNode>());
		}
		_finerNodes.Clear();
		GetFinerNodesAt(lowResPosition, _finerNodes);
		CreatePathNodesFromFinerNodes(_finerNodes, value);
	}

	private void CreatePathNodesFromFinerNodes(List<PathNode> finerNodes, ICollection<PathLodNode> results)
	{
		_linkedGroups.Clear();
		PathLodNode.GetNodesLinkedTogether(finerNodes, finerLod, _linkedGroups);
		foreach (List<PathNode> linkedGroup in _linkedGroups)
		{
			PathLodNode pathLodNode = new PathLodNode();
			pathLodNode.SetChildren(linkedGroup);
			results.Add(pathLodNode);
		}
	}

	public override PathNode GetPathNode(XYZ lowResPosition)
	{
		pathNodes.TryGetValue(lowResPosition, out var value);
		if (value == null || value.Count == 0 || value[0] == null)
		{
			return PathNode.Empty;
		}
		return value[0];
	}

	public List<PathLodNode> GetAllPathLodNodesAt(XYZ lowResPosition)
	{
		if (pathNodes.TryGetValue(lowResPosition, out var value))
		{
			return value;
		}
		return Brix.Utils.Empty.List<PathLodNode>();
	}

	public override void GetAllPathNodesAt(XYZ lowResPosition, ICollection<PathNode> results)
	{
		if (pathNodes.TryGetValue(lowResPosition, out var value))
		{
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				results.Add(value[i]);
			}
		}
	}

	public BranchIterator GetLodIterator()
	{
		return new BranchIterator(this);
	}

	public override PathBranchIterator GetIterator()
	{
		return GetLodIterator();
	}
}
