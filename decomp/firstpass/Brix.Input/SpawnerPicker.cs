using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Input;

public class SpawnerPicker : PickerComponent
{
	public Factory.AssetKey key;

	public override void OnFirstActionTerrain(Vector3? clickedVoxel, ClickInfo clickInfo)
	{
		if (clickInfo.actionName == "cam_action1")
		{
			ObjectPoolSingleton.Request_Unsafe(key, clickedVoxel.Value, Quaternion.identity);
		}
	}
}
