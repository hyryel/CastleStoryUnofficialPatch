using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine.Environment;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Engine;

public class MegaVoxelBoulderPrinter
{
	public static readonly Vector3 BaseTranslation = Vector3.zero;

	public static readonly Quaternion BaseRotation = Quaternion.identity;

	public static readonly Matrix4x4 BaseTransform = Matrix4x4.TRS(BaseTranslation, BaseRotation, Vector3.one);

	private readonly MeshModifiable[][] _meshModels;

	private readonly NatureLodPooler _pooler;

	private readonly int[] _variationCountsBuffer;

	public int MaxVariationCount => _variationCountsBuffer.Length;

	public MegaVoxelBoulderPrinter()
	{
		LodMeshFilter component = Factory.Peek(Brix.External.Factories.Environment.BoulderSmall).GetComponent<LodMeshFilter>();
		LodMeshFilter component2 = Factory.Peek(Brix.External.Factories.Environment.BoulderMedium).GetComponent<LodMeshFilter>();
		LodMeshFilter component3 = Factory.Peek(Brix.External.Factories.Environment.BoulderLarge).GetComponent<LodMeshFilter>();
		_meshModels = new MeshModifiable[5][]
		{
			new MeshModifiable[3]
			{
				ReadMesh(component.lod0Mesh),
				ReadMesh(component2.lod0Mesh),
				ReadMesh(component3.lod0Mesh)
			},
			new MeshModifiable[3]
			{
				ReadMesh(component.lod1Mesh),
				ReadMesh(component2.lod1Mesh),
				ReadMesh(component3.lod1Mesh)
			},
			new MeshModifiable[3]
			{
				ReadMesh(component.lod2Mesh),
				ReadMesh(component2.lod2Mesh),
				ReadMesh(component3.lod2Mesh)
			},
			new MeshModifiable[3]
			{
				ReadMesh(component.lod3Mesh),
				ReadMesh(component2.lod3Mesh),
				ReadMesh(component3.lod3Mesh)
			},
			new MeshModifiable[3]
			{
				ReadMesh(component.lod4Mesh),
				ReadMesh(component2.lod4Mesh),
				ReadMesh(component3.lod4Mesh)
			}
		};
		_pooler = new NatureLodPooler("Boulder", BaseTransform, _meshModels);
		_variationCountsBuffer = new int[_meshModels.Max((MeshModifiable[] ms) => ms.Length)];
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

	public Mesh GetCombinedMesh(HashSet<BoulderData> mineralsData, int lod, int texWidth)
	{
		if (_meshModels[lod].Length == 0)
		{
			return _pooler.GetCombinedMesh(0, 0, texWidth);
		}
		int[] variationCountsBuffer = _variationCountsBuffer;
		Array.Clear(variationCountsBuffer, 0, variationCountsBuffer.Length);
		foreach (BoulderData mineralsDatum in mineralsData)
		{
			int meshVariationIndex = mineralsDatum.GetMeshVariationIndex();
			variationCountsBuffer[meshVariationIndex]++;
		}
		if (_meshModels[lod].Length == 1)
		{
			int num = 0;
			for (int i = 0; i < variationCountsBuffer.Length; i++)
			{
				num += _pooler.GetNextCeil(variationCountsBuffer[i]);
			}
			return _pooler.GetCombinedMesh(num, lod, texWidth);
		}
		return _pooler.GetCombinedMesh(variationCountsBuffer, lod, texWidth);
	}

	public MeshModifiable ComputeColliderMeshInfo(HashSet<BoulderData> mineralsData, int lod, Vector3 baseTranslation)
	{
		if (_meshModels[lod].Length <= 1)
		{
			return NatureBatcher.BuildColliderMesh(_meshModels[lod][0], mineralsData.Count, (int i, BoulderData[] md, Vector3 t) => GetTransform(md[i].RootPos, t, md[i]), mineralsData.ToArray(), baseTranslation);
		}
		int[] variationCountsBuffer = _variationCountsBuffer;
		Array.Clear(variationCountsBuffer, 0, variationCountsBuffer.Length);
		foreach (BoulderData mineralsDatum in mineralsData)
		{
			int meshVariationIndex = mineralsDatum.GetMeshVariationIndex();
			variationCountsBuffer[meshVariationIndex]++;
		}
		int num = 0;
		int num2 = 0;
		for (int num3 = 0; num3 < variationCountsBuffer.Length; num3++)
		{
			int num4 = variationCountsBuffer[num3];
			MeshModifiable meshModifiable = _meshModels[lod][num3 % _meshModels[lod].Length];
			num += meshModifiable.GetVertexCount() * num4;
			num2 += meshModifiable.GetTriangleCount() * num4;
		}
		BuiltinMeshInfo builtinMeshInfo = new BuiltinMeshInfo();
		builtinMeshInfo.triangles = new int[num2];
		builtinMeshInfo.vertices = new Vector3[num];
		BuiltinMeshInfo builtinMeshInfo2 = builtinMeshInfo;
		int num5 = 0;
		int num6 = 0;
		foreach (BoulderData mineralsDatum2 in mineralsData)
		{
			int meshVariationIndex2 = mineralsDatum2.GetMeshVariationIndex();
			MeshModifiable meshModifiable2 = _meshModels[lod][meshVariationIndex2 % _meshModels[lod].Length];
			Vector3[] vertices = meshModifiable2.GetVertices();
			int[] triangles = meshModifiable2.GetTriangles();
			Matrix4x4 transform = GetTransform(mineralsDatum2.RootPos, baseTranslation, mineralsDatum2);
			MeshInfo.CopyTriangles(triangles, builtinMeshInfo2.triangles, num6, num5, triangles.Length);
			MeshInfo.CopyVertices(vertices, builtinMeshInfo2.vertices, transform, num6, vertices.Length);
			num6 += vertices.Length;
			num5 += triangles.Length;
		}
		return builtinMeshInfo2;
	}

	private static Matrix4x4 GetTransform(XYZ position, Vector3 baseTranslation, BoulderData mineralData)
	{
		int variationIndex = mineralData.GetVariationIndex(position);
		float angle = (float)variationIndex * 22.5f;
		float angle2 = (float)(variationIndex + 5) * 45f;
		float num = Mathf.Lerp(0.5f, 1.1f, Mathf.InverseLerp(0f, 1f, mineralData.Growth));
		Vector3 pos = position.ToVector3() - baseTranslation + mineralData.GetDisplacement(position) + BaseTranslation;
		Quaternion q = Quaternion.AngleAxis(angle2, Vector3.forward) * Quaternion.AngleAxis(angle, Vector3.up) * BaseRotation;
		Vector3 s = Vector3.one * num;
		return Matrix4x4.TRS(pos, q, s);
	}

	private static Color32 GetColor(BoulderData mineralData, XYZ position, XYZ mvOffset)
	{
		int variationIndex = mineralData.GetVariationIndex(position);
		int num = Mathf.RoundToInt(15f * Mathf.InverseLerp(0f, 1f, mineralData.Growth));
		int num2 = (variationIndex & 0xF) | ((num & 0xF) << 4);
		XYZ xYZ = position - mvOffset;
		Vector3 displacement = mineralData.GetDisplacement(position);
		xYZ = xYZ * 15 + XYZ.FromVector3((displacement + Vector3.one / 2f) * 15f);
		return new Color32((byte)xYZ.x, (byte)xYZ.y, (byte)xYZ.z, (byte)num2);
	}

	public void GetWarpTex(ref Texture2D resultTex, HashSet<BoulderData> mineralsData, XYZ mvOffset)
	{
		if (resultTex == null)
		{
			resultTex = new Texture2D(16, 16, TextureFormat.ARGB32, mipmap: false, QualitySettings.activeColorSpace == ColorSpace.Linear)
			{
				filterMode = FilterMode.Point,
				anisoLevel = 0
			};
		}
		List<Color32>[] array = (from i in Enumerable.Range(0, MaxVariationCount)
			select new List<Color32>()).ToArray();
		foreach (BoulderData mineralsDatum in mineralsData)
		{
			array[mineralsDatum.GetMeshVariationIndex()].Add(GetColor(mineralsDatum, mineralsDatum.RootPos, mvOffset));
		}
		Color32[] array2 = new Color32[256];
		Array.Clear(array2, 0, array2.Length);
		int num = 0;
		foreach (List<Color32> list in array)
		{
			int num3 = Mathf.Min(_pooler.GetNextCeil(list.Count), 256 - num);
			for (int num4 = 0; num4 < num3; num4++)
			{
				ref Color32 reference = ref array2[num++];
				reference = ((num4 >= list.Count) ? default(Color32) : list[num4]);
			}
		}
		resultTex.SetPixels32(array2);
		resultTex.Apply();
	}
}
