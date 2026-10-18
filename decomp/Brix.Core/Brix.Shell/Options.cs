using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Shell;

[Serializable]
public class Options
{
	public bool showImportOptions = false;

	public List<string> imports = new List<string>();

	public string newImport = "";

	public bool showDefineOptions = false;

	public List<string> defines = new List<string>();

	public string newDefine = "";

	public bool showPrintOptions = true;

	public bool printNulls = true;

	public bool printErrors = false;

	public bool printArrays = true;

	public bool splitWithNewlines = true;

	public bool richTextOutput = false;

	public bool showLayoutOptions = false;

	public bool enableQuickRefs = true;

	public bool enableBoo = false;

	public bool enableLua = false;

	public bool enableKeyboardShortcuts = true;

	public bool enableLanguageRecall = true;

	public bool drawOptions = false;

	public bool drawCodeDatabase = false;

	public bool drawHistory = false;

	public bool quickRefsWriteMode = false;

	public Vector2 codeInputScroll = Vector2.zero;

	public Vector2 codeOutputScroll = Vector2.zero;

	public Vector2 codeDatabaseScroll = Vector2.zero;

	public Vector2 codeHistoryScroll = Vector2.zero;
}
