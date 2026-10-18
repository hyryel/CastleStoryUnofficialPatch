using System.Collections.Generic;
using System.IO;
using I2.Loc.SimpleJSON;

namespace Brix.Game.Names;

public class NameJsonFile
{
	public interface IEntry
	{
		int Score { get; }
	}

	public class NameEntry : IEntry
	{
		public string Name;

		public int Score;

		int IEntry.Score => Score;
	}

	public class ThemeEntry : IEntry
	{
		public int Score;

		public int RepeatChance;

		public List<NameEntry> Data;

		int IEntry.Score => Score;
	}

	public string Path;

	public List<IEntry> Entries = new List<IEntry>();

	public NameJsonFile(string path)
	{
		Path = path;
	}

	public void ReadFromDisk()
	{
		string aJSON = File.ReadAllText(Path);
		JSONNode jSONNode = JSON.Parse(aJSON);
		foreach (JSONNode item in jSONNode.AsArray)
		{
			if (item.AsObject != null)
			{
				Entries.Add(ReadTheme(item));
			}
			else
			{
				Entries.Add(ReadName(item));
			}
		}
	}

	public void Destroy()
	{
		Entries.Clear();
	}

	private NameEntry ReadName(JSONNode node)
	{
		NameEntry nameEntry = new NameEntry();
		nameEntry.Name = node[0].Value;
		nameEntry.Score = node[1].AsInt;
		return nameEntry;
	}

	private ThemeEntry ReadTheme(JSONNode node)
	{
		ThemeEntry themeEntry = new ThemeEntry();
		themeEntry.Score = node["score"].AsInt;
		themeEntry.RepeatChance = node["chance"].AsInt;
		themeEntry.Data = new List<NameEntry>();
		foreach (JSONNode item in node["data"].AsArray)
		{
			themeEntry.Data.Add(ReadName(item));
		}
		return themeEntry;
	}
}
