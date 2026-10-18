using UnityEngine;

public static class IPlugExtension
{
	public static void ResetPlugCache(this IPlug _this)
	{
		Plug.ResetPlugCache(_this);
	}

	public static T GetPlugComponent<T>(this IPlug _this, string targetPlugName) where T : Component
	{
		return Plug.GetPlugComponent<T>(_this, targetPlugName);
	}

	public static bool TryGetPlugComponent<T>(this IPlug _this, string targetPlugName, out T outComponent) where T : Component
	{
		return Plug.TryGetPlugComponent<T>(_this, targetPlugName, out outComponent);
	}
}
