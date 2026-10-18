using System.Collections.Generic;
using Brix.Engine.Environment;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class FlatWorldInitializer : MonoBehaviour
{
	public static FlatWorldInitializer Instance;

	public GameObject engineProvider;

	public GameObject bouleDeLod;

	public MondeAtlasComponent atlasComponent;

	public List<Shader> ReferencedEngineShaders;

	public bool Initialized;

	public string biomeName;

	private void SetAtlasColorTypePair(int typeIndex, int terrainType)
	{
		if (atlasComponent == null)
		{
			atlasComponent = GetComponentsInChildren<MondeAtlasComponent>(includeInactive: true)[0];
		}
		atlasComponent.atlas.deepTypeColors[typeIndex].type = terrainType;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		Init();
		Refresh();
	}

	private void Init()
	{
		IVoxelEngineProvider voxelEngineProvider = null;
		if (engineProvider != null)
		{
			engineProvider.SetActive(value: true);
			voxelEngineProvider = engineProvider.GetInterface<IVoxelEngineProvider>();
		}
		if (voxelEngineProvider != null)
		{
			VoxelEngineImpl voxelEngineImpl = (VoxelEngine.Impl = voxelEngineProvider.GetVoxelEngineImpl());
			Voxel.SetVoxelEngine(voxelEngineImpl);
			Biome biome = GameParam.BiomeToLoad();
			biomeName = biome.Name;
			voxelEngineImpl.RenderMaterial = biome.BiomeMat;
		}
		if (bouleDeLod != null)
		{
			bouleDeLod.transform.parent = Camera.main.transform;
			bouleDeLod.transform.localPosition = Vector3.zero;
		}
		Initialized = true;
	}

	public static void Refresh()
	{
		if (!(Instance == null) && Instance.Initialized)
		{
			VoxelEngineImpl engine = Voxel.Engine;
			int overrideTerrainAlgos = Neo.overrideTerrainAlgos;
			int overrideTerrainLateAlgos = Neo.overrideTerrainLateAlgos;
			if (Neo.TerrainThreading == 0)
			{
				engine.PreferSynchronousDrawing = true;
			}
			else
			{
				engine.PreferSynchronousDrawing = false;
			}
			overrideTerrainAlgos = (overrideTerrainLateAlgos = -1);
			engine.SetSoftAlgos(overrideTerrainAlgos);
			engine.SetLateSoftAlgos(overrideTerrainLateAlgos);
			int lodMax = engine.GetLodMax();
			if (Neo.overrideTerrainLodMax >= 0)
			{
				lodMax = Neo.overrideTerrainLodMax;
			}
			if (Neo.terrainLodQuality == 3)
			{
				lodMax = 0;
			}
			engine.SetLodMax(lodMax);
			engine.SetLodQuality(Neo.terrainLodQuality);
		}
	}
}
