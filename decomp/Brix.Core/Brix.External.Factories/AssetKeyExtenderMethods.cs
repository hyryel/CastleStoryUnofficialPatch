namespace Brix.External.Factories;

public static class AssetKeyExtenderMethods
{
	public static bool IsNullOrInvalid(this Factory.AssetKey key)
	{
		return key == null || string.IsNullOrEmpty(key.Factory) || string.IsNullOrEmpty(key.Name);
	}
}
