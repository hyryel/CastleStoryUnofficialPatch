using System.Collections.Generic;
using UnityEngine;

namespace Brix.Input;

public class ControlContext : MonoBehaviour
{
	private static ControlContext instance;

	private Dictionary<string, List<string>> _links = new Dictionary<string, List<string>>();

	private Dictionary<string, List<string>> _currentContexts = new Dictionary<string, List<string>>();

	public static void CreateLink(string linkName, string category)
	{
		if (instance != null)
		{
			if (!instance._links.TryGetValue(linkName, out var value))
			{
				value = new List<string>();
				instance._links.Add(linkName, value);
			}
			value.Add(category);
		}
	}

	public static void SetContext(string linkname, bool focus)
	{
		if (!(instance != null))
		{
			return;
		}
		foreach (string item in instance._links[linkname])
		{
			List<string> value;
			if (focus)
			{
				if (!instance._currentContexts.TryGetValue(item, out value))
				{
					value = new List<string>();
					instance._currentContexts.Add(item, value);
				}
				if (!value.Contains(linkname))
				{
					value.Add(linkname);
				}
			}
			else if (instance._currentContexts.TryGetValue(item, out value) && value.Contains(linkname))
			{
				value.Remove(linkname);
			}
			if (instance._currentContexts.ContainsKey(item))
			{
				PickingUtility.Player.controllers.maps.SetMapsEnabled(instance._currentContexts[item].Count == 0, item);
			}
		}
	}

	public static bool ContextIsActive(string linkname)
	{
		if (instance != null && instance._links.ContainsKey(linkname))
		{
			foreach (string item in instance._links[linkname])
			{
				if (instance._currentContexts.TryGetValue(item, out var value) && value.Contains(linkname))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Awake()
	{
		instance = this;
	}
}
