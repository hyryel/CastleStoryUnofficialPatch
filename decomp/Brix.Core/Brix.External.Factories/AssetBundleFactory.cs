using UnityEngine;

namespace Brix.External.Factories;

public class AssetBundleFactory : Factory
{
	public string BundlePath;

	private AssetBundle AssetBundle;

	protected override void Preload()
	{
		AssetBundle = AssetBundle.LoadFromFile(BundlePath);
		Object[] array = AssetBundle.LoadAllAssets();
		Object[] array2 = array;
		foreach (Object obj in array2)
		{
			GameObject gameObject = obj as GameObject;
			gameObject.transform.parent = base.transform;
			gameObject.SetActive(value: false);
		}
		base.Preload();
	}
}
