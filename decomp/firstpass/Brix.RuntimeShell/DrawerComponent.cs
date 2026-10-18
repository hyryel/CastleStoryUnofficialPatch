using System.Collections.Generic;
using UnityEngine;

namespace Brix.RuntimeShell;

public abstract class DrawerComponent<Entry> : MonoBehaviour, Drawer<Entry>
{
	public abstract void Clear();

	public abstract void AddEntry(Entry entry);

	public abstract void Refresh();

	public void SetEntries(IEnumerable<Entry> entries)
	{
		Clear();
		foreach (Entry entry in entries)
		{
			AddEntry(entry);
		}
		Refresh();
	}
}
