using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Pathfinding;

public class PathLodNode : PathNode
{
	public class PathChildrenEvaluator : PathBranchEvaluator
	{
		public PathNode ParentNode;

		public override bool CanCross(PathBranch branch)
		{
			return branch.Node.ParentNode == ParentNode;
		}
	}

	private static BrixLogger logger = new BrixLogger();

	public PathNode[] Children;

	public PathBranch[] Siblings;

	private XYZ representativePosition;

	private Vector3 centerPosition;

	private int lod;

	private PathNode parentNode = PathNode.Empty;

	private PathNode representative = PathNode.Empty;

	public int obstacleCount;

	public int accessCount;

	public int surface;

	public int weightToParent;

	public int weightFromParent;

	private static PathSearch search;

	private static PathChildrenEvaluator evaluator;

	private static PathDirectBuilder builder;

	private static readonly Dictionary<PathNode, PathBranch> siblingsDico = new Dictionary<PathNode, PathBranch>();

	private static readonly List<Stack<PathBranch[]>> SiblingsPool = new List<Stack<PathBranch[]>>();

	private static readonly PathBranch[] NoSiblings = new PathBranch[0];

	private static readonly Dictionary<PathNode, UnionRegion<PathNode>> LinkDico = new Dictionary<PathNode, UnionRegion<PathNode>>();

	private static readonly Dictionary<Tuple<PathNode, PathNode>, PathBranch.BranchType> BranchDico = new Dictionary<Tuple<PathNode, PathNode>, PathBranch.BranchType>();

	private static readonly Dictionary<UnionRegion<PathNode>, List<PathNode>> ParentDico = new Dictionary<UnionRegion<PathNode>, List<PathNode>>();

	private static readonly HashSet<PathBranch.BranchType> UsedBranchTypes = new HashSet<PathBranch.BranchType>();

	private static readonly List<PathBranch.BranchType> OrderedBranchTypes = new List<PathBranch.BranchType>();

	private static readonly List<UnionRegion<PathNode>> ChildLinkPool = new List<UnionRegion<PathNode>>();

	private static readonly List<List<PathNode>> GroupPool = new List<List<PathNode>>();

	public override XYZ RepresentativePosition => representativePosition;

	public override XYZ LowResPosition => representativePosition >> lod;

	public override Vector3 CenterPosition => centerPosition;

	public override int Lod => lod;

	public override int ObstacleCount
	{
		get
		{
			return obstacleCount;
		}
		set
		{
			int num = value - obstacleCount;
			obstacleCount += num;
			ParentNode.ObstacleCount += num;
		}
	}

	public override int AccessCount
	{
		get
		{
			return accessCount;
		}
		set
		{
			int num = value - accessCount;
			accessCount += num;
			ParentNode.AccessCount += num;
		}
	}

	public override int Surface
	{
		get
		{
			return surface;
		}
		set
		{
			surface = value;
		}
	}

	public override PathNode ParentNode
	{
		get
		{
			return parentNode;
		}
		set
		{
			parentNode = value;
		}
	}

	public override int WeightToParent
	{
		get
		{
			return weightToParent;
		}
		set
		{
			weightToParent = value;
		}
	}

	public override int WeightFromParent
	{
		get
		{
			return weightFromParent;
		}
		set
		{
			weightFromParent = value;
		}
	}

	public PathNode RepresentativeChild => representative;

	public int GetWeightToChild(PathNode finerNode)
	{
		return finerNode.WeightFromParent;
	}

	public int GetWeightFromChild(PathNode finerNode)
	{
		return finerNode.WeightToParent;
	}

	public void InitChildrenPaths(PathLod finerLod, PathBranchIterator iterator)
	{
		int num = 0;
		int num2 = 0;
		PathNode[] children = Children;
		foreach (PathNode node in children)
		{
			foreach (PathBranch sibling in iterator.GetSiblings(node))
			{
				num += sibling.Weight;
				num2++;
			}
		}
		int num3 = Math.Max(1, ((num != 0) ? (num / num2) : 0) / 2);
		PathNode[] children2 = Children;
		foreach (PathNode pathNode in children2)
		{
			int num4 = (pathNode.WeightFromParent = num3);
			pathNode.WeightToParent = num4;
		}
	}

	public void InitChildren(PathLod finerLod, PathBranchIterator iterator)
	{
		siblingsDico.Clear();
		lod = finerLod.Lod + 1;
		PathNode[] children = Children;
		foreach (PathNode pathNode in children)
		{
			foreach (PathBranch sibling in iterator.GetSiblings(pathNode))
			{
				PathNode pathNode2 = sibling.Node.ParentNode;
				if (pathNode2 != this && pathNode2 is PathLodNode && (!siblingsDico.TryGetValue(pathNode2, out var value) || value.Type.CompareTo(sibling.Type) <= 0))
				{
					int num = GetWeightToChild(pathNode) + sibling.Weight + (pathNode2 as PathLodNode).GetWeightFromChild(sibling.Node);
					if (value.Type != sibling.Type || value.Weight >= num)
					{
						siblingsDico[pathNode2] = new PathBranch(pathNode2, sibling.Type, num);
					}
				}
			}
		}
		AllocSiblings(siblingsDico.Count);
		int num2 = 0;
		Dictionary<PathNode, PathBranch>.Enumerator enumerator2 = siblingsDico.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			Siblings[num2++] = enumerator2.Current.Value;
		}
	}

	private void AllocSiblings(int n)
	{
		if (Siblings == null)
		{
			if (n == 0)
			{
				Siblings = NoSiblings;
				return;
			}
			while (n >= SiblingsPool.Count)
			{
				SiblingsPool.Add(new Stack<PathBranch[]>());
			}
			if (SiblingsPool[n].Count == 0)
			{
				Siblings = new PathBranch[n];
			}
			else
			{
				Siblings = SiblingsPool[n].Pop();
			}
			return;
		}
		int num = Siblings.Length;
		if (num == n)
		{
			return;
		}
		if (num != 0)
		{
			while (num >= SiblingsPool.Count)
			{
				SiblingsPool.Add(new Stack<PathBranch[]>());
			}
			SiblingsPool[num].Push(Siblings);
		}
		if (n == 0)
		{
			Siblings = NoSiblings;
			return;
		}
		while (n >= SiblingsPool.Count)
		{
			SiblingsPool.Add(new Stack<PathBranch[]>());
		}
		if (SiblingsPool[n].Count == 0)
		{
			Siblings = new PathBranch[n];
		}
		else
		{
			Siblings = SiblingsPool[n].Pop();
		}
	}

	public void SetChildren(List<PathNode> finerNodes)
	{
		surface = 0;
		centerPosition = Vector3.zero;
		if (Children != null)
		{
			Array.Resize(ref Children, finerNodes.Count);
		}
		else
		{
			Children = new PathNode[finerNodes.Count];
		}
		finerNodes.CopyTo(Children);
		PathNode[] children = Children;
		foreach (PathNode pathNode in children)
		{
			pathNode.ParentNode = this;
			surface += pathNode.Surface;
			centerPosition += pathNode.RepresentativePosition.ToVector3() * pathNode.Surface;
		}
		representative = null;
		centerPosition /= (float)Math.Max(1, surface);
		float num = 0f;
		PathNode[] children2 = Children;
		foreach (PathNode pathNode2 in children2)
		{
			float sqrMagnitude = (pathNode2.RepresentativePosition.ToVector3() - centerPosition).sqrMagnitude;
			if (representative == null || sqrMagnitude < num)
			{
				representative = pathNode2;
				num = sqrMagnitude;
			}
		}
		if (representative != null)
		{
			representativePosition = representative.RepresentativePosition;
		}
	}

	public static void GetNodesLinkedTogether(List<PathNode> nodes, PathLod pathLod, ICollection<List<PathNode>> resultGroups)
	{
		LinkDico.Clear();
		int num = 0;
		foreach (PathNode node in nodes)
		{
			if (num >= ChildLinkPool.Count)
			{
				ChildLinkPool.Add(new UnionRegion<PathNode>());
			}
			UnionRegion<PathNode> unionRegion = ChildLinkPool[num++];
			unionRegion.Value = node;
			unionRegion.UnionParent = unionRegion;
			LinkDico.Add(node, unionRegion);
		}
		BranchDico.Clear();
		UsedBranchTypes.Clear();
		foreach (KeyValuePair<PathNode, UnionRegion<PathNode>> item in LinkDico)
		{
			UnionRegion<PathNode> value = item.Value;
			foreach (PathBranch sibling in pathLod.GetSiblings(value.Value))
			{
				if (sibling.Type != PathBranch.BranchType.Breaking)
				{
					Tuple<PathNode, PathNode> key = Tuple.Create(value.Value, sibling.Node);
					BranchDico.Add(key, sibling.Type);
					UsedBranchTypes.Add(sibling.Type);
				}
			}
		}
		OrderedBranchTypes.Clear();
		foreach (PathBranch.BranchType usedBranchType in UsedBranchTypes)
		{
			OrderedBranchTypes.Add(usedBranchType);
		}
		OrderedBranchTypes.Sort((PathBranch.BranchType a, PathBranch.BranchType b) => -a.CompareTo(b));
		for (int num2 = 0; num2 < OrderedBranchTypes.Count; num2++)
		{
			LinkNodesTogether(LinkDico, BranchDico, pathLod, OrderedBranchTypes[num2]);
		}
		num = 0;
		ParentDico.Clear();
		foreach (KeyValuePair<PathNode, UnionRegion<PathNode>> item2 in LinkDico)
		{
			UnionRegion<PathNode> topParent = item2.Value.TopParent;
			if (!ParentDico.TryGetValue(topParent, out var value2))
			{
				if (num >= GroupPool.Count)
				{
					GroupPool.Add(new List<PathNode>());
				}
				value2 = GroupPool[num++];
				value2.Clear();
				ParentDico.Add(topParent, value2);
				resultGroups.Add(value2);
			}
			value2.Add(item2.Key);
		}
	}

	private static void LinkNodesTogether(Dictionary<PathNode, UnionRegion<PathNode>> linkDico, Dictionary<Tuple<PathNode, PathNode>, PathBranch.BranchType> branchDico, PathLod pathLod, PathBranch.BranchType branchType)
	{
		foreach (KeyValuePair<Tuple<PathNode, PathNode>, PathBranch.BranchType> item in branchDico)
		{
			Tuple<PathNode, PathNode> key = item.Key;
			PathBranch.BranchType value = item.Value;
			Tuple<PathNode, PathNode> key2 = Tuple.Create(key._2, key._1);
			if (!branchDico.TryGetValue(key2, out var value2))
			{
				value2 = PathBranch.BranchType.Empty;
			}
			if (value.LodBits == branchType.LodBits && value2.LodBits == branchType.LodBits)
			{
				linkDico[key._1].UnionWith(linkDico[key._2]);
			}
		}
	}
}
