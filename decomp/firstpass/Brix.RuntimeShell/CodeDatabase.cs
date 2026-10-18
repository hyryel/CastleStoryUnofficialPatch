using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.RuntimeShell.Menu;
using UnityEngine;

namespace Brix.RuntimeShell;

public class CodeDatabase : MonoBehaviour
{
	[Serializable]
	public class LanguageBinding
	{
		public string language;

		public string fileExtension;
	}

	public DrawerComponent drawer;

	public LanguageBinding[] languages;

	public string currentPath;

	public CodeEvaluator evaluator;

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		IEnumerable<Entry> entries = GetEntries();
		drawer.SetEntries(entries);
		UIDrawer uIDrawer = drawer as UIDrawer;
		if (uIDrawer != null && uIDrawer.uiPathText != null)
		{
			string text = Application.dataPath.Replace("\\", "/").Replace("/Assets", "/");
			uIDrawer.uiPathText.text = currentPath.Replace("\\", "/");
			uIDrawer.uiPathText.text = ((!(text != string.Empty)) ? uIDrawer.uiPathText.text : uIDrawer.uiPathText.text.Replace(text, string.Empty));
		}
	}

	private IEnumerable<Entry> GetEntries()
	{
		yield return GetDirEntry(new DirectoryInfo(currentPath).Parent.FullName);
		string[] directories = Directory.GetDirectories(currentPath);
		foreach (string dir in directories)
		{
			yield return GetDirEntry(dir);
		}
		string[] files = Directory.GetFiles(currentPath);
		foreach (string file in files)
		{
			yield return GetFileEntry(file);
		}
	}

	private Entry GetDirEntry(string path)
	{
		Entry entry = new Entry();
		entry.type = EntryType.Folder;
		entry.text = new DirectoryInfo(path).Name;
		entry.onClick = delegate
		{
			Navigate(path);
		};
		return entry;
	}

	private Entry GetFileEntry(string path)
	{
		Entry entry = new Entry();
		entry.type = EntryType.Item;
		entry.text = new FileInfo(path).Name;
		entry.onClick = delegate
		{
			EvalFile(path);
		};
		return entry;
	}

	private void Navigate(string path)
	{
		currentPath = path;
		Refresh();
	}

	private void EvalFile(string path)
	{
		string language = (from binding in languages
			where path.ToLower().EndsWith(binding.fileExtension.ToLower())
			select binding.language).FirstOrDefault();
		string code = File.ReadAllText(path);
		evaluator.SendEvalRequest(code, language);
	}
}
