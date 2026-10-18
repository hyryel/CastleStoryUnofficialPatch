using System;
using System.Collections;
using Brix.External.Signals;
using Brix.Input;
using Brix.Lua;
using Brix.UI.Builder.Lui;
using MoonSharp.Interpreter;
using Rewired;
using UnityEngine;

namespace Brix.UI.Builder.Menu.Component;

public class LuaMenuComponent : MenuComponent
{
	public string Path = string.Empty;

	public bool LoadOnStart = true;

	public bool WaitForLevelLoaded;

	public LuaHook LoadHook;

	public LuaHook ReloadHook;

	public LuaHook OpenHook;

	public LuaHook ReopenHook;

	public LuaHook CloseHook;

	[LuaDrivable("hk_OnLoaded")]
	public LuaHook OnLoadedHook;

	public LuaHook<LuaDriver> OnTestHook;

	private LuaHook lua_OnSetIsShift = new LuaHook();

	private LuaHook lua_OnSetIsCtrl = new LuaHook();

	private static readonly Func<bool> IsShift = () => PickingUtility.Player.GetButton("cam_mouse_alt");

	private static readonly Func<bool> IsCtrl = () => PickingUtility.Player.GetButton("cam_mouse_sec");

	[LuaDrivable("hk_OnMenuLoaded")]
	public LuaHook OnMenuLoadedHook = new LuaHook();

	private bool _loadedOnce;

	private bool _firstRun = true;

	private bool _wasCtrl;

	private bool _wasShift;

	private Action<InputActionEventData> _onToggleModifierKeysAction;

	private readonly Signal _onDisableSignal = new Signal();

	public CanvasGroup CanvasGroup;

	private Script _ownerScript;

	private bool _isOwnerScriptOwner;

	private Func<DynValue, LuaMenuComponent, bool> _isStop;

	private DynValue _tMenu;

	public static int CoroutineCount = 0;

	private static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

	[LuaDrivable("Id")]
	public long Id => base.Menu.LuaId;

	protected override MenuType Type => MenuType.LuaMenu;

	public bool LoadedFromLua { get; set; }

	protected override bool Ready => ObserverReady && FactoriesReady && GoReady && LevelReady && GameReady;

	[LuaDrivable("loaded")]
	public bool Loaded => _menu != null;

	[LuaDrivable("LoadMenuFromClosure")]
	private static DynValue LoadMenuTable(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaMenuComponent luaMenuComponent = args.AsComponent<LuaMenuComponent>(0, "LoadMenuFromClosure");
		Closure function = args.AsType(1, "LoadMenuFromClosure", DataType.Function).Function;
		luaMenuComponent.LoadedFromLua = true;
		luaMenuComponent.DropScript();
		luaMenuComponent._menu = LuaMenu.LoadMenuTable(function, luaMenuComponent.GetParent, luaMenuComponent.OnMenuLoadedHook, asSubMenu: false, doApply: true, doLog: false);
		luaMenuComponent.AdoptScript(context.OwnerScript, asOwner: false);
		LuaHookBase.InvokeFromCLR(luaMenuComponent.OnLoadedHook);
		return DynValue.NewTable(luaMenuComponent._menu.LuaTable);
	}

	protected override void Start()
	{
		base.Start();
		InitHooks();
	}

	protected override void Release()
	{
		base.Release();
		LoadHook = null;
		ReloadHook = null;
		OpenHook = null;
		ReopenHook = null;
		CloseHook = null;
		OnLoadedHook = null;
		OnTestHook = null;
		if (lua_OnSetIsShift != null)
		{
			lua_OnSetIsShift.DisconnectAll();
			lua_OnSetIsShift = null;
		}
		if (lua_OnSetIsCtrl != null)
		{
			lua_OnSetIsCtrl.DisconnectAll();
			lua_OnSetIsCtrl = null;
		}
		if (OnMenuLoadedHook != null)
		{
			OnMenuLoadedHook.DisconnectAll();
			OnMenuLoadedHook = null;
		}
		if (_onDisableSignal != null)
		{
			_onDisableSignal.Clear();
		}
		_onToggleModifierKeysAction = null;
		CanvasGroup = null;
		_ownerScript = null;
		_isStop = null;
	}

	protected override void TryBegin()
	{
		if (LoadOnStart && Ready && !_loadedOnce)
		{
			_loadedOnce = Load();
		}
	}

	private void Update()
	{
		TryBegin();
	}

	private void InitHooks()
	{
		LoadHook = new LuaHook();
		ReloadHook = new LuaHook();
		OpenHook = new LuaHook();
		ReopenHook = new LuaHook();
		CloseHook = new LuaHook();
		OnLoadedHook = new LuaHook();
		OnTestHook = new LuaHook<LuaDriver>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_onToggleModifierKeysAction == null)
		{
			_onToggleModifierKeysAction = delegate
			{
				if (base.Menu != null && base.Menu.Opened)
				{
					if (_firstRun)
					{
						_firstRun = false;
						_wasCtrl = IsCtrl();
						_wasShift = IsShift();
						if (_wasCtrl)
						{
							LuaHookBase.InvokeFromCLR(lua_OnSetIsCtrl);
						}
						if (_wasShift)
						{
							LuaHookBase.InvokeFromCLR(lua_OnSetIsShift);
						}
					}
					else
					{
						bool flag = IsCtrl();
						bool flag2 = IsShift();
						if (_wasCtrl != flag)
						{
							LuaHookBase.InvokeFromCLR(lua_OnSetIsCtrl);
						}
						if (_wasShift != flag2)
						{
							LuaHookBase.InvokeFromCLR(lua_OnSetIsShift);
						}
						_wasCtrl = flag;
						_wasShift = flag2;
					}
				}
			};
		}
		PickingUtility.Player.AddInputEventDelegate(_onToggleModifierKeysAction, UpdateLoopType.Update);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		PickingUtility.Player.RemoveInputEventDelegate(_onToggleModifierKeysAction, UpdateLoopType.Update);
		_onDisableSignal.Invoke();
	}

	private void AddToTable(Table t, Script script)
	{
		t["hk_LoadMenu"] = LoadHook;
		t["hk_ReloadMenu"] = ReloadHook;
		t["hk_OpenMenu"] = OpenHook;
		t["hk_ReopenMenu"] = ReopenHook;
		t["hk_CloseMenu"] = CloseHook;
		Table value = new Table(t.OwnerScript);
		t["Bricktrons"] = value;
		t["hk_OnTest"] = OnTestHook;
		t["hk_OnSetIsShift"] = lua_OnSetIsShift;
		t["hk_OnSetIsCtrl"] = lua_OnSetIsCtrl;
		t["IsShift"] = IsShift;
		t["IsCtrl"] = IsCtrl;
		LuaRegistry.HookScript(t.OwnerScript);
	}

	[LuaDrivable("SetVisible")]
	private static DynValue lua_SetVisible(ScriptExecutionContext sec, CallbackArguments ca)
	{
		LuaMenuComponent luaMenuComponent = ca.AsComponent<LuaMenuComponent>(0, "SetVisible");
		bool flag = ca.AsBool(1, "SetVisible");
		if (luaMenuComponent.CanvasGroup != null)
		{
			luaMenuComponent.CanvasGroup.alpha = ((!flag) ? 0f : 1f);
			luaMenuComponent.CanvasGroup.blocksRaycasts = flag;
		}
		return DynValue.Void;
	}

	[LuaDrivable("LoadMenu")]
	private static DynValue lua_Load(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaMenuComponent luaMenuComponent = args.AsComponent<LuaMenuComponent>(0, "LoadMenu");
		return DynValue.NewBoolean(luaMenuComponent.Load());
	}

	public bool Load()
	{
		if (!string.IsNullOrEmpty(Path))
		{
			DropScript();
			_tMenu = LuaMenu.ReadMenuTable(out var outscript, Path, AddToTable);
			AdoptScript(outscript, asOwner: true);
			if (_tMenu.Type == DataType.Tuple)
			{
				_isStop = delegate(DynValue t, LuaMenuComponent lmc)
				{
					t.Tuple[1].Coroutine.Resume();
					if (t.Tuple[1].Coroutine.State != CoroutineState.Dead)
					{
						return false;
					}
					lmc._menu = LuaMenu.LoadMenuTable(t.Tuple[0].Table, lmc.GetParent, lmc.OnMenuLoadedHook, lmc.Path, asSubMenu: false, doApply: true, doLog: false);
					return true;
				};
				StartCoroutine(DoCoroutine());
			}
			else
			{
				_menu = LuaMenu.LoadMenuTable(_tMenu.Table, base.GetParent, OnMenuLoadedHook, Path, asSubMenu: false, doApply: true, doLog: false);
			}
			return true;
		}
		return false;
	}

	public IEnumerator DoCoroutine()
	{
		CoroutineCount++;
		while (true)
		{
			bool stop;
			try
			{
				stop = _isStop(_tMenu, this);
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
				stop = true;
			}
			if (stop)
			{
				break;
			}
			yield return WaitForEndOfFrame;
		}
		CoroutineCount--;
		_isStop = null;
	}

	public void Reload()
	{
		_menu.Reload();
	}

	[LuaDrivable("UnloadMenu")]
	private static DynValue lua_Unload(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaMenuComponent luaMenuComponent = args.AsComponent<LuaMenuComponent>(0, "UnloadMenu");
		luaMenuComponent.Unload();
		return DynValue.Void;
	}

	public void Unload()
	{
		if (_menu != null)
		{
			if (LoadedFromLua && _menu.LuaTable != null && _menu.LuaTable.OwnerScript != null)
			{
				LuaRegistry.DishookScript(_menu.LuaTable.OwnerScript);
			}
			_menu.Close();
			_menu.Unload();
			_menu.Dispose();
		}
		_tMenu = default(DynValue);
		DropScript();
		_menu = null;
	}

	public void Open()
	{
		_menu.Open();
	}

	public void Refresh()
	{
		_menu.Refresh();
	}

	public void Close()
	{
		_menu.Close();
	}

	private void AdoptScript(Script newScripts, bool asOwner)
	{
		DropScript();
		_ownerScript = newScripts;
		_ownerScript.OnScriptKilled += _OnScriptKilled;
		_isOwnerScriptOwner = asOwner;
	}

	private void DropScript()
	{
		if (_ownerScript != null)
		{
			_ownerScript.OnScriptKilled -= _OnScriptKilled;
			LuiBuilder.MapHelper.RemoveAllAtScript(_ownerScript);
			if (_isOwnerScriptOwner)
			{
				Script.Kill(ref _ownerScript);
			}
			_ownerScript = null;
		}
		_isOwnerScriptOwner = false;
	}

	private void _OnScriptKilled(Script script)
	{
		if (_ownerScript != null)
		{
			LuiBuilder.MapHelper.RemoveAllAtScript(_ownerScript);
			_ownerScript = null;
		}
		_isOwnerScriptOwner = false;
	}
}
