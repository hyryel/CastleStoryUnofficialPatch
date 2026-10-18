using System;
using Brix.Lua;
using Brix.UI.Builder.Lui;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class LuaMenu : BaseMenu
{
	protected override bool LoadOnInit => false;

	protected override bool LoadOnOpen => HasLuaBuilderFlag(BuilderFlag.LoadOnOpen);

	protected override bool ReloadOnOpen => HasLuaBuilderFlag(BuilderFlag.ReloadOnOpen);

	protected override bool ReloadOnReopen => HasLuaBuilderFlag(BuilderFlag.ReloadOnReopen);

	protected override bool OpenOnInit => false;

	protected override bool OpenOnLoad => HasLuaBuilderFlag(BuilderFlag.OpenOnLoad);

	protected override bool OpenOnReload => HasLuaBuilderFlag(BuilderFlag.OpenOnReload);

	protected override bool ReopenOnReload => HasLuaBuilderFlag(BuilderFlag.ReopenOnReload);

	protected override bool UnloadOnClose => HasLuaBuilderFlag(BuilderFlag.UnloadOnClose);

	protected override bool OpenedByParentMenu => HasLuaBuilderFlag(BuilderFlag.OpenedByParentMenu);

	protected override bool OpensSubMenus => HasLuaBuilderFlag(BuilderFlag.OpensChildMenus);

	protected override bool LuaDriven => true;

	public LuaMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static LuaMenu New(Func<RectTransform> getParent)
	{
		return new LuaMenu(getParent);
	}

	protected override void OpenSelf()
	{
		base.OpenSelf();
		if (FlagClosure.ContainsKey(BuilderFlag.OnOpen) && FlagClosure[BuilderFlag.OnOpen] != null)
		{
			FlagClosure[BuilderFlag.OnOpen].Call();
		}
	}

	protected override void BeforeCloseAll()
	{
		base.BeforeCloseAll();
		if (FlagClosure.ContainsKey(BuilderFlag.OnClose) && FlagClosure[BuilderFlag.OnClose] != null)
		{
			FlagClosure[BuilderFlag.OnClose].Call();
		}
	}

	protected override bool GetMenuSkipFlag(IMenu baseMenu)
	{
		return !baseMenu.Loaded || base.GetMenuSkipFlag(baseMenu);
	}

	public override bool GetToggleSkipFlag()
	{
		return !base.Loaded || base.GetToggleSkipFlag() || !OpenedByParentMenu;
	}

	protected override void ReloadSelf()
	{
		if (base.LuaMenuTableReader != null)
		{
			ReadMenuTable(base.LuaTable, base.LuaMenuTableReader);
		}
		else
		{
			ReadMenuTable(base.LuaTable);
		}
		ScanMenuTable(base.LuaTable, asSubMenu: false, doApply: true, doLog: false);
		base.ReloadSelf();
	}

	public static DynValue ReadMenuTable(out Script outscript, string path, Action<Table, Script> setGlobalContext = null)
	{
		DynValue result = LuaLoader.Load(out outscript, path, delegate(Script script)
		{
			LuaLibs.BigLib(script);
			if (setGlobalContext != null)
			{
				setGlobalContext(script.Globals, script);
			}
		}, null);
		Table table = ((result.Type != DataType.Tuple) ? result.Table : result.Tuple[0].Table);
		if (!string.IsNullOrEmpty(path))
		{
			table.Set("Path", DynValue.NewString(path));
		}
		return result;
	}

	public static Table ReadMenuTable(Script liveScript, string path)
	{
		Table table = liveScript.DoFile(path).Table;
		if (!string.IsNullOrEmpty(path))
		{
			table.Set("Path", DynValue.NewString(path));
		}
		return table;
	}

	public static void ReadMenuTable(Table tSrcMenu)
	{
		Closure function = tSrcMenu.OwnerScript.LoadFile(tSrcMenu.Get("Path").String).Function;
		Table table = function.Call(tSrcMenu.Get("data")).Table;
		CopyMenuTable(tSrcMenu, table);
	}

	public static void ReadMenuTable(Table tSrcMenu, Closure menuTableReader)
	{
		Table table = menuTableReader.Call(tSrcMenu.Get("data")).Table;
		CopyMenuTable(tSrcMenu, table);
	}

	public static IMenu LoadMenuTable(Table tMenu, Func<RectTransform> getParent, LuaHook onLoadedHook, string path, bool asSubMenu, bool doApply, bool doLog)
	{
		if (!string.IsNullOrEmpty(path))
		{
			tMenu.Set("Path", DynValue.NewString(path));
		}
		IMenu menu = ScanMenuTable(tMenu, asSubMenu, doApply, doLog);
		menu.GetParent = getParent;
		menu.OnLoadedHook = onLoadedHook;
		menu.Load();
		return menu;
	}

	public static IMenu LoadMenuTable(Closure menuTableReader, Func<RectTransform> getParent, LuaHook onLoadedHook, bool asSubMenu, bool doApply, bool doLog)
	{
		Table table = menuTableReader.Call().Table;
		IMenu menu = ScanMenuTable(table, asSubMenu, doApply, doLog);
		menu.GetParent = getParent;
		menu.OnLoadedHook = onLoadedHook;
		menu.LuaMenuTableReader = menuTableReader;
		menu.Load();
		return menu;
	}

	public static IMenu ScanMenuTable(Table tMenu, bool asSubMenu, bool doApply, bool doLog)
	{
		LuiBuilder.DoLog = doLog;
		LuiBuilder.DoApply = doApply;
		LuiBuilder.ScanAt.Root(tMenu, asSubMenu);
		LuiBuilder.DoApply = false;
		LuiBuilder.DoLog = false;
		return LuiBuilder.MapHelper.GetMenu(tMenu);
	}

	public static void CopyMenuTable(Table tMenu, Table tOtherMenu)
	{
		tOtherMenu.Set("Open", tMenu.Get("Open"));
		tOtherMenu.Set("Close", tMenu.Get("Close"));
		tOtherMenu.Set("Reopen", tMenu.Get("Reopen"));
		tOtherMenu.Set("Reload", tMenu.Get("Reload"));
		tOtherMenu.Set("Refresh", tMenu.Get("Refresh"));
		tMenu.Set("Panels", tOtherMenu.Get("Panels"));
		tMenu.Set("Menus", tOtherMenu.Get("Menus"));
		tMenu.Set("Connections", tOtherMenu.Get("Connections"));
		tMenu.Set("Flags", tOtherMenu.Get("Flags"));
	}

	protected override PanelHandle AddPanels()
	{
		PanelHandle result = base.AddPanels();
		foreach (Table rootLuaPanel in RootLuaPanels)
		{
			result.AddPanel(LuiBuilder.MapHelper.GetPanel(rootLuaPanel));
		}
		return result;
	}

	protected override MenuHandle AddMenus()
	{
		MenuHandle result = base.AddMenus();
		foreach (IMenu luaMenu in LuaMenus)
		{
			result.AddMenu(luaMenu);
		}
		return result;
	}

	protected override HookConnectionHandle AddHookConnections()
	{
		HookConnectionHandle result = base.AddHookConnections();
		foreach (HookConnection luaConnection in LuaConnections)
		{
			result.AddHookConnection(luaConnection);
		}
		return result;
	}
}
