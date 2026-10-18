using System;
using System.IO;

namespace Brix.Shell;

[Serializable]
public class DatabaseFile
{
	public string path;

	public string name => new FileInfo(path).Name;

	public string language
	{
		get
		{
			string text = path.ToLower();
			if (text.EndsWith(".cs"))
			{
				return "C#";
			}
			if (text.EndsWith(".js"))
			{
				return "JS";
			}
			if (text.EndsWith(".boo"))
			{
				return "Boo";
			}
			return "";
		}
	}

	public string trimmedCode
	{
		get
		{
			if (!new FileInfo(path).Exists)
			{
				return "";
			}
			return File.ReadAllText(path);
		}
	}

	public Command command => new Command
	{
		language = language,
		trimmedCode = trimmedCode
	};

	public DatabaseFile(string path)
	{
		this.path = path;
	}
}
