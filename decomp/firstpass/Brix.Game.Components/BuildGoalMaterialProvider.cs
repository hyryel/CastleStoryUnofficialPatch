using System.Collections.Generic;
using Brix.External.Factories;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

public class BuildGoalMaterialProvider
{
	public enum BlueprintMaterialType
	{
		none,
		placed,
		commited,
		buildable,
		damaged
	}

	private Dictionary<Factory.AssetKey, Dictionary<PlacementFlags, BlueprintMaterialData>> materialForAsset = new Dictionary<Factory.AssetKey, Dictionary<PlacementFlags, BlueprintMaterialData>>();

	private bool focused;

	public void ToggleSharedMaterialFocus(bool value)
	{
		focused = value;
		foreach (KeyValuePair<Factory.AssetKey, Dictionary<PlacementFlags, BlueprintMaterialData>> item in materialForAsset)
		{
			foreach (KeyValuePair<PlacementFlags, BlueprintMaterialData> item2 in item.Value)
			{
				Color color = (value ? item2.Value.OriginalColor : Color.white);
				Material[] materials = item2.Value.materials;
				foreach (Material material in materials)
				{
					material.color = color;
				}
			}
		}
	}

	public BlueprintMaterialData GetSharedMaterialForAsset(Factory.AssetKey asset, PlacementFlags mode)
	{
		if (!materialForAsset.TryGetValue(asset, out var _))
		{
			materialForAsset[asset] = new Dictionary<PlacementFlags, BlueprintMaterialData>();
		}
		if (!materialForAsset[asset].TryGetValue(mode, out var _))
		{
			BlueprintMaterialData blueprintMaterialData = new BlueprintMaterialData();
			BlueprintMaterialCache.BlueprintDisplay blueprintDisplay = BlueprintMaterialCache.ForMode(mode);
			blueprintMaterialData.OriginalColor = blueprintDisplay.color;
			blueprintMaterialData.Scale = blueprintDisplay.Scale;
			blueprintMaterialData.materials = blueprintDisplay.ForKey(asset, UseCache: false).WithPasses(BlueprintMaterialCache.MaterialTrio.Mode.ThreePass);
			blueprintMaterialData.OriginalOpacity = blueprintMaterialData.materials[1].GetFloat("_FillOpacity");
			materialForAsset[asset][mode] = blueprintMaterialData;
			if (!focused)
			{
				materialForAsset[asset][mode].materials.Slinq().ForEach(delegate(Material mat)
				{
					mat.color = Color.white;
				});
			}
		}
		return materialForAsset[asset][mode];
	}

	public void Clearcache()
	{
		foreach (KeyValuePair<Factory.AssetKey, Dictionary<PlacementFlags, BlueprintMaterialData>> item in materialForAsset)
		{
			foreach (KeyValuePair<PlacementFlags, BlueprintMaterialData> item2 in item.Value)
			{
				Material[] materials = item2.Value.materials;
				foreach (Material obj in materials)
				{
					Object.Destroy(obj);
				}
			}
		}
	}
}
