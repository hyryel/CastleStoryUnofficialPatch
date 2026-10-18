using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Brix.Utils.Logging;
using UnityEngine;

public class IdGenerator
{
	private static BrixLogger logger = new BrixLogger(typeof(IdGenerator).FullName);

	public string pattern = "\"\\$id\": \"(go\\:-?)?([0-9]+)\"";

	public List<string> Ids;

	public string rootId;

	private int IdCounter;

	public IdGenerator(string txt)
	{
		try
		{
			IdCounter = 0;
			Ids = GetAllIdsSorted(txt);
			logger.Info("Ids: " + string.Join(",", Ids.ToArray()));
			rootId = FindFirstNonConsecutive(Ids, 3);
			logger.Info("Root: " + rootId);
		}
		catch (Exception ex)
		{
			logger.Error(ex.Message + "at :" + ex.StackTrace);
			throw;
		}
	}

	public string GetNewId(bool isGameObject)
	{
		string text = string.Empty;
		if (isGameObject)
		{
			text += "go:";
		}
		text = text + rootId + IdCounter;
		IdCounter++;
		return text;
	}

	private List<string> GetAllIdsSorted(string txt)
	{
		List<string> list = new List<string>();
		MatchCollection matchCollection = Regex.Matches(txt, pattern);
		foreach (Match item in matchCollection)
		{
			list.Add(item.Groups[2].Value);
		}
		list.Sort();
		return list;
	}

	private string FindFirstNonConsecutive(List<string> sortedList, int digit_precision)
	{
		if (sortedList.Count == 0)
		{
			return "0";
		}
		if (sortedList.Count == 1)
		{
			return (int.Parse(sortedList[0]) + 1).ToString();
		}
		for (int i = 0; i < sortedList.Count - 1; i++)
		{
			if (!IsConsecutive(digit_precision, sortedList[i], sortedList[i + 1]))
			{
				string text = sortedList[i].Substring(0, Mathf.Min(sortedList[i].Length, digit_precision));
				logger.Info("Id goes after id: " + text);
				return (int.Parse(text) + 1).ToString();
			}
		}
		return FindFirstNonConsecutive(sortedList, digit_precision + 1);
	}

	private bool IsConsecutive(int digit_precision, string id1, string id2)
	{
		int b = Mathf.Min(id1.Length, id2.Length);
		int length = Mathf.Min(digit_precision, b);
		string s = id1.Substring(0, length);
		string s2 = id2.Substring(0, length);
		if (int.Parse(s2) - int.Parse(s) <= 1)
		{
			return true;
		}
		return false;
	}
}
