using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Lua;
using Brix.UI.Builder.Menu;
using Brix.UI.Builder.Panel;
using Brix.UI.Builder.Widget;
using Brix.Utils;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Slinq;

namespace Brix.UI.Builder.Lui;

public static class LuiBuilder
{
	public abstract class BaseLuiBuilder<TLui> where TLui : BaseLui
	{
		public static BrixLogger logger = new BrixLogger(typeof(BaseLuiBuilder<TLui>).FullName);

		protected bool Debug;

		private static DynValue _New = DynValue.NewCallback(New);

		private static DynValue _Get = DynValue.NewCallback(Get);

		public Script OwnerScript { get; private set; }

		public string TypeName { get; private set; }

		protected string DebugName { get; private set; }

		public Table T { get; private set; }

		protected string Path { get; private set; }

		protected BaseLuiBuilder(Script script, string typeName)
		{
			OwnerScript = script;
			TypeName = typeName;
			T = new Table(script);
			T.Set("Name", DynValue.NewString(TypeName));
			T.Set("_debugName", DynValue.NewString(DebugName));
			T.Set("__self", UserData.CreateWithDescriptor(new LuaMenuUtility.BuilderWrapper
			{
				wrap = this
			}, LuaMenuUtility.builderWrapperDescriptor));
			T.Set("New", _New);
			T.Set("Get", _Get);
		}

		protected abstract TLui DoNewLui();

		public TLui NewLui(string debugName, string path = null)
		{
			DebugName = debugName;
			Path = path;
			TLui result = DoNewLui();
			LogNew(result.T);
			return result;
		}

		private static DynValue New(ScriptExecutionContext context, CallbackArguments args)
		{
			Table table = args.AsTable(0, "New");
			string text = args.AsString(1, "New", allowNil: true);
			text = ((!string.IsNullOrEmpty(text)) ? text : string.Empty);
			TLui val = table.Get("__self").ToObject<LuaMenuUtility.BuilderWrapper>().As<BaseLuiBuilder<TLui>>()
				.NewLui(text);
			return val.D;
		}

		private static DynValue Get(ScriptExecutionContext context, CallbackArguments args)
		{
			Table table = args.AsTable(0, "Get");
			string text = args.AsString(1, "Get", allowNil: true);
			text = ((!string.IsNullOrEmpty(text)) ? text : string.Empty);
			string path = args.AsString(2, "Get");
			TLui val = table.Get("__self").ToObject<LuaMenuUtility.BuilderWrapper>().As<BaseLuiBuilder<TLui>>()
				.NewLui(text, path);
			return val.D;
		}

		public static void PasteTo(Table src, Table dst, bool panelsOnly = false)
		{
			if (panelsOnly)
			{
				dst.Set("Panels", src.Get("Panels"));
				return;
			}
			foreach (TablePair pair in src.Pairs)
			{
				dst.Set(pair.Key, pair.Value);
			}
		}

		protected void LogNew(Table t)
		{
			LogTypeId(t, "New");
		}

		protected void LogTypeId(Table t, string methodName)
		{
			if (Debug)
			{
				logger.Info(string.Empty + TypeName + "<size=8>#" + t.ReferenceID + "</size>." + methodName + "()\n");
			}
		}
	}

	public class MenuLuiBuilder : BaseLuiBuilder<MenuLui>
	{
		public MenuLuiBuilder(Script script, string typeName)
			: base(script, typeName)
		{
		}

		protected override MenuLui DoNewLui()
		{
			return (base.Path != null) ? new MenuLui(LuaMenu.ReadMenuTable(base.OwnerScript, base.Path)) : new MenuLui(base.OwnerScript, base.TypeName, base.DebugName);
		}
	}

	public class LayoutPanelLuiBuilder : BaseLuiBuilder<LayoutPanelLui>
	{
		public LayoutPanelLuiBuilder(Script script, string typeName)
			: base(script, typeName)
		{
		}

		protected override LayoutPanelLui DoNewLui()
		{
			return new LayoutPanelLui(base.OwnerScript, base.TypeName, base.DebugName);
		}
	}

	public class PanelLuiBuilder : BaseLuiBuilder<PanelLui>
	{
		public PanelLuiBuilder(Script script, string typeName)
			: base(script, typeName)
		{
		}

		protected override PanelLui DoNewLui()
		{
			return new PanelLui(base.OwnerScript, base.TypeName, base.DebugName);
		}
	}

	public class WidgetLuiBuilder : BaseLuiBuilder<WidgetLui>
	{
		public WidgetLuiBuilder(Script script, string typeName)
			: base(script, typeName)
		{
		}

		protected override WidgetLui DoNewLui()
		{
			return new WidgetLui(base.OwnerScript, base.TypeName, base.DebugName);
		}
	}

	public class HandleLuiBuilder : BaseLuiBuilder<HandleLui>
	{
		public HandleLuiBuilder(Script script, string typeName)
			: base(script, typeName)
		{
		}

		protected override HandleLui DoNewLui()
		{
			return new HandleLui(base.OwnerScript, base.TypeName, base.DebugName);
		}
	}

	public class ConnectionLuiBuilder : BaseLuiBuilder<ConnectionLui>
	{
		public ConnectionLuiBuilder(Script script, string typeName)
			: base(script, typeName)
		{
		}

		protected override ConnectionLui DoNewLui()
		{
			return new ConnectionLui(base.OwnerScript, base.TypeName, base.DebugName);
		}
	}

	public abstract class BaseLui : IBaseLui
	{
		private readonly DynValue _dT;

		private readonly DynValue _dId;

		private readonly Table _T;

		private readonly int _Id;

		private readonly DynValue _D;

		public Script OwnerScript { get; private set; }

		public string TypeName { get; private set; }

		public string DebugName { get; private set; }

		public Table T => _T;

		public int Id => _Id;

		public DynValue D => _D;

		protected BaseLui(Table t)
		{
			OwnerScript = t.OwnerScript;
			TypeName = t.Get("Name").String;
			DebugName = t.Get("_debugName").String ?? string.Empty;
			_dT = DynValue.NewTable(OwnerScript);
			_T = _dT.Table;
			_Id = _T.ReferenceID;
			_dId = DynValue.NewNumber(_Id);
			_D = DynValue.NewTuple(_dT, _dId);
			_T.Set("Name", DynValue.NewString(TypeName));
			_T.Set("_debugName", DynValue.NewString(DebugName));
			_T.Set("Id", DynValue.NewNumber(_Id));
		}

		protected BaseLui(Script script, string typeName, string debugName)
		{
			OwnerScript = script;
			TypeName = typeName;
			DebugName = debugName ?? string.Empty;
			_dT = DynValue.NewTable(script);
			_T = _dT.Table;
			_Id = _T.ReferenceID;
			_dId = DynValue.NewNumber(_Id);
			_D = DynValue.NewTuple(_dT, _dId);
			_T.Set("Name", DynValue.NewString(TypeName));
			_T.Set("_debugName", DynValue.NewString(DebugName));
			_T.Set("Id", DynValue.NewNumber(_Id));
		}
	}

	public class MenuLui : BaseLui, IIsMenuLui, IHasPanelsLui, IHasConnectionsLui, IHasBuilderFlagsLui, IHasMenusLui, IBaseLui
	{
		public MenuLui(Script script, string typeName, string debugName)
			: base(script, typeName, debugName)
		{
			this.InitHasPanels();
			this.InitHasConnections();
			this.InitHasFlags();
			this.InitHasMenus();
			this.InitIsMenu();
			this.InitScan();
		}

		public MenuLui(Table t)
			: base(t)
		{
			this.InitHasPanels();
			this.InitHasConnections();
			this.InitHasFlags();
			this.InitIsMenu();
			this.InitScan();
		}
	}

	public class LayoutPanelLui : BaseLui, IIsLayoutPanelLui, IIsPanelLui, IHasPanelsLui, IHasMenusLui, IHasWidgetsLui, IBaseLui
	{
		public LayoutPanelLui(Script script, string typeName, string debugName)
			: base(script, typeName, debugName)
		{
			this.InitHasMenus();
			this.InitHasPanels();
			this.InitWidgets();
			this.InitIsPanel();
		}
	}

	public class PanelLui : BaseLui, IIsPanelLui, IHasWidgetsLui, IBaseLui
	{
		public PanelLui(Script ownerScript, string typeName, string debugName)
			: base(ownerScript, typeName, debugName)
		{
			this.InitWidgets();
			this.InitIsPanel();
		}
	}

	public class WidgetLui : BaseLui, IIsWidgetLui, IHasHandlesLui, IBaseLui
	{
		public WidgetLui(Script ownerScript, string typeName, string debugName)
			: base(ownerScript, typeName, debugName)
		{
			this.InitHasHandles();
		}
	}

	public class HandleLui : BaseLui, IIsHandleLui, IHasLiveValueLui, IBaseLui
	{
		public HandleLui(Script ownerScript, string typeName, string debugName)
			: base(ownerScript, typeName, debugName)
		{
			this.InitHasLiveValue();
		}
	}

	public class ConnectionLui : BaseLui, IIsConnectionLui, IHasLiveValueLui, IBaseLui
	{
		public ConnectionLui(Script ownerScript, string typeName, string debugName)
			: base(ownerScript, typeName, debugName)
		{
			this.InitHasLiveValue();
		}
	}

	public static class ScanAt
	{
		public static BrixLogger logger = new BrixLogger(typeof(ScanAt).FullName);

		public static void Root(Table t, bool asSubMenu = false)
		{
			string log = string.Empty;
			string text = t.Get("Name").String;
			if (text == "Menu")
			{
				Menu(t, string.Empty, ref log, asSubMenu);
			}
			else if (text.Contains("Panel"))
			{
				Panel(t.Get("Menu").Table, t, string.Empty, ref log);
			}
			if (DoLog)
			{
				logger.Info(log);
			}
		}

		public static void Menu(Table tMenu, string prefix, ref string log, bool asSubMenu = false)
		{
			if (DoApply)
			{
				Apply.Menu.New(tMenu, asSubMenu);
			}
			if (DoLog)
			{
				Log.TypeId(tMenu, ref prefix, ref log);
			}
			MenuMenus(tMenu, prefix, ref log);
			MenuFlags(tMenu, prefix, ref log);
			MenuConnections(tMenu, prefix, ref log);
			MenuPanels(tMenu, prefix, ref log);
		}

		public static void MenuMenus(Table tMenu, string prefix, ref string log)
		{
			Table table = tMenu.Get("Menus").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Menus.";
			}
			if (DoApply)
			{
				Apply.Menu.RemoveAllMenus(tMenu);
			}
			foreach (DynValue item in (from i in table.Keys.Slinq()
				orderby i.String descending
				select i).NoLeak())
			{
				Table table2 = table.Get(item.Number).Table;
				if (table2 != null)
				{
					Menu(table2, prefix, ref log, asSubMenu: true);
					if (DoApply)
					{
						Apply.Menu.AddMenu(tMenu, table2);
					}
				}
			}
		}

		public static void MenuPanels(Table tMenu, string prefix, ref string log)
		{
			Table table = tMenu.Get("Panels").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Panels.";
			}
			if (DoApply)
			{
				Apply.Menu.RemoveAllPanels(tMenu);
			}
			foreach (DynValue item in (from i in table.Keys.Slinq()
				orderby i.String descending
				select i).NoLeak())
			{
				Table table2 = table.Get(item.Number).Table;
				if (table2 != null)
				{
					Panel(tMenu, table2, prefix, ref log);
					if (DoApply)
					{
						Apply.Menu.AddRootPanel(tMenu, table2);
					}
				}
			}
		}

		public static void MenuConnections(Table tMenu, string prefix, ref string log)
		{
			Table table = tMenu.Get("Connections").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Connections.";
			}
			if (DoApply)
			{
				Apply.Menu.ClearAllConnections(tMenu);
			}
			foreach (DynValue key in table.Keys)
			{
				Table table2 = table.Get(key.Number).Table;
				if (table2 != null)
				{
					if (DoLog)
					{
						Log.MenuConnection(table2, prefix, ref log);
					}
					if (DoApply)
					{
						Apply.Menu.AddConnection(tMenu, table2);
					}
				}
			}
		}

		public static void MenuFlags(Table tMenu, string prefix, ref string log)
		{
			Table table = tMenu.Get("Flags").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Flags=";
			}
			if (DoApply)
			{
				Apply.Menu.ClearAllFlags(tMenu);
			}
			if (IsDebug)
			{
				logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + table.Length + "\n");
			}
			foreach (DynValue key in table.Keys)
			{
				if (IsDebug)
				{
					logger.Info(string.Concat("LuaMenuTable#", tMenu.ReferenceID, ".Flags[", key, "]\n"));
				}
				BaseMenu.BuilderFlag builderFlag = key.ToEnum<BaseMenu.BuilderFlag>();
				if (builderFlag == (BaseMenu.BuilderFlag)0)
				{
					continue;
				}
				DynValue dynValue = table.Get(key);
				if (!dynValue.IsNil())
				{
					Closure function = dynValue.Function;
					if (DoLog)
					{
						Log.MenuBuilderFlag(builderFlag, (int)builderFlag, prefix, ref log);
					}
					if (DoApply)
					{
						Apply.Menu.SetFlag(tMenu, builderFlag, function);
					}
				}
			}
		}

		public static void Panel(Table tMenu, Table tPanel, string prefix, ref string log)
		{
			if (DoApply)
			{
				Apply.Panel.New(tPanel);
			}
			if (DoLog)
			{
				Log.TypeId(tPanel, ref prefix, ref log);
			}
			PanelWidgets(tPanel, prefix, ref log);
			PanelPanels(tMenu, tPanel, prefix, ref log);
			PanelMenu(tPanel, prefix, ref log);
		}

		public static void PanelMenu(Table tLayoutPanel, string prefix, ref string log)
		{
			Table table = tLayoutPanel.Get("Menu").Table;
			if (table != null && DoApply)
			{
				Apply.Panel.SetMenu(tLayoutPanel, table);
			}
		}

		public static void PanelPanels(Table tMenu, Table tLayoutPanel, string prefix, ref string log)
		{
			Table table = tLayoutPanel.Get("Panels").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Panels.";
			}
			if (DoApply)
			{
				Apply.Panel.RemoveAllPanels(tLayoutPanel);
			}
			foreach (DynValue item in (from k in table.Keys.Slinq()
				orderby k.String descending
				select k).NoLeak())
			{
				Table table2 = table.Get(item.Number).Table;
				if (table2 != null)
				{
					Panel(tMenu, table2, prefix, ref log);
					if (DoApply)
					{
						Apply.Panel.AddPanel(tLayoutPanel, table2);
					}
					if (DoApply)
					{
						Apply.Menu.AddRootPanel(tMenu, table2);
					}
				}
			}
		}

		public static void PanelWidgets(Table tPanel, string prefix, ref string log)
		{
			Table table = tPanel.Get("Widgets").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Widgets.";
			}
			if (DoApply)
			{
				Apply.Panel.ClearAllWidgets(tPanel);
			}
			foreach (DynValue key in table.Keys)
			{
				Table table2 = table.Get(key.String).Table;
				if (table2 != null)
				{
					WidgetNew(table2, prefix, ref log);
					if (DoApply)
					{
						Apply.Panel.SetWidget(tPanel, table2);
					}
				}
			}
		}

		public static void WidgetNew(Table tWidget, string prefix, ref string log)
		{
			if (DoApply)
			{
				Apply.Widget.New(tWidget);
			}
			if (DoLog)
			{
				Log.TypeId(tWidget, ref prefix, ref log);
			}
			WidgetHandles(tWidget, prefix, ref log);
			WidgetInit(tWidget);
		}

		public static void WidgetHandles(Table tWidget, string prefix, ref string log)
		{
			Table table = tWidget.Get("Handles").Table;
			if (table == null)
			{
				return;
			}
			if (DoLog)
			{
				prefix += ".Handles.";
			}
			if (DoApply)
			{
				Apply.Widget.ClearAllHandles(tWidget);
			}
			foreach (DynValue key in table.Keys)
			{
				Table table2 = table.Get(key.String).Table;
				if (table2 != null)
				{
					if (DoLog)
					{
						Log.WidgetHandle(table2, prefix, ref log);
					}
					if (DoApply)
					{
						Apply.Widget.SetHandle(tWidget, table2);
					}
				}
			}
		}

		public static void WidgetInit(Table tWidget)
		{
			Apply.Widget.Init(tWidget);
		}
	}

	public static class Log
	{
		public static void TypeId(Table t, ref string prefix, ref string log)
		{
			string text = t.Get("Name").String;
			prefix = prefix + string.Empty + text + string.Empty;
			string text2 = prefix;
			prefix = text2 + "<size=8>#" + t.ReferenceID + "</size>";
			log += prefix;
			log += "\n";
		}

		public static void WidgetHandle(Table tHandle, string prefix, ref string log)
		{
			TypeId(tHandle, ref prefix, ref log);
			prefix += ".LiveValue=";
			HandleLiveValue(tHandle, ref prefix, ref log);
		}

		public static void MenuConnection(Table tHook, string prefix, ref string log)
		{
			if (DoLog)
			{
				TypeId(tHook, ref prefix, ref log);
			}
			prefix += ".Callback=";
			ConnectionCallback(tHook, ref prefix, ref log);
		}

		public static void MenuBuilderFlag(BaseMenu.BuilderFlag menuBuilderFlag, int i, string prefix, ref string log)
		{
			string text = prefix;
			prefix = text + string.Empty + i + string.Empty;
			text = prefix;
			prefix = string.Concat(text, "(", menuBuilderFlag, ")");
			log += prefix;
			log += "\n";
		}

		public static void HandleLiveValue(Table t, ref string prefix, ref string log)
		{
			Closure function = t.Get("LiveValue").Function;
			string text = prefix;
			prefix = text + "<size=8>#" + function.ReferenceID + "</size>";
			log += prefix;
			log += "\n";
		}

		public static void ConnectionCallback(Table t, ref string prefix, ref string log)
		{
			DynValue dynValue = t.Get("Callback");
			string text = prefix;
			prefix = text + "<size=8>#" + dynValue.ReferenceID + "</size>";
			log += prefix;
			log += "\n";
		}
	}

	public static class MapHelper
	{
		public static void RemoveAllAtScript(Script s)
		{
			RemoveAllScriptTablesAtMap(s, WidgetMap);
			RemoveAllScriptTablesAtMap(s, PanelMap);
			RemoveAllScriptTablesAtMap(s, MenuMap);
		}

		private static void RemoveAllScriptTablesAtMap<T>(Script s, Dictionary<Table, T> map)
		{
			List<Table> list = CollectionPool.RequestList<Table>();
			foreach (Table key in map.Keys)
			{
				if (!key.isAlive || key.OwnerScript == null || !key.OwnerScript.IsAlive || key.OwnerScript == s)
				{
					list.Add(key);
				}
			}
			if (list.Count == map.Count)
			{
				map.Clear();
				list.Clear();
			}
			else
			{
				foreach (Table item in list)
				{
					map.Remove(item);
				}
			}
			list.Dispose();
		}

		public static bool HasMenu(Table t)
		{
			return MenuMap.ContainsKey(t);
		}

		public static IMenu GetMenu(Table t)
		{
			return MenuMap[t];
		}

		public static void AddMenu(Table t, IMenu menu)
		{
			MenuMap.Add(t, menu);
		}

		public static bool HasPanel(Table t)
		{
			return PanelMap.ContainsKey(t);
		}

		public static IPanel GetPanel(Table t)
		{
			return PanelMap[t];
		}

		public static void AddPanel(Table t, IPanel panel)
		{
			PanelMap.Add(t, panel);
		}

		public static bool HasWidget(Table t)
		{
			return WidgetMap.ContainsKey(t);
		}

		public static IWidget GetWidget(Table t)
		{
			return WidgetMap[t];
		}

		public static void AddWidget(Table t, IWidget widget)
		{
			WidgetMap.Add(t, widget);
		}
	}

	private static class Apply
	{
		public static class Menu
		{
			public static BrixLogger logger = new BrixLogger(typeof(Menu).FullName);

			public static void New(Table tMenu, bool asSubMenu = false)
			{
				if (!MapHelper.HasMenu(tMenu))
				{
					LuaMenu luaMenu = LuaMenu.New(null);
					luaMenu.LuaTable = tMenu;
					luaMenu.SuffixName = tMenu.Get("_debugName").String ?? string.Empty;
					MapHelper.AddMenu(tMenu, luaMenu);
					if (IsDebug)
					{
						logger.Info("LuaMenu#" + tMenu.ReferenceID + ".New()\n");
					}
					if (IsDebug)
					{
						logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + tMenu.Get("Flags").Table.Length + "\nNew()");
					}
				}
			}

			public static void AddMenu(Table tMenu, Table tSubMenu)
			{
				IMenu menu = MapHelper.GetMenu(tSubMenu);
				MapHelper.GetMenu(tMenu).AddLuaMenu(menu);
				if (IsDebug)
				{
					logger.Info("LuaMenu#" + tMenu.ReferenceID + ".AddMenu(#" + tSubMenu.ReferenceID + ")\n");
				}
				if (IsDebug)
				{
					logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + tMenu.Get("Flags").Table.Length + "\nAddMenu()");
				}
			}

			public static void RemoveAllMenus(Table tMenu)
			{
				MapHelper.GetMenu(tMenu).RemoveAllLuaMenus();
			}

			public static void AddRootPanel(Table tMenu, Table tPanel)
			{
				MapHelper.GetMenu(tMenu).AddRootLuaPanel(tPanel);
				MapHelper.GetMenu(tMenu).AddLuaPanel(tPanel);
			}

			public static void AddPanel(Table tMenu, Table tPanel)
			{
				MapHelper.GetMenu(tMenu).AddLuaPanel(tPanel);
			}

			public static void RemoveAllPanels(Table tMenu)
			{
				MapHelper.GetMenu(tMenu).RemoveAllLuaPanels();
			}

			public static void ClearAllConnections(Table tMenu)
			{
				MapHelper.GetMenu(tMenu).RemoveAllLuaConnections();
			}

			public static void AddConnection(Table tMenu, Table tConnection)
			{
				LuaHookBase h = tConnection.Get("Hook").ToObject<LuaHookBase>();
				Closure c = tConnection.Get("Callback").ToObject<Closure>();
				BaseMenu.MenuState e = tConnection.Get("MenuEvent").ToEnum<BaseMenu.MenuState>();
				MapHelper.GetMenu(tMenu).AddLuaConnection(new BaseMenu.HookConnection(h, c, e));
			}

			public static void ClearAllFlags(Table tMenu)
			{
				if (IsDebug)
				{
					logger.Info("LuaMenu#" + tMenu.ReferenceID + ".ClearAllFlags()\n");
				}
				if (IsDebug)
				{
					logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + tMenu.Get("Flags").Table.Length + "\nClearAllFlags() A");
				}
				MapHelper.GetMenu(tMenu).ClearAllLuaBuilderFlags();
				if (IsDebug)
				{
					logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + tMenu.Get("Flags").Table.Length + "\nClearAllFlags() B");
				}
			}

			public static void SetFlag(Table tMenu, BaseMenu.BuilderFlag menuBuilderFlag, Closure c)
			{
				if (IsDebug)
				{
					logger.Info(string.Concat("LuaMenu#", tMenu.ReferenceID, ".SetFlag(", menuBuilderFlag, ")\n"));
				}
				if (IsDebug)
				{
					logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + tMenu.Get("Flags").Table.Length + "\nSetFlag() A");
				}
				MapHelper.GetMenu(tMenu).SetLuaBuilderFlag(menuBuilderFlag, c);
				if (IsDebug)
				{
					logger.Info("LuaMenuTable#" + tMenu.ReferenceID + ".Flags.Count = " + tMenu.Get("Flags").Table.Length + "\nSetFlag() B");
				}
			}
		}

		public static class Panel
		{
			public static void New(Table tPanel)
			{
				if (!MapHelper.HasPanel(tPanel))
				{
					IPanel newPanel = GetNewPanel(tPanel.Get("Name").String);
					newPanel.SuffixName = tPanel.Get("_debugName").String ?? string.Empty;
					MapHelper.AddPanel(tPanel, newPanel);
				}
			}

			public static void AddPanel(Table tLayoutPanel, Table tPanel)
			{
				New(tLayoutPanel);
				New(tPanel);
				ILayoutPanel layoutPanel = MapHelper.GetPanel(tLayoutPanel) as ILayoutPanel;
				IPanel panel = MapHelper.GetPanel(tPanel);
				layoutPanel.AddPanel(panel);
			}

			public static void RemoveAllPanels(Table tLayoutPanel)
			{
				ILayoutPanel layoutPanel = MapHelper.GetPanel(tLayoutPanel) as ILayoutPanel;
				layoutPanel.RemoveAllPanels();
			}

			public static void SetWidget(Table tPanel, Table tWidget)
			{
				IWidget widget = MapHelper.GetWidget(tWidget);
				string key = tWidget.Get("Name").String;
				IPanel panel = MapHelper.GetPanel(tPanel);
				if (panel.LuaWidget.ContainsKey(key))
				{
					panel.LuaWidget[key] = widget;
				}
				else
				{
					panel.LuaWidget.Add(key, widget);
				}
				panel.TrySetLuaWidgets();
			}

			public static void ClearAllWidgets(Table tPanel)
			{
				IPanel panel = MapHelper.GetPanel(tPanel);
				panel.LuaWidget.Clear();
				panel.TrySetLuaWidgets();
			}

			public static void SetMenu(Table tPanel, Table tMenu)
			{
				if (MapHelper.GetPanel(tPanel) is ILayoutPanel layoutPanel)
				{
					IMenu menu = MapHelper.GetMenu(tMenu);
					layoutPanel.SetMenu(menu);
				}
			}
		}

		public static class Widget
		{
			public static void New(Table tWidget)
			{
				if (!MapHelper.HasWidget(tWidget))
				{
					MapHelper.AddWidget(tWidget, new LuaWidget());
				}
			}

			public static void Init(Table tWidget)
			{
				IWidget widget = MapHelper.GetWidget(tWidget);
				widget.Lua_Init();
			}

			public static void SetHandle(Table tWidget, Table tHandle)
			{
				string handleKey = tHandle.Get("Name").String;
				IWidget widget = MapHelper.GetWidget(tWidget);
				widget.Lua_SetHandle(handleKey, tHandle.Get("LiveValue").Function, tHandle.Get("LiveValueIsOnce"));
			}

			public static void ClearAllHandles(Table tWidget)
			{
				IWidget widget = MapHelper.GetWidget(tWidget);
				widget.Lua_ClearAllHandles();
			}
		}
	}

	public const string StrDebugName = "_debugName";

	public const string StrName = "Name";

	public const string StrId = "Id";

	public const string StrInstance = "Instance";

	public const string StrNew = "New";

	public const string StrGet = "Get";

	public const string StrScan = "Scan";

	public const string StrPath = "Path";

	public const string StrExecuteFile = "executeFile";

	public const string StrColors = "Colors";

	public const string StrEnums = "Enums";

	public const string StrFuncs = "Funcs";

	public const string StrMenu = "Menu";

	public const string StrMenus = "Menus";

	public const string StrMenuEvent = "MenuEvent";

	public const string StrOpen = "Open";

	public const string StrReopen = "Reopen";

	public const string StrClose = "Close";

	public const string StrRefresh = "Refresh";

	public const string StrFlags = "Flags";

	public const string StrSetFlag = "SetFlag";

	public const string StrClearFlag = "ClearFlag";

	public const string StrClearAllFlags = "ClearAllFlags";

	public const string StrLoad = "Load";

	public const string StrLoaded = "loaded";

	public const string StrReload = "Reload";

	public const string StrReloadAll = "ReloadAll";

	public const string StrReloadPanels = "ReloadPanels";

	public const string StrReloadMenus = "ReloadMenus";

	public const string StrReloadConnections = "ReloadConnections";

	public const string StrReloadFlags = "ReloadFlags";

	public const string StrHkOnNew = "hk_OnNew";

	public const string StrHkOnLoad = "hk_OnLoad";

	public const string StrHkOnOpen = "hk_OnOpen";

	public const string StrPanel = "Panel";

	public const string StrPanels = "Panels";

	public const string StrAddPanel = "AddPanel";

	public const string StrAddPanels = "AddPanels";

	public const string StrLayoutPanel = "LayoutPanel";

	public const string StrLayoutPanels = "LayoutPanels";

	public const string StrSetMenu = "SetMenu";

	public const string StrClearMenu = "ClearMenu";

	public const string StrAddMenu = "AddMenu";

	public const string StrRemoveAllMenus = "RemoveAllMenus";

	public const string StrWidget = "Widget";

	public const string StrWidgets = "Widgets";

	public const string StrSetWidget = "SetWidget";

	public const string StrSetWidgets = "SetWidgets";

	public const string StrSetWidgetHandle = "SetWidgetHandle";

	public const string StrHandle = "Handle";

	public const string StrHandles = "Handles";

	public const string StrSetHandle = "SetHandle";

	public const string StrSetHandles = "SetHandles";

	public const string StrLiveValue = "LiveValue";

	public const string StrSetLiveValue = "SetLiveValue";

	public const string StrClearLiveValue = "ClearLiveValue";

	public const string StrLiveValueIsOnce = "LiveValueIsOnce";

	public const string StrSignal = "Signal";

	public const string StrSignals = "Signals";

	public const string StrSetSignal = "SetSignal";

	public const string StrClearSignal = "ClearSignal";

	public const string StrHook = "Hook";

	public const string StrConnections = "Connections";

	public const string StrConnectWhen = "ConnectWhen";

	public const string StrDisconnectAll = "DisconnectAll";

	public const string StrCallback = "Callback";

	public const string StrSetCallback = "SetCallback";

	public const string StrClearCallback = "ClearCallback";

	public const string StrGetRect = "GetRect";

	public const string StrGetLayoutRect = "GetLayoutRect";

	public const string StrGetParentLayoutRect = "GetParentLayoutRect";

	public const string IdPrefix = "<size=8>#";

	public const string IdSuffix = "</size>";

	public const string KeyPrefix = ".";

	public const string KeySuffix = ".";

	public const string ValuePrefix = "";

	public const string ValueSuffix = "=";

	public const string TypePrefix = "";

	public const string TypeSuffix = "";

	public const string BuilderFlagIdxPrefix = "";

	public const string BuilderFlagIdxSuffix = "";

	public const string BuilderFlagPrefix = "(";

	public const string BuilderFlagSuffix = ")";

	public static bool IsDebug;

	public static bool DoLog;

	public static bool DoApply;

	public static Dictionary<Table, IMenu> MenuMap = new Dictionary<Table, IMenu>();

	public static Dictionary<Table, IPanel> PanelMap = new Dictionary<Table, IPanel>();

	public static Dictionary<Table, IWidget> WidgetMap = new Dictionary<Table, IWidget>();

	private static IPanel GetNewPanel(string typeName)
	{
		return typeName switch
		{
			"LayoutPanels.Vertical" => VerticalLayoutPanel.New(), 
			"LayoutPanels.Horizontal" => HorizontalLayoutPanel.New(), 
			"LayoutPanels.Grid" => GridLayoutPanel.New(), 
			"Panels.Label" => LabelPanel.New(), 
			"Panels.Image" => ImagePanel.New(), 
			"Panels.Slider" => SliderPanel.New(), 
			"LayoutPanels.SquareButton" => SquareButtonPanel.New(), 
			"Panels.Buttons.Standard" => StandardButtonPanel.New(), 
			"LayoutPanels.Slidables.Top" => SlidableLayoutPanel.New().As(SlidableLayoutPanel.Variant.Top), 
			"LayoutPanels.Slidables.Bottom" => SlidableLayoutPanel.New().As(SlidableLayoutPanel.Variant.Bottom), 
			"LayoutPanels.Slidables.Left" => SlidableLayoutPanel.New().As(SlidableLayoutPanel.Variant.Left), 
			"LayoutPanels.Slidables.Right" => SlidableLayoutPanel.New().As(SlidableLayoutPanel.Variant.Right), 
			"Panels.Dropdown" => LuaDropdownPanel.New().As(LuaDropdownPanel.Variant.Default), 
			"Panels.InputField" => LuaInputFieldPanel.New().As(LuaInputFieldPanel.Variant.Default), 
			"Panels.RawImage" => LuaTexturePanel.New().As(LuaTexturePanel.Variant.Default), 
			"Panels.Compass" => LuaCompassPanel.New().As(LuaCompassPanel.Variant.Default), 
			"LayoutPanels.Scrollable" => LuaScrollableLayoutPanel.New().As(LuaScrollableLayoutPanel.Variant.Default), 
			"Panels.Media" => MediaPanel.New(), 
			"Panels.Minimap" => MinimapPanel.New().As(MinimapPanel.Variant.Default), 
			_ => throw new NotImplementedException(), 
		};
	}
}
