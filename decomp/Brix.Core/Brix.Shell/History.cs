using System;

namespace Brix.Shell;

[Serializable]
public class History
{
	public Command[] prevCommands = new Command[2]
	{
		new Command(),
		new Command()
	};

	public int prevFocus = 0;

	private Command GetCurrent()
	{
		return prevCommands[prevFocus];
	}

	private Command GetLast()
	{
		return prevCommands[prevCommands.Length - 2];
	}

	private bool IsCurrent(string language, string trimmedCode)
	{
		Command current = GetCurrent();
		return language == current.language && trimmedCode == current.trimmedCode;
	}

	private bool IsLast(string language, string trimmedCode)
	{
		Command last = GetLast();
		return language == last.language && trimmedCode == last.trimmedCode;
	}

	public Command RecallPrevious(string language, string trimmedCode)
	{
		if ((prevFocus > 0 && IsCurrent(language, trimmedCode)) || prevFocus == prevCommands.Length - 1)
		{
			prevFocus--;
		}
		return GetCurrent();
	}

	public Command RecallNext(string language, string trimmedCode)
	{
		if ((prevFocus < prevCommands.Length - 1 && IsCurrent(language, trimmedCode)) || prevFocus == 0)
		{
			prevFocus++;
		}
		return GetCurrent();
	}

	public void RecordCurrent(string language, string trimmedCode)
	{
		if (!IsLast(language, trimmedCode))
		{
			Command command = new Command
			{
				language = language,
				trimmedCode = trimmedCode
			};
			prevCommands[prevCommands.Length - 1] = command;
			Array.Resize(ref prevCommands, prevCommands.Length + 1);
			prevCommands[prevCommands.Length - 1] = new Command();
			prevFocus = prevCommands.Length - 1;
		}
	}

	public void ClearRecords()
	{
		prevCommands = new Command[2]
		{
			new Command(),
			new Command()
		};
		prevFocus = 0;
	}
}
