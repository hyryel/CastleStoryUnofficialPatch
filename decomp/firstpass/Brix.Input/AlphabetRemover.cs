using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Input;

internal class AlphabetRemover : PickerComponent
{
	public override void OnActionGameObject(GameObject clickedGameObject, ClickInfo clickInfo)
	{
		AssetKeyHolder component = clickedGameObject.GetComponent<AssetKeyHolder>();
		if (component != null && component.AssetKey.Factory == "Alphabet")
		{
			ObjectPoolSingleton.Release(clickedGameObject);
		}
	}
}
