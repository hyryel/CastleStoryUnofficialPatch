using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Brix.Thirdparty;

public class FindLocalizedStrings : MonoBehaviour
{
	public string[] rootFolders = new string[6] { "Assets/FactoryPrefabs", "Assets/IndexedResources", "Assets/Resources", "Assets/Scenes", "Assets/Standard Assets", "Info" };

	public HashSet<string> fileExtensions = new HashSet<string> { "txt", "prefab", "asset", "unity", "cs", "lua", "json" };

	public int iterations = 100;

	public string publishPath = "localized.txt";

	public string foundWherePath = "localized_sources.txt";

	public string comparePath = "localization_keys.txt";

	public string usedPath = "used.txt";

	public string unusedPath = "unused.txt";

	public bool includeNonStandard;

	public bool search;

	public bool stop;

	public bool publish;

	public bool compare;

	public string currentRoot;

	public string currentFile;

	public List<string> results = new List<string>();

	public Dictionary<string, string> foundWhere = new Dictionary<string, string>();

	public List<string> nonStandardKeys = new List<string>();

	public List<string> usedKeys = new List<string>();

	public List<string> unusedKeys = new List<string>();

	private IEnumerator enumerator;

	private void Update()
	{
		if (search)
		{
			search = false;
			enumerator = Search();
		}
		if (stop)
		{
			stop = false;
			enumerator = null;
		}
		if (publish)
		{
			publish = false;
			Publish();
		}
		if (compare)
		{
			compare = false;
			Compare();
		}
		if (enumerator == null)
		{
			return;
		}
		for (int i = 0; i < iterations; i++)
		{
			if (!enumerator.MoveNext())
			{
				enumerator = null;
				break;
			}
		}
	}

	private IEnumerator Search()
	{
		results.Clear();
		foundWhere.Clear();
		if (includeNonStandard)
		{
			InitNonStandardKeys();
		}
		for (int i = 0; i < rootFolders.Length; i++)
		{
			currentRoot = rootFolders[i];
			IEnumerator e = SearchFolder(currentRoot);
			while (e.MoveNext())
			{
				yield return null;
			}
		}
	}

	private void InitNonStandardKeys()
	{
		nonStandardKeys.Clear();
		string[] array = File.ReadAllLines(comparePath);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!(text == string.Empty) && !text.StartsWith("str_") && !text.StartsWith("##"))
			{
				nonStandardKeys.Add(text);
			}
		}
		nonStandardKeys.Sort();
	}

	private IEnumerator SearchFolder(string folder)
	{
		string[] files = Directory.GetFiles(folder);
		foreach (string file in files)
		{
			string extension = file.Substring(file.LastIndexOf('.') + 1);
			if (fileExtensions.Contains(extension))
			{
				try
				{
					SearchFile(file);
				}
				catch (Exception message)
				{
					Debug.LogWarning(message);
				}
				yield return null;
			}
		}
		string[] directories = Directory.GetDirectories(folder);
		foreach (string subfolder in directories)
		{
			IEnumerator e = SearchFolder(subfolder);
			while (e.MoveNext())
			{
				yield return null;
			}
		}
	}

	private void SearchFile(string file)
	{
		currentFile = file;
		string text = File.ReadAllText(file);
		SearchString(file, text, "##");
		SearchString(file, text, "str_");
		if (!includeNonStandard)
		{
			return;
		}
		foreach (string nonStandardKey in nonStandardKeys)
		{
			if (text.IndexOf(nonStandardKey) >= 0)
			{
				results.Add(nonStandardKey);
				foundWhere[nonStandardKey] = file;
			}
		}
	}

	private void SearchString(string file, string text, string prefix)
	{
		int num = 0;
		int num2 = 0;
		while (num >= 0 && num < text.Length)
		{
			num = text.IndexOf(prefix, num);
			if (num < 0)
			{
				break;
			}
			for (num2 = num + prefix.Length; num2 < text.Length && IsKeyCharacter(text[num2]); num2++)
			{
			}
			string text2 = text.Substring(num, num2 - num);
			results.Add(text2);
			foundWhere[text2] = file;
			num = num2;
		}
	}

	private bool IsKeyCharacter(char c)
	{
		return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
	}

	private void Publish()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (string item in from r in results.Distinct()
			orderby r
			select r)
		{
			stringBuilder.AppendLine(item);
			stringBuilder2.AppendLine(foundWhere[item]);
		}
		File.WriteAllText(publishPath, stringBuilder.ToString());
		File.WriteAllText(foundWherePath, stringBuilder2.ToString());
	}

	private void Compare()
	{
		usedKeys.Clear();
		unusedKeys.Clear();
		string[] array = File.ReadAllLines(comparePath);
		HashSet<string> hashSet = new HashSet<string>(File.ReadAllLines(publishPath));
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!(text == string.Empty))
			{
				if (hashSet.Contains(text))
				{
					usedKeys.Add(text);
				}
				else
				{
					unusedKeys.Add(text);
				}
			}
		}
		usedKeys.Sort();
		unusedKeys.Sort();
		File.WriteAllLines(usedPath, usedKeys.ToArray());
		File.WriteAllLines(unusedPath, unusedKeys.ToArray());
	}
}
