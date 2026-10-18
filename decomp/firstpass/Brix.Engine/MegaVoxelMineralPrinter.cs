using System.Collections.Generic;
using Brix.Engine.Environment;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Legacy;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine;

public class MegaVoxelMineralPrinter : NaturePrinter
{
	public static readonly Quaternion BaseRotation = Quaternion.identity;

	public static readonly Matrix4x4 BaseTransform = Matrix4x4.TRS(Vector3.zero, BaseRotation, Vector3.one);

	public const int MeshCountPerVoxel = 3;

	private readonly MeshModifiable[] _meshModels;

	private readonly NatureLodPooler _pooler;

	public MegaVoxelMineralPrinter()
	{
		LodMeshFilter component = Factory.Peek(Brix.External.Factories.Environment.MineralMesh).GetComponent<LodMeshFilter>();
		_meshModels = new MeshModifiable[3]
		{
			ReadMesh(component.lod0Mesh),
			ReadMesh(component.lod1Mesh),
			ReadMesh(component.lod2Mesh)
		};
		_pooler = new NatureLodPooler("Crystal", BaseTransform, _meshModels);
	}

	protected virtual MineralData GetMineral(XYZ p)
	{
		return null;
	}

	private MeshModifiable ReadMesh(Mesh mesh)
	{
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo(mesh);
		builtinMeshInfo.uv2 = new Vector2[mesh.vertexCount];
		return builtinMeshInfo;
	}

	public void Destroy()
	{
		_pooler.Destroy();
	}

	public override MeshModifiable ComputeColliderMeshInfo(List<XYZ> positions, int lod, Vector3 baseTranslation)
	{
		List<MineralShardData> shards = CollectionPool.RequestList<MineralShardData>();
		List<MineralData> minerals = CollectionPool.RequestList<MineralData>();
		foreach (XYZ position in positions)
		{
			MineralData mineral = GetMineral(position);
			mineral.GetShards(position, ref shards);
			while (minerals.Count < shards.Count)
			{
				minerals.Add(mineral);
			}
		}
		MeshModifiable result = NatureBatcher.BuildColliderMesh(_meshModels[lod], shards.Count, (int i, Vector3 t, List<MineralShardData> ss) => GetTransform(minerals[i].RootPos, t, minerals[i], shards[i]), baseTranslation, shards);
		shards.Dispose();
		minerals.Dispose();
		return result;
	}

	private Matrix4x4 GetTransform(XYZ position, Vector3 baseTranslation, MineralData mineral, MineralShardData shard)
	{
		Vector3 pos = mineral.GetShardDisplacement(position, shard) - baseTranslation;
		Quaternion q = mineral.GetShardOrientation(position, shard) * BaseRotation;
		Vector3 s = Vector3.one * mineral.GetShardScale(position, shard);
		return Matrix4x4.TRS(pos, q, s);
	}

	public override Mesh GetCombinedMesh(int count, int maxCountSoFar, int lod, int texWidth, int variation)
	{
		return _pooler.GetCombinedMeshOrLarger(count * 3, maxCountSoFar * 3, lod, texWidth, variation);
	}

	public override void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset)
	{
		List<MineralShardData> list = CollectionPool.RequestList<MineralShardData>();
		NatureBatcher.GetWarpTex(ref resultTex, positions.Count * 3, GetColor, positions, mvOffset, list);
		list.Dispose();
	}

	private Color32 GetColor(int i, List<XYZ> ps, XYZ mv, List<MineralShardData> shards)
	{
		XYZ xYZ = ps[i / 3];
		if (i % 3 == 0)
		{
			shards.Clear();
			GetMineral(xYZ).GetShards(xYZ, ref shards);
		}
		return GetColor(xYZ, mv, shards[i % 3]);
	}

	private Color32 GetColor(XYZ position, XYZ mvOffset, MineralShardData shard)
	{
		MineralData mineral = GetMineral(position);
		if (mineral != null)
		{
			int variationIndex = mineral.GetVariationIndex(position);
			int num = Mathf.RoundToInt(mineral.Growth * shard.Weight * 15f);
			int num2 = (variationIndex & 0xF) | ((num & 0xF) << 4);
			XYZ xYZ = position - mvOffset;
			Vector3 offset = shard.Offset;
			xYZ = xYZ * 15 + XYZ.FromVector3((offset + Vector3.one / 2f) * 15f);
			return new Color32((byte)xYZ.x, (byte)xYZ.y, (byte)xYZ.z, (byte)num2);
		}
		return default(Color32);
	}
}
