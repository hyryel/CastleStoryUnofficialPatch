using System;

namespace Brix.Shell;

[Serializable]
public class QuickRefs
{
	public Command[] slots = new Command[10];

	public Command Recall(int i)
	{
		if (slots[i] == null)
		{
			slots[i] = new Command();
		}
		return slots[i];
	}

	public void Record(int i, string language, string trimmedCode)
	{
		slots[i] = new Command
		{
			language = language,
			trimmedCode = trimmedCode
		};
	}

	public bool Assigned(int i)
	{
		return slots[i] != null && !string.IsNullOrEmpty(slots[i].trimmedCode);
	}
}
