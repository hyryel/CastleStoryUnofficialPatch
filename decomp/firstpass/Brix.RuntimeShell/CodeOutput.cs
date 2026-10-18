using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.RuntimeShell;

public class CodeOutput : DrawerComponent<CodeOutput.Entry>
{
	[Serializable]
	public class Entry
	{
		public string language;

		public string input;

		public object output;

		public Exception error;
	}

	[Serializable]
	public class LanguageBinding
	{
		public string language;

		public string prefix;

		public Color color;
	}

	public LanguageBinding[] languages;

	public Color outputColor;

	public Color errorColor;

	[HideInInspector]
	public string output = string.Empty;

	[HideInInspector]
	public string richOutput = string.Empty;

	public InputField uiText;

	[HideInInspector]
	public Vector2 scrollResults;

	public override void Clear()
	{
		output = (richOutput = string.Empty);
	}

	public override void AddEntry(Entry entry)
	{
		LanguageBinding languageBinding = languages.FirstOrDefault((LanguageBinding binding) => binding.language == entry.language);
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
			AppendColored(entry.error.ToString(), errorColor);
		}
		else
		{
			AppendColored((entry.output ?? string.Empty).ToString(), outputColor);
		}
		Append("\n\n");
	}

	public override void Refresh()
	{
		uiText.text = richOutput;
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

	public void DrawGUI(params GUILayoutOption[] options)
	{
		GUILayout.BeginVertical(options);
		scrollResults = GUILayout.BeginScrollView(scrollResults);
		string text = output;
		if (GUI.GetNameOfFocusedControl() != "RuntimeShell.Results")
		{
			GUI.skin.textArea.richText = true;
			text = richOutput;
		}
		GUI.SetNextControlName("RuntimeShell.Results");
		GUILayout.TextArea(text, GUILayout.ExpandHeight(expand: true));
		GUILayout.EndScrollView();
		if (GUILayout.Button("Clear"))
		{
			output = (richOutput = string.Empty);
		}
		GUILayout.EndVertical();
	}
}
