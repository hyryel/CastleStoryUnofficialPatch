using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine.Environment;

public class NatureLodPooler
{
	private readonly NaturePooler[] _lodPoolers;

	public NatureLodPooler(string namePrefix, Matrix4x4 transform, MeshModifiable[] modelMesh)
	{
		_lodPoolers = new NaturePooler[modelMesh.Length];
		for (int i = 0; i < modelMesh.Length; i++)
		{
			_lodPoolers[i] = new NaturePooler(namePrefix + " L" + i, transform, modelMesh[i]);
		}
	}

	public NatureLodPooler(string namePrefix, Matrix4x4 transform, MeshModifiable[][] modelMesh)
	{
		_lodPoolers = new NaturePooler[modelMesh.Length];
		for (int i = 0; i < modelMesh.Length; i++)
		{
			_lodPoolers[i] = new NaturePooler(namePrefix + " L" + i, transform, modelMesh[i]);
		}
	}

	public NatureLodPooler(string namePrefix, Matrix4x4 transform, MeshModifiable[] modelMesh, int lodCount, params int[] variationsPerLOD)
	{
		_lodPoolers = new NaturePooler[lodCount];
		int num = 0;
		for (int i = 0; i < lodCount; i++)
		{
			int num2 = ((i >= variationsPerLOD.Length) ? 1 : variationsPerLOD[i]);
			MeshModifiable[] meshVariations = modelMesh.TakeSome(num, num2);
			_lodPoolers[i] = new NaturePooler(namePrefix + " L" + i, transform, meshVariations);
			num += num2;
		}
	}

	public void Destroy()
	{
		for (int i = 0; i < _lodPoolers.Length; i++)
		{
			if (_lodPoolers[i] != null)
			{
				_lodPoolers[i].Destroy();
			}
			_lodPoolers[i] = null;
		}
	}

	public Mesh GetCombinedMesh(int count, int lod, int texWidth, int variation = 0)
	{
		if (lod >= _lodPoolers.Length)
		{
			return _lodPoolers[0].GetCombinedMesh(0, texWidth, 0);
		}
		return _lodPoolers[lod].GetCombinedMesh(count, texWidth, variation);
	}

	public Mesh GetCombinedMesh(IList<int> variationCounts, int lod, int texWidth)
	{
		if (lod >= _lodPoolers.Length)
		{
			return _lodPoolers[0].GetCombinedMesh(0, texWidth, 0);
		}
		return _lodPoolers[lod].GetCombinedMesh(variationCounts, texWidth);
	}

	public Mesh GetCombinedMeshOrLarger(int count, int maxCountSoFar, int lod, int texWidth, int variation = 0)
	{
		if (lod >= _lodPoolers.Length)
		{
			return _lodPoolers[0].GetCombinedMesh(0, texWidth, 0);
		}
		Mesh combinedMeshOrDefault = _lodPoolers[lod].GetCombinedMeshOrDefault(count, variation, texWidth);
		if (combinedMeshOrDefault != null)
		{
			return combinedMeshOrDefault;
		}
		combinedMeshOrDefault = _lodPoolers[lod].GetCombinedMeshOrDefault(maxCountSoFar, variation, texWidth);
		if (combinedMeshOrDefault != null)
		{
			return combinedMeshOrDefault;
		}
		return _lodPoolers[lod].GetCombinedMesh(count, texWidth, variation);
	}

	public int GetNextCeil(int count)
	{
		return Mathf.NextPowerOfTwo(count);
	}
}
