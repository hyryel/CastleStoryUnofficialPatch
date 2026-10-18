using System.Collections.Generic;
using Brix.Engine.Environment;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Engine;

public class MegaVoxelTreePrinter : NaturePrinter
{
	public static readonly Quaternion BaseRotation = Quaternion.identity;

	public static readonly Matrix4x4 BaseTransform = Matrix4x4.TRS(-Vector3.up, BaseRotation, Vector3.one);

	private readonly MeshModifiable[] _meshModels;

	private readonly NatureLodPooler _pooler;

	public MegaVoxelTreePrinter()
	{
		Biome biome = GameParam.BiomeToLoad();
		LodMeshFilter component = biome.Trees.GetComponent<LodMeshFilter>();
		_meshModels = new MeshModifiable[4]
		{
			ReadMesh(component.lod0Mesh),
			ReadMesh(component.lod1Mesh),
			ReadMesh(component.lod2Mesh),
			ReadMesh(component.lod3Mesh)
		};
		_pooler = new NatureLodPooler("Tree", BaseTransform, _meshModels);
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

	public override MeshModifiable ComputeColliderMeshInfo(List<XYZ> positions, int lod, Vector3 baseTranslation)
	{
		return NatureBatcher.BuildColliderMesh(_meshModels[lod], positions.Count, (int i, List<XYZ> ps, Vector3 t) => GetTransform(ps[i], t, TreeRegistry.Instance.GetTree(ps[i])), positions, baseTranslation);
	}

	private static Matrix4x4 GetTransform(XYZ position, Vector3 baseTranslation, int arbreInfo)
	{
		float num = Mathf.Lerp(0.45f, 1f, (float)((arbreInfo >> 4) & 0xF) / 15f);
		Vector3 pos = position.ToVector3() - baseTranslation;
		Quaternion q = Quaternion.AngleAxis(22.5f * (float)arbreInfo, Vector3.up) * BaseRotation;
		Vector3 s = new Vector3(0.5f, 1f, 0.5f) * num * (1f + (float)(((arbreInfo >> 1) & 3) - 3) * 0.05f);
		return Matrix4x4.TRS(pos, q, s);
	}

	public override Mesh GetCombinedMesh(int count, int maxCountSoFar, int lod, int texWidth, int variation)
	{
		return _pooler.GetCombinedMeshOrLarger(count, maxCountSoFar, lod, texWidth, variation);
	}

	public override void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset)
	{
		NatureBatcher.GetWarpTex(ref resultTex, positions.Count, (int i, List<XYZ> ps, XYZ mv) => GetColor(ps[i], mv), positions, mvOffset);
	}

	public void GetWarpTex(ref Texture2D resultTex, IEnumerable<XYZ> positions, int count, XYZ mvOffset)
	{
		NatureBatcher.GetWarpTex(ref resultTex, count, delegate(int i, IEnumerator<XYZ> ps, XYZ mv)
		{
			ps.MoveNext();
			return GetColor(ps.Current, mv);
		}, positions.GetEnumerator(), mvOffset);
	}

	private static Color32 GetColor(XYZ position, XYZ mvOffset)
	{
		Color32 result = default(Color32);
		int tree = TreeRegistry.Instance.GetTree(position);
		if (tree != 0)
		{
			XYZ xYZ = position - mvOffset;
			result = new Color32((byte)xYZ.x, (byte)xYZ.y, (byte)xYZ.z, (byte)tree);
		}
		return result;
	}
}
