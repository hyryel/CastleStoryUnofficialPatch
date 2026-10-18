using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Rules;
using Brix.Legacy;
using Brix.Pathfinding;
using Brix.Pathfinding.Test;
using Brix.Utils;
using Brix.Utils.Collections;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.KnowledgeSpace;

public class KnowledgeDisjointRegionsFinder
{
	private struct RegionData
	{
		public IVolume Volume;

		public PathNode AccessNode;

		public int AccessIndex;
	}

	public class Builder : PathLodBuilder
	{
		public KnowledgeDisjointRegionsFinder Finder;

		public bool Found;

		public Builder(PathLod pathLod, PathBranchEvaluator evaluator, int lodCount = -1)
			: base(pathLod, evaluator, lodCount)
		{
		}

		public new void Reset(PathLod pathLod, int lodCount = -1)
		{
			base.Reset(pathLod, lodCount);
			Found = false;
		}

		protected override bool IsSuitableParent(PathNode parentNode)
		{
			if (base.IsSuitableParent(parentNode))
			{
				return true;
			}
			return false;
		}

		protected override bool IsSuitableBranch(PathBranch branch)
		{
			if (base.IsSuitableBranch(branch))
			{
				return true;
			}
			return false;
		}
	}

	public class Accepter : PathAccepter
	{
		public KnowledgeDisjointRegionsFinder Finder;

		public override bool Visit(PathLink link, PathLink fromSource)
		{
			return Finder.Visit(link.head.Node);
		}
	}

	public class BranchEvaluator : PathBranchEvaluator
	{
		public PathBranch.BranchType AcceptedType = PathBranch.BranchType.Climbing;

		public static bool PresenceTest(XYZ p)
		{
			return !VolumeRegistry.HasVolumesWithPropertyAt(p, VPropertyMask.obstacle | VPropertyMask.character, IsBlocker);
		}

		private static bool IsBlocker(IVolume v)
		{
			if (v is CharacterVolume)
			{
				return false;
			}
			return !v.PassThrough.IsWalkable();
		}

		public override bool CanCross(PathBranch branch)
		{
			if (branch.Type.CompareTo(AcceptedType) < 0)
			{
				return false;
			}
			if (branch.Node.IsFull)
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

	private const int InitPerFrame = 100;

	public PathNode FindAllOrigin;

	public PathBreadthFirstQueue FindAllQueue;

	public PathBranchEvaluator FindAllEvaluator;

	public Builder FindAllBuilder;

	public Accepter FindAllAccepter;

	public PathSearch FindAllSearch;

	public int FindAllSearched;

	public Dictionary<PathNode, PathLink> FindAllDictionary;

	public PathLink FindAllLink;

	public readonly int SearchPerFrame;

	private Stack<UnionRegion<RegionData>> _pooledRegions = new Stack<UnionRegion<RegionData>>();

	private Queue<IVolume> _interestingVolumes = new Queue<IVolume>();

	private List<UnionRegion<RegionData>> _regions = new List<UnionRegion<RegionData>>();

	private List<UnionRegion<RegionData>> _activeRegions = new List<UnionRegion<RegionData>>();

	private HashSet<UnionRegion<RegionData>> _visitedRegions = new HashSet<UnionRegion<RegionData>>();

	private Dictionary<UnionRegion<RegionData>, int> _disjointRegions = new Dictionary<UnionRegion<RegionData>, int>();

	private Dictionary<PathNode, UnionRegion<RegionData>> _nodeRegions = new Dictionary<PathNode, UnionRegion<RegionData>>();

	private UnionRegion<RegionData> _currentRegion;

	public List<Dictionary<IVolume, int>> Results = new List<Dictionary<IVolume, int>>();

	public KnowledgeDisjointRegionsFinder()
	{
		FindAllQueue = new PathBreadthFirstQueue();
		FindAllEvaluator = new BranchEvaluator();
		FindAllBuilder = new Builder(TestPathfinding.Instance.terrainLod, FindAllEvaluator)
		{
			Finder = this
		};
		FindAllAccepter = new Accepter
		{
			Finder = this
		};
		FindAllDictionary = new Dictionary<PathNode, PathLink>();
		FindAllSearch = new PathSearch(FindAllQueue, FindAllBuilder, FindAllAccepter, FindAllDictionary);
		FindAllLink = new PathLink();
		FindAllSearched = 0;
		SearchPerFrame = GeneralRules.KnowledgeUpdaterSearchPathBudget;
	}

	public void Destroy()
	{
		foreach (Dictionary<IVolume, int> result in Results)
		{
			result.Dispose();
		}
		Results.Clear();
		Results = null;
		FindAllQueue = null;
		FindAllEvaluator = null;
		FindAllBuilder = null;
		FindAllAccepter = null;
		FindAllDictionary = null;
		FindAllSearch = null;
		FindAllLink = null;
	}

	public List<Dictionary<IVolume, int>> ComputeDisjointRegions()
	{
		PrepareVolumesStep();
		while (!AddRootsStep())
		{
		}
		while (!MapStep())
		{
		}
		ResultStep();
		return Results;
	}

	public StaticNode ComputeDisjointRegionsNode()
	{
		return Node.Sequence.Do(delegate(KnowledgeDisjointRegionsFinder self)
		{
			self.PrepareVolumesStep();
		}, this).Label("Step 1").Do(Node.Selector.When((KnowledgeDisjointRegionsFinder self) => self.AddRootsStep(), this).Label("Step 2").Do(Node.Empty)
			.Label("Stop")
			.Else()
			.Do(Node.WaitFor.Ever())
			.Label("Wait forever"))
			.Do(Node.Selector.When((KnowledgeDisjointRegionsFinder self) => self.MapStep(), this).Label("Step 3a").Do(Node.Empty)
				.Label("Stop")
				.Else()
				.Do(Node.WaitFor.Ever())
				.Label("Wait forever"))
			.Do(delegate(KnowledgeDisjointRegionsFinder self)
			{
				self.ResultStep();
			}, this)
			.Label("Step 4");
	}

	private void PrepareVolumesStep()
	{
		foreach (Dictionary<IVolume, int> result in Results)
		{
			result.Dispose();
		}
		Results.Clear();
		FindAllSearch.Clear();
		FindAllLink.Reset(null);
		FindAllQueue.Clear();
		FindAllBuilder.Reset(PathLod.Empty);
		FindAllSearched = 0;
		_interestingVolumes.Clear();
		_regions.Clear();
		_activeRegions.Clear();
		_visitedRegions.Clear();
		_nodeRegions.Clear();
		foreach (GameObject item in Knowledge.Instance.GetAllInterestingGameObjects().NoLeak())
		{
			_interestingVolumes.Enqueue(item.GetComponent<IVolume>());
		}
	}

	private bool AddRootsStep()
	{
		for (int i = 0; i < 100; i++)
		{
			if (_interestingVolumes.Count == 0)
			{
				return true;
			}
			IVolume volume = _interestingVolumes.Dequeue();
			if (volume.GameObject.IsNullOrReleased())
			{
				continue;
			}
			TestTerrainLod terrainLod = TestPathfinding.Instance.terrainLod;
			int num = -1;
			foreach (XYZ @case in volume.GetCases(VProperty.access))
			{
				num++;
				PathNode pathNode = terrainLod.GetPathNode(@case);
				if (pathNode.IsEmpty || TreeRegistry.Instance.IsNearATree(@case) || !BranchEvaluator.PresenceTest(@case))
				{
					continue;
				}
				UnionRegion<RegionData> unionRegion = ((_pooledRegions.Count <= 0) ? new UnionRegion<RegionData>() : _pooledRegions.Pop());
				unionRegion.Value = new RegionData
				{
					Volume = volume,
					AccessNode = pathNode,
					AccessIndex = num
				};
				do
				{
					if (_nodeRegions.TryGetValue(pathNode, out var value))
					{
						unionRegion.UnionWith(value);
					}
					else
					{
						_nodeRegions.Add(pathNode, unionRegion);
					}
					pathNode = pathNode.ParentNode;
				}
				while (pathNode.IsFull);
				_regions.Add(unionRegion);
				if (unionRegion.UnionParent == unionRegion)
				{
					_activeRegions.Add(unionRegion);
				}
			}
		}
		return false;
	}

	private bool MapStep()
	{
		if (_currentRegion == null && MapStep_SelectRegion())
		{
			return true;
		}
		if (MapStep_Search())
		{
			_currentRegion = null;
		}
		return false;
	}

	private bool MapStep_SelectRegion()
	{
		if (_currentRegion != null)
		{
			return false;
		}
		int num;
		for (num = _activeRegions.Count; num > 0; num--)
		{
			UnionRegion<RegionData> unionRegion = _activeRegions[num - 1];
			if (unionRegion.UnionParent == unionRegion && !_visitedRegions.Contains(unionRegion))
			{
				break;
			}
		}
		if (num == 0)
		{
			return true;
		}
		if (num < _activeRegions.Count)
		{
			_activeRegions.RemoveRange(num, _activeRegions.Count - num);
		}
		_currentRegion = _activeRegions[num - 1];
		TestTerrainLod terrainLod = TestPathfinding.Instance.terrainLod;
		FindAllLink.Reset(_currentRegion.Value.AccessNode);
		FindAllQueue.Clear();
		FindAllBuilder.Reset(terrainLod);
		FindAllSearch.Start(FindAllLink);
		return false;
	}

	private bool MapStep_Search()
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
			_visitedRegions.Add(_currentRegion.TopParent);
			return true;
		}
		return false;
	}

	private void ResultStep()
	{
		FindAllSearch.Clear();
		FindAllLink.Reset(null);
		FindAllQueue.Clear();
		FindAllBuilder.Reset(PathLod.Empty);
		FindAllSearched = 0;
		_disjointRegions.Clear();
		foreach (UnionRegion<RegionData> region in _regions)
		{
			if (region.UnionParent == region)
			{
				_disjointRegions.Add(region, _disjointRegions.Count);
				Results.Add(CollectionPool.RequestDictionary<IVolume, int>());
			}
		}
		foreach (UnionRegion<RegionData> region2 in _regions)
		{
			int index = _disjointRegions[region2.TopParent];
			Dictionary<IVolume, int> dictionary = Results[index];
			IVolume volume = region2.Value.Volume;
			if (!volume.GameComponent.IsNullOrReleased())
			{
				dictionary[volume] = dictionary.GetValueOrDefault(volume) | (1 << region2.Value.AccessIndex);
			}
		}
		foreach (UnionRegion<RegionData> region3 in _regions)
		{
			region3.UnionParent = region3;
			region3.Value = default(RegionData);
			_pooledRegions.Push(region3);
		}
		_regions.Clear();
		_activeRegions.Clear();
		_visitedRegions.Clear();
		_nodeRegions.Clear();
		_disjointRegions.Clear();
	}

	public void HighlightResults()
	{
		Signal<GameObject> s = new Signal<GameObject>();
		foreach (Dictionary<IVolume, int> result in Results)
		{
			Color color = Random.ColorHSV();
			foreach (KeyValuePair<IVolume, int> item in result)
			{
				IVolume key = item.Key;
				VolumePropertyRequest cases = key.GetCases(VProperty.access);
				int value = item.Value;
				for (int i = 0; i < cases.Count; i++)
				{
					if ((value & (1 << i)) != 0)
					{
						Highlighter.highlight.At(cases[i], color, s);
					}
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
		if (_nodeRegions.TryGetValue(node, out var value) && _currentRegion != value)
		{
			_currentRegion.UnionWith(value);
		}
		return false;
	}
}
