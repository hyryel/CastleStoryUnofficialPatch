using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Compile;
using UnityEngine;

namespace Brix.Shell;

public class Drawer
{
	private class Profile
	{
		public string name;

		public string fullname;

		public Color color;

		public Func<string, AbstractContext, object> eval;
	}

	private class Colors
	{
		public class QuickRefs
		{
			public static Color Unassigned = new Color(0f, 0f, 0f);

			public static Color Assigned = new Color(1f, 0.5f, 0f);

			public static Color Hovered = new Color(1f, 0.7f, 0.7f);
		}

		public class CodeDatabase
		{
			public static Color Hovered = new Color(0.8f, 1f, 0.7f);
		}

		public static Color JS = new Color(0f, 0.5f, 1f);

		public static Color CS = new Color(0.9f, 0.8f, 0f);

		public static Color Boo = new Color(0.6f, 0.3f, 0.6f);

		public static Color Lua = new Color(0.6f, 0.8f, 0.6f);

		public static Color Result = new Color(0.75f, 1f, 0.5f);

		public static Color Error = new Color(1f, 0.75f, 0.5f);

		public static Color Tooltip = new Color(1f, 0.8f, 0.4f);

		public static Color InputPreview = new Color(1f, 0.7f, 0.7f);
	}

	public Data data;

	public Layout layout;

	private Profile JS = new Profile
	{
		name = "JS",
		fullname = "UnityScript",
		color = Colors.JS,
		eval = (string s, AbstractContext c) => Brix.Compile.JS.Eval(new JS.Context(c), s)
	};

	private Profile CS = new Profile
	{
		name = "C#",
		fullname = "Mono.CSharp",
		color = Colors.CS,
		eval = (string s, AbstractContext c) => Brix.Compile.CS.Eval(new CS.Context(c), s)
	};

	private Profile Boo = new Profile
	{
		name = "Boo",
		fullname = "Boo",
		color = Colors.Boo,
		eval = (string s, AbstractContext c) => Brix.Compile.Boo.Eval(new Brix.Compile.Boo.Context(c), s)
	};

	private Profile Lua = new Profile
	{
		name = "Lua",
		fullname = "Lua",
		color = Colors.Lua,
		eval = (string s, AbstractContext c) => Brix.Compile.Lua.Eval(new Lua.Context(c), s)
	};

	private Profile profile = null;

	private bool formatOutput = true;

	private bool control = false;

	private bool alt = false;

	private bool shift = false;

	private string tooltip = "";

	private string inputPreview = "";

	private Rect hoveredRect;

	private Vector2 mousePosition;

	private int optionsTab = 0;

	private Vector2 databaseMouse;

	private Vector2 historyMouse;

	private Queue<Action> guiTaskQueue = new Queue<Action>();

	private string newline = Environment.NewLine;

	private void SwitchProfile()
	{
		if (data.options.enableBoo && data.options.enableLua)
		{
			profile = ((profile == JS) ? CS : ((profile == CS) ? Boo : ((profile == Boo) ? Lua : JS)));
		}
		else if (data.options.enableBoo)
		{
			profile = ((profile == JS) ? CS : ((profile == CS) ? Boo : JS));
		}
		else if (data.options.enableLua)
		{
			profile = ((profile == JS) ? CS : ((profile == CS) ? Lua : JS));
		}
		else
		{
			profile = ((profile == JS) ? CS : JS);
		}
		data.language = profile.name;
		RefreshInput();
	}

	private Profile GetProfileFromLanguage(string language)
	{
		if (language == JS.name)
		{
			return JS;
		}
		if (language == CS.name)
		{
			return CS;
		}
		if (language == Boo.name)
		{
			return Boo;
		}
		if (language == Lua.name)
		{
			return Lua;
		}
		return null;
	}

	public void OnGUI()
	{
		profile = GetProfileFromLanguage(data.language);
		if (profile == null)
		{
			SwitchProfile();
		}
		HandleEvents();
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		DrawProfileChoices();
		DrawToolbar();
		GUILayout.EndHorizontal();
		DrawInputWithPreview();
		DrawOutput();
		DrawTooltip();
		Options options = data.options;
		if (options.enableQuickRefs)
		{
			GUILayout.BeginHorizontal();
			DrawQuickRefs();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		if (options.drawOptions)
		{
			options.drawOptions = layout.Window("Options", DrawOptions);
		}
		if (options.drawCodeDatabase)
		{
			options.drawCodeDatabase = layout.Window("CodeDatabase", DrawCodeDatabase);
		}
		if (options.drawHistory)
		{
			options.drawHistory = layout.Window("History", DrawHistory);
		}
		if (Event.current != null && Event.current.type == EventType.Repaint && tooltip != GUI.tooltip)
		{
			tooltip = GUI.tooltip;
		}
		layout.Repaint();
	}

	private void DrawProfileChoices()
	{
		Color color = GUI.color;
		GUI.color = profile.color;
		bool enableKeyboardShortcuts = data.options.enableKeyboardShortcuts;
		if (GUILayout.Button(new GUIContent(profile.name, "Change Language" + (enableKeyboardShortcuts ? " (Ctrl+Space)" : "") + "\nSelected: " + profile.fullname)))
		{
			SwitchProfile();
		}
		GUI.color = color;
	}

	private void DrawToolbar()
	{
		bool enableKeyboardShortcuts = data.options.enableKeyboardShortcuts;
		if (GUILayout.Button(new GUIContent("←", "Recall Previous Command" + (enableKeyboardShortcuts ? " (Ctrl+Up)" : ""))))
		{
			RecallPrevious();
		}
		if (GUILayout.Button(new GUIContent("→", "Recall Next Command" + (enableKeyboardShortcuts ? " (Ctrl+Down)" : ""))))
		{
			RecallNext();
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(new GUIContent("$", "Evaluate Input Now" + (enableKeyboardShortcuts ? " (Enter)" : ""))))
		{
			EvalCurrent();
		}
		if (GUILayout.Button(new GUIContent("✂", "Copy Last Output")))
		{
			CopyLastOutput();
		}
		if (GUILayout.Button(new GUIContent("Ø", "Clear Output" + (enableKeyboardShortcuts ? " (Ctrl+Backspace)" : ""))))
		{
			ClearOutput();
		}
		if (GUILayout.Button(new GUIContent("{ }", "Clear History")))
		{
			ClearHistory();
		}
		if (GUILayout.Button(new GUIContent("❄", "Code History Window")))
		{
			data.options.drawHistory = true;
		}
		if (GUILayout.Button(new GUIContent("★", "Code Database Window")))
		{
			data.options.drawCodeDatabase = true;
		}
		if (GUILayout.Button(new GUIContent("☰", "Options Window"), "Button", GUILayout.ExpandWidth(expand: false), GUILayout.ExpandHeight(expand: false)))
		{
			data.options.drawOptions = true;
		}
	}

	private void DrawInput()
	{
		Options options = data.options;
		Rect position = layout.GetPosition();
		options.codeInputScroll = GUILayout.BeginScrollView(options.codeInputScroll, GUILayout.ExpandHeight(expand: true), GUILayout.MinHeight(position.height / 3f));
		GUI.SetNextControlName("Shell.CodeInput");
		data.input = layout.CodeInput(data.input);
		GUILayout.EndScrollView();
	}

	private void DrawOutput()
	{
		Options options = data.options;
		options.codeOutputScroll = GUILayout.BeginScrollView(options.codeOutputScroll, GUILayout.ExpandHeight(expand: true));
		GUI.SetNextControlName("Shell.CodeOutput");
		string s = (formatOutput ? data.richOutput : data.output);
		layout.CodeOutput(s, formatOutput);
		GUILayout.EndScrollView();
	}

	private void DrawInputWithPreview()
	{
		if (inputPreview != "")
		{
			Color color = GUI.color;
			GUI.color = Color.clear;
			DrawInput();
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect = GetResizedRect(lastRect, new Vector2(lastRect.width - 8f, lastRect.height - 4f));
			GUI.color = Colors.InputPreview;
			GUI.TextArea(lastRect, inputPreview);
			GUI.color = color;
		}
		else
		{
			DrawInput();
		}
	}

	private bool SetInputPreviewIfHovered(Vector2 mousePosition, Func<string> getPreview, bool autoRefresh = false)
	{
		return SetInputPreviewIfHovered(GUILayoutUtility.GetLastRect(), mousePosition, getPreview, autoRefresh);
	}

	private bool SetInputPreviewIfHovered(Rect rect, Vector2 mousePosition, Func<string> getPreview, bool autoRefresh = false)
	{
		if (Event.current == null || Event.current.type != EventType.Repaint)
		{
			return inputPreview != "";
		}
		bool flag = rect.Contains(mousePosition);
		if (flag && (hoveredRect != rect || autoRefresh))
		{
			inputPreview = getPreview();
			hoveredRect = rect;
		}
		else if (!flag && hoveredRect == rect)
		{
			inputPreview = "";
			hoveredRect = default(Rect);
		}
		return flag;
	}

	private void InvokeQuickRef(int choice, bool write)
	{
		QuickRefs quickRefs = data.quickRefs;
		if (write)
		{
			quickRefs.Record(choice, data.language, data.input);
		}
		else if (quickRefs.Assigned(choice))
		{
			Command command = quickRefs.Recall(choice);
			if (data.options.enableLanguageRecall)
			{
				data.language = command.language;
			}
			data.input = command.trimmedCode;
			RefreshInput();
		}
	}

	private void DrawQuickRefs()
	{
		bool enableKeyboardShortcuts = data.options.enableKeyboardShortcuts;
		Options options = data.options;
		QuickRefs quickRefs = data.quickRefs;
		Color color = GUI.color;
		TextAnchor alignment = GUI.skin.label.alignment;
		int num = -1;
		bool write = (control ? alt : options.quickRefsWriteMode);
		options.quickRefsWriteMode = GUILayout.Toggle(options.quickRefsWriteMode, "", GUILayout.ExpandWidth(expand: false));
		for (int i = 0; i < 10; i++)
		{
			int i2 = (i + 1) % 10;
			bool flag = quickRefs.Assigned(i2);
			if (!flag && !write)
			{
				GUI.color = Colors.QuickRefs.Unassigned;
			}
			GUILayout.Box("", GUILayout.ExpandWidth(expand: true));
			Rect lastRect = GUILayoutUtility.GetLastRect();
			if (flag)
			{
				GUI.color = Colors.QuickRefs.Assigned;
			}
			string text = (flag ? ("Recall Input from Slot #" + i2 + (enableKeyboardShortcuts ? (" (Ctrl+" + i2 + ")") : "")) : "");
			string text2 = "Save Input in Slot #" + i2 + (enableKeyboardShortcuts ? (" (Ctrl+Alt+" + i2 + ")") : "");
			GUIContent content = new GUIContent(string.Concat(i2), write ? text2 : text);
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			if (GUI.Button(lastRect, content, write ? "Button" : "Label"))
			{
				num = i2;
			}
			SetInputPreviewIfHovered(mousePosition, () => write ? "" : data.quickRefs.Recall(i2).trimmedCode, autoRefresh: true);
			GUI.skin.label.alignment = alignment;
			GUI.color = color;
		}
		if (num >= 0)
		{
			InvokeQuickRef(num, write);
		}
	}

	private Rect GetResizedRect(Rect rect, Vector2 size)
	{
		float x = size.x;
		float y = size.y;
		return new Rect(rect.x + rect.width / 2f - x / 2f, rect.y + rect.height / 2f - y / 2f, x, y);
	}

	private void DrawTooltip()
	{
		if (tooltip != "")
		{
			Rect rect = new Rect(0f, 0f, layout.GetPosition().width, layout.GetPosition().height);
			Color color = GUI.color;
			Color backgroundColor = GUI.backgroundColor;
			int fontSize = GUI.skin.label.fontSize;
			TextAnchor alignment = GUI.skin.label.alignment;
			GUIContent content = new GUIContent("[" + tooltip + "]");
			GUI.skin.label.fontSize = 18;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			Vector2 vector = GUI.skin.label.CalcSize(content);
			GUI.backgroundColor = Color.black;
			GUI.Box(GetResizedRect(rect, vector + Vector2.one * 50f), "", "TextArea");
			GUI.color = Colors.Tooltip;
			GUI.Label(GetResizedRect(rect, vector), content);
			GUI.color = color;
			GUI.backgroundColor = backgroundColor;
			GUI.skin.label.fontSize = fontSize;
			GUI.skin.label.alignment = alignment;
		}
	}

	private void DrawOptions()
	{
		Options options = data.options;
		string[] array = new string[4] { "Imports", "Defines", "Output", "Layout" };
		optionsTab = GUILayout.Toolbar(optionsTab, array);
		string text = array[optionsTab];
		options.showImportOptions = text == "Imports";
		options.showDefineOptions = text == "Defines";
		options.showPrintOptions = text == "Output";
		options.showLayoutOptions = text == "Layout";
		if (options.showImportOptions)
		{
			layout.BeginIndentedSection();
			options.newImport = layout.DynamicList(options.newImport, options.imports);
			layout.EndIndentedSection();
		}
		if (options.showDefineOptions)
		{
			layout.BeginIndentedSection();
			options.newDefine = layout.DynamicList(options.newDefine, options.defines);
			layout.EndIndentedSection();
		}
		if (options.showPrintOptions)
		{
			layout.BeginIndentedSection();
			options.printNulls = GUILayout.Toggle(options.printNulls, "Print Empty Values");
			options.printErrors = GUILayout.Toggle(options.printErrors, "Print Error Stacktraces");
			options.printArrays = GUILayout.Toggle(options.printArrays, "Expand Enumerables");
			GUI.enabled = options.printArrays;
			options.splitWithNewlines = GUILayout.Toggle(options.splitWithNewlines, "Split with Line Breaks");
			GUI.enabled = true;
			options.richTextOutput = GUILayout.Toggle(options.richTextOutput, "Rich Text Output");
			layout.EndIndentedSection();
		}
		if (options.showLayoutOptions)
		{
			layout.BeginIndentedSection();
			options.enableQuickRefs = GUILayout.Toggle(options.enableQuickRefs, "Enable QuickRefs Toolbar");
			options.enableBoo = GUILayout.Toggle(options.enableBoo, "Enable Boo (Experimental)");
			GUILayout.BeginHorizontal();
			options.enableLua = GUILayout.Toggle(options.enableLua, "Enable Lua");
			if (options.enableLua)
			{
				Brix.Compile.Lua.Target = layout.ObjectField(Brix.Compile.Lua.Target);
			}
			GUILayout.EndHorizontal();
			options.enableKeyboardShortcuts = GUILayout.Toggle(options.enableKeyboardShortcuts, "Enable Keyboard Shortcuts");
			options.enableLanguageRecall = GUILayout.Toggle(options.enableLanguageRecall, "Enable Language Switching on Recall");
			layout.EndIndentedSection();
		}
	}

	private void DrawCodeDatabase()
	{
		if (Event.current != null && Event.current.type == EventType.Repaint)
		{
			databaseMouse = Event.current.mousePosition - Vector2.up * 10f;
		}
		Options options = data.options;
		options.codeDatabaseScroll = GUILayout.BeginScrollView(options.codeDatabaseScroll, GUILayout.ExpandHeight(expand: true));
		GUI.skin.label.richText = true;
		DrawDatabaseDirectoryContent(data.database.Root);
		GUI.skin.label.richText = false;
		GUILayout.EndScrollView();
	}

	private void DrawDatabaseDirectoryContent(DatabaseDirectory dir)
	{
		Color color = GUI.color;
		DatabaseFile[] files = dir.files;
		foreach (DatabaseFile file in files)
		{
			string language = file.language;
			Profile profileFromLanguage = GetProfileFromLanguage(language);
			string text = ColorText("[" + profileFromLanguage.name + "]", profileFromLanguage.color);
			GUIContent content = new GUIContent(text + " " + file.name);
			Rect rect = GUILayoutUtility.GetRect(content, GUI.skin.label);
			if (rect.Contains(databaseMouse))
			{
				GUI.color = Colors.CodeDatabase.Hovered;
			}
			if (GUI.Button(rect, content, "Label"))
			{
				string trimmedCode = file.trimmedCode;
				if (data.options.enableLanguageRecall)
				{
					data.language = language;
				}
				data.input = trimmedCode;
				RefreshInput();
			}
			SetInputPreviewIfHovered(databaseMouse, () => file.trimmedCode);
			GUI.color = color;
		}
		DatabaseDirectory[] directories = dir.directories;
		foreach (DatabaseDirectory databaseDirectory in directories)
		{
			bool expanded = data.database.GetExpanded(databaseDirectory);
			GUIContent content2 = new GUIContent(databaseDirectory.name);
			Rect rect2 = GUILayoutUtility.GetRect(content2, GUI.skin.label);
			if (rect2.Contains(databaseMouse))
			{
				GUI.color = Colors.CodeDatabase.Hovered;
			}
			bool flag = layout.Foldout(rect2, expanded, databaseDirectory.name, "Label");
			GUI.color = color;
			if (expanded != flag)
			{
				data.database.SetExpanded(databaseDirectory, flag);
			}
			if (flag)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(10f);
				GUILayout.BeginVertical();
				DrawDatabaseDirectoryContent(databaseDirectory);
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}
	}

	private void DrawHistory()
	{
		if (Event.current != null && Event.current.type == EventType.Repaint)
		{
			historyMouse = Event.current.mousePosition - Vector2.up * 10f;
		}
		Options options = data.options;
		options.codeHistoryScroll = GUILayout.BeginScrollView(options.codeHistoryScroll, GUILayout.ExpandHeight(expand: true));
		GUI.skin.button.richText = true;
		for (int i = 1; i < data.history.prevCommands.Length - 1; i++)
		{
			Command command = data.history.prevCommands[i];
			Profile profileFromLanguage = GetProfileFromLanguage(command.language);
			string text = ColorText("[" + profileFromLanguage.name + "]", profileFromLanguage.color);
			string text2 = command.trimmedCode;
			if (text2.IndexOf("\n") >= 0)
			{
				text2 = text2.Substring(0, text2.IndexOf("\n"));
			}
			if (text2.Length > 20)
			{
				text2 = text2.Substring(0, 20);
			}
			GUIContent content = new GUIContent(text + " " + text2);
			bool flag = data.history.prevFocus == i;
			if (flag != GUILayout.Toggle(flag, content, "Button"))
			{
				data.history.prevFocus = i;
				data.input = "";
				RecallNext();
			}
			SetInputPreviewIfHovered(historyMouse, () => command.trimmedCode);
		}
		GUI.skin.button.richText = false;
		GUILayout.EndScrollView();
	}

	private void HandleEvents()
	{
		Event current = Event.current;
		if (guiTaskQueue.Count > 0)
		{
			if (current.type == EventType.Layout)
			{
				guiTaskQueue.Dequeue()();
				layout.Repaint();
			}
			return;
		}
		if (current != null)
		{
			control = current.control;
			alt = current.alt;
			shift = current.shift;
			mousePosition = current.mousePosition;
		}
		bool enableKeyboardShortcuts = data.options.enableKeyboardShortcuts;
		if (current != null && current.type == EventType.KeyDown && InputHasFocus() && enableKeyboardShortcuts)
		{
			if (current.keyCode == KeyCode.UpArrow && current.control)
			{
				RecallPrevious();
			}
			if (current.keyCode == KeyCode.DownArrow && current.control)
			{
				RecallNext();
			}
			if (current.keyCode == KeyCode.Return && !current.shift && !current.control && !current.numeric && !current.command)
			{
				EvalCurrent();
			}
			if (current.keyCode == KeyCode.Space && current.control)
			{
				SwitchProfile();
			}
			if (current.keyCode == KeyCode.Backspace && current.control)
			{
				ClearOutput();
			}
		}
		if (current != null && current.type == EventType.KeyUp && InputHasFocus() && enableKeyboardShortcuts && current.keyCode >= KeyCode.Alpha0 && current.keyCode <= KeyCode.Alpha9 && current.control)
		{
			InvokeQuickRef((int)(current.keyCode - 48), current.alt);
		}
		bool flag = !OutputHasFocus();
		if (formatOutput != flag)
		{
			formatOutput = flag;
			RefreshOutput();
		}
	}

	private void RefreshInput()
	{
		string savedInput = data.input;
		int savedFocus = data.history.prevFocus;
		Action item = delegate
		{
			data.input = savedInput;
			data.history.prevFocus = savedFocus;
		};
		if (InputHasFocus())
		{
			guiTaskQueue.Enqueue(FocusOutput);
			guiTaskQueue.Enqueue(item);
			guiTaskQueue.Enqueue(FocusInput);
		}
		else
		{
			guiTaskQueue.Enqueue(item);
		}
	}

	private void RefreshOutput()
	{
		bool savedFormat = formatOutput;
		Action item = delegate
		{
			formatOutput = savedFormat;
		};
		formatOutput = true;
		if (OutputHasFocus())
		{
			FocusInput();
			guiTaskQueue.Enqueue(item);
			guiTaskQueue.Enqueue(FocusOutput);
		}
		else
		{
			guiTaskQueue.Enqueue(item);
		}
	}

	private void FocusInput()
	{
		layout.FocusControl("Shell.CodeInput");
	}

	private void FocusOutput()
	{
		layout.FocusControl("Shell.CodeOutput");
	}

	private bool InputHasFocus()
	{
		return GUI.GetNameOfFocusedControl() == "Shell.CodeInput";
	}

	private bool OutputHasFocus()
	{
		return GUI.GetNameOfFocusedControl() == "Shell.CodeOutput";
	}

	private void RecallPrevious()
	{
		Command command = data.history.RecallPrevious(data.language, data.input);
		if (data.options.enableLanguageRecall)
		{
			data.language = command.language;
		}
		data.input = command.trimmedCode;
		RefreshInput();
	}

	private void RecallNext()
	{
		Command command = data.history.RecallNext(data.language, data.input);
		if (data.options.enableLanguageRecall)
		{
			data.language = command.language;
		}
		data.input = command.trimmedCode;
		RefreshInput();
	}

	private void ClearOutput()
	{
		data.output = (data.richOutput = "");
		RefreshOutput();
	}

	private void ClearHistory()
	{
		data.history = new History();
	}

	private void CopyLastOutput()
	{
		string lastOutput = data.lastOutput;
		TextEditor textEditor = new TextEditor();
		textEditor.text = lastOutput;
		textEditor.SelectAll();
		textEditor.Copy();
	}

	private void EvalCurrent()
	{
		string text = data.input.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Options options = data.options;
		AbstractContext abstractContext = new AbstractContext();
		abstractContext.Namespaces = options.imports.ToArray();
		abstractContext.Defines = options.defines.ToArray();
		object obj = null;
		string text2 = "[" + profile.name + "] ";
		string text3 = text + newline;
		Data obj2 = data;
		obj2.output = obj2.output + text2 + text3;
		Data obj3 = data;
		obj3.richOutput = obj3.richOutput + ColorText(text2, profile.color) + EscapeText(text3);
		try
		{
			obj = profile.eval(text, abstractContext);
			string text4 = FormatResult(obj);
			Data obj4 = data;
			obj4.output = obj4.output + text4 + newline + newline;
			Data obj5 = data;
			obj5.richOutput = obj5.richOutput + ColorText(EscapeText(text4), Colors.Result) + newline + newline;
			data.lastOutput = text4;
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex);
			string text5 = "Error.";
			if (options.printErrors)
			{
				text5 = text5 + " " + ex;
			}
			Data obj6 = data;
			obj6.output = obj6.output + text5 + newline + newline;
			Data obj7 = data;
			obj7.richOutput = obj7.richOutput + ColorText(text5, Colors.Error) + newline + newline;
		}
		data.history.RecordCurrent(data.language, text);
		data.input = "";
		RefreshInput();
	}

	private string FormatResult(object r)
	{
		Options options = data.options;
		if (r == null)
		{
			return options.printNulls ? "null" : "";
		}
		if ("" == r as string)
		{
			return options.printNulls ? "\"\"" : "";
		}
		if (r is string)
		{
			return (string)r;
		}
		if (options.printArrays && r is IEnumerable)
		{
			IEnumerable<object> source = (r as IEnumerable).Cast<object>();
			IEnumerable<string> source2 = source.Select((object obj) => FormatResult(obj));
			if (options.splitWithNewlines)
			{
				return string.Join("," + newline, source2.ToArray());
			}
			return "{ " + string.Join(", ", source2.ToArray()) + " }";
		}
		return r.ToString();
	}

	private string ColorText(string text, Color32 c)
	{
		return ColorText(text, $"#{c.r:x2}{c.g:x2}{c.b:x2}");
	}

	private string ColorText(string text, string color)
	{
		string text2 = "<color=" + color + ">";
		string text3 = "</color>";
		return text2 + text + text3;
	}

	private string EscapeText(string text)
	{
		if (data.options.richTextOutput)
		{
			return text;
		}
		return text.Replace("<", "<\0").Replace(">", ">\0");
	}
}
