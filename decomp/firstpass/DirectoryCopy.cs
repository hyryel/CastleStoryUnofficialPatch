using System.IO;

public static class DirectoryCopy
{
	public static void Copy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (Directory.Exists(destDirName))
		{
			Directory.Delete(destDirName, recursive: true);
		}
		Directory.CreateDirectory(destDirName);
		FileInfo[] files = directoryInfo.GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, overwrite: false);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array2 = directories;
			foreach (DirectoryInfo directoryInfo2 in array2)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				Copy(directoryInfo2.FullName, destDirName2, copySubDirs);
			}
		}
	}
}
