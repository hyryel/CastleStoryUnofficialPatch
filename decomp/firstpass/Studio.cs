using System.Collections.Generic;

public class Studio
{
	public string Name { get; private set; }

	public List<string> Games { get; private set; }

	public List<string> Tokens { get; private set; }

	public List<int> GameIDs { get; private set; }

	public Studio(string name, List<string> games, List<string> tokens, List<int> ids)
	{
		Name = name;
		Games = games;
		Tokens = tokens;
		GameIDs = ids;
	}

	public static string[] GetStudioNames(List<Studio> studios)
	{
		if (studios == null)
		{
			return new string[1] { "-" };
		}
		string[] array = new string[studios.Count + 1];
		array[0] = "-";
		string text = string.Empty;
		for (int i = 0; i < studios.Count; i++)
		{
			array[i + 1] = studios[i].Name + text;
			text += " ";
		}
		return array;
	}

	public static string[] GetGameNames(int index, List<Studio> studios)
	{
		if (studios == null || studios[index].Games == null)
		{
			return new string[1] { "-" };
		}
		string[] array = new string[studios[index].Games.Count + 1];
		array[0] = "-";
		string text = string.Empty;
		for (int i = 0; i < studios[index].Games.Count; i++)
		{
			array[i + 1] = studios[index].Games[i] + text;
			text += " ";
		}
		return array;
	}
}
