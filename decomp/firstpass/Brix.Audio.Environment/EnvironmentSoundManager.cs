using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.Nature;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Audio.Environment;

public class EnvironmentSoundManager : UpdatableGameComponent<EnvironmentSoundManager>
{
	public enum RegionType
	{
		Tree,
		Pyrite
	}

	public class RegionData
	{
		public int Count;

		public Vector3 Center;

		public GameObject GameObject;

		public float PriorityHint;

		public RegionType RegionType;

		public void Add(Vector3 p)
		{
			Center = (Center * Count + p) / (Count + 1);
			Count++;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(EnvironmentSoundManager).FullName);

	public Transform Focus;

	public float MaxRadius = 200f;

	public int MaxVisibleCount = 20;

	public int IslandDistancePreciseRange = 40;

	public int IslandDistanceMax = 200;

	public int IslandMassMax = 50000000;

	[Range(0f, 1f)]
	public float DistanceVsQuantity;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int VisibleRegionCount;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int IslandDistancePrecise;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int IslandDistanceCoarse;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int IslandDistance;

	private int _nextId;

	private bool _initialized;

	private IClosestNonEmptyVoxelEnumerator _closestVoxelEnumerator;

	private readonly List<XYZ> _dirtyTreeVoxels = new List<XYZ>();

	private readonly List<XYZ> _dirtyPyriteVoxels = new List<XYZ>();

	private readonly List<RegionData> _enabledTreeRegions = new List<RegionData>();

	private readonly List<RegionData> _enabledPyriteRegions = new List<RegionData>();

	private readonly Dictionary<XYZ, HashSet<UnionRegion<RegionData>>> _mvDico = new Dictionary<XYZ, HashSet<UnionRegion<RegionData>>>();

	private readonly XYZ[] _neighbors = Calcule.VolumeBetween(-XYZ.one, XYZ.one).ToArray();

	private int _visibleVoxels = -1;

	protected override void Start()
	{
		base.Start();
		if (Focus == null)
		{
			Focus = Camera.main.transform;
		}
		GameSignals.LateConnect(GameSignals.LevelReady, delegate
		{
			Brix.Engine.Neo.Relais(InitAll, 1f, "EnvironmentSoundManager.InitAll");
		});
		GameSignals.Connect(GameSignals.TreeChanged, delegate(XYZ p)
		{
			_dirtyTreeVoxels.Add(p);
		});
		GameSignals.Connect(GameSignals.MineralsChanged, delegate(XYZ p)
		{
			_dirtyPyriteVoxels.Add(p);
		});
	}

	protected override void BrixUpdate()
	{
		if (_initialized)
		{
			UpdateIslandDistance();
		}
	}

	protected override void BrixSlowUpdate()
	{
		if (!_initialized)
		{
			return;
		}
		if (_dirtyTreeVoxels.Count > 0)
		{
			HashSet<MegaVoxel> hashSet = (from voxel in _dirtyTreeVoxels.Slinq()
				select Brix.Engine.Architecte.TryGetMegaVoxel(Brix.Engine.Abstract.Voxel.GetGScalaire(voxel.x, voxel.y, voxel.z))).ToPooledHashSet();
			foreach (MegaVoxel item in hashSet)
			{
				if (item != null)
				{
					RefreshTreesFromMv(item);
				}
			}
			hashSet.Dispose();
			_dirtyTreeVoxels.Clear();
		}
		if (_dirtyPyriteVoxels.Count > 0)
		{
			HashSet<MegaVoxel> hashSet2 = (from voxel in _dirtyPyriteVoxels.Slinq()
				select Brix.Engine.Architecte.TryGetMegaVoxel(Brix.Engine.Abstract.Voxel.GetGScalaire(voxel.x, voxel.y, voxel.z))).ToPooledHashSet();
			foreach (MegaVoxel item2 in hashSet2)
			{
				if (item2 != null)
				{
					RefreshPyritesFromMv(item2);
				}
			}
			hashSet2.Dispose();
			_dirtyPyriteVoxels.Clear();
		}
		Vector3 position = Focus.position;
		Vector3 vector = Vector3.one * MaxRadius;
		XYZ b = XYZ.one * 16;
		XYZ min = XYZ.Div(XYZ.Floor(position - vector), b);
		XYZ max = XYZ.Div(XYZ.Ceil(position + vector), b);
		for (int num = 0; num < _enabledTreeRegions.Count; num++)
		{
			_enabledTreeRegions[num].GameObject.SetActive(value: false);
		}
		_enabledTreeRegions.Clear();
		for (int num2 = 0; num2 < _enabledPyriteRegions.Count; num2++)
		{
			_enabledPyriteRegions[num2].GameObject.SetActive(value: false);
		}
		_enabledPyriteRegions.Clear();
		foreach (XYZ item3 in Calcule.VolumeBetween(min, max))
		{
			HashSet<UnionRegion<RegionData>> valueOrDefault = _mvDico.GetValueOrDefault(item3);
			if (valueOrDefault == null)
			{
				continue;
			}
			foreach (UnionRegion<RegionData> item4 in valueOrDefault)
			{
				RegionData value = item4.Value;
				GameObject gameObject = value.GameObject;
				if (!(gameObject == null))
				{
					switch (value.RegionType)
					{
					case RegionType.Tree:
						_enabledTreeRegions.Add(value);
						break;
					case RegionType.Pyrite:
						_enabledPyriteRegions.Add(value);
						break;
					}
				}
			}
		}
		if (_enabledTreeRegions.Count > MaxVisibleCount)
		{
			KeepBestCandidates(_enabledTreeRegions, MaxVisibleCount);
		}
		if (_enabledPyriteRegions.Count > MaxVisibleCount)
		{
			KeepBestCandidates(_enabledPyriteRegions, MaxVisibleCount);
		}
		VisibleRegionCount = 0;
		for (int num3 = 0; num3 < _enabledTreeRegions.Count; num3++)
		{
			_enabledTreeRegions[num3].GameObject.SetActive(value: true);
		}
		VisibleRegionCount += _enabledTreeRegions.Count;
		for (int num4 = 0; num4 < _enabledPyriteRegions.Count; num4++)
		{
			_enabledPyriteRegions[num4].GameObject.SetActive(value: true);
		}
		VisibleRegionCount += _enabledPyriteRegions.Count;
		SlowUpdateIslandDistance();
		SoundEngine.SetParam("ENV_Island_Distance", DistanceToIsland(), base.gameObject);
	}

	private void KeepBestCandidates(List<RegionData> regions, int count)
	{
		Vector3 position = Focus.position;
		for (int i = 0; i < regions.Count; i++)
		{
			RegionData regionData = regions[i];
			regionData.PriorityHint = Mathf.Lerp((regionData.Center - position).sqrMagnitude, regionData.Count, DistanceVsQuantity);
		}
		regions.Sort((RegionData a, RegionData b) => a.PriorityHint.CompareTo(b.PriorityHint));
		regions.RemoveRange(count, regions.Count - count);
	}

	[ContextMenu("InitAll")]
	private void InitAll()
	{
		ClearAllGameObjects();
		_dirtyTreeVoxels.Clear();
		_dirtyPyriteVoxels.Clear();
		InitAllTrees(_mvDico);
		InitAllPyrites(_mvDico);
		InitIsland();
		_initialized = true;
	}

	private void InitAllTrees(Dictionary<XYZ, HashSet<UnionRegion<RegionData>>> mvDico)
	{
		HashSet<UnionRegion<RegionData>> hashSet = CollectionPool.RequestHashSet<UnionRegion<RegionData>>();
		List<XYZ> list = TreeRegistry.Instance.ListAllTreePosition().Slinq().ToPooledList();
		InitRegionsFromMvs(list, mvDico, hashSet);
		CreateGameObjects(hashSet, RegionType.Tree);
		hashSet.Dispose();
		list.Dispose();
	}

	private void InitAllPyrites(Dictionary<XYZ, HashSet<UnionRegion<RegionData>>> mvDico)
	{
		HashSet<UnionRegion<RegionData>> hashSet = CollectionPool.RequestHashSet<UnionRegion<RegionData>>();
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		PyriteRegistry.AddAllMineralPositionsToList(ref list);
		InitRegionsFromMvs(list, mvDico, hashSet);
		CreateGameObjects(hashSet, RegionType.Pyrite);
		hashSet.Dispose();
		list.Dispose();
	}

	private void RefreshTreesFromMv(MegaVoxel mv)
	{
		XYZ key = mv.MvOffset / 16;
		HashSet<UnionRegion<RegionData>> valueOrCreate = _mvDico.GetValueOrCreate(key);
		RefreshFromMv(mv.TreePositions, RegionType.Tree, valueOrCreate);
	}

	private void RefreshPyritesFromMv(MegaVoxel mv)
	{
		XYZ key = mv.MvOffset / 16;
		HashSet<UnionRegion<RegionData>> valueOrCreate = _mvDico.GetValueOrCreate(key);
		RefreshFromMv(mv.PyritePositions, RegionType.Pyrite, valueOrCreate);
	}

	private void RefreshFromMv(List<XYZ> selectedVoxels, RegionType regionType, HashSet<UnionRegion<RegionData>> mvData)
	{
		List<UnionRegion<RegionData>> list = mvData.Slinq().Where((UnionRegion<RegionData> region, RegionType type) => region.Value.RegionType == type, regionType).ToPooledList();
		foreach (UnionRegion<RegionData> item in list)
		{
			DestroyRegion(item);
			mvData.Remove(item);
		}
		list.Dispose();
		InitRegions(selectedVoxels, mvData);
		CreateGameObjects(mvData, regionType);
	}

	private void ClearAllGameObjects()
	{
		foreach (KeyValuePair<XYZ, HashSet<UnionRegion<RegionData>>> item in _mvDico)
		{
			HashSet<UnionRegion<RegionData>> value = item.Value;
			foreach (UnionRegion<RegionData> item2 in value)
			{
				DestroyRegion(item2);
			}
		}
		_mvDico.Clear();
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = base.transform.GetChild(num);
			logger.WarnFormat("EnvironmentSoundManager destroying unknown object '{0}'", child.name);
			Object.Destroy(child.gameObject);
		}
	}

	private void CreateGameObjects(HashSet<UnionRegion<RegionData>> treeRegionTops, RegionType regionType)
	{
		foreach (UnionRegion<RegionData> treeRegionTop in treeRegionTops)
		{
			if (treeRegionTop.Value.GameObject != null)
			{
				continue;
			}
			string text;
			string text2;
			string text3;
			if (regionType != RegionType.Tree)
			{
				if (regionType != RegionType.Pyrite)
				{
					continue;
				}
				text = $"Brimstones {_nextId} ({treeRegionTop.Value.Count})";
				text2 = "ENV_Brimstone_Cluster";
				text3 = "ENV_Brimstone_Cluster_Density";
			}
			else
			{
				text = $"Trees {_nextId} ({treeRegionTop.Value.Count})";
				text2 = "ENV_Tree_Cluster";
				text3 = "ENV_Tree_Cluster_Density";
			}
			GameObject gameObject = new GameObject(text);
			gameObject.AddComponent<EnvironmentSoundCluster>().RegionData = treeRegionTop.Value;
			gameObject.transform.position = treeRegionTop.Value.Center;
			gameObject.transform.SetParent(base.transform);
			SoundEngine.Play(text2, gameObject);
			SoundEngine.SetParam(text3, treeRegionTop.Value.Count, gameObject);
			gameObject.SetActive(value: false);
			treeRegionTop.Value.GameObject = gameObject;
			treeRegionTop.Value.RegionType = regionType;
			_nextId++;
		}
	}

	private void DestroyRegion(UnionRegion<RegionData> top)
	{
		if (!(top.Value.GameObject == null))
		{
			string text;
			switch (top.Value.RegionType)
			{
			default:
				return;
			case RegionType.Tree:
				text = "ENV_Tree_Cluster_Kill";
				break;
			case RegionType.Pyrite:
				text = "ENV_Brimstone_Cluster_Kill";
				break;
			}
			SoundEngine.Play(text, top.Value.GameObject);
			Object.Destroy(top.Value.GameObject);
		}
	}

	private void InitRegionsFromMvs(ICollection<XYZ> selectedVoxels, Dictionary<XYZ, HashSet<UnionRegion<RegionData>>> mvDico, HashSet<UnionRegion<RegionData>> resultRegions)
	{
		Dictionary<XYZ, List<XYZ>> dictionary = CollectionPool.RequestDictionary<XYZ, List<XYZ>>();
		foreach (XYZ selectedVoxel in selectedVoxels)
		{
			dictionary.GetValueOrCreateThenAdd(selectedVoxel / 16, selectedVoxel, CollectionPool.RequestList<XYZ>);
		}
		foreach (KeyValuePair<XYZ, List<XYZ>> item in dictionary)
		{
			HashSet<UnionRegion<RegionData>> valueOrCreate = mvDico.GetValueOrCreate(item.Key);
			InitRegions(item.Value, valueOrCreate);
			resultRegions.UnionWith(valueOrCreate);
			item.Value.Dispose();
		}
		dictionary.Dispose();
	}

	private void InitRegions1(ICollection<XYZ> selectedVoxels, HashSet<UnionRegion<RegionData>> resultRegions)
	{
		Dictionary<XZ, List<XZ>> dictionary = CollectionPool.RequestDictionary<XZ, List<XZ>>();
		foreach (XYZ selectedVoxel in selectedVoxels)
		{
			for (int i = selectedVoxel.x - 2; i <= selectedVoxel.x + 2; i++)
			{
				for (int j = selectedVoxel.z - 2; j <= selectedVoxel.z + 2; j++)
				{
					dictionary.GetValueOrCreateThenAdd(new XZ(i, j), XZ.FromXYZ(selectedVoxel));
				}
			}
		}
		Dictionary<XZ, UnionRegion<RegionData>> dictionary2 = CollectionPool.RequestDictionary<XZ, UnionRegion<RegionData>>();
		foreach (KeyValuePair<XZ, List<XZ>> item in dictionary)
		{
			dictionary2.Add(item.Key, new UnionRegion<RegionData>());
		}
		foreach (KeyValuePair<XZ, List<XZ>> item2 in dictionary)
		{
			UnionRegion<RegionData> other = dictionary2[item2.Value[0]];
			for (int k = 1; k < item2.Value.Count; k++)
			{
				dictionary2[item2.Value[k]].UnionWith(other);
			}
		}
		dictionary.Dispose();
		foreach (XYZ selectedVoxel2 in selectedVoxels)
		{
			UnionRegion<RegionData> topParent = dictionary2[XZ.FromXYZ(selectedVoxel2)].TopParent;
			if (topParent.Value == null)
			{
				topParent.Value = new RegionData();
			}
			topParent.Value.Add(selectedVoxel2.ToVector3());
			resultRegions.Add(topParent);
		}
		dictionary2.Dispose();
	}

	private void InitRegions(ICollection<XYZ> selectedVoxels, HashSet<UnionRegion<RegionData>> resultRegions)
	{
		Dictionary<XYZ, List<XYZ>> dictionary = CollectionPool.RequestDictionary<XYZ, List<XYZ>>();
		foreach (XYZ selectedVoxel in selectedVoxels)
		{
			dictionary.GetValueOrCreateThenAdd(selectedVoxel / 3, selectedVoxel);
		}
		Dictionary<XYZ, UnionRegion<RegionData>> dictionary2 = CollectionPool.RequestDictionary<XYZ, UnionRegion<RegionData>>();
		foreach (KeyValuePair<XYZ, List<XYZ>> item in dictionary)
		{
			foreach (XYZ item2 in item.Value)
			{
				dictionary2.Add(item2, new UnionRegion<RegionData>());
			}
		}
		foreach (KeyValuePair<XYZ, List<XYZ>> item3 in dictionary)
		{
			XYZ[] neighbors = _neighbors;
			foreach (XYZ xYZ in neighbors)
			{
				if (!dictionary.TryGetValue(item3.Key + xYZ, out var value))
				{
					continue;
				}
				foreach (XYZ item4 in item3.Value)
				{
					foreach (XYZ item5 in value)
					{
						if (XYZ.SqrMagnitude(item4 - item5) <= 9)
						{
							dictionary2[item4].UnionWith(dictionary2[item5]);
						}
					}
				}
			}
		}
		dictionary.Dispose();
		foreach (XYZ selectedVoxel2 in selectedVoxels)
		{
			UnionRegion<RegionData> topParent = dictionary2[selectedVoxel2].TopParent;
			if (topParent.Value == null)
			{
				topParent.Value = new RegionData();
			}
			topParent.Value.Add(selectedVoxel2.ToVector3());
			resultRegions.Add(topParent);
		}
		dictionary2.Dispose();
	}

	private void InitIsland()
	{
		SlowUpdateIslandDistance();
		SoundEngine.Play("ENV_Island_Ambiance", base.gameObject);
		SoundEngine.SetParam("ENV_Island_Distance", DistanceToIsland(), base.gameObject);
		SoundEngine.SetParam("ENV_Island_Mass", IslandMass(), base.gameObject);
	}

	private void UpdateIslandDistance()
	{
		if (IslandDistancePreciseRange <= 0 || IslandDistancePreciseRange >= IslandDistanceMax)
		{
			return;
		}
		XYZ xYZ = XYZ.FromVector3(Focus.position);
		if (_closestVoxelEnumerator == null)
		{
			_closestVoxelEnumerator = Brix.Engine.Voxel.Engine.GetClosestNonEmptyVoxelEnumerator(xYZ, IslandDistancePreciseRange, IslandDistanceMax);
		}
		for (int i = 0; i < 100; i++)
		{
			if (!_closestVoxelEnumerator.MoveNext())
			{
				IslandDistancePrecise = (int)Mathf.Sqrt(XYZ.SqrMagnitude(_closestVoxelEnumerator.Current - _closestVoxelEnumerator.Position));
				_closestVoxelEnumerator.Position = xYZ;
				_closestVoxelEnumerator.CloseDistance = IslandDistancePreciseRange;
				_closestVoxelEnumerator.MaxDistance = IslandDistanceMax;
				_closestVoxelEnumerator.Reset();
				break;
			}
		}
	}

	private void SlowUpdateIslandDistance()
	{
		if (IslandDistancePreciseRange < IslandDistanceMax && IslandDistancePrecise <= IslandDistancePreciseRange)
		{
			IslandDistance = IslandDistancePrecise;
			return;
		}
		XYZ xYZ = XYZ.FromVector3(Focus.position);
		IslandDistanceCoarse = (int)Mathf.Sqrt(XYZ.SqrMagnitude(Brix.Engine.Voxel.Engine.GetClosestNonEmptyVoxelLowRes(xYZ, IslandDistanceMax) - xYZ));
		IslandDistance = (int)Mathf.Min(IslandDistancePrecise, Mathf.Lerp(IslandDistancePrecise, IslandDistanceCoarse, (float)IslandDistancePrecise / ((float)IslandDistancePreciseRange * 1.1f)));
	}

	public float DistanceToIsland()
	{
		return 100f * Mathf.InverseLerp((float)IslandDistanceMax * 0.1f, IslandDistanceMax, IslandDistance);
	}

	public float IslandMass()
	{
		if (_visibleVoxels < 0)
		{
			_visibleVoxels = 0;
			foreach (XYZ topVoxelPositionsFromLayer in Brix.Engine.Voxel.Engine.GetTopVoxelPositionsFromLayers())
			{
				Brix.Engine.Voxel.Engine.GetColumnData(topVoxelPositionsFromLayer.x, topVoxelPositionsFromLayer.z, out var bottom, out var top);
				_visibleVoxels += top - bottom;
			}
		}
		return 100f * Mathf.InverseLerp(0f, Mathf.Log(IslandMassMax, 10f), Mathf.Log(_visibleVoxels + 1, 10f));
	}
}
