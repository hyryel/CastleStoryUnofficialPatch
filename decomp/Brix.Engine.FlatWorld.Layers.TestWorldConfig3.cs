using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Common.Collections;
using Brix.Engine.Abstract;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Engine.Geometrie;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

internal class TestWorldConfig3 : TerrainLodProvider, IVoxelEngineProvider
{
	[Serializable]
	public class GenerateBottomData
	{
		public float minInclination = 0.5f;

		public float maxInclination = 2f;

		public float maxDistance = 20f;

		public float randomness = 0f;

		public int minThickness = 3;

		public int topMax;

		public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public Tex worldTex;

		public Tex resultTex;

		public bool generate;

		public bool draw;

		public float GetInclination(XYZ p, XYZ top)
		{
			float a = UnityEngine.Random.Range(0f, randomness);
			float b = 1f - UnityEngine.Random.Range(0f, randomness);
			float num = Mathf.Lerp(minInclination, maxInclination, Mathf.Min(a, b));
			float num2 = Mathf.Lerp(minInclination, maxInclination, Mathf.Max(a, b));
			return Soft.InclinationToBottom(p, top, num, num2, maxDistance, curve);
		}

		public void ReadWorld(TestWorldConfig3 test)
		{
			Tex.Destroy(worldTex);
			if (test.layers.GetLayer<HeightmapLayer>() != null)
			{
				worldTex = new Tex();
				Soft.Copy(worldTex, test.layers.GetLayer<HeightmapLayer>().fillingTex);
				return;
			}
			if (test.layers.GetLayer<FillRangeLayer>() != null)
			{
				worldTex = new Tex();
				test.layers.GetLayer<FillRangeLayer>().CopyToHeightmap(worldTex, test.worldSize.x, test.worldSize.z);
				return;
			}
			HeightmapLayer heightmapLayer = new HeightmapLayer();
			heightmapLayer.Load(test.mapFolder);
			Soft.Copy(worldTex, heightmapLayer.fillingTex);
			heightmapLayer.Destroy();
		}

		public void Translate(XYZ translation)
		{
			Tex tex = new Tex();
			tex.Swap(worldTex);
			Soft.TranslateWorld(worldTex, tex, translation.x, translation.z, -translation.y, translation.y);
			Tex.Destroy(tex);
			topMax += translation.y;
		}

		public void GenerateBottom()
		{
			Tex.Destroy(resultTex);
			resultTex = new Tex();
			Soft.GetDefaultBottomTex(resultTex, worldTex, GetInclination, topMax, minThickness);
		}

		public void DrawBottom(TestWorldConfig3 test)
		{
			HeightmapLayer layer = test.layers.GetLayer<HeightmapLayer>();
			FillRangeLayer layer2 = test.layers.GetLayer<FillRangeLayer>();
			Layer layer3 = (Layer)(((object)layer) ?? ((object)layer2));
			XYZ[] second = layer3.LowRes.GetVisibleOrUncertain().ToArray();
			if (layer != null)
			{
				layer.SetAllColumns(resultTex);
			}
			else
			{
				layer2.AddAllColumns(resultTex);
			}
			XYZ[] first = layer3.LowRes.GetVisibleOrUncertain().ToArray();
			HashSet<XYZ> hashSet = new HashSet<XYZ>(first.Concat(second));
			foreach (XYZ item in hashSet)
			{
				test.drawer.MarkAsDirty(item);
			}
			Neo.DoAll(test.QuickDrawWorld_Coroutine());
		}
	}

	private static BrixLogger logger = new BrixLogger();

	public Common common;

	public Transform renderTarget;

	public LodSorter lodSorter;

	public LodUpdater lodUpdater = null;

	public Texture2D noiseTex = null;

	public Texture2D permTex = null;

	public Texture2D warpTex = null;

	public MondeAtlasComponent atlasComponent;

	public BouleDeLod3 bouleDeLod3 = null;

	public LodRangeDriver lodRangeDriver = null;

	public LayersPopulator layersPopulator = null;

	public Layers layers;

	public LayersDrawer drawer;

	public bool initLayers;

	public bool clear;

	public bool clearCommon;

	public bool render;

	public bool quickDraw;

	public bool continuousLayers = false;

	public bool continuousRender = false;

	public int softAlgos;

	public int initialSoftAlgos;

	public int lateSoftAlgos;

	public XYZ worldStart;

	public XYZ worldSize;

	public XYZ mvSize = XYZ.one * 16;

	public XYZ border = XYZ.one * 2;

	public int lod;

	public bool verbose = false;

	public bool useColliders = true;

	public bool useTangents = false;

	public bool useLateSoftAlgos;

	public bool forceSynchronousUpdate;

	public bool forceSynchronousDrawing;

	public float applicationLostFocus;

	public float applicationRegainedFocus;

	public float applicationUpdateBegun;

	public float applicationUpdateEnded;

	public bool redrawIfFocusLost = true;

	public bool disableBeforeReady = false;

	public float quickDrawRate = 0.04f;

	public GenerateBottomData generateBottom = null;

	public bool useDeepTypeNoise = true;

	public bool drawFullInitialWorld = true;

	public bool drawInitialLods = false;

	public int maxThreadCount = 20;

	public int maxThreadSync = 8;

	public bool threadWaitForCollider = true;

	public string mapFolder;

	private Queue<IEnumerator> workLoad = new Queue<IEnumerator>();

	public PhysicMaterial physicsMaterial = null;

	public bool testTrees = false;

	public GameObject treePrefab;

	public Vector2 shaderOffset = new Vector2(0.8f, 0.8f);

	private bool _quitting;

	private bool _isInitLayers;

	private NegativeLayer _negativeLayer;

	private TerrainSurfaceLayer _terrainSurfaceLayer;

	private FlatWorldSoftRenderer _softRenderer = new FlatWorldSoftRenderer();

	private LevelConfigNormalUvs _levelConfigNormalUvs = new LevelConfigNormalUvs();

	private IEnumerator _quickDrawEnumerator;

	private readonly MeshPool _meshPool = new MeshPool();

	private readonly CubeDrawer _cubeDrawer = new CubeDrawer();

	private readonly MeshAugmentableSimple _colliderMeshInfo = new MeshAugmentableSimple();

	private readonly MeshAugmentable _meshInfo = new MeshAugmentableDebordant(0.03f);

	[SerializeField]
	private ThreadedWorkList _threadedWorkList = new ThreadedWorkList();

	private List<ThreadedHeavyWorker> _threadedWorkers = new List<ThreadedHeavyWorker>();

	private XYZ[] _searchList;

	public float redrawScheduled;

	public float redrawExpired;

	public XYZ Border => border;

	public XYZ MvSize => mvSize;

	public bool IsWorking => workLoad.Count > 0;

	public bool Threaded => maxThreadCount > 0 && !forceSynchronousDrawing;

	public Common Common
	{
		get
		{
			if (common == null)
			{
				common = new Common(noiseTex, permTex, warpTex, GetAtlas(), useDeepTypeNoise);
			}
			return common;
		}
		set
		{
			if (common != null && common != value)
			{
				common.Destroy();
			}
			common = value;
		}
	}

	public LodSorter LodSorter
	{
		get
		{
			if (lodSorter == null)
			{
				lodSorter = new LodSorter();
			}
			return lodSorter;
		}
		set
		{
			if (lodSorter != null && lodSorter != value)
			{
				lodSorter.Clear();
			}
			lodSorter = value;
		}
	}

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
	}

	public void Init()
	{
		if (atlasComponent == null)
		{
			atlasComponent = UnityEngine.Object.FindObjectOfType<MondeAtlasComponent>();
		}
		if (renderTarget == null)
		{
			renderTarget = base.transform;
		}
		if (lodSorter == null)
		{
			lodSorter = new LodSorter();
		}
		if (drawer == null)
		{
			drawer = new LayersDrawer();
		}
		if (layers == null)
		{
			layers = new Layers();
		}
		_isInitLayers = false;
		softAlgos = initialSoftAlgos;
	}

	private void Update()
	{
		applicationUpdateBegun = Time.realtimeSinceStartup;
		if (initLayers)
		{
			initLayers = false;
			InitLayers(force: true, clear: true);
		}
		if (clear)
		{
			clear = false;
			Clear();
		}
		if (clearCommon)
		{
			clearCommon = false;
			Common = null;
		}
		if (render)
		{
			render = false;
			FinishWork();
		}
		if (quickDraw)
		{
			quickDraw = false;
			QuickDrawWorld();
		}
		UpdateQuickDraw();
		if (IsWorking)
		{
			Work();
		}
		if (continuousLayers)
		{
			ContinuousLayers();
		}
		if (continuousRender)
		{
			ContinuousRender();
		}
		if (redrawIfFocusLost)
		{
			RedrawIfFocusLost();
		}
		if (generateBottom.generate)
		{
			generateBottom.generate = false;
			GenerateBottom();
		}
		if (generateBottom.draw)
		{
			generateBottom.draw = false;
			DrawBottom();
		}
		applicationUpdateEnded = Time.realtimeSinceStartup;
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!focusStatus)
		{
			applicationLostFocus = realtimeSinceStartup;
		}
		else
		{
			applicationRegainedFocus = realtimeSinceStartup;
		}
		logger.Info((focusStatus ? "Gained" : "Lost") + " focus at " + realtimeSinceStartup);
	}

	private void OnApplicationQuit()
	{
		_quitting = true;
		UnInit();
	}

	private void OnDestroy()
	{
		UnInit();
	}

	private void UnInit()
	{
		StopAllCoroutines();
		if (!_quitting || Application.isEditor)
		{
			Clear();
			Common = null;
		}
	}

	public void LoadMap(string folder)
	{
		mapFolder = folder;
		QuickDrawWorld();
	}

	public void SaveMap(string folder)
	{
		InitLayers();
		foreach (Layer layer in layers.layers)
		{
			layer.Save(folder);
		}
	}

	public void InitLayers(bool force = false, bool clear = false)
	{
		if (!force && _isInitLayers)
		{
			return;
		}
		layers.Clear();
		layersPopulator.Populate(layers);
		foreach (Layer layer in layers.layers)
		{
			layer.Init(atlasComponent.atlas, Common);
			layer.Load(mapFolder);
		}
		drawer.InitLayers(layers, MvSize);
		_isInitLayers = true;
	}

	private NegativeLayer GetNegativeLayer()
	{
		return _negativeLayer ?? (_negativeLayer = layers.GetLayer<NegativeLayer>());
	}

	private TerrainSurfaceLayer GetTerrainSurfaceLayer()
	{
		return _terrainSurfaceLayer ?? (_terrainSurfaceLayer = layers.GetLayer<TerrainSurfaceLayer>());
	}

	public void Clear()
	{
		StopQuickDraw(finishDrawing: false);
		StopWork();
		if (testTrees)
		{
			ClearTrees();
		}
		if (lodUpdater != null)
		{
			lodUpdater.ClearAll();
		}
		if (lodSorter != null)
		{
			lodSorter.Clear();
		}
		if (bouleDeLod3 != null)
		{
			bouleDeLod3.enabled = false;
		}
		drawer.Clear();
		layers.Clear();
		_negativeLayer = null;
		_terrainSurfaceLayer = null;
		_isInitLayers = false;
	}

	public XYZ GetPaddedStart(XYZ start, XYZ border)
	{
		return start - border;
	}

	public static bool WorldNeedsInitialData(IFlatWorldColors world)
	{
		return world.Type.Empty;
	}

	public static bool WorldNeedsRenderData(IFlatWorldColors world)
	{
		return world.Level.Empty;
	}

	public void InitFromLayers(IFlatWorldColors world, XYZ start, XYZ size, XYZ border, bool force = false)
	{
		if (force || WorldNeedsInitialData(world))
		{
			InitLayers();
			XYZ xYZ = size + border * 2;
			xYZ.x = 1 << Calcule.GetBestPower(xYZ.x);
			xYZ.z = 1 << Calcule.GetBestPower(xYZ.z);
			XYZ paddedStart = GetPaddedStart(start, border);
			if (world is FlatWorld)
			{
				(world as FlatWorld).InitFromLayers(layers, Common, paddedStart, xYZ.x, xYZ.z);
				return;
			}
			_softRenderer.Init(world, layers, common, start, paddedStart, Border, MvSize, xYZ.x, xYZ.z, 0, useColliders, useTangents);
			_softRenderer.InitFromLayers();
		}
	}

	public void InitRender(IFlatWorldColors world, XYZ start, bool force = false)
	{
		if (force || WorldNeedsRenderData(world))
		{
			if (world is FlatWorld)
			{
				(world as FlatWorld).InitPreRender(Common, start, 0, softAlgos, force: true);
				(world as FlatWorld).InitRender(Common, start, 0, softAlgos, force: true);
			}
			else
			{
				world.InitPreRender(Common, start, 0, _levelConfigNormalUvs);
				world.InitRender(Common, start, _levelConfigNormalUvs);
			}
		}
	}

	private void GetWorldSection(IFlatWorldColors world, XYZ start, XYZ size, XYZ border, bool initRender = false, bool force = false)
	{
		InitFromLayers(world, start, size, border, force);
		if (initRender)
		{
			InitRender(world, start - border, force);
		}
	}

	public void InitLodProfile(int lod)
	{
		if (!lodSorter.HasProfile(lod))
		{
			lodSorter.CreateProfile(renderTarget, lod, MvSize, shaderOffset.x + (float)lod * shaderOffset.y);
		}
	}

	public void InitLod(FlatWorld world, FlatWorld sup)
	{
		world.InitLod(sup, Common, softAlgos, XYZ.zero);
	}

	public void StartQuickDraw()
	{
		_quickDrawEnumerator = Neo.EnumAll(QuickDrawWorld_Coroutine());
	}

	public void UpdateQuickDraw()
	{
		if (_quickDrawEnumerator != null && !_quickDrawEnumerator.MoveNext())
		{
			_quickDrawEnumerator = null;
		}
	}

	public void StopQuickDraw(bool finishDrawing)
	{
		if (_quickDrawEnumerator != null)
		{
			if (finishDrawing)
			{
				Neo.DoAll(_quickDrawEnumerator);
			}
			_quickDrawEnumerator = null;
		}
	}

	public bool IsQuickDrawing()
	{
		return _quickDrawEnumerator != null;
	}

	public void QuickDrawWorld()
	{
		if (quickDrawRate > 0f)
		{
			StartQuickDraw();
			return;
		}
		Neo.DoAll(QuickDrawWorld_Coroutine());
		FinishWork();
	}

	public IEnumerator QuickDrawWorld_Coroutine()
	{
		List<XYZ> dirtyMVs = CollectionPool.RequestList<XYZ>();
		GetDirtyMegaVoxels(worldStart, worldSize, dirtyMVs);
		InitLodProfile(0);
		if (drawFullInitialWorld)
		{
			yield return QuickDrawAsFull_Coroutine(dirtyMVs);
		}
		else
		{
			yield return QuickDrawIndividually_Coroutine(dirtyMVs);
		}
		if (bouleDeLod3 != null)
		{
			bouleDeLod3.Init(forced: true);
			bouleDeLod3.enabled = true;
			yield return null;
		}
		if (useLateSoftAlgos)
		{
			softAlgos = lateSoftAlgos;
		}
		if (testTrees)
		{
			LoadTrees();
		}
		dirtyMVs.Dispose();
	}

	private IEnumerator QuickDrawAsFull_Coroutine(List<XYZ> dirtyMVs)
	{
		XYZ border = Border;
		GetWorldSection(lodSorter.GetLodInitialWorld(0), worldStart + border, worldSize - border * 2, border, drawFullInitialWorld, force: true);
		yield return null;
		for (int lod = 1; lod <= this.lod; lod++)
		{
			InitLodProfile(lod);
			FlatWorld world = lodSorter.GetLodInitialWorld(lod);
			XYZ lodWorldStart = XYZ.zero;
			InitLod(world, lodSorter.GetLodInitialWorld(lod - 1));
			yield return null;
			InitRender(world, lodWorldStart, force: true);
			yield return null;
		}
		yield return DrawMegaVoxels_Coroutine(lodSorter.GetLodInitialWorld(0), dirtyMVs);
		MarkMegaVoxelsAsDrawn(dirtyMVs);
		yield return null;
		for (int i = 1; i <= this.lod; i++)
		{
			yield return DrawMegaVoxelsLod_Coroutine(lodSorter.GetLodInitialWorld(i), dirtyMVs, i, visible: false);
		}
	}

	private IEnumerator QuickDrawIndividually_Coroutine(List<XYZ> dirtyMVs)
	{
		yield return Draw_Coroutine(dirtyMVs);
		yield return QuickDrawLods_Coroutine(dirtyMVs);
	}

	public IEnumerator QuickDrawLods_Coroutine(List<XYZ> finerMvs)
	{
		if (lodUpdater == null)
		{
			LodUpdater.LodBatchDrawer batchDrawer = ObjectPool<LodUpdater.LodBatchDrawer>.Reserve().Init(this, Threaded);
			yield return batchDrawer.QuickDrawLods_Coroutine(finerMvs);
			ObjectPool<LodUpdater.LodBatchDrawer>.Release(batchDrawer);
		}
		else if (drawInitialLods)
		{
			yield return lodUpdater.StartWorking();
		}
	}

	public void LoadTrees()
	{
		string path = Path.Combine(mapFolder, "trees.json");
		string value = File.ReadAllText(path);
		XYZ[] array = JsonConvert.DeserializeObject<XYZ[]>(value);
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		XYZ[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			XYZ p = array2[i];
			SetVoxelCheck(p, check: false);
			if (treePrefab != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(treePrefab);
				Transform transform = base.transform.Find("Trees");
				if (transform == null)
				{
					transform = new GameObject("Trees").transform;
					transform.SetParent(base.transform);
					transform.localPosition = Vector3.zero;
				}
				gameObject.transform.SetParent(transform);
				gameObject.transform.position = p.ToVector3();
				gameObject.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 10));
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				component.SetPropertyBlock(propertyBlock);
			}
		}
		DrawDirtyMegaVoxels();
		if (forceSynchronousUpdate)
		{
			FinishWork();
		}
	}

	public void ClearTrees()
	{
		Transform transform = base.transform.Find("Trees");
		if (transform != null)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	private void GetDirtyMegaVoxels(XYZ start, XYZ size, ICollection<XYZ> results)
	{
		InitLayers();
		XYZ xYZ = XYZ.Div(start << lod, MvSize);
		XYZ size2 = XYZ.Div((start + size << lod) + MvSize - XYZ.one, MvSize) - xYZ;
		drawer.GetDirty(xYZ, size2, results);
	}

	private void MarkMegaVoxelsAsDrawn(IEnumerable<XYZ> dirtyMVs)
	{
		dirtyMVs.ApplyNow(drawer.MarkAsDrawn);
	}

	private IEnumerator DrawMegaVoxels_Coroutine(FlatWorld world, IEnumerable<XYZ> dirtyMVs)
	{
		foreach (XYZ q in dirtyMVs)
		{
			XYZ p = XYZ.Mul(q, MvSize);
			RenderMegaVoxel(world, p, p);
			yield return null;
		}
	}

	private IEnumerator DrawMegaVoxelsLod_Coroutine(FlatWorld world, IEnumerable<XYZ> dirtyMVs, int lod, bool visible = true)
	{
		if (!visible)
		{
			InitLodProfile(lod);
			lodSorter.GetLodRenderParent(lod).gameObject.SetActive(value: false);
		}
		foreach (XYZ q in dirtyMVs.Slinq().Select((XYZ xYZ, int _lod) => xYZ >> _lod, lod).Distinct()
			.NoLeak())
		{
			XYZ p = XYZ.Mul(q, MvSize);
			RenderMegaVoxelLod(world, p, p, lod);
			yield return null;
		}
	}

	public static void CreateLocalizedMesh(IFlatWorldColors section, CubeDrawer drawer, XYZ sampleStart, XYZ size, MeshAugmentable meshInfo)
	{
		int xzStart = sampleStart.x + sampleStart.z * section.Xz.Width;
		int width = section.Xz.Width;
		XYZ zero = XYZ.zero;
		zero.y = sampleStart.y;
		Display.CreateLocalizedMesh(meshInfo, drawer, xzStart, width, zero, size, section.Xz.GetPixels32(), section.Column.GetPixels32(), section.Offset.GetPixels32(), section.Config.GetPixels32(), section.Config.GetPixels32(), section.Level.GetPixels32(), section.Normal.GetPixels32(), section.UvBase.GetPixels32(), section.UvTrans.GetPixels32(), section.UvSpecial.GetPixels32());
	}

	public static void CountLocalizedMesh(IFlatWorldColors section, XYZ sampleStart, XYZ size, ref int vertexCount, ref int triangleCount)
	{
		int xzStart = sampleStart.x + sampleStart.z * section.Xz.Width;
		int width = section.Xz.Width;
		XYZ zero = XYZ.zero;
		zero.y = sampleStart.y;
		Display.CountLocalizedMesh(ref vertexCount, ref triangleCount, xzStart, width, zero, size, section.Xz.GetPixels32(), section.Column.GetPixels32(), section.Config.GetPixels32());
	}

	public static void CreateLocalizedCollider(IFlatWorldColors section, CubeDrawer drawer, XYZ sampleStart, XYZ size, MeshAugmentable meshInfo)
	{
		int xzStart = sampleStart.x + sampleStart.z * section.Xz.Width;
		int width = section.Xz.Width;
		XYZ zero = XYZ.zero;
		zero.y = sampleStart.y;
		Display.CreateLocalizedCollider(meshInfo, drawer, xzStart, width, zero, size, section.Xz.GetPixels32(), section.Column.GetPixels32(), section.Offset.GetPixels32(), section.Config.GetPixels32(), section.Config.GetPixels32(), section.Level.GetPixels32(), section.Normal.GetPixels32(), section.UvBase.GetPixels32(), section.UvTrans.GetPixels32(), section.UvSpecial.GetPixels32());
	}

	private void RenderMegaVoxel(IFlatWorldColors section, XYZ resultPosition, XYZ sampleStart)
	{
		int vertexCount = 0;
		int triangleCount = 0;
		CountLocalizedMesh(section, sampleStart, MvSize, ref vertexCount, ref triangleCount);
		MeshAugmentablePrealloue pooledMesh = _meshPool.GetPooledMesh(vertexCount, triangleCount, useTangents);
		pooledMesh.Clear(realloc: false);
		_cubeDrawer.SetUvDecals(Common.atlas.uvInnerDecal, Common.atlas.uvOuterDecal, Common.atlas.uvAtlasWidth / 16, Common.atlas.uvAtlasWidth / 4);
		CreateLocalizedMesh(section, _cubeDrawer, sampleStart, MvSize, pooledMesh);
		if (useColliders)
		{
			_colliderMeshInfo.Clear();
			CreateLocalizedCollider(section, _cubeDrawer, sampleStart, MvSize, _colliderMeshInfo);
		}
		RenderMegaVoxel(pooledMesh, _colliderMeshInfo, resultPosition);
	}

	private void RenderMegaVoxel(MeshModifiable meshInfo, MeshModifiable colliderMeshInfo, XYZ resultPosition)
	{
		MegaVoxelGeo mv = RenderMegaVoxel(meshInfo, resultPosition);
		if (useColliders)
		{
			RefreshCollider(mv, colliderMeshInfo);
		}
	}

	public MegaVoxelGeo RenderMegaVoxel(MeshModifiable meshInfo, XYZ resultPosition)
	{
		InitLodProfile(0);
		MegaVoxelGeo mV = Display.GetMV(lodSorter.GetLodRenderParent(0), XYZ.zero, lodSorter, resultPosition, MvSize, 0, searchParent: false);
		if (!mV.started)
		{
			mV._Start();
		}
		if (meshInfo.GetVertexCount() >= 65000 || meshInfo.GetTriangleCount() >= 64998)
		{
			int start = MeshCombineMaison.SplitToMeshes(meshInfo, MeshCombineMaison.PopulateMeshes(mV.gameObject, 100), immediate: true).Slinq().Count();
			MeshCombineMaison.ClearMeshes(mV.gameObject, start);
			mV.mesh = mV.filter.sharedMesh;
		}
		else
		{
			mV.filter.sharedMesh = (mV.mesh = meshInfo.Rendre(mV.mesh));
		}
		return mV;
	}

	public void RefreshCollider(MegaVoxelGeo mv, MeshModifiable colliderMeshInfo)
	{
		mv.hasCollider = useColliders;
		if (mv.colliderMesh == null)
		{
			mv.colliderMesh = new Mesh();
		}
		else
		{
			mv.colliderMesh.Clear();
		}
		mv.colliderMesh.vertices = colliderMeshInfo.GetVertices();
		mv.colliderMesh.triangles = colliderMeshInfo.GetTriangles();
		mv.RefreshCollider(physicsMaterial);
	}

	public void RenderMegaVoxelLod(IFlatWorldColors section, XYZ resultPosition, XYZ sampleStart, int lod)
	{
		int vertexCount = 0;
		int triangleCount = 0;
		CountLocalizedMesh(section, sampleStart, MvSize, ref vertexCount, ref triangleCount);
		MeshAugmentablePrealloue pooledMesh = _meshPool.GetPooledMesh(vertexCount, triangleCount, useTangents);
		pooledMesh.Clear(realloc: false);
		_cubeDrawer.SetUvDecals(Common.atlas.uvInnerDecal, Common.atlas.uvOuterDecal, Common.atlas.uvAtlasWidth / 16, Common.atlas.uvAtlasWidth / 4);
		CreateLocalizedMesh(section, _cubeDrawer, sampleStart, MvSize, pooledMesh);
		Matrix4x4 matrix = Matrix4x4.TRS((Vector3.one * (1 << lod) - Vector3.one) / 2f, Quaternion.identity, Vector3.one * (1 << lod));
		pooledMesh.Transform(matrix);
		RenderMegaVoxelLod(pooledMesh, resultPosition, lod);
	}

	public void RenderMegaVoxelLod(MeshModifiable meshInfo, XYZ resultPosition, int lod)
	{
		InitLodProfile(lod);
		MegaVoxelGeo mV = Display.GetMV(lodSorter.GetLodRenderParent(lod), XYZ.zero, lodSorter, resultPosition << lod, MvSize << lod, lod, searchParent: false);
		if (!mV.started)
		{
			mV._Start();
		}
		mV.filter.sharedMesh = (mV.mesh = meshInfo.Rendre(mV.mesh));
	}

	public void InitSoftRenderer(FlatWorldSoftRenderer renderer, IFlatWorldColors world, XYZ q, int width, int height, int lod = 0)
	{
		XYZ start = XYZ.Mul(q, mvSize);
		XYZ paddedStart = GetPaddedStart(start, border);
		renderer.TempWorld = world;
		world = new FlatWorldColors();
		renderer.Init(world, layers, Common, start, paddedStart, Border, MvSize, width, height, lod, useColliders, useTangents);
	}

	private void InitFullFromLayers_Init(List<XYZ> qs, int lod)
	{
		for (int i = 0; i < qs.Count; i++)
		{
			_threadedWorkList.SetDirty(qs[i], lod);
		}
		if (_threadedWorkers.Count != maxThreadCount)
		{
			for (int j = 0; j < _threadedWorkers.Count; j++)
			{
				_threadedWorkers[j].Destroy();
			}
			_threadedWorkers = (from num in Enumerable.Range(0, maxThreadCount)
				select new ThreadedHeavyWorker(this, _threadedWorkList)).ToList();
		}
	}

	private IEnumerator InitFullFromLayers_Loop()
	{
		while (_threadedWorkList.HasWork)
		{
			int nWork = 0;
			for (int i = 0; i < _threadedWorkers.Count; i++)
			{
				if (_threadedWorkers[i].MainThreadWork())
				{
					nWork++;
					if (nWork >= maxThreadSync)
					{
						nWork = 0;
						yield return null;
					}
				}
			}
			yield return null;
		}
	}

	private IEnumerator Draw_Coroutine(List<XYZ> qs)
	{
		InitLayers();
		for (int i = 0; i < qs.Count; i++)
		{
			drawer.MarkAsDrawn(qs[i]);
		}
		if (Threaded)
		{
			InitFullFromLayers_Init(qs, 0);
			yield return InitFullFromLayers_Loop();
			yield break;
		}
		XYZ totalSize = MvSize + Border * 2;
		totalSize.x = 1 << Calcule.GetBestPower(totalSize.x);
		totalSize.z = 1 << Calcule.GetBestPower(totalSize.z);
		for (int j = 0; j < qs.Count; j++)
		{
			_Draw(qs[j], totalSize);
			yield return null;
		}
		if (lodUpdater != null)
		{
			for (int k = 0; k < qs.Count; k++)
			{
				lodUpdater.SetDirty(qs[k]);
			}
		}
	}

	private void Draw(List<XYZ> qs)
	{
		InitLayers();
		for (int i = 0; i < qs.Count; i++)
		{
			drawer.MarkAsDrawn(qs[i]);
		}
		if (Threaded)
		{
			InitFullFromLayers_Init(qs, 0);
			workLoad.Enqueue(InitFullFromLayers_Loop());
			return;
		}
		XYZ totalSize = MvSize + Border * 2;
		totalSize.x = 1 << Calcule.GetBestPower(totalSize.x);
		totalSize.z = 1 << Calcule.GetBestPower(totalSize.z);
		for (int j = 0; j < qs.Count; j++)
		{
			_Draw(qs[j], totalSize);
		}
		if (lodUpdater != null)
		{
			for (int k = 0; k < qs.Count; k++)
			{
				lodUpdater.SetDirty(qs[k]);
			}
		}
	}

	private void Draw(XYZ q)
	{
		InitLayers();
		drawer.MarkAsDrawn(q);
		XYZ totalSize = MvSize + Border * 2;
		totalSize.x = 1 << Calcule.GetBestPower(totalSize.x);
		totalSize.z = 1 << Calcule.GetBestPower(totalSize.z);
		_Draw(q, totalSize);
		if (lodUpdater != null)
		{
			lodUpdater.SetDirty(q);
		}
	}

	private void _Draw(XYZ q, XYZ totalSize)
	{
		IFlatWorldColors cachedWorld = drawer.GetCachedWorld(q);
		if (cachedWorld is FlatWorld flatWorld)
		{
			XYZ xYZ = XYZ.Mul(q, MvSize);
			XYZ paddedStart = GetPaddedStart(xYZ, Border);
			flatWorld.InitFromLayers(layers, common, paddedStart, totalSize.x, totalSize.z);
			flatWorld.InitPreRender(common, paddedStart, 0, softAlgos, force: true);
			flatWorld.InitRender(common, paddedStart, 0, softAlgos, force: true);
			RenderMegaVoxel(cachedWorld, xYZ, Border);
		}
		else
		{
			InitSoftRenderer(_softRenderer, cachedWorld, q, totalSize.x, totalSize.z);
			_softRenderer.InitFromLayers();
			_softRenderer.InitPreRender();
			_softRenderer.InitRender();
			_softRenderer.AllocateMesh();
			_softRenderer.PopulateMesh();
			RenderMegaVoxel(_softRenderer.MeshInfo, _softRenderer.ColliderMeshInfo, _softRenderer.Start);
			_softRenderer.World.SwapPostRender(cachedWorld);
		}
	}

	public override void DrawDirtyMegaVoxels()
	{
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		drawer.GetDirty(list);
		Draw(list);
		list.Dispose();
	}

	public override void FinishWork()
	{
		StopQuickDraw(finishDrawing: true);
		while (workLoad.Count > 0)
		{
			Work();
		}
	}

	public void Work()
	{
		if (!workLoad.Peek().MoveNext())
		{
			workLoad.Dequeue();
		}
	}

	private void StopWork()
	{
		workLoad.Clear();
		_threadedWorkList.Clear();
		for (int i = 0; i < _threadedWorkers.Count; i++)
		{
			_threadedWorkers[i].Destroy();
		}
		_threadedWorkers.Clear();
	}

	private void ContinuousLayers()
	{
		Layers b = new Layers();
		layersPopulator.Populate(b);
		if (!Layers.Equals(layers, b))
		{
			InitLayers();
		}
	}

	private void ContinuousRender()
	{
		if (Camera.main == null)
		{
			return;
		}
		XYZ a = XYZ.FromVector3(Camera.main.transform.position);
		InitLayers();
		XYZ xYZ = XYZ.Div(a, MvSize);
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		if (_searchList == null)
		{
			_searchList = Calcule.CubeConcentrique(XYZ.zero, 5).ToArray();
		}
		for (int i = 0; i < _searchList.Length; i++)
		{
			XYZ xYZ2 = xYZ + _searchList[i];
			if (drawer.CheckDirty(xYZ2))
			{
				list.Add(xYZ2);
			}
		}
		if (list.Count > 0)
		{
			Draw(list);
		}
		list.Dispose();
	}

	private void RedrawIfFocusLost()
	{
		if (applicationLostFocus > applicationRegainedFocus && redrawScheduled < applicationLostFocus)
		{
			redrawScheduled = Time.realtimeSinceStartup;
			softAlgos = -1;
			int num = 0;
			List<XYZ> list = CollectionPool.RequestList<XYZ>();
			drawer.GetNeedingDrawConfirmation(list);
			foreach (XYZ item in list)
			{
				Draw(item);
				drawer.MarkAsDrawConfirmed(item);
				num++;
			}
			drawer.frameIndex++;
			list.Dispose();
			if (num > 0)
			{
				logger.WarnFormat("VoxelEngine detected Application lost focus, redrawn {0} MegaVoxels!", num);
			}
			else
			{
				logger.Info("VoxelEngine detected Application lost focus.");
			}
		}
		else if (redrawScheduled > redrawExpired)
		{
			redrawExpired = Time.realtimeSinceStartup;
			softAlgos = (useLateSoftAlgos ? lateSoftAlgos : initialSoftAlgos);
		}
		else
		{
			drawer.frameIndex++;
		}
	}

	public void GenerateBottom()
	{
		generateBottom.ReadWorld(this);
		generateBottom.GenerateBottom();
	}

	public void DrawBottom()
	{
		generateBottom.DrawBottom(this);
	}

	private IFlatWorldColors GetVoxelWorld(ref XYZ p, bool updateIfNeeded = true)
	{
		p -= worldStart;
		if (!(p >= XYZ.zero) || !(p < worldSize))
		{
			return null;
		}
		XYZ xYZ = XYZ.Div(p, MvSize);
		if (!drawer.Ready || !drawer.CheckOccupied(xYZ))
		{
			return null;
		}
		IFlatWorldColors cachedWorld = drawer.GetCachedWorld(xYZ);
		if (cachedWorld.Type.Empty && !updateIfNeeded)
		{
			InitLodProfile(0);
			IFlatWorldColors lodInitialWorld = lodSorter.GetLodInitialWorld(0);
			if (!lodInitialWorld.Type.Empty)
			{
				return lodInitialWorld;
			}
		}
		XYZ start = XYZ.Mul(xYZ, MvSize);
		GetWorldSection(cachedWorld, start, MvSize, Border, initRender: true);
		XYZ paddedStart = GetPaddedStart(start, Border);
		p -= paddedStart;
		return cachedWorld;
	}

	private int GetVoxelOffset(IFlatWorldColors world, XYZ p)
	{
		if (world == null)
		{
			return -1;
		}
		Color32 pixel = world.Column.GetPixel(p.x, p.z);
		if (p.y < pixel.r || p.y >= pixel.g)
		{
			return -1;
		}
		if (p.y >= pixel.b && p.y < pixel.a)
		{
			return -2;
		}
		int num = p.y - pixel.r - ((p.y >= pixel.b) ? (pixel.a - pixel.b) : 0);
		pixel = world.Xz.GetPixel(p.x, p.z);
		return Soft.DecodeInt(pixel) + num;
	}

	public void AssignVoxelHandle(ref FlatWorldHandle h, XYZ p, bool updateIfNeeded)
	{
		h.w = GetVoxelWorld(ref p, updateIfNeeded);
		h.i = GetVoxelOffset(h.w, p);
	}

	public FlatWorldHandle GetVoxelHandle(XYZ p, bool updateIfNeeded)
	{
		FlatWorldHandle h = default(FlatWorldHandle);
		AssignVoxelHandle(ref h, p, updateIfNeeded);
		return h;
	}

	public bool IsVoxelReady(XYZ p)
	{
		XYZ p2 = XYZ.Div(p, MvSize);
		return drawer.CheckDirty(p2);
	}

	public bool GetVoxelNonEmptyFromLayers(XYZ p)
	{
		bool result = false;
		for (int i = 0; i < layers.layers.Count; i++)
		{
			layers.layers[i].IsNonEmpty(p, ref result);
		}
		return result;
	}

	public Color32 GetVoxelDataFromLayers(XYZ p)
	{
		Color32 data = default(Color32);
		for (int i = 0; i < layers.layers.Count; i++)
		{
			layers.layers[i].GetVoxelData(p, ref data);
		}
		if (data.r == byte.MaxValue)
		{
			data.r = (byte)atlasComponent.atlas.DefaultDeepType;
		}
		return data;
	}

	public override bool GetVoxelCheck(XYZ p)
	{
		return GetVoxelHandle(p, updateIfNeeded: false).IsNonEmpty;
	}

	public void SetDirty(XYZ p)
	{
		XYZ xYZ = XYZ.Div(p, MvSize);
		drawer.MarkAsDirty(xYZ);
		XYZ xYZ2 = XYZ.Div(p - XYZ.one * 2, MvSize);
		XYZ xYZ3 = XYZ.Div(p + XYZ.one * 2, MvSize);
		if (xYZ == xYZ2 && xYZ == xYZ3)
		{
			return;
		}
		XYZ xYZ4 = default(XYZ);
		xYZ4.x = xYZ2.x;
		while (xYZ4.x <= xYZ3.x)
		{
			xYZ4.y = xYZ2.y;
			while (xYZ4.y <= xYZ3.y)
			{
				xYZ4.z = xYZ2.z;
				while (xYZ4.z <= xYZ3.z)
				{
					if (xYZ4 != xYZ)
					{
						drawer.MarkAsDirty(xYZ4);
					}
					xYZ4.z++;
				}
				xYZ4.y++;
			}
			xYZ4.x++;
		}
	}

	public bool SetVoxelCheck(XYZ p, bool check)
	{
		InitLayers();
		NegativeLayer negativeLayer = GetNegativeLayer();
		if (negativeLayer == null)
		{
			return false;
		}
		if (!check)
		{
			if (!negativeLayer.removedSet.Add(p))
			{
				return false;
			}
			GetTerrainSurfaceLayer()?.addedSet.Remove(p);
		}
		else if (!negativeLayer.removedSet.Remove(p))
		{
			return false;
		}
		SetDirty(p);
		return true;
	}

	public void SetCustomTerrainResources(XYZ p, VoxelTerrainResources r)
	{
		TerrainSurfaceLayer terrainSurfaceLayer = GetTerrainSurfaceLayer();
		if (terrainSurfaceLayer != null)
		{
			bool flag = SetVoxelCheck(p, check: true);
			terrainSurfaceLayer.addedSet.Add(p);
			terrainSurfaceLayer.p2resources[p] = r;
			if (!flag)
			{
				SetDirty(p);
			}
		}
	}

	public VoxelTerrainResources GetCustomTerrainResources(XYZ p)
	{
		TerrainSurfaceLayer terrainSurfaceLayer = GetTerrainSurfaceLayer();
		if (terrainSurfaceLayer == null)
		{
			return null;
		}
		if (!terrainSurfaceLayer.addedSet.Contains(p))
		{
			return null;
		}
		return terrainSurfaceLayer.p2resources.GetValueOrDefault(p);
	}

	public override void SetVoxel(XYZ p, int type)
	{
		SetVoxelCheck(p, type != 0);
	}

	private IEnumerable<XYZ> GetVisibleVoxels(IEnumerable<XYZ> occupiedMVs)
	{
		foreach (XYZ q in occupiedMVs)
		{
			XYZ start = XYZ.Mul(q, MvSize);
			IFlatWorldColors world = drawer.GetCachedWorld(q);
			if (!world.Type.Empty)
			{
				GetWorldSection(world, start, MvSize, Border, initRender: true);
				if (world.Config.Empty)
				{
					continue;
				}
				XYZ paddedStart = GetPaddedStart(start, Border);
				foreach (KeyValuePair<int, XYZ> nonEmptyOffset in Soft.GetNonEmptyOffsets(world.Config, world.Xz, world.Column, Border, MvSize))
				{
					yield return nonEmptyOffset.Value + paddedStart;
				}
				continue;
			}
			InitLodProfile(0);
			FlatWorld initialWorld = lodSorter.GetLodInitialWorld(0);
			if (initialWorld.typeTex.empty)
			{
				continue;
			}
			foreach (KeyValuePair<int, XYZ> visibleVoxel in initialWorld.GetVisibleVoxels(start, MvSize))
			{
				yield return visibleVoxel.Value;
			}
		}
	}

	public void GetTopVoxelsFromLayers(HashSet<XYZ> result)
	{
		for (int i = 0; i < layers.layers.Count; i++)
		{
			layers.layers[i].GetPotentialTopVoxels(result);
		}
		result.RemoveWhere(delegate(XYZ p)
		{
			Color32 data = default(Color32);
			for (int j = 0; j < layers.layers.Count; j++)
			{
				layers.layers[j].GetVoxelData(p + XYZ.up, ref data);
			}
			return data.r != 0;
		});
	}

	public void GetVisibleVoxelsFromLayers(HashSet<XYZ> result)
	{
		for (int i = 0; i < layers.layers.Count; i++)
		{
			layers.layers[i].GetPotentialVisibleVoxels(result);
		}
		result.RemoveWhere(delegate(XYZ p)
		{
			for (int j = 0; j < 6; j++)
			{
				Color32 data = default(Color32);
				for (int k = 0; k < layers.layers.Count; k++)
				{
					layers.layers[k].GetVoxelData(p + CubeModele.XYZDirs[j], ref data);
				}
				if (data.r == 0)
				{
					return false;
				}
			}
			return true;
		});
	}

	public IEnumerable<XYZ> GetVisibleVoxels()
	{
		InitLayers();
		HashSet<XYZ> occupied = CollectionPool.RequestHashSet<XYZ>();
		drawer.GetOccupied(occupied);
		foreach (XYZ visibleVoxel in GetVisibleVoxels(occupied))
		{
			yield return visibleVoxel;
		}
		occupied.Dispose();
	}

	public IEnumerable<XYZ> GetVisibleVoxels(XYZ start, XYZ size, int lod)
	{
		if (lod == 0)
		{
			if (XYZ.Mod(start, MvSize) == XYZ.zero && XYZ.Mod(size, MvSize) == XYZ.zero)
			{
				IEnumerable<XYZ> occupied = drawer.GetOccupied(XYZ.Div(start, MvSize), XYZ.DivCeil(size, MvSize));
				return GetVisibleVoxels(occupied);
			}
			XYZ xYZ = XYZ.Div(start, MvSize);
			XYZ xYZ2 = XYZ.Div(start + size - XYZ.one, MvSize);
			XYZ size2 = xYZ2 - xYZ + XYZ.one;
			IEnumerable<XYZ> occupied2 = drawer.GetOccupied(xYZ, size2);
			return from p in GetVisibleVoxels(occupied2)
				where p >= start && p < start + size
				select p;
		}
		if (!lodSorter.HasProfile(lod))
		{
			logger.Warn("TestWorldConfig3.GetVisibleVoxels only supports lods 0 and 3 for now.");
			return Enumerable.Empty<XYZ>();
		}
		FlatWorld lodInitialWorld = lodSorter.GetLodInitialWorld(lod);
		if (!lodInitialWorld.typeTex.empty)
		{
			return lodInitialWorld.GetVoxelPositions(lodInitialWorld.GetVisibleVoxelOffsets());
		}
		return GetVisibleVoxels_2(start, size, lod);
	}

	private IEnumerable<XYZ> GetVisibleVoxels_2(XYZ start, XYZ size, int lod)
	{
		IEnumerable<XYZ> occupied = drawer.GetOccupied(XYZ.Div(start << lod, MvSize), XYZ.DivCeil(size << lod, MvSize));
		XYZ scale = XYZ.DivCeil(MvSize, XYZ.one << lod);
		XYZ[] vIn = Calcule.VolumeBetween(XYZ.zero, scale - XYZ.one).ToArray();
		return from mv in occupied
			from d in vIn
			select XYZ.Mul(mv, scale) + d;
	}

	private IEnumerable<XYZ> GetSurfaceVoxels(IEnumerable<XYZ> occupiedMVs, XYZ start, XYZ size)
	{
		foreach (XYZ q in occupiedMVs)
		{
			XYZ p = XYZ.Mul(q, MvSize);
			IFlatWorldColors world = drawer.GetCachedWorld(q);
			if (!world.Type.Empty)
			{
				GetWorldSection(world, p, MvSize, Border, initRender: true);
				if (world.Config.Empty)
				{
					continue;
				}
				XYZ paddedStart = GetPaddedStart(p, Border);
				foreach (KeyValuePair<int, XYZ> surfaceOffset in Soft.GetSurfaceOffsets(start: XYZ.Max(Border, start - p), size: XYZ.Min(MvSize, start + size - p), dataTex: world.Config, xzTex: world.Xz, columnTex: world.Column))
				{
					yield return surfaceOffset.Value + paddedStart;
				}
				continue;
			}
			InitLodProfile(0);
			foreach (KeyValuePair<int, XYZ> surfaceVoxel in lodSorter.GetLodInitialWorld(0).GetSurfaceVoxels(p, MvSize))
			{
				yield return surfaceVoxel.Value;
			}
		}
	}

	public IEnumerable<XYZ> GetSurfaceVoxels()
	{
		InitLayers();
		return GetSurfaceVoxels(drawer.GetOccupied(), XYZ.zero, worldSize);
	}

	public IEnumerable<XYZ> GetSurfaceVoxels(XYZ start, XYZ size, int lod)
	{
		if (lod == 0)
		{
			IEnumerable<XYZ> occupied = drawer.GetOccupied(XYZ.Div(start << lod, MvSize), XYZ.DivCeil(size << lod, MvSize));
			return GetSurfaceVoxels(occupied, start, size);
		}
		throw new NotImplementedException();
	}

	public bool IsOccupiedRegion(XYZ region)
	{
		InitLayers();
		return drawer.IsOccupied(region);
	}

	public bool IsOccupiedRegion(XYZ region, XYZ regionSize)
	{
		if (regionSize != MvSize)
		{
			throw new NotSupportedException();
		}
		InitLayers();
		return drawer.IsOccupied(region);
	}

	public IEnumerable<XYZ> GetOccupiedRegions(XYZ regionSize)
	{
		InitLayers();
		return GetOccupiedRegions(drawer.GetOccupied(), regionSize);
	}

	public IEnumerable<XYZ> GetOccupiedRegions(XYZ regionSize, XYZ start, XYZ size)
	{
		InitLayers();
		return GetOccupiedRegions(drawer.GetOccupied(XYZ.Div(start, MvSize), XYZ.DivCeil(size, MvSize)), regionSize);
	}

	private IEnumerable<XYZ> GetOccupiedRegions(IEnumerable<XYZ> occupiedMVs, XYZ regionSize)
	{
		if (regionSize < MvSize)
		{
			XYZ scale = XYZ.Div(MvSize, regionSize);
			XYZ[] vIn = Calcule.VolumeBetween(XYZ.zero, scale - XYZ.one).ToArray();
			return from mv in occupiedMVs
				from d in vIn
				select XYZ.Mul(mv, scale) + d;
		}
		if (regionSize == MvSize)
		{
			return occupiedMVs;
		}
		return occupiedMVs.Select((XYZ mv) => XYZ.Div(XYZ.Mul(mv, MvSize), regionSize)).Distinct();
	}

	public IEnumerable<KeyValuePair<XYZ, FlatWorldHandle>> GetNonEmptyVoxels(XYZ start, XYZ size)
	{
		FlatWorld world = new FlatWorld();
		InitLayers();
		XYZ totalSize = size + border * 2;
		totalSize.x = 1 << Calcule.GetBestPower(totalSize.x);
		totalSize.z = 1 << Calcule.GetBestPower(totalSize.z);
		XYZ paddedStart = GetPaddedStart(start, border);
		LayersApplyingSession session = new LayersApplyingSession();
		session.Init(world, layers, paddedStart, totalSize.x, totalSize.z);
		session.Filling();
		session.InitColumnAndOffset();
		session.InitialData();
		session.ProcessedData();
		session.CopyDataToWorld();
		InitRender(world, start - border, force: true);
		FlatWorldHandle h = new FlatWorldHandle
		{
			w = world
		};
		foreach (XYZ p in Calcule.VolumeBetween(start, start + size - XYZ.one))
		{
			h.i = GetVoxelOffset(world, p - worldStart - paddedStart);
			yield return new KeyValuePair<XYZ, FlatWorldHandle>(p, h);
		}
		world.Destroy();
	}

	public XYZ GetClosestVisibleVoxelLowRes(XYZ p, int maxDistance = 200)
	{
		InitLayers();
		HeightmapLayer layer = layers.GetLayer<HeightmapLayer>();
		Color32[] pixels = layer.fillingTex.GetPixels32();
		int width = layer.fillingTex.Width;
		HeightmapLayer.LowResHeightmap lowResHeightmap = (HeightmapLayer.LowResHeightmap)layer.LowRes;
		Color32[] pixels2 = lowResHeightmap.fillingTex.GetPixels32();
		int width2 = lowResHeightmap.fillingTex.Width;
		XZ q = XZ.FromXYZ(XYZ.Div(p, MvSize));
		XYZ bestP = p + XYZ.forward * maxDistance;
		int bestD = maxDistance * maxDistance;
		for (int i = 1; i * i < bestD; i++)
		{
			int num = i * 2;
			while (num > 1)
			{
				DistanceToLayer(p, q, pixels2, width2, pixels, width, ref bestD, ref bestP);
				num--;
				q.x++;
			}
			int num2 = i * 2;
			while (num2 > 1)
			{
				DistanceToLayer(p, q, pixels2, width2, pixels, width, ref bestD, ref bestP);
				num2--;
				q.z++;
			}
			int num3 = i * 2;
			while (num3 > 0)
			{
				DistanceToLayer(p, q, pixels2, width2, pixels, width, ref bestD, ref bestP);
				num3--;
				q.x--;
			}
			int num4 = i * 2;
			while (num4 > 0)
			{
				DistanceToLayer(p, q, pixels2, width2, pixels, width, ref bestD, ref bestP);
				num4--;
				q.z--;
			}
		}
		return bestP;
	}

	private void DistanceToLayer(XYZ p, XZ q, Color32[] lowResFilling, int lfw, Color32[] filling, int fw, ref int bestD2, ref XYZ bestP)
	{
		if (q.x < 0 || q.x >= lfw || q.z < 0 || q.z >= lfw)
		{
			return;
		}
		Color32 color = lowResFilling[q.x + q.z * lfw];
		if (color.r < color.g)
		{
			int num = color.g * MvSize.y - 1;
			int num2 = color.r * MvSize.y;
			int num3 = Mathf.Max(0, Mathf.Max(p.y - num, num2 - p.y));
			XZ xZ = new XZ(p.x / MvSize.x - q.x, p.z / MvSize.z - q.z);
			int num4 = xZ.x * xZ.x - 2 * Mathf.Abs(xZ.x) - 1 + xZ.z * xZ.z - 2 * Mathf.Abs(xZ.z) - 1;
			int num5 = num4 + num3 * num3;
			if (num5 < bestD2)
			{
				bestD2 = num5;
				bestP = new XYZ(q.x * MvSize.x + MvSize.x / 2, (p.y >= num) ? num : ((p.y < num2) ? num2 : p.y), q.z * MvSize.z + MvSize.z / 2);
			}
		}
	}

	public XYZ TerrainRaycast(XYZ a, XYZ b)
	{
		XYZ xYZ = XYZ.Abs(b - a);
		XYZ xYZ2 = xYZ;
		XYZ xYZ3 = new XYZ((b.x >= a.x) ? 1 : (-1), (b.y >= a.y) ? 1 : (-1), (b.z >= a.z) ? 1 : (-1));
		XYZ xYZ4 = ((xYZ.y <= 0) ? new XYZ(1, 1000000000, 1000000000) : ((xYZ.z > 0) ? new XYZ(xYZ.y * xYZ.z, xYZ.x * xYZ.z, xYZ.x * xYZ.y) : new XYZ(xYZ.y, xYZ.x, 1000000000)));
		XYZ xYZ5 = a;
		XYZ xYZ6 = b;
		for (int num = XYZ.ManhattanMagnitude(xYZ6 - xYZ5); num >= 0; num--)
		{
			if (GetVoxelCheck(xYZ5))
			{
				return xYZ5;
			}
			if (!(xYZ5 >= worldStart) || !(xYZ5 < worldStart + worldSize))
			{
				break;
			}
			XYZ xYZ7 = xYZ2 - xYZ4;
			if (xYZ7.x >= xYZ7.y && xYZ7.x >= xYZ7.z)
			{
				xYZ5.x += xYZ3.x;
				xYZ2.x -= xYZ4.x;
			}
			else if (xYZ7.y >= xYZ7.z)
			{
				xYZ5.y += xYZ3.y;
				xYZ2.y -= xYZ4.y;
			}
			else
			{
				xYZ5.z += xYZ3.z;
				xYZ2.z -= xYZ4.z;
			}
		}
		return XYZ.zero;
	}

	public Vector3 TerrainRaycast(Vector3 a, Vector3 b)
	{
		Vector3 vector = Calcule.AbsVecteur(b - a);
		XYZ xYZ = new XYZ((b.x >= a.x) ? 1 : (-1), (b.y >= a.y) ? 1 : (-1), (b.z >= a.z) ? 1 : (-1));
		XYZ xYZ2 = XYZ.FromVector3(a);
		XYZ xYZ3 = XYZ.FromVector3(b);
		float num = ((b.x >= a.x) ? (0.5f - (a.x - (float)xYZ2.x)) : (0.5f + (a.x - (float)xYZ2.x)));
		float num2 = ((b.y >= a.y) ? (0.5f - (a.y - (float)xYZ2.y)) : (0.5f + (a.y - (float)xYZ2.y)));
		float num3 = ((b.z >= a.z) ? (0.5f - (a.z - (float)xYZ2.z)) : (0.5f + (a.z - (float)xYZ2.z)));
		for (int num4 = XYZ.ManhattanMagnitude(xYZ3 - xYZ2); num4 >= 0; num4--)
		{
			FlatWorldHandle voxelHandle = GetVoxelHandle(xYZ2, updateIfNeeded: false);
			if (voxelHandle.IsNonEmpty)
			{
				float num5 = ProceduralCube.ComputeSurfaceHeightFromLevels(voxelHandle.Levels, a.x - (float)xYZ2.x, a.z - (float)xYZ2.z);
				if (a.y - (float)xYZ2.y <= num5)
				{
					return a;
				}
			}
			if (!(xYZ2 >= worldStart) || !(xYZ2 < worldStart + worldSize))
			{
				break;
			}
			float num6 = num / vector.x;
			float num7 = num2 / vector.y;
			float num8 = num3 / vector.z;
			if (num6 <= num7 && num6 <= num8)
			{
				xYZ2.x += xYZ.x;
				a.x += (float)xYZ.x * num;
				a.y += (float)xYZ.y * (vector.y * num6);
				a.z += (float)xYZ.z * (vector.z * num6);
				num2 -= vector.y * num6;
				num3 -= vector.z * num6;
				num = 1f;
			}
			else if (num7 <= num8)
			{
				xYZ2.y += xYZ.y;
				a.x += (float)xYZ.x * (vector.x * num7);
				a.y += (float)xYZ.y * num2;
				a.z += (float)xYZ.z * (vector.z * num7);
				num -= vector.x * num7;
				num3 -= vector.z * num7;
				num2 = 1f;
			}
			else
			{
				xYZ2.z += xYZ.z;
				a.x += (float)xYZ.x * (vector.x * num8);
				a.y += (float)xYZ.y * (vector.y * num8);
				a.z += (float)xYZ.z * num3;
				num -= vector.x * num8;
				num2 -= vector.y * num8;
				num3 = 1f;
			}
		}
		return Vector3.zero;
	}

	public XYZ EstimateOccupiedSize(out XYZ start)
	{
		InitLayers();
		XYZ xYZ = new XYZ(int.MaxValue, int.MaxValue, int.MaxValue);
		XYZ xYZ2 = new XYZ(int.MinValue, int.MinValue, int.MinValue);
		foreach (Layer layer in layers.layers)
		{
			XYZ occupiedOrUncertainTotalSize = layer.LowRes.GetOccupiedOrUncertainTotalSize(out start);
			xYZ = XYZ.Min(xYZ, start);
			xYZ2 = XYZ.Max(xYZ2, start + occupiedOrUncertainTotalSize);
		}
		start = xYZ;
		return xYZ2 - xYZ;
	}

	public override void InitAllLods()
	{
		throw new NotImplementedException();
	}

	public override void UpdateLod(Func<Vector3, float, int, int> lodFunc)
	{
		throw new NotImplementedException();
	}

	public override void DiscardUnusedLod()
	{
		throw new NotImplementedException();
	}

	public override void RenderMissingLod()
	{
		throw new NotImplementedException();
	}

	public override LodSorter GetLodSorter()
	{
		return LodSorter;
	}

	public override MondeAtlas GetAtlas()
	{
		if (atlasComponent != null)
		{
			return atlasComponent.atlas;
		}
		return MondeAtlas.GetAtlas("Terrain_LOD0");
	}

	public VoxelEngineImpl GetVoxelEngineImpl()
	{
		return GetSpecializedVoxelEngine();
	}

	public LayerVoxelEngine GetSpecializedVoxelEngine()
	{
		return new LayerVoxelEngine(this);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
