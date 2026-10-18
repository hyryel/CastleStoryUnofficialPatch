using System;
using System.Collections.Generic;

namespace Brix.Shell;

[Serializable]
public class CodeDatabase
{
	public string path;

	public HashSet<string> expanded;

	public DatabaseDirectory Root => new DatabaseDirectory(path);

	public bool GetExpanded(DatabaseDirectory dir)
	{
		return expanded.Contains(dir.path.Substring(path.Length));
	}

	public void SetExpanded(DatabaseDirectory dir, bool b)
	{
		if (b)
		{
			expanded.Add(dir.path.Substring(path.Length));
		}
		else
		{
			expanded.Remove(dir.path.Substring(path.Length));
		}
	}
}
