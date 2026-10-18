using System;

namespace Brix.RuntimeShell.Menu;

[Serializable]
public class Entry
{
	public EntryType type;

	public string text;

	public Action onClick;
}
