using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Brix.Engine.Geometrie;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

internal class LayerVoxelEngine : VoxelEngineImpl
{
	public struct GetClosestVisibleVoxelEnumerator : IClosestNonEmptyVoxelEnumerator, IEnumerator<XYZ>, IDisposable, IEnumerator
	{
		private XYZ p;

		private int closeDistance;

		private int maxDistance;

		private readonly Color32[] filling;

		private readonly int width;

		private int r;

		private int i;

		private XYZ bestP;

		private int bestD2;

		public XYZ Position
		{
			get
			{
				return p;
			}
			set
			{
				p = value;
			}
		}

		public int CloseDistance
		{
			get
			{
				return closeDistance;
			}
			set
			{
				closeDistance = value;
			}
		}

		public int MaxDistance
		{
			get
			{
				return maxDistance;
			}
			set
			{
				maxDistance = value;
			}
		}

		public XYZ Current => bestP;

		object IEnumerator.Current => Current;

		internal GetClosestVisibleVoxelEnumerator(TestWorldConfig3 test, XYZ position, int closeDistance, int maxDistance)
		{
			p = position;
			this.closeDistance = closeDistance;
			this.maxDistance = maxDistance;
			r = 0;
			i = 0;
			test.InitLayers();
			HeightmapLayer layer = test.layers.GetLayer<HeightmapLayer>();
			filling = layer.fillingTex.GetPixels32();
			width = layer.fillingTex.Width;
			bestP = position + XYZ.forward * maxDistance;
			bestD2 = maxDistance * maxDistance;
			DistanceToLayer(new XZ(position.x, position.z));
		}

		public bool MoveNext()
		{
			if (i >= r * 8)
			{
				r++;
				if (r * r >= bestD2 || r > closeDistance)
				{
					return false;
				}
				i = 0;
			}
			XZ q = new XZ(p.x, p.z);
			if (i < r * 2)
			{
				q.x += r - i - 1;
				q.z += r;
			}
			else if (i < r * 4)
			{
				q.x -= r;
				q.z += r - (i - r * 2) - 1;
			}
			else if (i < r * 6)
			{
				q.x -= r - (i - r * 4) - 1;
				q.z -= r;
			}
			else
			{
				q.x += r;
				q.z -= r - (i - r * 6) - 1;
			}
			if (DistanceToLayer(q) && r * r >= bestD2)
			{
				return false;
			}
			i++;
			return true;
		}

		public void Reset()
		{
			r = 1;
			i = 0;
			bestP = p + XYZ.forward * maxDistance;
			bestD2 = maxDistance * maxDistance;
			DistanceToLayer(new XZ(p.x, p.z));
		}

		public void Dispose()
		{
		}

		private bool DistanceToLayer(XZ q)
		{
			if (q.x < 0 || q.x >= width || q.z < 0 || q.z >= width)
			{
				return false;
			}
			Color32 color = filling[q.x + q.z * width];
			if (color.r >= color.g)
			{
				return false;
			}
			int num = color.g - 1;
			int num2 = color.r;
			int num3 = Mathf.Max(0, Mathf.Max(p.y - num, num2 - p.y));
			XZ xZ = new XZ(p.x - q.x, p.z - q.z);
			int num4 = xZ.x * xZ.x + xZ.z * xZ.z;
			int num5 = num4 + num3 * num3;
			if (num5 >= bestD2)
			{
				return false;
			}
			bestD2 = num5;
			bestP = new XYZ(q.x, (p.y >= num) ? num : ((p.y < num2) ? num2 : p.y), p.z);
			return true;
		}
	}

	private static BrixLogger logger = new BrixLogger();

	private readonly TestWorldConfig3 test;

	private HeightmapLayer _heightmapLayer;

	private FillRangeLayer _fillRangeLayer;

	private NegativeLayer _negativeLayer;

	private TerrainSurfaceLayer _terrainSurfaceLayer;

	private bool exForceSynchronousUpdate;

	private bool isChanging;

	public Dictionary<XYZ, int> p2i = new Dictionary<XYZ, int>();

	public List<int> types = new List<int>();

	public List<int> volumes = new List<int>();

	public HashSet<XYZ> removeGrass = new HashSet<XYZ>();

	public bool dirtyTerrainResources = false;

	private int handleError = 0;

	private Dictionary<int, Color32> c2i = new Dictionary<int, Color32>();

	private List<Tuple<XZ, Color32>> addedColumns = new List<Tuple<XZ, Color32>>();

	private HeightmapLayer heightmapLayer => _heightmapLayer ?? (_heightmapLayer = test.layers.GetLayer<HeightmapLayer>());

	private FillRangeLayer fillRangeLayer => _fillRangeLayer ?? (_fillRangeLayer = test.layers.GetLayer<FillRangeLayer>());

	private NegativeLayer negativeLayer => _negativeLayer ?? (_negativeLayer = test.layers.GetLayer<NegativeLayer>());

	private TerrainSurfaceLayer terrainSurfaceLayer => _terrainSurfaceLayer ?? (_terrainSurfaceLayer = test.layers.GetLayer<TerrainSurfaceLayer>());

	public override bool PreferSynchronousUpdate
	{
		get
		{
			return test.forceSynchronousUpdate;
		}
		set
		{
			test.forceSynchronousUpdate = value;
		}
	}

	public override bool PreferSynchronousDrawing
	{
		get
		{
			return test.forceSynchronousDrawing;
		}
		set
		{
			test.forceSynchronousDrawing = value;
			test.drawFullInitialWorld = value;
		}
	}

	public override bool UseDeepTypeNoise
	{
		get
		{
			return test.useDeepTypeNoise;
		}
		set
		{
			test.useDeepTypeNoise = value;
		}
	}

	public override Material RenderMaterial
	{
		get
		{
			return test.renderTarget.GetComponent<Renderer>().sharedMaterial;
		}
		set
		{
			test.renderTarget.GetComponent<Renderer>().sharedMaterial = value;
		}
	}

	public override XYZ PreferredOccupiedVoxelRegionSize => test.MvSize;

	internal LayerVoxelEngine(TestWorldConfig3 test)
	{
		this.test = test;
	}

	public bool IsValidVoxelHandle(VoxelHandle h)
	{
		return h.VoxelEngine == this && test != null;
	}

	public override Brix.Engine.VoxelHandle GetVoxelHandle(XYZ p)
	{
		return GetSpecializedVoxelHandle(p);
	}

	public VoxelHandle GetSpecializedVoxelHandle(XYZ p)
	{
		VoxelHandle voxelHandle = new VoxelHandle();
		AssignVoxelHandle(voxelHandle, p);
		return voxelHandle;
	}

	public void AssignVoxelHandle(VoxelHandle h, XYZ p, bool updateIfNeeded = false)
	{
		h.VoxelEngine = this;
		h.p = p;
	}

	public override void BeginChanges()
	{
		if (!isChanging)
		{
			exForceSynchronousUpdate = test.forceSynchronousUpdate;
			test.forceSynchronousUpdate = false;
			isChanging = true;
		}
	}

	public override void ApplyChanges(bool removeUnusedVoxels = false)
	{
		if (isChanging)
		{
			test.forceSynchronousUpdate = exForceSynchronousUpdate;
			isChanging = false;
		}
		bool flag = false;
		if (c2i.Count != 0 && this.fillRangeLayer != null)
		{
			List<Tuple<XZ, Color32>> list = CollectionPool.RequestList<Tuple<XZ, Color32>>();
			FillRangeLayer fillRangeLayer = this.fillRangeLayer;
			fillRangeLayer.BeginChanges();
			foreach (KeyValuePair<int, Color32> item2 in c2i)
			{
				XZ xZ = new XZ(item2.Key & 0xFFFF, (item2.Key >> 16) & 0xFFFF);
				if (fillRangeLayer.SetColumn(xZ, item2.Value))
				{
					list.Add(Tuple.Create(xZ, item2.Value));
				}
			}
			c2i.Clear();
			fillRangeLayer.ApplyChanges();
			for (int i = 0; i < list.Count; i++)
			{
				Tuple<XZ, Color32> tuple = list[i];
				for (byte b = tuple._2.r; b < tuple._2.g; b++)
				{
					test.SetDirty(new XYZ(tuple._1.x, b, tuple._1.z));
				}
			}
			list.Dispose();
		}
		if (c2i.Count != 0)
		{
			heightmapLayer.BeginChanges();
			foreach (KeyValuePair<int, Color32> item3 in c2i)
			{
				XYZ xYZ = new XYZ(item3.Key & 0xFFFF, item3.Value.g - 1, (item3.Key >> 16) & 0xFFFF);
				if (heightmapLayer.SetColumn(new XZ(xYZ.x, xYZ.z), item3.Value))
				{
					flag = true;
				}
			}
			heightmapLayer.ApplyChanges();
			foreach (XYZ dirtyVoxel in heightmapLayer.DirtyVoxels)
			{
				test.SetDirty(dirtyVoxel);
			}
			c2i.Clear();
		}
		if (addedColumns.Count != 0)
		{
			List<Tuple<XZ, Color32>> list2 = CollectionPool.RequestList<Tuple<XZ, Color32>>();
			FillRangeLayer fillRangeLayer2 = this.fillRangeLayer;
			fillRangeLayer2.BeginChanges();
			for (int j = 0; j < addedColumns.Count; j++)
			{
				Tuple<XZ, Color32> item = addedColumns[j];
				if (fillRangeLayer2.AddColumn(item._1, item._2))
				{
					list2.Add(item);
				}
			}
			addedColumns.Clear();
			fillRangeLayer2.ApplyChanges();
			for (int k = 0; k < list2.Count; k++)
			{
				Tuple<XZ, Color32> tuple2 = list2[k];
				for (byte b2 = tuple2._2.r; b2 < tuple2._2.g; b2++)
				{
					test.SetDirty(new XYZ(tuple2._1.x, b2, tuple2._1.z));
				}
			}
			list2.Dispose();
		}
		if (p2i.Count != 0)
		{
			foreach (KeyValuePair<XYZ, int> item4 in p2i)
			{
				bool check = types[item4.Value] != 0;
				if (test.SetVoxelCheck(item4.Key, check))
				{
					flag = true;
				}
			}
			p2i.Clear();
			types.Clear();
			volumes.Clear();
		}
		if (removeGrass.Count != 0)
		{
			removeGrass.Clear();
		}
		if (dirtyTerrainResources)
		{
			flag = true;
			dirtyTerrainResources = false;
		}
		if (flag)
		{
			test.DrawDirtyMegaVoxels();
			if (test.forceSynchronousUpdate)
			{
				FinishWork();
			}
		}
	}

	public override void ClearNegativeLayer()
	{
		NegativeLayer.LowResNegative lowResNegative = (NegativeLayer.LowResNegative)negativeLayer.LowRes;
		HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
		foreach (XYZ item in lowResNegative.GetOccupiedOrUncertain())
		{
			for (int i = 0; i < 6; i++)
			{
				hashSet.Add(item + CubeModele.XYZDirs[i]);
			}
		}
		foreach (XYZ item2 in hashSet)
		{
			test.drawer.MarkAsDirty(item2);
		}
		hashSet.Dispose();
		negativeLayer.removedSet.Clear();
		test.DrawDirtyMegaVoxels();
		if (test.forceSynchronousUpdate)
		{
			FinishWork();
		}
	}

	public override void ClearTerrainSurfaceLayer()
	{
		TerrainSurfaceLayer.LowResTerrainSurface lowResTerrainSurface = (TerrainSurfaceLayer.LowResTerrainSurface)terrainSurfaceLayer.LowRes;
		HashSet<XYZ> hashSet = CollectionPool.RequestHashSet<XYZ>();
		foreach (XYZ item in lowResTerrainSurface.GetOccupiedOrUncertain())
		{
			for (int i = 0; i < 6; i++)
			{
				hashSet.Add(item + CubeModele.XYZDirs[i]);
			}
		}
		foreach (XYZ item2 in hashSet)
		{
			test.drawer.MarkAsDirty(item2);
		}
		hashSet.Dispose();
		terrainSurfaceLayer.addedSet.Clear();
		test.DrawDirtyMegaVoxels();
		if (test.forceSynchronousUpdate)
		{
			FinishWork();
		}
	}

	public int DirtyVoxel(VoxelHandle h)
	{
		return p2i.NewKey(h.p, (Func<XYZ, int>)delegate
		{
			types.Add(GetVoxelType(h));
			volumes.Add(GetVoxelVolume(h));
			return p2i.Count;
		});
	}

	public void SetVoxelData(VoxelHandle h, int t, int v, int d)
	{
		int index = DirtyVoxel(h);
		types[index] = t;
		volumes[index] = v;
	}

	public void SetVoxelType(VoxelHandle h, int t)
	{
		logger.Warn("LayerVoxelEngine.SetVoxelType is not fully supported.");
		types[DirtyVoxel(h)] = t;
	}

	public void SetVoxelVolume(VoxelHandle h, int v)
	{
		logger.Warn("LayerVoxelEngine.SetVoxelVolume is not fully supported.");
		volumes[DirtyVoxel(h)] = v;
	}

	public void RemoveVoxel(VoxelHandle h)
	{
		if (test.forceSynchronousUpdate)
		{
			if (test.SetVoxelCheck(h.p, check: false))
			{
				FinishWork();
			}
		}
		else
		{
			SetVoxelData(h, 0, 0, 0);
		}
	}

	public void RemoveGrass(VoxelHandle h)
	{
		removeGrass.Add(h.p);
	}

	public void CompactCrust(VoxelHandle h)
	{
		MondeAtlas atlas = test.GetAtlas();
		if (test.GetAtlas().IsCroute(GetVoxelType(h)))
		{
			VoxelTerrainResources terrainResources = GetTerrainResources(h);
			atlas.PackResources(terrainResources);
			test.SetCustomTerrainResources(h.p, terrainResources);
			dirtyTerrainResources = true;
		}
	}

	private void GetBasicTerrainResource(VoxelHandle h, out int type, out int amount)
	{
		int num;
		if (p2i.TryGetValue(h.p, out var value))
		{
			type = types[value];
			num = volumes[value];
		}
		else
		{
			Color32 voxelDataFromLayers = test.GetVoxelDataFromLayers(h.p);
			type = voxelDataFromLayers.r;
			num = voxelDataFromLayers.g;
		}
		amount = ((num > 0) ? Calcule.RemapMinMax(num, 1, 4, 1, 10) : 0);
	}

	public int GetBasicTerrainResourceAmount(VoxelHandle h)
	{
		int voxelVolume = GetVoxelVolume(h);
		return (voxelVolume > 0) ? Calcule.RemapMinMax(voxelVolume, 1, 4, 1, 10) : 0;
	}

	public void AddTerrainResources(VoxelHandle h, int t, int amount)
	{
		if (amount == 0)
		{
			return;
		}
		int num = amount;
		XYZ p = h.p;
		XYZ p2 = p;
		MondeAtlas atlas = test.GetAtlas();
		for (int i = 0; i < 5; i++)
		{
			VoxelTerrainResources terrainResources = GetTerrainResources(h);
			int num2 = 10 - terrainResources.TotalAmount;
			int num3 = Mathf.Min(num, num2);
			terrainResources.Add(t, num3);
			if (num >= num2)
			{
				atlas.PackResources(terrainResources);
			}
			num -= num3;
			test.SetCustomTerrainResources(p2, terrainResources);
			dirtyTerrainResources = true;
			if (num == 0)
			{
				break;
			}
			p2.y++;
			AssignVoxelHandle(h, p2);
		}
		p2 = p - XYZ.up;
		AssignVoxelHandle(h, p2);
		CompactCrust(h);
	}

	public void AddTerrainResources(VoxelHandle h, VoxelTerrainResources r)
	{
		foreach (KeyValuePair<int, int> item in r.All)
		{
			AddTerrainResources(h, item.Key, item.Value);
		}
	}

	public void GetTerrainResources(VoxelHandle h, ICollection<int> resultTypes, ICollection<int> resultAmounts)
	{
		VoxelTerrainResources customTerrainResources = test.GetCustomTerrainResources(h.p);
		if (customTerrainResources != null)
		{
			for (int i = 0; i < customTerrainResources.types.Length; i++)
			{
				resultTypes.Add(customTerrainResources.types[i]);
				resultAmounts.Add(customTerrainResources.amounts[i]);
			}
			return;
		}
		GetBasicTerrainResource(h, out var type, out var amount);
		if (type != 0 && amount > 0)
		{
			resultTypes.Add(type);
			resultAmounts.Add(amount);
		}
	}

	public XYZ GetVoxelPosition(VoxelHandle h)
	{
		return h.p;
	}

	public int GetVoxelType(VoxelHandle h)
	{
		if (p2i.TryGetValue(h.p, out var value))
		{
			return types[value];
		}
		return test.GetVoxelDataFromLayers(h.p).r;
	}

	public int GetVoxelVolume(VoxelHandle h)
	{
		if (p2i.TryGetValue(h.p, out var value))
		{
			return volumes[value];
		}
		return test.GetVoxelDataFromLayers(h.p).g;
	}

	public int EstimateVoxelLegacyLevels(VoxelHandle h)
	{
		Color32 voxelDataFromLayers = test.GetVoxelDataFromLayers(h.p);
		return (voxelDataFromLayers.g != 0) ? ((voxelDataFromLayers.g > 4) ? 13107 : 4369) : 0;
	}

	public int EstimateVoxelLevels(VoxelHandle h)
	{
		Color32 voxelDataFromLayers = test.GetVoxelDataFromLayers(h.p);
		return (voxelDataFromLayers.g != 0) ? ((voxelDataFromLayers.g > 4) ? (-1) : 2139062143) : 0;
	}

	public int EstimateVoxelConfig(VoxelHandle h)
	{
		if (IsVoxelEmpty(h.p))
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (IsVoxelEmpty(CubeModele.XYZDirs[i]))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public int EstimateVoxelDeepType(VoxelHandle h)
	{
		return test.GetVoxelDataFromLayers(h.p).r;
	}

	private void Error(string s)
	{
		if (handleError++ < 10)
		{
			Debug.LogError(s);
		}
		else
		{
			logger.Info(s);
		}
	}

	private T Error<T>(T t, string s)
	{
		Error(s);
		return t;
	}

	private void PrepareTerrainComputation(VoxelHandle h)
	{
		ApplyChanges();
		test.AssignVoxelHandle(ref h.h, h.p, updateIfNeeded: true);
	}

	public int ComputeVoxelLegacyLevels(VoxelHandle h)
	{
		PrepareTerrainComputation(h);
		return h.h.LegacyLevels;
	}

	public int ComputeVoxelLevels(VoxelHandle h)
	{
		PrepareTerrainComputation(h);
		return h.h.Levels;
	}

	public int ComputeVoxelConfig(VoxelHandle h)
	{
		PrepareTerrainComputation(h);
		return h.h.Config;
	}

	public int ComputeVoxelDeepType(VoxelHandle h)
	{
		PrepareTerrainComputation(h);
		return h.h.GetProcessedType(test.atlasComponent.atlas);
	}

	public override bool IsVoxelNonEmpty(XYZ p)
	{
		return test.GetVoxelNonEmptyFromLayers(p);
	}

	public override bool IsVoxelEmpty(XYZ p)
	{
		return !IsVoxelNonEmpty(p);
	}

	public bool IsVoxelEmpty(VoxelHandle h)
	{
		return IsVoxelEmpty(h.p);
	}

	public bool IsVoxelNonEmpty(VoxelHandle h)
	{
		return IsVoxelNonEmpty(h.p);
	}

	public bool IsVoxelVisible(VoxelHandle h)
	{
		return ComputeVoxelConfig(h) != 0;
	}

	public bool IsVoxelInternal(VoxelHandle h)
	{
		return !IsVoxelVisible(h) && !IsVoxelEmpty(h);
	}

	public bool IsVoxelSmooth(VoxelHandle h)
	{
		if (IsVoxelNonEmpty(h.p + XYZ.up))
		{
			return false;
		}
		return test.GetAtlas().IsCroute(GetVoxelType(h));
	}

	public override int EstimateVisibleVoxelsInMap(string folder)
	{
		Tex tex = new Tex();
		Tex b = new Tex();
		try
		{
			string path = Path.Combine(folder, "Heightmap.png");
			tex.LoadPNG(File.ReadAllBytes(path));
			Color32[] pixels = tex.GetPixels32();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < pixels.Length; i++)
			{
				Color32 color = pixels[i];
				if (color.g > color.r)
				{
					num++;
				}
				num2 += color.g - color.r;
			}
			if (num == 0)
			{
				return 0;
			}
			int num4 = 3 * num2 / num;
			float num5 = Mathf.Sqrt(num);
			return num * 2 + Mathf.RoundToInt((float)num4 * num5 * 1.7f);
		}
		finally
		{
			Tex.Destroy(tex, b);
		}
	}

	public override XYZ EstimateMapCenter(string folder)
	{
		Tex tex = new Tex();
		try
		{
			string path = Path.Combine(folder, "Heightmap.png");
			tex.LoadPNG(File.ReadAllBytes(path));
			return new XYZ(tex.width, 256, tex.height) / 2;
		}
		finally
		{
			Tex.Destroy(tex);
		}
	}

	public override void LoadMap(string folder)
	{
		test.LoadMap(folder);
	}

	public override void SaveMap(string folder)
	{
		test.SaveMap(folder);
	}

	public void ConvertOldFormat(string folder)
	{
		GridToLayersConverter.Convert(folder);
	}

	public override void RenderMap()
	{
		FinishWork();
	}

	public override void ClearMap()
	{
		test.Clear();
	}

	public override void FinishWork()
	{
		test.FinishWork();
	}

	public override bool IsWorking()
	{
		if (test.IsQuickDrawing())
		{
			return true;
		}
		ApplyChanges();
		return test.IsWorking;
	}

	public override void OnFinishedWorking(Action a)
	{
		test.StartCoroutine(_WaitForWorker(a));
	}

	private IEnumerator _WaitForWorker(Action a)
	{
		while (IsWorking())
		{
			yield return new WaitForEndOfFrame();
		}
		a();
	}

	public override bool IsEditingHeightmap()
	{
		return false;
	}

	public override bool IsGrassAutoRemoved()
	{
		return true;
	}

	public override Bounds GetGeneratedBottomBounds(float thicknessFactor = 0.5f, float randomness = 0.5f, int yMaxPlane = 255)
	{
		TestWorldConfig3.GenerateBottomData generateBottom = test.generateBottom;
		generateBottom.minInclination = Mathf.Lerp(0.1f, 1f, thicknessFactor);
		generateBottom.maxInclination = Mathf.Lerp(0.2f, 4f, thicknessFactor);
		generateBottom.topMax = yMaxPlane;
		generateBottom.randomness = randomness;
		generateBottom.ReadWorld(test);
		Bounds bounds = Layer.GetBoundsFromColumnTex(generateBottom.worldTex).ToVectorBounds();
		int num = 255 - Mathf.Max(yMaxPlane, Mathf.RoundToInt(bounds.max.y));
		generateBottom.Translate(new XYZ(0, num, 0));
		generateBottom.GenerateBottom();
		Bounds result = Layer.GetBoundsFromColumnTex(generateBottom.resultTex).ToVectorBounds();
		result.center -= Vector3.up * num;
		result.max = bounds.max;
		return result;
	}

	public override void GenerateDefaultBottom(XYZ translation = default(XYZ), float thicknessFactor = 0.5f, float randomness = 0.5f, int yMaxPlane = 255)
	{
		TestWorldConfig3.GenerateBottomData generateBottom = test.generateBottom;
		generateBottom.minInclination = Mathf.Lerp(0.1f, 1f, thicknessFactor);
		generateBottom.maxInclination = Mathf.Lerp(0.2f, 4f, thicknessFactor);
		generateBottom.topMax = yMaxPlane;
		generateBottom.randomness = randomness;
		generateBottom.ReadWorld(test);
		generateBottom.Translate(translation);
		generateBottom.GenerateBottom();
		generateBottom.DrawBottom(test);
	}

	public override void SetSoftAlgos(int softAlgos)
	{
		test.initialSoftAlgos = (test.softAlgos = softAlgos);
	}

	public override void SetLateSoftAlgos(int lateSoftAlgos)
	{
		test.lateSoftAlgos = lateSoftAlgos;
		test.useLateSoftAlgos = true;
	}

	public override void SetLodMax(int lodMax)
	{
		test.lod = lodMax;
	}

	public override int GetLodMax()
	{
		return test.lod;
	}

	public override void SetLodQuality(int lodQuality)
	{
		if (!(test.lodRangeDriver == null))
		{
			test.lodRangeDriver.qualityLevel = (LodRangeDriver.QualityLevel)lodQuality;
		}
	}

	public override Transform GetLodTerrainObject(int lod)
	{
		return test.lodSorter.GetLodRenderParent(lod);
	}

	public bool IsOutside(int x, int z)
	{
		return x < test.worldStart.x || x >= test.worldStart.x + test.worldSize.x || z < test.worldStart.z || z >= test.worldStart.z + test.worldSize.z;
	}

	public Color32 HeightData(int x, int z)
	{
		if (c2i.TryGetValue(x | (z << 16), out var value))
		{
			return value;
		}
		if (heightmapLayer != null)
		{
			return heightmapLayer.fillingTex.GetPixel(x, z);
		}
		if (fillRangeLayer != null)
		{
			Classeur<Color32> valueOrDefault = fillRangeLayer.Columns.GetValueOrDefault(new XZ(x, z));
			if (valueOrDefault != null && valueOrDefault.Count > 0)
			{
				return valueOrDefault[0];
			}
		}
		return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}

	public override int GetColumnBottom(int x, int z)
	{
		if (IsOutside(x, z))
		{
			return 0;
		}
		return HeightData(x, z).r;
	}

	public override int GetColumnTop(int x, int z)
	{
		if (IsOutside(x, z))
		{
			return 0;
		}
		return HeightData(x, z).g;
	}

	public override int GetColumnCrustType(int x, int z)
	{
		if (IsOutside(x, z))
		{
			return 0;
		}
		return HeightData(x, z).b;
	}

	public override int GetColumnCrustVolume(int x, int z)
	{
		if (IsOutside(x, z))
		{
			return 0;
		}
		return 256 - HeightData(x, z).a;
	}

	public override void SetColumnBottom(int x, int z, int bottom)
	{
		if (!IsOutside(x, z))
		{
			Color32 value = HeightData(x, z);
			value.r = (byte)bottom;
			int key = x | (z << 16);
			c2i[key] = value;
		}
	}

	public override void SetColumnTop(int x, int z, int top)
	{
		if (!IsOutside(x, z))
		{
			Color32 value = HeightData(x, z);
			value.g = (byte)top;
			int key = x | (z << 16);
			c2i[key] = value;
		}
	}

	public override void SetColumnCrustType(int x, int z, int type)
	{
		if (!IsOutside(x, z))
		{
			Color32 value = HeightData(x, z);
			int key = x | (z << 16);
			value.b = (byte)type;
			c2i[key] = value;
		}
	}

	public override void SetColumnCrustVolume(int x, int z, int volume)
	{
		if (!IsOutside(x, z))
		{
			Color32 value = HeightData(x, z);
			value.a = (byte)(256 - Mathf.Clamp(volume, 1, 4));
			int key = x | (z << 16);
			c2i[key] = value;
		}
	}

	public override void GetColumnData(int x, int z, out int bottom, out int top)
	{
		Color32 color = (IsOutside(x, z) ? default(Color32) : HeightData(x, z));
		bottom = color.r;
		top = color.g;
	}

	public override void GetColumnData(int x, int z, out int bottom, out int top, out int type, out int volume)
	{
		Color32 color = (IsOutside(x, z) ? default(Color32) : HeightData(x, z));
		bottom = color.r;
		top = color.g;
		type = color.b;
		volume = 256 - color.a;
	}

	public override void SetColumnData(int x, int z, int bottom, int top, int type, int volume)
	{
		if (!IsOutside(x, z))
		{
			Color32 value = new Color32((byte)bottom, (byte)top, (byte)type, (byte)(256 - Mathf.Clamp(volume, 1, 4)));
			int key = x | (z << 16);
			c2i[key] = value;
		}
	}

	public override void AddColumnData(int x, int z, int bottom, int top, int type, int volume)
	{
		if (!IsOutside(x, z) && bottom < top)
		{
			Color32 t = new Color32((byte)bottom, (byte)top, (byte)type, (byte)(256 - Mathf.Clamp(volume, 1, 4)));
			addedColumns.Add(Tuple.Create(new XZ(x, z), t));
		}
	}

	public override IEnumerable<Brix.Engine.VoxelHandle> GetEmptyVoxels(Bounds b)
	{
		throw new NotImplementedException();
	}

	private IEnumerable<Brix.Engine.VoxelHandle> GetEngineHandles(IEnumerable<XYZ> handles, Func<XYZ, bool> acceptHandle)
	{
		VoxelHandle handle = new VoxelHandle
		{
			VoxelEngine = this
		};
		foreach (XYZ p in handles)
		{
			if (acceptHandle(p))
			{
				AssignVoxelHandle(handle, p);
				yield return handle;
			}
		}
	}

	public override IEnumerable<Brix.Engine.VoxelHandle> GetVisibleVoxels()
	{
		return GetEngineHandles(GetVisibleVoxelPositions(), (XYZ p) => true);
	}

	public override IEnumerable<Brix.Engine.VoxelHandle> GetVisibleVoxels(Bounds b)
	{
		return GetEngineHandles(GetVisibleVoxelPositions(b), (XYZ p) => true);
	}

	public override IEnumerable<Brix.Engine.VoxelHandle> GetSurfaceVoxels()
	{
		return GetEngineHandles(GetVisibleVoxelPositions(), (XYZ p) => true);
	}

	public override IEnumerable<Brix.Engine.VoxelHandle> GetSurfaceVoxels(Bounds b)
	{
		return GetEngineHandles(GetVisibleVoxelPositions(b), (XYZ p) => true);
	}

	public override IEnumerable<XYZ> GetVisibleVoxelPositions()
	{
		return test.GetVisibleVoxels();
	}

	public override IEnumerable<XYZ> GetVisibleVoxelPositions(Bounds b)
	{
		return test.GetVisibleVoxels(XYZ.FromVector3(b.min), XYZ.FromVector3(b.size), 0);
	}

	public override IEnumerable<XYZ> GetVisibleVoxelPositions(Bounds b, int lod)
	{
		return test.GetVisibleVoxels(XYZ.FromVector3(b.min), XYZ.FromVector3(b.size), lod);
	}

	public override IEnumerable<XYZ> GetSurfaceVoxelPositions()
	{
		return test.GetSurfaceVoxels();
	}

	public override IEnumerable<XYZ> GetSurfaceVoxelPositions(Bounds b)
	{
		return test.GetSurfaceVoxels(XYZ.FromVector3(b.min), XYZ.FromVector3(b.size), 0);
	}

	public override IEnumerable<XYZ> GetSurfaceVoxelPositions(Bounds b, int lod)
	{
		return test.GetSurfaceVoxels(XYZ.FromVector3(b.min), XYZ.FromVector3(b.size), lod);
	}

	public override bool IsOccupiedVoxelRegion(XYZ region)
	{
		return test.IsOccupiedRegion(region);
	}

	public override bool IsOccupiedVoxelRegion(XYZ region, XYZ regionSize)
	{
		return test.IsOccupiedRegion(region, regionSize);
	}

	public override IEnumerable<XYZ> GetOccupiedVoxelRegions(XYZ regionSize)
	{
		return test.GetOccupiedRegions(regionSize);
	}

	public override IEnumerable<XYZ> GetOccupiedVoxelRegions(XYZ regionSize, Bounds b)
	{
		return test.GetOccupiedRegions(regionSize, XYZ.FromVector3(b.min), XYZ.FromVector3(b.size));
	}

	public override IEnumerable<Brix.Engine.VoxelHandle> GetAllVoxels(Bounds b)
	{
		VoxelHandle handle = new VoxelHandle
		{
			VoxelEngine = this
		};
		foreach (KeyValuePair<XYZ, FlatWorldHandle> kv in test.GetNonEmptyVoxels(XYZ.FromVector3(b.min), XYZ.FromVector3(b.size)))
		{
			handle.p = kv.Key;
			handle.h = kv.Value;
			yield return handle;
		}
	}

	public override IEnumerable<XYZ> GetVisibleVoxelPositionsFromLayers()
	{
		HashSet<XYZ> result = new HashSet<XYZ>();
		test.GetVisibleVoxelsFromLayers(result);
		return result;
	}

	public override IEnumerable<XYZ> GetTopVoxelPositionsFromLayers()
	{
		HashSet<XYZ> result = new HashSet<XYZ>();
		test.GetTopVoxelsFromLayers(result);
		return result;
	}

	public override IEnumerable<XYZ> GetNaturalBottomVoxelPositionsFromLayers()
	{
		HashSet<XYZ> result = new HashSet<XYZ>();
		heightmapLayer.GetPotentialBottomVoxels(result);
		return result;
	}

	public override Bounds ComputeOccupiedBounds()
	{
		VoxelBounds bounds = default(VoxelBounds);
		foreach (Layer layer in test.layers.layers)
		{
			layer.AdjustBounds(ref bounds);
		}
		return bounds.ToVectorBounds();
	}

	public override XYZ GetClosestNonEmptyVoxel(XYZ p)
	{
		if (IsVoxelNonEmpty(p))
		{
			return p;
		}
		return test.GetClosestVisibleVoxelLowRes(p);
	}

	public override XYZ GetClosestNonEmptyVoxelLowRes(XYZ p, int maxDistance)
	{
		if (IsVoxelNonEmpty(p))
		{
			return p;
		}
		return test.GetClosestVisibleVoxelLowRes(p);
	}

	public override IClosestNonEmptyVoxelEnumerator GetClosestNonEmptyVoxelEnumerator(XYZ p, int closeDistance, int maxDistance)
	{
		return new GetClosestVisibleVoxelEnumerator(test, p, closeDistance, maxDistance);
	}

	public override Vector3 TerrainRaycast(Vector3 a, Vector3 b)
	{
		return test.TerrainRaycast(a, b);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
