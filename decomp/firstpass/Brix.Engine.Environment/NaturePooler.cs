using System.Collections.Generic;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Engine.Environment;

public class NaturePooler
{
	private readonly MeshModifiable[] _meshVariations;

	private readonly string _namePrefix;

	private readonly Matrix4x4 _transform;

	private readonly Dictionary<int, Mesh> _pool = new Dictionary<int, Mesh>();

	public NaturePooler(string namePrefix, Matrix4x4 transform, params MeshModifiable[] meshVariations)
	{
		_meshVariations = meshVariations;
		_namePrefix = namePrefix;
		_transform = transform;
		GameSignals.Connect(GameSignals.LeaveGame, Destroy);
	}

	public void Destroy()
	{
		foreach (KeyValuePair<int, Mesh> item in _pool)
		{
			Object.Destroy(item.Value);
		}
		_pool.Clear();
	}

	public MeshModifiable GetMeshVariation(int variation)
	{
		return _meshVariations[variation % _meshVariations.Length];
	}

	public Mesh GetCombinedMeshOrDefault(int count, int variation, int texWidth)
	{
		int num = ((count != 0) ? (1 + Mathf.CeilToInt(Mathf.Log(count, 2f))) : 0);
		num = num * _meshVariations.Length + variation;
		return _pool.GetValueOrDefault(num);
	}

	public Mesh GetCombinedMesh(int count, int texWidth, int variation)
	{
		int num = ((count != 0) ? (1 + Mathf.CeilToInt(Mathf.Log(count, 2f))) : 0);
		num = num * _meshVariations.Length + variation;
		Mesh mesh = _pool.GetValueOrDefault(num);
		if (mesh == null)
		{
			int num2 = Mathf.NextPowerOfTwo(count);
			BuiltinMeshInfo builtinMeshInfo = NatureBatcher.BuildWarpableMesh(GetMeshVariation(variation), 0, num2, texWidth, _transform);
			mesh = builtinMeshInfo.Rendre(new Mesh());
			mesh.name = _namePrefix + "." + variation + "x" + num2;
			mesh.bounds = new Bounds(Vector3.up, Vector3.one * 18f);
			_pool[num] = mesh;
		}
		return mesh;
	}

	public Mesh GetCombinedMesh(IList<int> variationCounts, int texWidth)
	{
		int num = 0;
		for (int i = 0; i < variationCounts.Count; i++)
		{
			int num2 = ((variationCounts[i] != 0) ? (1 + Mathf.CeilToInt(Mathf.Log(variationCounts[i], 2f))) : 0);
			num |= num2 << 4 * i;
		}
		Mesh mesh = _pool.GetValueOrDefault(num);
		if (mesh == null)
		{
			int num3 = 0;
			int num4 = 0;
			for (int j = 0; j < variationCounts.Count; j++)
			{
				MeshModifiable meshVariation = GetMeshVariation(j);
				int num5 = Mathf.NextPowerOfTwo(variationCounts[j]);
				num3 += meshVariation.GetVertexCount() * num5;
				num4 += meshVariation.GetTriangleCount() * num5;
			}
			MeshAugmentablePrealloue meshAugmentablePrealloue = new MeshAugmentablePrealloue(num3, num4);
			meshAugmentablePrealloue.AllocVertices(num3, useTangents: true);
			int num6 = 0;
			for (int k = 0; k < variationCounts.Count; k++)
			{
				int num7 = Mathf.NextPowerOfTwo(variationCounts[k]);
				NatureBatcher.BuildWarpableMesh(meshAugmentablePrealloue, _meshVariations[k % _meshVariations.Length], num6, num7, texWidth, _transform);
				num6 += num7;
			}
			mesh = meshAugmentablePrealloue.Rendre(new Mesh());
			mesh.name = _namePrefix + "x" + num6 + "." + num;
			mesh.bounds = new Bounds(Vector3.up, Vector3.one * 18f);
			_pool[num] = mesh;
		}
		return mesh;
	}
}
