using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.RuntimeShell.Menu;

public class UIDrawer : DrawerComponent
{
	public GameObject itemPrefab;

	public GameObject folderPrefab;

	public Transform uiParent;

	public Text uiPathText;

	public GameObject uiPathGo;

	private List<Entry> entries = new List<Entry>();

	public override void Clear()
	{
		entries.Clear();
	}

	public override void AddEntry(Entry entry)
	{
		entries.Add(entry);
	}

	public override void Refresh()
	{
		string empty = string.Empty;
		empty = ((!(uiPathText != null)) ? empty : uiPathText.text);
		Transform[] array = (from Transform child in uiParent
			select (child)).ToArray();
		Transform[] array2 = array;
		foreach (Transform transform in array2)
		{
			if (transform.gameObject.activeSelf && (!(uiPathGo != null) || !(transform.gameObject == uiPathGo)))
			{
				Object.Destroy(transform.gameObject);
			}
		}
		bool flag = true;
		int num2 = 0;
		foreach (Entry entry in entries)
		{
			if (entry.text == "Shell")
			{
				flag = false;
				continue;
			}
			GameObject original = ((entry.type != EntryType.Item) ? folderPrefab : itemPrefab);
			GameObject gameObject = Object.Instantiate(original);
			gameObject.name = "Listing[" + num2++ + "]: " + entry.type;
			gameObject.transform.SetParent(uiParent);
			gameObject.SetActive(value: true);
			Text componentInChildren = gameObject.GetComponentInChildren<Text>();
			if (componentInChildren != null)
			{
				componentInChildren.text = entry.text;
			}
			if (componentInChildren != null)
			{
				componentInChildren.text = ((entry.type != EntryType.Item) ? ("/" + componentInChildren.text + string.Empty) : componentInChildren.text);
			}
			if (componentInChildren != null)
			{
				componentInChildren.text = ((!flag) ? componentInChildren.text : "/..");
			}
			Button componentInChildren2 = gameObject.GetComponentInChildren<Button>();
			if (componentInChildren2 != null)
			{
				componentInChildren2.onClick.AddListener(entry.onClick.Invoke);
			}
			if (entry.type == EntryType.Folder)
			{
				flag = false;
			}
		}
	}
}
