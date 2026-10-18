using System.Collections.Generic;
using Brix.Engine.Environment;
using Brix.Engine.Nature;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Engine;

public class MegaVoxelPlantPrinter : NaturePrinter
{
	private readonly MeshModifiable[][] _meshModels;

	private readonly NatureLodPooler _pooler;

	public int TranslationPacking = 7;

	public MegaVoxelPlantPrinter()
	{
		Biome biome = GameParam.BiomeToLoad();
		LodMeshFilter[] array = new LodMeshFilter[6]
		{
			biome.Plants[0].GetComponent<LodMeshFilter>(),
			biome.Plants[1].GetComponent<LodMeshFilter>(),
			biome.Plants[2].GetComponent<LodMeshFilter>(),
			biome.Plants[3].GetComponent<LodMeshFilter>(),
			biome.Plants[4].GetComponent<LodMeshFilter>(),
			biome.Plants[5].GetComponent<LodMeshFilter>()
		};
		LodMeshFilter lodMeshFilter = array[0];
		LodMeshFilter lodMeshFilter2 = array[1];
		LodMeshFilter lodMeshFilter3 = array[2];
		LodMeshFilter lodMeshFilter4 = array[3];
		LodMeshFilter lodMeshFilter5 = array[4];
		LodMeshFilter lodMeshFilter6 = array[5];
		_meshModels = new MeshModifiable[6][]
		{
			new MeshModifiable[6]
			{
				ReadMesh(lodMeshFilter.lod0Mesh),
				ReadMesh(lodMeshFilter2.lod0Mesh),
				ReadMesh(lodMeshFilter3.lod0Mesh),
				ReadMesh(lodMeshFilter4.lod0Mesh),
				ReadMesh(lodMeshFilter5.lod0Mesh),
				ReadMesh(lodMeshFilter6.lod0Mesh)
			},
			new MeshModifiable[6]
			{
				ReadMesh(lodMeshFilter.lod1Mesh),
				ReadMesh(lodMeshFilter2.lod1Mesh),
				ReadMesh(lodMeshFilter3.lod1Mesh),
				ReadMesh(lodMeshFilter4.lod1Mesh),
				ReadMesh(lodMeshFilter5.lod1Mesh),
				ReadMesh(lodMeshFilter6.lod1Mesh)
			},
			new MeshModifiable[6]
			{
				ReadMesh(lodMeshFilter.lod2Mesh),
				ReadMesh(lodMeshFilter2.lod2Mesh),
				ReadMesh(lodMeshFilter3.lod2Mesh),
				ReadMesh(lodMeshFilter4.lod2Mesh),
				ReadMesh(lodMeshFilter5.lod2Mesh),
				ReadMesh(lodMeshFilter6.lod2Mesh)
			},
			new MeshModifiable[6]
			{
				ReadMesh(lodMeshFilter.lod3Mesh),
				ReadMesh(lodMeshFilter2.lod3Mesh),
				ReadMesh(lodMeshFilter3.lod3Mesh),
				ReadMesh(lodMeshFilter4.lod3Mesh),
				ReadMesh(lodMeshFilter5.lod3Mesh),
				ReadMesh(lodMeshFilter6.lod3Mesh)
			},
			new MeshModifiable[6]
			{
				ReadMesh(lodMeshFilter.lod4Mesh),
				ReadMesh(lodMeshFilter2.lod4Mesh),
				ReadMesh(lodMeshFilter3.lod4Mesh),
				ReadMesh(lodMeshFilter4.lod4Mesh),
				ReadMesh(lodMeshFilter5.lod4Mesh),
				ReadMesh(lodMeshFilter6.lod4Mesh)
			},
			new MeshModifiable[6]
			{
				ReadMesh(lodMeshFilter.lod5Mesh),
				ReadMesh(lodMeshFilter2.lod5Mesh),
				ReadMesh(lodMeshFilter3.lod5Mesh),
				ReadMesh(lodMeshFilter4.lod5Mesh),
				ReadMesh(lodMeshFilter5.lod5Mesh),
				ReadMesh(lodMeshFilter6.lod5Mesh)
			}
		};
		_pooler = new NatureLodPooler("Plant", Matrix4x4.identity, _meshModels);
	}

	private static MeshModifiable ReadMesh(Mesh mesh)
	{
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo(mesh);
		builtinMeshInfo.uv2 = new Vector2[mesh.vertexCount];
		return builtinMeshInfo;
	}

	public void Destroy()
	{
		_pooler.Destroy();
	}

	public override Mesh GetCombinedMesh(int count, int maxCountSoFar, int lod, int texWidth, int variation)
	{
		return _pooler.GetCombinedMeshOrLarger(count, maxCountSoFar, lod, texWidth, variation);
	}

	public override MeshModifiable ComputeColliderMeshInfo(List<XYZ> positions, int lod, Vector3 baseTranslation)
	{
		return null;
	}

	public override void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset)
	{
		NatureBatcher.GetWarpTex(ref resultTex, positions.Count, (int i, List<XYZ> ps, XYZ mv, int t) => GetColor(ps[i], mv, t), positions, mvOffset, TranslationPacking);
	}

	private static Color32 GetColor(XYZ position, XYZ mvOffset, int packing)
	{
		Color32 result = default(Color32);
		PlantData plantData = PlantRegistry.PlantAt(position);
		if (plantData != null)
		{
			int num = Mathf.FloorToInt(16f * plantData.Variation.Orientation.eulerAngles.y / 360f);
			int num2 = Mathf.RoundToInt(15f * Mathf.InverseLerp(0f, 1f, plantData.Growth));
			int num3 = (num & 0xF) | ((num2 & 0xF) << 4);
			XYZ xYZ = position - mvOffset;
			Vector3 displacement = plantData.Displacement;
			xYZ = xYZ * packing + XYZ.FromVector3((displacement + Vector3.one / 2f) * packing);
			result = new Color32((byte)xYZ.x, (byte)xYZ.y, (byte)xYZ.z, (byte)num3);
		}
		return result;
	}
}
