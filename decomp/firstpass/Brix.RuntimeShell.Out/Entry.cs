using System;

namespace Brix.RuntimeShell.Out;

[Serializable]
public class Entry
{
	public string language;

	public string input;

	public object output;

	public Exception error;
}
