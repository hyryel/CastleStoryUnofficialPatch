using System.Collections.Generic;
using System.IO;
using System.Text;
using Brix.Utils;
using I2.Loc;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.ThirdParty;

public class LocalizationCSVSplitter
{
	public static void Split(string pathIn, string pathEnglishOut, string pathFrenchOut)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		Read(pathIn, new List<string>[3] { list, list2, list3 });
		Write(pathEnglishOut, new List<string>[2] { list, list2 });
		Write(pathFrenchOut, new List<string>[2] { list, list3 });
	}

	public static void Read(string path, List<string>[] columns)
	{
		string text = File.ReadAllText(path);
		bool flag = false;
		int num = 0;
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '"':
				if (flag && text[i + 1] == '"')
				{
					text2 += '"';
					i++;
				}
				else
				{
					flag = !flag;
				}
				continue;
			case ',':
				if (!flag)
				{
					if (num < columns.Length)
					{
						columns[num].Add(text2);
					}
					else if (num != columns.Length - 1 || !string.IsNullOrEmpty(text2))
					{
						Debug.Log($"LocalizationCSVSplitter.Read('{path}' line={columns[0].Count}, column={num}) overflow for '{text2}'");
					}
					text2 = string.Empty;
					num++;
					continue;
				}
				break;
			}
			if (c == '\n' && !flag)
			{
				if (num < columns.Length)
				{
					Debug.Log($"LocalizationCSVSplitter.Read('{path}' line={columns[0].Count}, column={num}) missing columns");
					while (num < columns.Length)
					{
						columns[num++].Add(string.Empty);
					}
				}
				num = 0;
			}
			else
			{
				text2 += c;
			}
		}
	}

	public static void Write(string path, List<string>[] columns)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < columns[0].Count; i++)
		{
			for (int j = 0; j < columns.Length; j++)
			{
				string text = columns[j][i];
				if (text.Slinq().Any((char c2) => c2 == ',' || c2 == '"' || c2 == '\n'))
				{
					stringBuilder.Append('"');
					string text2 = text;
					foreach (char c in text2)
					{
						if (c == '"')
						{
							stringBuilder.Append('"').Append('"');
						}
						else
						{
							stringBuilder.Append(c);
						}
					}
					stringBuilder.Append('"');
				}
				else
				{
					stringBuilder.Append(text);
				}
				stringBuilder.Append(',');
			}
			stringBuilder.Append('\n');
		}
		File.WriteAllText(path, stringBuilder.ToString());
		Debug.Log("Wrote CSV file '" + path + "'.");
	}

	public static void ConvertI2ToCSV(string outPath, string i2raw)
	{
		string text = i2raw.SubstringBetween("[i2category]Default[/i2category]", "[/i2csv]");
		List<string[]> list = LocalizationReader.ReadI2CSV(text);
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		List<string> list6 = new List<string>();
		List<string>[] array = new List<string>[5] { list2, list3, list4, list5, list6 };
		foreach (string[] item in list)
		{
			for (int i = 0; i < item.Length; i++)
			{
				if (i < array.Length)
				{
					array[i].Add(item[i]);
				}
				else if (i != item.Length - 1 || !string.IsNullOrEmpty(item[i]))
				{
					Debug.Log($"LocalizationCSVSplitter.Convert(line={array[0].Count}, column={i}) overflow for word '{item[i]}'");
				}
			}
			if (item.Length < array.Length)
			{
				Debug.Log($"LocalizationCSVSplitter.Convert(line={array[0].Count}, column={item.Length}) missing columns");
				for (int j = item.Length; j < array.Length; j++)
				{
					array[j].Add(string.Empty);
				}
			}
		}
		Write(outPath, new List<string>[3] { list2, list5, list6 });
	}
}
