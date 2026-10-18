using System.Collections.Generic;
using UnityEngine;

namespace Brix.RuntimeShell.Menu;

public class GUIDrawer : DrawerComponent
{
	[HideInInspector]
	public List<Entry> entries = new List<Entry>();

	[HideInInspector]
	public Vector2 scrollDatabase;

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
	}

	public void DrawGUI(params GUILayoutOption[] options)
	{
		Entry entry = null;
		GUILayout.BeginVertical(options);
		scrollDatabase = GUILayout.BeginScrollView(scrollDatabase);
		foreach (Entry entry2 in entries)
		{
			if (GUILayout.Button(entry2.text))
			{
				entry = entry2;
			}
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		entry?.onClick();
	}
}
