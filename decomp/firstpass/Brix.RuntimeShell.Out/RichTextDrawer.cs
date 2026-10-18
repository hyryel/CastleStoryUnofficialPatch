using System.Linq;
using UnityEngine;

namespace Brix.RuntimeShell.Out;

public abstract class RichTextDrawer : DrawerComponent
{
	public Settings settings;

	[HideInInspector]
	public string output = string.Empty;

	[HideInInspector]
	public string richOutput = string.Empty;

	public override void Clear()
	{
		output = (richOutput = string.Empty);
	}

	public override void AddEntry(Entry entry)
	{
		Settings.LanguageBinding languageBinding = settings.languages.FirstOrDefault((Settings.LanguageBinding binding) => binding.language == entry.language);
		if (languageBinding != null)
		{
			AppendColored("[" + languageBinding.prefix + "]", languageBinding.color);
		}
		else
		{
			Append("[" + entry.language + "]");
		}
		Append(" " + entry.input + "\n");
		if (entry.error != null)
		{
			AppendColored(entry.error.ToString(), settings.errorColor);
		}
		else
		{
			AppendColored((entry.output ?? string.Empty).ToString(), settings.outputColor);
		}
		Append("\n\n");
	}

	private void Append(string text)
	{
		output += text;
		richOutput += text;
	}

	private void AppendColored(string text, Color32 c)
	{
		output += text;
		string text2 = richOutput;
		richOutput = text2 + "<color=" + $"#{c.r:x2}{c.g:x2}{c.b:x2}" + ">" + text + "</color>";
	}
}
