using System.Collections.Generic;
using Brix.Engine.Blocks;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

public class MaterialCombineDispatcher : IVoxelDispatcher
{
	private Dictionary<string, List<MaterialCombineHarvester>> indexedHarvesters = new Dictionary<string, List<MaterialCombineHarvester>>();

	private List<MaterialCombineHarvester> _harvesters = new List<MaterialCombineHarvester>();

	private int layer = 10;

	public void Add(List<IBlockData> iBds)
	{
		foreach (IBlockData iBd in iBds)
		{
			iBd.RedrawVoxel(this);
		}
	}

	public void Dispatch(VoxelDisplayData voxelDisplayData)
	{
		MaterialCombineHarvester materialCombineHarvester = FindOrCreateHarvester(voxelDisplayData);
		materialCombineHarvester.Add(voxelDisplayData);
	}

	private MaterialCombineHarvester FindOrCreateHarvester(VoxelDisplayData voxelDisplayData)
	{
		List<MaterialCombineHarvester> valueOrCreate = indexedHarvesters.GetValueOrCreate(voxelDisplayData.ID);
		MaterialCombineHarvester materialCombineHarvester = valueOrCreate.Slinq().FirstOrDefault((MaterialCombineHarvester m, VoxelDisplayData d) => m.Matches(d.ID, d.Mats), voxelDisplayData);
		if (materialCombineHarvester == null)
		{
			materialCombineHarvester = SpawnHarvester(voxelDisplayData);
			valueOrCreate.Add(materialCombineHarvester);
		}
		return materialCombineHarvester;
	}

	private MaterialCombineHarvester SpawnHarvester(VoxelDisplayData voxelDisplayData)
	{
		GameObject gameObject = new GameObject("Harvester");
		gameObject.isStatic = true;
		gameObject.layer = layer;
		MaterialCombineHarvester materialCombineHarvester = gameObject.AddComponent<MaterialCombineHarvester>();
		indexedHarvesters.GetValueOrCreate(voxelDisplayData.ID).Add(materialCombineHarvester);
		materialCombineHarvester.ConfigureHarvester(voxelDisplayData.ID, voxelDisplayData.Mats);
		_harvesters.Add(materialCombineHarvester);
		Neo.ClasseMoi("MegaVoxels", gameObject);
		return materialCombineHarvester;
	}

	public void ApplyCombines()
	{
		foreach (MaterialCombineHarvester harvester in _harvesters)
		{
			harvester.ApplyCombine();
		}
	}

	public void SetLayer(int newLayer)
	{
		layer = newLayer;
		foreach (MaterialCombineHarvester harvester in _harvesters)
		{
			harvester.gameObject.layer = newLayer;
		}
	}

	public void ClearAll()
	{
		indexedHarvesters.Slinq().ForEach(delegate(KeyValuePair<string, List<MaterialCombineHarvester>> harvesters)
		{
			ClearHarvesterGroup(harvesters.Value);
		});
		_harvesters.Clear();
	}

	private void ClearHarvesterGroup(List<MaterialCombineHarvester> harvesters)
	{
		harvesters.Slinq().ForEach(delegate(MaterialCombineHarvester h)
		{
			h.Clear();
		});
		harvesters.Clear();
	}
}
