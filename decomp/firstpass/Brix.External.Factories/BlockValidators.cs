namespace Brix.External.Factories;

public static class BlockValidators
{
	public static Factory.AssetKey DefaultBlueprintValidator = new Factory.AssetKey("BlockValidators", "DefaultBlueprintValidator");

	public static Factory.AssetKey DefaultConcreteValidator = new Factory.AssetKey("BlockValidators", "DefaultConcreteValidator");

	public static Factory.AssetKey MineStairBlueprintValidator = new Factory.AssetKey("BlockValidators", "MineStairBlueprintValidator");

	public static Factory.AssetKey MineStairConcreteValidator = new Factory.AssetKey("BlockValidators", "MineStairConcreteValidator");
}
