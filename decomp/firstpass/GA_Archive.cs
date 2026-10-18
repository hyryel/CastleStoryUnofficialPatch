using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GA_Archive
{
	public string FILE_NAME = "GA_archive";

	public void ArchiveData(string json, GA_Submit.CategoryType serviceType)
	{
		StreamWriter streamWriter = null;
		string text = Application.persistentDataPath + "/" + FILE_NAME;
		if (File.Exists(text))
		{
			if (new FileInfo(text).Length + Encoding.Unicode.GetByteCount(json) <= GA.SettingsGA.ArchiveMaxFileSize)
			{
				streamWriter = File.AppendText(text);
			}
		}
		else if (Encoding.Unicode.GetByteCount(json) <= GA.SettingsGA.ArchiveMaxFileSize)
		{
			streamWriter = File.CreateText(text);
		}
		if (streamWriter != null)
		{
			streamWriter.WriteLine(string.Concat(serviceType, " ", json));
			streamWriter.Close();
		}
	}

	public List<GA_Submit.Item> GetArchivedData()
	{
		List<GA_Submit.Item> list = new List<GA_Submit.Item>();
		StreamReader streamReader = null;
		string path = Application.persistentDataPath + "/" + FILE_NAME;
		if (File.Exists(path))
		{
			streamReader = File.OpenText(path);
		}
		if (streamReader != null)
		{
			string text = null;
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(' ');
				if (array.Length < 2)
				{
					continue;
				}
				string value = array[0];
				string json = text.Substring(array[0].Length + 1);
				bool flag = false;
				GA_Submit.CategoryType type = GA_Submit.CategoryType.GA_User;
				foreach (KeyValuePair<GA_Submit.CategoryType, string> category in GA_Submit.Categories)
				{
					if (category.Key.ToString().Equals(value))
					{
						type = category.Key;
						flag = true;
					}
				}
				if (!flag)
				{
					continue;
				}
				ArrayList arrayList = (ArrayList)GA_MiniJSON.JsonDecode(json);
				foreach (Hashtable item2 in arrayList)
				{
					GA_Submit.Item item = new GA_Submit.Item
					{
						Type = type,
						Parameters = item2,
						AddTime = Time.time
					};
					list.Add(item);
				}
			}
			streamReader.Close();
			File.Delete(path);
		}
		return list;
	}
}
