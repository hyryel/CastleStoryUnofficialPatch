using UnityEngine;

namespace Brix.Util;

internal class BundleArchive : Archive
{
	public AssetBundle bundle;

	public override bool FileExists(string path)
	{
		return bundle.Contains(path);
	}

	public override bool DirectoryExists(string path)
	{
		return false;
	}

	public override string ReadAllText(string path)
	{
		return (bundle.LoadAsset(path) as TextAsset).text;
	}

	public override byte[] ReadAllBytes(string path)
	{
		return (bundle.LoadAsset(path) as TextAsset).bytes;
	}
}
