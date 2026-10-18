using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Rules;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Motus.Behavior;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class KnowledgeConnectedAccessFinder
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

	public class Accepter : PathAccepter
	{
		public KnowledgeConnectedAccessFinder Finder;

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			return Finder.Visit(link.head.Node);
		}
	}

	public class BranchEvaluator : PathBranchEvaluator
	{
		public PathBranch.BranchType AcceptedType = PathBranch.BranchType.Climbing;

		public bool PresenceTest(XYZ p)
		{
			return !VolumeRegistry.HasVolumesWithPropertyAt(p, VPropertyMask.obstacle | VPropertyMask.character);
		}

		public override bool CanCross(PathBranch branch)
		{
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
	}

	public PathNode FindAllOrigin;

	public PathBreadthFirstQueue FindAllQueue;

	public PathBranchEvaluator FindAllEvaluator;

	public Builder FindAllBuilder;

	public Accepter FindAllAccepter;

	public PathSearch FindAllSearch;

	public int FindAllSearched;

	public Dictionary<PathNode, PathLink> FindAllDictionary;

	public readonly int SearchPerFrame = 20;

	public Dictionary<IVolume, int> Results = new Dictionary<IVolume, int>();

	private List<Tuple<IVolume, XYZ>> _visitedAccesses = new List<Tuple<IVolume, XYZ>>();

	public KnowledgeConnectedAccessFinder()
	{
		FindAllQueue = new PathBreadthFirstQueue();
		FindAllEvaluator = new BranchEvaluator();
		FindAllBuilder = new Builder(TestPathfinding.Instance.terrainLod, FindAllEvaluator);
		FindAllAccepter = new Accepter
		{
			Finder = this
		};
		FindAllDictionary = new Dictionary<PathNode, PathLink>();
		FindAllSearch = new PathSearch(FindAllQueue, FindAllBuilder, FindAllAccepter, FindAllDictionary);
		FindAllSearched = 0;
		SearchPerFrame = GeneralRules.KnowledgeExcluderSearchPathBudget;
	}

	public void Destroy()
	{
		Results.Clear();
		Results = null;
		FindAllQueue = null;
		FindAllEvaluator = null;
		FindAllBuilder = null;
		FindAllAccepter = null;
		FindAllDictionary = null;
		FindAllSearch = null;
	}

	private void PrepareStep(VolumePropertyRequest accesses)
	{
		Results.Clear();
		_visitedAccesses.Clear();
		TestTerrainLod terrainLod = TestPathfinding.Instance.terrainLod;
		FindAllSearch.Clear();
		FindAllSearched = 0;
		FindAllQueue.Clear();
		FindAllBuilder.Reset(terrainLod);
		foreach (XYZ item in accesses)
		{
			PathNode pathNode = terrainLod.GetPathNode(item);
			if (!pathNode.IsEmpty)
			{
				FindAllSearch.Start(pathNode);
			}
		}
		if (FindAllQueue.Empty)
		{
			FindAllSearch.Start(PathNode.Empty);
		}
	}

	private bool SearchStep()
	{
		bool flag = false;
		for (int i = 0; i < SearchPerFrame; i++)
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

	private void ResultStep()
	{
		FindAllSearch.Clear();
		FindAllQueue.Clear();
		FindAllBuilder.Reset(PathLod.Empty);
		FindAllSearched = 0;
		foreach (Tuple<IVolume, XYZ> visitedAccess in _visitedAccesses)
		{
			IVolume _ = visitedAccess._1;
			if (!_.GameObject.IsNullOrReleased())
			{
				Results[_] = Results.GetValueOrDefault(_) | (1 << _.GetCases(VProperty.access).IndexOf(visitedAccess._2));
			}
		}
		_visitedAccesses.Clear();
	}

	public Dictionary<IVolume, int> ComputeConnectedAccesses(VolumePropertyRequest accesses)
	{
		PrepareStep(accesses);
		while (!SearchStep())
		{
		}
		ResultStep();
		return Results;
	}

	public StaticNode ComputeConnectedAccessesNode(VolumePropertyRequest accesses)
	{
		return Node.Sequence.Do(delegate(KnowledgeConnectedAccessFinder self, VolumePropertyRequest a)
		{
			self.PrepareStep(a);
		}, this, accesses).Label("Step 1").Do(Node.Selector.When((KnowledgeConnectedAccessFinder self) => self.SearchStep(), this).Label("Step 2").Do(Node.Empty)
			.Label("Stop")
			.Else()
			.Do(Node.WaitFor.Ever())
			.Label("Wait forever"))
			.Do(delegate(KnowledgeConnectedAccessFinder self)
			{
				self.ResultStep();
			}, this)
			.Label("Step 3");
	}

	public void HighlightResults()
	{
		Signal<GameObject> s = new Signal<GameObject>();
		foreach (KeyValuePair<IVolume, int> result in Results)
		{
			Color color = Random.ColorHSV();
			IVolume key = result.Key;
			VolumePropertyRequest cases = key.GetCases(VProperty.access);
			int value = result.Value;
			for (int i = 0; i < cases.Count; i++)
			{
				if ((value & (1 << i)) != 0)
				{
					Highlighter.highlight.At(cases[i], color, s);
				}
			}
		}
		Neo.Relais(delegate
		{
			s.Invoke(null);
		}, 3f, "Knowledge Highlight Results");
	}

	private bool Visit(PathNode node)
	{
		if (node.AccessCount > 0)
		{
			if (node.Lod != 0)
			{
				return false;
			}
			if (TestPathfinding.Instance.AccessPositions.TryGetValue(node.RepresentativePosition, out var value))
			{
				foreach (IVolume item in value)
				{
					_visitedAccesses.Add(Tuple.Create(item, node.RepresentativePosition));
				}
			}
		}
		return false;
	}
}
