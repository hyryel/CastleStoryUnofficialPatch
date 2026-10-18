using System.IO;

namespace Brix.Util;

internal class DiskArchive : Archive
{
	public string prefix = "";

	public static DiskArchive Root = new DiskArchive();

	public override bool FileExists(string path)
	{
		return File.Exists(prefix + path);
	}

	public override bool DirectoryExists(string path)
	{
		return Directory.Exists(prefix + path);
	}

	public override string ReadAllText(string path)
	{
		return File.ReadAllText(prefix + path);
	}

	public override byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(prefix + path);
	}
}
