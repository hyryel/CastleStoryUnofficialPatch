using System;
using System.Collections.Generic;
using Brix.Utils.Logging;
using Brix.Utils.Unity;
using UnityEngine;

[Serializable]
public class Plug
{
	private static BrixLogger logger = new BrixLogger(typeof(Plug).FullName);

	public string name = string.Empty;

	[EditorComponentPopup("")]
	public Component component;

	public static void ResetPlugCache(IPlug iPlug)
	{
		iPlug.PlugMap.Clear();
		foreach (Plug plug in iPlug.Plugs)
		{
			Type type = plug.component.GetType();
			if (!iPlug.PlugMap.ContainsKey(type))
			{
				iPlug.PlugMap.Add(type, new Dictionary<string, Plug>());
			}
			string key = plug.name;
			Dictionary<string, Plug> dictionary = iPlug.PlugMap[type];
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, plug);
				continue;
			}
			logger.Warn("Duplicate plug detected\nplug index == " + iPlug.Plugs.IndexOf(plug) + " vs " + iPlug.Plugs.IndexOf(dictionary[key]));
		}
	}

	public static T GetPlugComponent<T>(IPlug iPlug, string targetPlugName) where T : Component
	{
		Type typeFromHandle = typeof(T);
		foreach (Type key in iPlug.PlugMap.Keys)
		{
			if (key != typeFromHandle)
			{
				continue;
			}
			Dictionary<string, Plug> dictionary = iPlug.PlugMap[key];
			foreach (string key2 in dictionary.Keys)
			{
				if (key2 != targetPlugName)
				{
					continue;
				}
				return dictionary[key2].component as T;
			}
		}
		return (T)null;
	}

	public static bool TryGetPlugComponent<T>(IPlug iPlug, string targetPlugName, out T outComponent) where T : Component
	{
		return (outComponent = iPlug.GetPlugComponent<T>(targetPlugName)) != null;
	}
}
