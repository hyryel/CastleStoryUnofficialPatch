using Brix.External.Factories;

namespace Brix.Game.Components;

internal interface IBlueprintMaterialProvider
{
	BlueprintMaterialData GetSharedMaterialForAsset(Factory.AssetKey state, PlacementFlags mode);

	void ToggleSharedMaterialFocus(bool value);
}
