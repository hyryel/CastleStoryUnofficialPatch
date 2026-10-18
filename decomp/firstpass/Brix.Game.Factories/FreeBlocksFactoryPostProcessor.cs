using System.Linq;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.Factories;

public class FreeBlocksFactoryPostProcessor : MonoBehaviour, IFactoryPostProcessor
{
	public void PostProcess(Factory f)
	{
		foreach (VolumeDataComponent item in from v in f.Templates.Select((FactoryTemplate t) => t.GetComponent<VolumeDataComponent>()).NotNull()
			where v.enabled
			select v)
		{
			VolumePreprocessor.CreatePreprocessedVolume(item.GetInterface<AssetKeyHolder>().AssetKey, item);
			ReplaceVolume(item);
		}
	}

	private void ReplaceVolume(VolumeDataComponent volumeDataComponent)
	{
		IVolume component = volumeDataComponent.gameObject.GetComponent<IVolume>();
		if (component == null)
		{
			MovableVolume movableVolume = volumeDataComponent.gameObject.AddComponent<MovableVolume>();
			movableVolume.RegistryMode = volumeDataComponent.RegistryMode;
		}
		Object.Destroy(volumeDataComponent);
		volumeDataComponent.enabled = false;
	}
}
