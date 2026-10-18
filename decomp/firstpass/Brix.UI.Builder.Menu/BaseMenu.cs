using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Signals;
using Brix.Lua;
using Brix.UI.Builder.Lui;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public abstract class BaseMenu : IMenu, ISignalConnector<BaseMenu.MenuState>, ILuaId, IHierarchyName
{
	public enum MenuState
	{
		Loaded,
		Opened
	}

	protected struct MenuHandle
	{
		private readonly BaseMenu _menu;

		public MenuHandle(BaseMenu menu)
		{
			_menu = menu;
		}

		public MenuHandle AddMenu(IMenu subMenu)
		{
			_menu._menus.Add(subMenu);
			return this;
		}
	}

	public struct PanelHandle
	{
		private readonly BaseMenu _menu;

		public PanelHandle(BaseMenu menu)
		{
			_menu = menu;
		}

		public PanelHandle AddPanel(IPanel panel)
		{
			_menu._panels.Add(panel);
			return this;
		}

		public PanelHandle AddPanels(PanelList panels)
		{
			foreach (IPanel panel in panels)
			{
				_menu._panels.Add(panel);
			}
			PanelList.Release(panels);
			return this;
		}

		public PanelHandle AddPanelsNoLeak(Action<PanelList> getPanelsAction)
		{
			PanelList panelList = PanelList.Request();
			getPanelsAction(panelList);
			foreach (IPanel item in panelList)
			{
				_menu._panels.Add(item);
			}
			PanelList.Release(panelList);
			return this;
		}

		public PanelHandle AddPanelsNoLeak(Action<PanelList, BaseMenu> getPanelsAction, BaseMenu menu)
		{
			PanelList panelList = PanelList.Request();
			getPanelsAction(panelList, menu);
			foreach (IPanel item in panelList)
			{
				_menu._panels.Add(item);
			}
			PanelList.Release(panelList);
			return this;
		}
	}

	public interface IPanelHandle<TMenu> where TMenu : BaseMenu
	{
		Signal OnClearSignal { get; }

		PanelHandle AddPanel(IPanel panel);

		PanelHandle AddPanels(PanelList panels);

		PanelHandle AddPanelsNoLeak(Action<PanelList, TMenu> getPanelsAction);

		PanelHandle AddPanelsNoLeak(Action<PanelList, TMenu> getPanelsAction, TMenu menu);
	}

	public static class Colors
	{
		public static readonly Color Brown = new Color(0.7490196f, 0.4862745f, 16f / 85f, 1f);

		public static readonly Color LightBlue = new Color(0.8f, 46f / 51f, 1f, 1f);

		public static readonly Color LighterBlue = new Color(46f / 51f, 81f / 85f, 1f, 1f);

		public static readonly Color Blue = new Color(0.18039216f, 0.6392157f, 0.8509804f, 1f);

		public static readonly Color Yellow = new Color(1f, 0.77254903f, 2f / 51f, 1f);

		public static readonly Color LightYellow = new Color(1f, 0.83137256f, 0.29411766f, 1f);

		public static readonly Color LighterYellow = new Color(1f, 78f / 85f, 0.64705884f, 1f);

		public static readonly Color White = new Color(1f, 1f, 1f, 1f);

		public static readonly Color Black = new Color(0f, 0f, 0f, 1f);

		public static readonly Color LightBlack = new Color(0f, 0f, 0f, 61f / 85f);

		public static readonly Color HalfBlack = new Color(0f, 0f, 0f, 0.5f);

		public static readonly Color Gray = new Color(0.5019608f, 0.5019608f, 0.5019608f, 1f);

		public static readonly Color Red = new Color(1f, 0f, 0f, 1f);

		public static readonly Color PlayButtonLabel_Enabled = Black;

		public static readonly Color PlayButtonLabel_Disabled = Brown;

		public static readonly Color PlayButtonBg_Enabled = White;

		public static readonly Color PlayButtonBg_Disabled = Yellow;
	}

	protected static class ColorBlocks
	{
		public static ColorBlock PlayButtonBar_Enabled = new ColorBlock
		{
			colorMultiplier = 1f,
			disabledColor = Colors.Blue,
			normalColor = Colors.Blue,
			highlightedColor = Colors.LightBlue,
			pressedColor = Colors.LighterBlue
		};

		public static ColorBlock PlayButtonBar_Disabled = new ColorBlock
		{
			colorMultiplier = 1f,
			disabledColor = Colors.Brown,
			normalColor = Colors.Brown,
			highlightedColor = Colors.Brown,
			pressedColor = Colors.Brown
		};
	}

	protected struct HookConnectionHandle
	{
		private readonly BaseMenu _menu;

		public HookConnectionHandle(BaseMenu menu)
		{
			_menu = menu;
			_menu._hookConnections.Clear();
		}

		public HookConnectionHandle AddHookConnection(MenuState menuState, LuaHookBase hook, Closure closure)
		{
			_menu._hookConnections.Add(new HookConnection(hook, closure, menuState));
			return this;
		}

		public HookConnectionHandle AddHookConnection(HookConnection hc)
		{
			_menu._hookConnections.Add(hc);
			return this;
		}
	}

	public class HookConnection
	{
		private readonly LuaHookBase _h;

		private readonly Closure _c;

		public MenuState E;

		public HookConnection(LuaHookBase h, Closure c, MenuState e)
		{
			_h = h;
			_c = c;
			E = e;
		}

		public void ConnectUntil(LuaHook disconnectHook)
		{
			LuaHookBase.ConnectFromCLR(_h, _c, disconnectHook);
		}
	}

	[Flags]
	public enum BuilderFlag
	{
		LoadOnInit = 1,
		LoadOnOpen = 2,
		ReloadOnOpen = 4,
		ReloadOnReopen = 8,
		OpenOnInit = 0x10,
		OpenOnLoad = 0x20,
		OpenOnReload = 0x40,
		ReopenOnReload = 0x80,
		UnloadOnClose = 0x100,
		OpensChildMenus = 0x200,
		OpenedByParentMenu = 0x400,
		OnOpen = 0x800,
		OnClose = 0x1000
	}

	private static readonly object[] TmpBinGetter = new object[1];

	protected static BrixLogger logger = new BrixLogger(typeof(BaseMenu).FullName);

	protected static bool IsDebug = false;

	protected Signal _onClearSignal = new Signal();

	private static readonly object TmpObjTrue = true;

	protected static Action<BaseMenu> OnOpen = delegate(BaseMenu menu)
	{
		menu.Open();
	};

	protected static Action<BaseMenu> OnReopen = delegate(BaseMenu menu)
	{
		menu.Reopen();
	};

	protected static Action<BaseMenu> OnClose = delegate(BaseMenu menu)
	{
		menu.Close();
	};

	protected static Action<BaseMenu> OnReset = delegate(BaseMenu menu)
	{
		menu.Reset();
	};

	protected static Action<BaseMenu> OnRefresh = delegate(BaseMenu menu)
	{
		menu.Refresh();
	};

	private readonly List<IMenu> _menus = new List<IMenu>();

	private readonly List<IPanel> _panels = new List<IPanel>();

	protected readonly List<Table> RootLuaPanels = new List<Table>();

	protected readonly List<Table> LuaPanels = new List<Table>();

	protected readonly List<IMenu> LuaMenus = new List<IMenu>();

	protected readonly List<HookConnection> LuaConnections = new List<HookConnection>();

	private static string _hexCastleYellow = string.Empty;

	private static string _hexCastleBrown = string.Empty;

	private readonly List<HookConnection> _hookConnections = new List<HookConnection>();

	private readonly Dictionary<MenuState, LuaHook> _disconnectHook = new Dictionary<MenuState, LuaHook>(FastEnumIntEqualityComparer<MenuState>.Instance)
	{
		{
			MenuState.Loaded,
			new LuaHook()
		},
		{
			MenuState.Opened,
			new LuaHook()
		}
	};

	private We.KeyBindingCategoryUtil _cameraKeyBindings = new We.KeyBindingCategoryUtil("Camera");

	private We.KeyBindingCategoryUtil _generalKeyBindings = new We.KeyBindingCategoryUtil("General");

	private We.KeyBindingCategoryUtil _weMenuKeyBindings = new We.KeyBindingCategoryUtil("we_menu");

	private We.KeyBindingCategoryUtil _weTerrainMenuKeyBindings = new We.KeyBindingCategoryUtil("we_terrain_menu");

	private We.KeyBindingCategoryUtil _weMarkersMenuKeyBindings = new We.KeyBindingCategoryUtil("we_markers_menu");

	private We.KeyBindingCategoryUtil _debugBindings = new We.KeyBindingCategoryUtil("debug");

	protected BuilderFlag BuilderFlags = BuilderFlag.LoadOnInit | BuilderFlag.OpenOnInit | BuilderFlag.OpenOnLoad | BuilderFlag.ReopenOnReload;

	protected readonly Dictionary<BuilderFlag, Closure> FlagClosure = new Dictionary<BuilderFlag, Closure>(FastEnumIntEqualityComparer<BuilderFlag>.Instance);

	private readonly char[] _separator = new char[1] { '.' };

	private string _suffixName;

	Dictionary<MenuState, Tuple<List<LifeSignalConnectionBase>, Signal>> ISignalConnector<MenuState>.SignalMap { get; set; }

	Signal ISignalConnector<MenuState>.OnClearSignal => _onClearSignal;

	public Func<RectTransform> GetParent { get; set; }

	protected virtual bool InstantiateOnlyIfMissing => false;

	public bool Loaded { get; private set; }

	public LuaHook OnLoadedHook { get; set; }

	public Closure LuaMenuTableReader { get; set; }

	public bool Opened { get; private set; }

	public bool CanOpen => Closed;

	public bool Closed => !Opened;

	public bool CanClose => Opened;

	public bool CanToggle => (!Opened) ? CanOpen : CanClose;

	public List<IPanel> Panels => _panels;

	public string LuaPath => LuaTable.Get("Path").String;

	public Table LuaTable { get; set; }

	public static string HexCastleYellow
	{
		get
		{
			if (_hexCastleYellow == string.Empty)
			{
				Color32 color = Colors.Yellow;
				_hexCastleYellow = string.Format("{0}{1}{2}", color.r.ToString("X2"), color.g.ToString("X2"), color.b.ToString("X2"));
			}
			return _hexCastleYellow;
		}
	}

	public static string HexCastleBrown
	{
		get
		{
			if (_hexCastleBrown == string.Empty)
			{
				Color32 color = Colors.Brown;
				_hexCastleBrown = string.Format("{0}{1}{2}", color.r.ToString("X2"), color.g.ToString("X2"), color.b.ToString("X2"));
			}
			return _hexCastleBrown;
		}
	}

	public static Sprite IconCheck => IconKeys._UI_Check.Get32();

	protected virtual MenuSound.Prefix SoundPrefix => MenuSound.Prefix.Main;

	public IMenu ParentMenu { get; set; }

	protected virtual bool LoadOnInit => true;

	protected virtual bool LoadOnOpen => false;

	protected virtual bool ReloadOnOpen => false;

	protected virtual bool ReloadOnReopen => false;

	protected virtual bool OpenOnInit => false;

	protected virtual bool OpenOnLoad => false;

	protected virtual bool OpenOnReload => false;

	protected virtual bool ReopenOnReload => false;

	protected virtual bool UnloadOnClose => false;

	protected virtual bool OpenedByParentMenu => true;

	protected virtual bool OpensSubMenus => true;

	protected virtual bool LuaDriven => false;

	public long LuaId { get; set; }

	public string Name => PrettyTypeName;

	public string HierarchyName => "{" + Name + ((SuffixName != null) ? ("::" + SuffixName) : string.Empty) + "}";

	public string FullName => HierarchyName;

	protected string PrettyTypeName => $"{GetType().ToString().Split(_separator).Slinq().Last()},{SuffixName ?? string.Empty}";

	public string SuffixName
	{
		get
		{
			return _suffixName ?? string.Empty;
		}
		set
		{
			_suffixName = value;
		}
	}

	protected BaseMenu(Func<RectTransform> getParent)
	{
		GetParent = getParent;
		Init();
	}

	protected virtual SignalHandle<MenuState> AddSignals()
	{
		return new SignalHandle<MenuState>(this);
	}

	public static TMenu CreateInstance<TMenu>(Func<RectTransform> binGetter) where TMenu : BaseMenu
	{
		TmpBinGetter[0] = binGetter;
		return (TMenu)Activator.CreateInstance(typeof(TMenu), TmpBinGetter);
	}

	public virtual bool GetToggleSkipFlag()
	{
		return false;
	}

	protected virtual bool GetMenuSkipFlag(IMenu baseMenu)
	{
		return false;
	}

	public void Init(bool includeChildren = true)
	{
		InitAll(includeChildren);
		if (LoadOnInit)
		{
			Load();
		}
		if (OpenOnInit)
		{
			Open();
		}
	}

	private void InitAll(bool includeChildren)
	{
		InitSelf();
		InitChildren(includeChildren);
	}

	protected virtual void InitSelf()
	{
		if (LuaDriven)
		{
			LuaIdUtility.RegisterMenu(this);
		}
	}

	private void InitChildren(bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.Init(includeChildren);
		}
	}

	public void Dispose()
	{
		DisposeAll();
	}

	private void DisposeAll()
	{
		DisposeChildren();
		DisposeSelf();
	}

	protected virtual void DisposeSelf()
	{
		if (Opened)
		{
			Close(includeChildren: false);
		}
		if (Loaded)
		{
			Unload(includeChildren: false);
		}
		NullEverything();
	}

	protected void NullEverything()
	{
		if (LuaDriven)
		{
			LuaIdUtility.UnregisterMenu(this);
		}
		GetParent = null;
		OnLoadedHook = null;
		LuaMenuTableReader = null;
		if (_menus != null)
		{
			_menus.Clear();
		}
		if (_panels != null)
		{
			_panels.Clear();
		}
		if (Panels != null)
		{
			Panels.Clear();
		}
		if (LuaPanels != null)
		{
			LuaPanels.Clear();
		}
		RemoveAllLuaMenus();
		RemoveAllLuaConnections();
		if (LuaTable != null)
		{
			Table tokill = LuaTable;
			Table.Kill(ref tokill);
			LuaTable = null;
		}
		_onClearSignal.Invoke();
		_onClearSignal.Clear();
		_onClearSignal = null;
		_hookConnections.Clear();
		_disconnectHook.Clear();
		_cameraKeyBindings = null;
		_generalKeyBindings = null;
		_weMenuKeyBindings = null;
		_weTerrainMenuKeyBindings = null;
		_weMarkersMenuKeyBindings = null;
		_debugBindings = null;
		ParentMenu = null;
		FlagClosure.Clear();
	}

	private void DisposeChildren()
	{
		foreach (IMenu item in (from m in _menus.Slinq()
			where m != null
			select m).NoLeak())
		{
			item.Dispose();
		}
		foreach (IMenu item2 in (from m in LuaMenus.Slinq()
			where m != null
			select m).NoLeak())
		{
			item2.Dispose();
		}
	}

	public void Reload(bool includeChildren = true, bool canOpen = true)
	{
		if (ReopenOnReload && canOpen)
		{
			Close();
		}
		if (!Opened && Loaded)
		{
			UnloadAll(includeChildren);
			ReloadSelf();
			LoadSelf();
			LoadChildren(includeChildren, canOpen: false);
			if (ReopenOnReload && canOpen)
			{
				Open();
			}
		}
	}

	protected virtual void ReloadSelf()
	{
	}

	public void Load(bool includeChildren = true, bool canOpen = true)
	{
		if (!Opened && !Loaded)
		{
			LoadAll(includeChildren);
			if (OpenOnLoad && canOpen)
			{
				Open();
			}
		}
	}

	protected void LoadAll(bool includeChildren = true)
	{
		LoadSelf();
		if (includeChildren)
		{
			LoadChildren(includeChildren: true, canOpen: false);
		}
	}

	protected virtual void LoadSelf()
	{
		LoadPanels();
		LoadMenus();
		LoadConnections();
		Connect(MenuState.Loaded, includeChildren: false);
		Loaded = true;
		if (LuaTable != null)
		{
			LuaTable["loaded"] = TmpObjTrue;
		}
		if (LuaDriven && OnLoadedHook != null)
		{
			LuaHookBase.InvokeFromCLR(OnLoadedHook);
		}
	}

	private void LoadPanels()
	{
		AddPanels();
		BuilderUtility.AssignTree(this);
	}

	private void LoadMenus()
	{
		AddMenus();
	}

	private void LoadConnections()
	{
		AddSignals();
		AddHookConnections();
	}

	private void LoadChildren(bool includeChildren = true, bool canOpen = true)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.Load(includeChildren, canOpen);
		}
	}

	public void Unload(bool includeChildren = true)
	{
		if (!Opened)
		{
			UnloadAll(includeChildren);
		}
	}

	private void UnloadAll(bool includeChildren)
	{
		if (includeChildren)
		{
			UnloadChildren();
		}
		UnloadSelf();
	}

	protected virtual void UnloadSelf()
	{
		Loaded = false;
		if (LuaTable != null)
		{
			LuaTable.Remove("loaded");
		}
		Disconnect(MenuState.Loaded, includeChildren: false);
		UnloadPanels();
		UnloadMenus();
		UnloadConnections();
	}

	private void UnloadPanels()
	{
		BuilderUtility.UnassignTree(this);
		RemoveAllPanels();
		RemoveAllLuaPanels();
	}

	private void UnloadMenus()
	{
		RemoveAllMenus();
	}

	private void UnloadConnections()
	{
		RemoveAllHookConnections();
		RemoveAllSignalConnections();
	}

	private void UnloadChildren(bool includeChildren = true)
	{
		foreach (IMenu item in (from m in _menus.Slinq()
			where m != null
			select m).NoLeak())
		{
			item.Unload(includeChildren);
		}
		foreach (IMenu item2 in (from m in LuaMenus.Slinq()
			where m != null
			select m).NoLeak())
		{
			item2.Unload(includeChildren);
		}
	}

	public void Open()
	{
		if (IsDebug)
		{
			logger.Info(PrettyTypeName + "\nOpen?");
		}
		if (CanOpen)
		{
			if (IsDebug)
			{
				logger.Info(PrettyTypeName + "\nOpen");
			}
			if (ReloadOnOpen)
			{
				Reload();
			}
			if (LoadOnOpen)
			{
				Load();
			}
			OpenAll();
		}
	}

	private void OpenAll()
	{
		OpenSelf();
		if (OpensSubMenus)
		{
			OpenChildren();
		}
	}

	protected virtual void OpenSelf()
	{
		BuilderUtility.AssignTree(this);
		BuilderUtility.InstantiateTree(this, InstantiateOnlyIfMissing);
		BuilderUtility.ParentTree(this);
		BuilderUtility.ResetTree(this);
		BuilderUtility.QueueRefreshTree(this);
		Connect(MenuState.Opened, includeChildren: false);
		Opened = true;
	}

	public void OpenChildren()
	{
		foreach (IMenu item in (from m in _menus.Slinq()
			where m != null
			select m).NoLeak())
		{
			if (!GetMenuSkipFlag(item) && !item.GetToggleSkipFlag())
			{
				item.Open();
			}
		}
	}

	public void Close()
	{
		Close(includeChildren: true);
	}

	public void Close(bool includeChildren)
	{
		if (CanClose)
		{
			CloseAll(includeChildren);
			if (UnloadOnClose)
			{
				Unload();
			}
		}
	}

	private void CloseAll(bool includeChildren)
	{
		BeforeCloseAll();
		if (includeChildren)
		{
			CloseChildren();
		}
		CloseSelf();
	}

	protected virtual void BeforeCloseAll()
	{
	}

	protected virtual void CloseSelf()
	{
		Opened = false;
		Disconnect(MenuState.Opened, includeChildren: false);
		BuilderUtility.KillTree(this);
	}

	public void CloseChildren()
	{
		foreach (IMenu item in (from m in _menus.Slinq()
			where m != null
			select m).NoLeak())
		{
			item.Close();
		}
		foreach (IMenu item2 in (from m in LuaMenus.Slinq()
			where m != null
			select m).NoLeak())
		{
			item2.Close();
		}
	}

	public void Reopen()
	{
		if (!Closed)
		{
			Close();
			if (ReloadOnReopen)
			{
				Reload(includeChildren: true, canOpen: false);
			}
			Open();
		}
	}

	public void Toggle(bool flag)
	{
		if (flag)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	public void Toggle()
	{
		Toggle(Closed);
	}

	public void Reset()
	{
		Reset(includeChildren: true);
	}

	public void Reset(bool includeChildren)
	{
		if (Opened)
		{
			ResetSelf();
			if (includeChildren)
			{
				ResetChildren();
			}
		}
	}

	public void ResetSelf()
	{
		BuilderUtility.ResetTree(this);
	}

	public void ResetChildren()
	{
		foreach (IMenu item in (from m in _menus.Slinq()
			where m != null
			select m).NoLeak())
		{
			if (!GetMenuSkipFlag(item))
			{
				item.Reset();
			}
		}
	}

	public void Refresh()
	{
		Refresh(includeChildren: true);
	}

	public void Refresh(bool includeChildren)
	{
		if (Opened)
		{
			BuilderUtility.QueueRefreshTree(this);
			if (includeChildren)
			{
				RefreshAtMenus();
			}
		}
	}

	public void RefreshAtMenus()
	{
		foreach (IMenu item in (from m in _menus.Slinq()
			where m != null
			select m).NoLeak())
		{
			if (!GetMenuSkipFlag(item))
			{
				item.Refresh();
			}
		}
	}

	protected virtual MenuHandle AddMenus()
	{
		return new MenuHandle(this);
	}

	protected virtual PanelHandle AddPanels()
	{
		return new PanelHandle(this);
	}

	public void AddRootLuaPanel(Table panel)
	{
		RootLuaPanels.Add(panel);
	}

	public void AddLuaPanel(Table panel)
	{
		LuaPanels.Add(panel);
	}

	public void AddLuaMenu(IMenu menu)
	{
		LuaMenus.Add(menu);
	}

	public void AddLuaConnection(HookConnection hookConnection)
	{
		LuaConnections.Add(hookConnection);
	}

	public void RemoveAllLuaPanels()
	{
		foreach (Table luaPanel in LuaPanels)
		{
			LuiBuilder.PanelMap.Remove(luaPanel);
		}
		LuaPanels.Clear();
		RootLuaPanels.Clear();
	}

	public void RemoveAllLuaMenus()
	{
		LuaMenus.Clear();
	}

	public void RemoveAllLuaConnections()
	{
		LuaConnections.Clear();
	}

	protected virtual HookConnectionHandle AddHookConnections()
	{
		return new HookConnectionHandle(this);
	}

	public void Connect(MenuState e, bool includeChildren)
	{
		ConnectSignals(e, includeChildren: false);
		ConnectHooks(e, includeChildren: false);
		if (includeChildren)
		{
			ConnectAtSubMenus(e, includeChildren: true);
		}
	}

	public void ConnectSignals(MenuState e, bool includeChildren)
	{
		LifeSignalConnectionBase.Connect(this, e);
		if (includeChildren)
		{
			ConnectSignalsAtSubMenus(e, includeChildren: true);
		}
	}

	public void ConnectHooks(MenuState e, bool includeChildren)
	{
		foreach (HookConnection hookConnection in _hookConnections)
		{
			if (hookConnection.E == e)
			{
				hookConnection.ConnectUntil(_disconnectHook[e]);
			}
		}
		if (includeChildren)
		{
			ConnectHooksAtSubMenus(e, includeChildren: true);
		}
	}

	public void Disconnect(MenuState e, bool includeChildren)
	{
		if (includeChildren)
		{
			DisconnectAtSubMenus(e, includeChildren: true);
		}
		DisconnectSignals(e, includeChildren: false);
		DisconnectHooks(e, includeChildren: false);
	}

	public void DisconnectSignals(MenuState e, bool includeChildren)
	{
		LifeSignalConnectionBase.Disconnect(this, e);
		if (includeChildren)
		{
			DisconnectSignalsAtSubMenus(e, includeChildren: true);
		}
	}

	public void DisconnectHooks(MenuState e, bool includeChildren)
	{
		if (_disconnectHook.ContainsKey(e))
		{
			LuaHookBase.InvokeFromCLR(_disconnectHook[e]);
		}
		if (includeChildren)
		{
			DisconnectHooksAtSubMenus(e, includeChildren: true);
		}
	}

	public void DisconnectAll(bool includeChildren)
	{
		if (includeChildren)
		{
			DisconnectAllAtSubMenus(includeChildren: true);
		}
		DisconnectAllSignals(includeChildren: false);
		DisconnectAllHooks(includeChildren: false);
	}

	public void DisconnectAllSignals(bool includeChildren)
	{
		LifeSignalConnectionBase.Disconnect(this, MenuState.Loaded);
		LifeSignalConnectionBase.Disconnect(this, MenuState.Opened);
		if (includeChildren)
		{
			DisconnectAllSignalsAtSubMenus(includeChildren: true);
		}
	}

	public void DisconnectAllHooks(bool includeChildren)
	{
		LuaHookBase.InvokeFromCLR(_disconnectHook[MenuState.Loaded]);
		LuaHookBase.InvokeFromCLR(_disconnectHook[MenuState.Opened]);
		if (includeChildren)
		{
			DisconnectAllHooksAtSubMenus(includeChildren: true);
		}
	}

	public void ConnectAtSubMenus(MenuState e, bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.Connect(e, includeChildren);
		}
	}

	public void ConnectSignalsAtSubMenus(MenuState e, bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.ConnectSignals(e, includeChildren);
		}
	}

	public void ConnectHooksAtSubMenus(MenuState e, bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.ConnectHooks(e, includeChildren);
		}
	}

	public void DisconnectAtSubMenus(MenuState e, bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.Disconnect(e, includeChildren);
		}
	}

	public void DisconnectSignalsAtSubMenus(MenuState e, bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.DisconnectSignals(e, includeChildren);
		}
	}

	public void DisconnectHooksAtSubMenus(MenuState e, bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.DisconnectHooks(e, includeChildren);
		}
	}

	public void DisconnectAllAtSubMenus(bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.DisconnectAll(includeChildren);
		}
	}

	public void DisconnectAllSignalsAtSubMenus(bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.DisconnectAllSignals(includeChildren);
		}
	}

	public void DisconnectAllHooksAtSubMenus(bool includeChildren)
	{
		foreach (IMenu item in (from subMenu in _menus.Slinq()
			where subMenu != null
			select subMenu).NoLeak())
		{
			item.DisconnectAllHooks(includeChildren);
		}
	}

	public void SetKeyBindingCategories(bool flag)
	{
		_cameraKeyBindings.Set(flag);
		_generalKeyBindings.Set(flag);
		_weMenuKeyBindings.Set(flag);
		_weTerrainMenuKeyBindings.Set(flag);
		_weMarkersMenuKeyBindings.Set(flag);
		_debugBindings.Set(flag);
	}

	public void ResetKeyBindingCategories()
	{
		_cameraKeyBindings.Reset();
		_generalKeyBindings.Reset();
		_weMenuKeyBindings.Reset();
		_weTerrainMenuKeyBindings.Reset();
		_weMarkersMenuKeyBindings.Reset();
		_debugBindings.Reset();
	}

	public void RemoveAllPanels()
	{
		Panels.Clear();
	}

	public void RemoveAllMenus()
	{
		_menus.Clear();
	}

	public void AddHookConnection(MenuState a, LuaHookBase h, Closure c)
	{
		_hookConnections.Add(new HookConnection(h, c, a));
	}

	public void RemoveAllHookConnections()
	{
		_hookConnections.Clear();
	}

	public void RemoveAllSignalConnections()
	{
		LifeSignalConnectionBase.Clear(this);
	}

	public void SetLuaBuilderFlag(BuilderFlag builderFlag, Closure c)
	{
		BuilderFlags |= builderFlag;
		FlagClosure.Add(builderFlag, c);
	}

	public void ClearLuaBuilderFlag(BuilderFlag builderFlag)
	{
		BuilderFlags &= ~builderFlag;
		FlagClosure.Remove(builderFlag);
	}

	public void ClearAllLuaBuilderFlags()
	{
		BuilderFlags = (BuilderFlag)0;
		FlagClosure.Clear();
	}

	public bool HasLuaBuilderFlag(BuilderFlag n)
	{
		bool flag = (BuilderFlags & n) != 0;
		if (flag && FlagClosure.ContainsKey(n) && FlagClosure[n] != null)
		{
			return FlagClosure[n].Call().Boolean;
		}
		return flag;
	}

	public List<IPanel> GetPanels()
	{
		return Panels;
	}

	public RectTransform GetTrsForChild(IPanel child)
	{
		return GetParent();
	}

	public RectTransform GetRootTrs()
	{
		return GetParent();
	}
}
