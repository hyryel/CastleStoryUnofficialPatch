using UnityEngine;

namespace Brix.Util;

public abstract class Archive
{
	public abstract bool FileExists(string path);

	public abstract bool DirectoryExists(string path);

	public abstract string ReadAllText(string path);

	public abstract byte[] ReadAllBytes(string path);

	public static Archive FromDisk()
	{
		return DiskArchive.Root;
	}

	public static Archive FromDirectory(string path)
	{
		path.Replace("\\", "/");
		if (!path.EndsWith("/"))
		{
			path += "/";
		}
		return new DiskArchive
		{
			prefix = path
		};
	}

	public static Archive FromBundle(AssetBundle bundle)
	{
		return new BundleArchive
		{
			bundle = bundle
		};
	}
}
