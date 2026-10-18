using System.Collections.Generic;
using System.Linq;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Smooth.Algebraics;
using UnityEngine;

namespace Brix.Game.Factories;

public class BlueprintVolumeFactoryPostProcessor : MonoBehaviour, IFactoryPostProcessor
{
	private const VPropertyMask AntiPropertiesToKeep = VPropertyMask.access | VPropertyMask.fantome | VPropertyMask.meshSwapTrigger;

	public void PostProcess(Factory f)
	{
		List<Tuple<VProperty, VProperty>> list = new List<Tuple<VProperty, VProperty>>();
		list.Add(Tuple.Create(VProperty.breakableAccess, VProperty.access));
		foreach (VolumeDataComponent item in from v in f.Templates.Select((FactoryTemplate t) => t.GetComponent<VolumeDataComponent>()).NotNull()
			where v.enabled
			select v)
		{
			if (item.GetComponent<Blueprint>() == null)
			{
				break;
			}
			Factory.AssetKey assetKey = item.GetInterface<AssetKeyHolder>().AssetKey;
			VolumePreprocessor.CreatePreprocessedVolume(assetKey, item);
			ConfigureAntiVolumeData(assetKey, list);
			VolumePreprocessor.RemovePropertiesFromVolume(VProperty.breakableAccess, assetKey);
			ReplaceVolume(item);
		}
	}

	private void ReplaceVolume(VolumeDataComponent volumeDataComponent)
	{
		IVolume component = volumeDataComponent.gameObject.GetComponent<IVolume>();
		if (component == null)
		{
			volumeDataComponent.gameObject.AddComponent<BlueprintVolume>();
		}
		Object.Destroy(volumeDataComponent);
		volumeDataComponent.enabled = false;
	}

	private static void ConfigureAntiVolumeData(Factory.AssetKey bpKey, List<Tuple<VProperty, VProperty>> conversionRules)
	{
		int configurationId = VolumePreprocessor.CreatePreprocessedVolumeSubEntry(bpKey);
		VolumePreprocessor.CopyPropertyIndicesToPropertyFromVolume(conversionRules, bpKey, configurationId);
		VolumePreprocessor.KeepOnlySelectedPropertiesForVolume(VPropertyMask.access | VPropertyMask.fantome | VPropertyMask.meshSwapTrigger, bpKey, configurationId);
	}
}
