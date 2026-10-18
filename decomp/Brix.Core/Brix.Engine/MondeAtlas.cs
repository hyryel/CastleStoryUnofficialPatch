using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Core;
using Brix.Engine.Abstract;
using Brix.Engine.FlatWorld;
using Brix.Engine.Geometrie;
using Brix.Engine.TerrainAtlas;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public class MondeAtlas
{
	private static BrixLogger logger = new BrixLogger();

	public string name = "Terrain_LOD0";

	public Texture deepTypeTex;

	public TypeLookup[] deepTypeColors;

	public MondeAtlasEntry[] mapping;

	public CrustTypeConversion[] crustTypeConversions;

	public TerrainAtlasPaddingSettings paddingSettings;

	[VoxelTerrainType]
	public int GrassType = 2;

	[VoxelTerrainType]
	public int SandType = 4;

	[VoxelTerrainType]
	public int EarthType = 6;

	[VoxelTerrainType]
	public int DefaultDeepType = 6;

	private Dictionary<int, MondeAtlasEntry> terrainType2entry = null;

	private Dictionary<int, int> crustToPackedTypeDico = null;

	public int uvAtlasWidth => (paddingSettings != null) ? paddingSettings.uvAtlasWidth : 1024;

	public int uvInnerDecal => (paddingSettings != null) ? paddingSettings.uvInnerDecal : 2;

	public int uvOuterDecal => (paddingSettings != null) ? paddingSettings.uvOuterDecal : 0;

	public void Init()
	{
		if (terrainType2entry == null)
		{
			terrainType2entry = new Dictionary<int, MondeAtlasEntry>();
			MondeAtlasEntry[] array = mapping;
			foreach (MondeAtlasEntry mondeAtlasEntry in array)
			{
				terrainType2entry[mondeAtlasEntry.terrainType] = mondeAtlasEntry;
			}
		}
		if (crustToPackedTypeDico == null)
		{
			crustToPackedTypeDico = crustTypeConversions.ToDictionary((CrustTypeConversion c) => c.lightType, (CrustTypeConversion c) => c.denseType);
		}
	}

	public void Clear()
	{
		if (terrainType2entry != null)
		{
			terrainType2entry.Clear();
		}
		terrainType2entry = null;
	}

	public IEnumerable<int> GetVoxelTypes()
	{
		Init();
		return terrainType2entry.Values.Select((MondeAtlasEntry entry) => entry.terrainType);
	}

	private MondeAtlasEntry GetEntry(int typeVoxel)
	{
		Init();
		return terrainType2entry.GetValueOrDefault(typeVoxel) ?? mapping[0];
	}

	private TerrainAtlasVisualEntry GetVisuals(int typeVoxel)
	{
		return GetEntry(typeVoxel).visuals;
	}

	public Vector2 GetUv(int typeVoxel)
	{
		return GetVisuals(typeVoxel).uv;
	}

	public bool HasUvTransition(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.useTransitions;
	}

	public Vector2 GetUvTransition(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.useTransitions ? visuals.uvTransition : Vector2.one;
	}

	public bool HasUvSpecial(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.useSpecial;
	}

	public Vector2 GetUvSpecial(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.useSpecial ? visuals.uvSpecial : Vector2.one;
	}

	public bool UsesWeightedNoise(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.useWeightedNoise;
	}

	public WeightedNoiseData GetNoiseRowWeights(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.weightedNoise;
	}

	public int GetLayer(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals.useTransitions ? visuals.layer : 0;
	}

	public bool IsCroute(int typeVoxel)
	{
		return GetEntry(typeVoxel).densityLevel == DensityLevel.Crust;
	}

	public bool IsCroute(int typeVoxel, int lod)
	{
		DensityLevel densityLevel = GetEntry(typeVoxel).densityLevel;
		return densityLevel <= DensityLevel.Crust || (lod > 0 && densityLevel <= DensityLevel.Solid);
	}

	public bool IsCroute(int typeVoxel, bool dessous)
	{
		DensityLevel densityLevel = GetEntry(typeVoxel).densityLevel;
		return densityLevel <= DensityLevel.Crust || (dessous && densityLevel <= DensityLevel.Packed);
	}

	public bool IsCroute(int typeVoxel, bool dessous, int lod)
	{
		DensityLevel densityLevel = GetEntry(typeVoxel).densityLevel;
		return densityLevel <= DensityLevel.Crust || (dessous && densityLevel <= DensityLevel.Packed) || (lod > 0 && densityLevel <= DensityLevel.Solid);
	}

	public bool IsEmpty(int typeVoxel)
	{
		TerrainAtlasVisualEntry visuals = GetVisuals(typeVoxel);
		return visuals == mapping[0].visuals;
	}

	public string GetTypeName(int typeVoxel)
	{
		return GetVisuals(typeVoxel).name;
	}

	public void PackResources(VoxelTerrainResources r)
	{
		r.Convert(crustToPackedTypeDico);
	}

	public void RenderVoxel(int scalaire, int type, int config, IGeometrie geometrie, MeshAugmentable meshInfo)
	{
		Vector3 vecteur = Voxel.GetVecteur(scalaire);
		Vector2 uv = GetUv(type);
		geometrie.Render(meshInfo, vecteur, uv, config);
	}

	public bool InitTypeSampleTex(Tex tex, Tex A)
	{
		if (deepTypeTex == null)
		{
			return false;
		}
		int[] types = deepTypeColors.Select((TypeLookup c) => c.type).ToArray();
		Color[] colors = deepTypeColors.Select((TypeLookup c) => c.color).ToArray();
		Brix.Engine.FlatWorld.FlatWorld.GetTypeSampleTex(tex, A, deepTypeTex, colors, types);
		return true;
	}

	public bool InitTypeAtlasTexture(Tex tex)
	{
		Color32[] orAllocPixels = Soft.GetOrAllocPixels32(tex, 256, 8);
		Func<Vector2, byte> func = (Vector2 uv) => (byte)(Mathf.Clamp(Mathf.RoundToInt(uv.x), 0, 3) + Mathf.Clamp(Mathf.RoundToInt(uv.y), 0, 3) * 4);
		for (int num = 0; num < 256; num++)
		{
			bool flag = IsCroute(num);
			bool flag2 = IsCroute(num, dessous: true);
			bool flag3 = IsCroute(num, 1);
			TerrainAtlasVisualEntry visuals = GetVisuals(num);
			byte r = func(visuals.uv);
			byte g = func(visuals.uvTransition);
			byte b = (byte)visuals.layer;
			byte a = (byte)((flag ? 1 : 0) + (flag2 ? 2 : 0) + (flag3 ? 4 : 0) + (visuals.useTransitions ? 8 : 0) + (visuals.useSpecial ? 16 : 0) + (visuals.useWeightedNoise ? 32 : 0));
			if (visuals.useSpecial)
			{
				g = func(visuals.uvSpecial);
			}
			Color32 color = new Color32(r, g, b, a);
			for (int num2 = 0; num2 < 8; num2++)
			{
				orAllocPixels[num + num2 * 256] = color;
			}
		}
		tex.SetPixels32(orAllocPixels, 256, 8);
		return true;
	}

	public static MondeAtlas GetAtlas(string atlasName)
	{
		MondeAtlasComponent.dicoAtlas.TryGetValue(atlasName, out var value);
		return value;
	}
}
