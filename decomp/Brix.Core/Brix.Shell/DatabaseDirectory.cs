using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Brix.Shell;

[Serializable]
public class DatabaseDirectory
{
	public string path;

	public string name => new DirectoryInfo(path).Name;

	public DatabaseFile[] files
	{
		get
		{
			if (!new DirectoryInfo(path).Exists)
			{
				return new DatabaseFile[0];
			}
			IEnumerable<DatabaseFile> source = from file in Directory.GetFiles(path)
				select new DatabaseFile(file);
			return source.ToArray();
		}
	}

	public DatabaseDirectory[] directories
	{
		get
		{
			if (!new DirectoryInfo(path).Exists)
			{
				return new DatabaseDirectory[0];
			}
			IEnumerable<DatabaseDirectory> source = from subpath in Directory.GetDirectories(path)
				select new DatabaseDirectory(subpath);
			return source.ToArray();
		}
	}

	public DatabaseDirectory(string path)
	{
		this.path = path;
	}
}
