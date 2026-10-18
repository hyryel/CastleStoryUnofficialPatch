using System;
using Brix.Components;
using Brix.Lua;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Menu.Component;

public class LuiMenu : BrixComponent
{
	[Flags]
	public enum WaitForFlag
	{
		FactoriesReady = 1,
		GoEnabled = 2
	}

	[Flags]
	public enum ThenDoFlag
	{
		LoadRoot = 1,
		SetPath = 2,
		Open = 4,
		Show = 8
	}

	[SerializeField]
	private string _rootPath = string.Empty;

	[SerializeField]
	private string _path = string.Empty;

	[SerializeField]
	private WaitForFlag _waitFor = WaitForFlag.FactoriesReady | WaitForFlag.GoEnabled;

	[SerializeField]
	private ThenDoFlag _thenDo = ThenDoFlag.LoadRoot | ThenDoFlag.SetPath | ThenDoFlag.Open | ThenDoFlag.Show;

	private bool _factoriesReady;

	private bool _goReady;

	private bool _isRootLoaded;

	private bool _hasSetPath;

	private bool _isOpened;

	private bool _isShown;

	private bool _isRequested;

	private DynValue _dynValueMenu = DynValue.Nil;

	private Script _script;

	private static readonly DynValue DynValueStrPath = DynValue.NewString("_str_path");

	public LuaHook hk_Destroy = new LuaHook();

	public LuaHook<string> hk_SetPath = new LuaHook<string>();

	public LuaHook hk_Open = new LuaHook();

	public LuaHook hk_Close = new LuaHook();

	public LuaHook hk_Reopen = new LuaHook();

	public LuaHook hk_Show = new LuaHook();

	public LuaHook hk_Hide = new LuaHook();

	public LuaHook hk_Reshow = new LuaHook();

	public LuaHook hk_Refresh = new LuaHook();

	public LuaHook hk_Request = new LuaHook();

	public LuaHook hk_Rerequest = new LuaHook();

	public LuaHook hk_Release = new LuaHook();

	public LuaHook hk_Apply = new LuaHook();

	public string RootPath
	{
		get
		{
			return _rootPath;
		}
		set
		{
			_rootPath = value;
		}
	}

	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			_path = value;
		}
	}

	public bool WaitForFactoriesReady => WaitFor.HasFlag(WaitForFlag.FactoriesReady);

	public bool WaitForGoReady => WaitFor.HasFlag(WaitForFlag.GoEnabled);

	public WaitForFlag WaitFor
	{
		get
		{
			return _waitFor;
		}
		set
		{
			_waitFor = value;
		}
	}

	public ThenDoFlag ThenDo
	{
		get
		{
			return _thenDo;
		}
		set
		{
			_thenDo = value;
		}
	}

	private bool Ready
	{
		get
		{
			bool flag = true;
			if (WaitForFactoriesReady)
			{
				flag = flag && _factoriesReady;
			}
			if (WaitForGoReady)
			{
				flag = flag && _goReady;
			}
			return flag;
		}
	}

	public bool IsRootLoaded
	{
		get
		{
			return _isRootLoaded;
		}
		private set
		{
			_isRootLoaded = value;
		}
	}

	public bool CanSetRootPath => Ready && !IsRootLoaded;

	public bool CanLoadRoot => Ready && !IsRootLoaded;

	public bool CanUnloadRoot => Ready && IsRootLoaded && !IsOpened;

	public bool CanReloadRoot => Ready && IsRootLoaded && !IsOpened;

	public bool HasSetPath
	{
		get
		{
			return _hasSetPath;
		}
		private set
		{
			_hasSetPath = value;
		}
	}

	public bool IsOpened
	{
		get
		{
			return _isOpened;
		}
		private set
		{
			_isOpened = value;
		}
	}

	public bool IsShown
	{
		get
		{
			return _isShown;
		}
		private set
		{
			_isShown = value;
		}
	}

	public bool IsRequested
	{
		get
		{
			return _isRequested;
		}
		private set
		{
			_isRequested = value;
		}
	}

	public bool CanSetPath => Ready && IsRootLoaded && !IsOpened;

	public bool CanOpen => Ready && IsRootLoaded && HasSetPath && !IsOpened;

	public bool CanClose => Ready && IsRootLoaded && HasSetPath && IsOpened && !IsShown;

	public bool CanReopen => Ready && IsRootLoaded && HasSetPath && IsOpened && !IsShown;

	public bool CanRefresh => Ready && IsRootLoaded && HasSetPath && IsOpened;

	public bool CanShow => Ready && IsRootLoaded && HasSetPath && IsOpened && !IsShown;

	public bool CanHide => Ready && IsRootLoaded && HasSetPath && IsOpened && IsShown;

	public bool CanReshow => Ready && IsRootLoaded && HasSetPath && IsOpened && IsShown;

	public bool CanRequest => Ready && IsRootLoaded && HasSetPath && IsOpened && !IsRequested;

	public bool CanRelease => Ready && IsRootLoaded && HasSetPath && IsOpened && IsRequested;

	public bool CanRerequest => Ready && IsRootLoaded && HasSetPath && IsOpened && IsRequested;

	public bool CanApply => Ready && IsRootLoaded && HasSetPath && IsOpened && IsRequested;

	protected override void Start()
	{
		base.Start();
		_factoriesReady = false;
		FactorySignalInvoker.OnFactoriesReadySig.LateConnectOnce(OnFactoriesReady).DisconnectWhen(base.OnReleaseSignal);
		base.OnReleaseSignal.ConnectOnce(OnRelease);
	}

	private void OnEnable()
	{
		_goReady = true;
		if (WaitForGoReady)
		{
			OnReady();
		}
	}

	private void OnDisable()
	{
		_goReady = false;
	}

	protected override void OnDestroy()
	{
		OnRelease();
		base.OnDestroy();
	}

	private void OnRelease()
	{
	}

	private void OnFactoriesReady()
	{
		_factoriesReady = true;
		if (WaitForFactoriesReady)
		{
			OnReady();
		}
	}

	private void OnReady()
	{
		if (Ready)
		{
			if (ThenDo.HasFlag(ThenDoFlag.LoadRoot))
			{
				LoadRoot();
			}
			if (ThenDo.HasFlag(ThenDoFlag.SetPath))
			{
				SetPath();
			}
			if (ThenDo.HasFlag(ThenDoFlag.Open))
			{
				Open();
			}
			if (ThenDo.HasFlag(ThenDoFlag.Show))
			{
				Show();
			}
		}
	}

	public void LoadRoot()
	{
		if (CanLoadRoot)
		{
			IsRootLoaded = DoLoadRoot();
		}
	}

	public void ReloadRoot()
	{
		if (CanReloadRoot)
		{
			UnloadRoot();
			LoadRoot();
		}
	}

	public void UnloadRoot()
	{
		if (CanUnloadRoot)
		{
			IsRootLoaded = false;
			HasSetPath = false;
			LuaHookBase.InvokeFromCLR(hk_Destroy);
			DoUnloadRoot();
		}
	}

	public void SetPath()
	{
		if (CanSetPath)
		{
			HasSetPath = true;
			LuaHookBase.InvokeFromCLR(hk_SetPath, Path);
		}
	}

	public void Open()
	{
		if (CanOpen)
		{
			IsOpened = true;
			LuaHookBase.InvokeFromCLR(hk_Open);
		}
	}

	public void Close()
	{
		if (CanClose)
		{
			IsOpened = false;
			LuaHookBase.InvokeFromCLR(hk_Close);
		}
	}

	public void Reopen()
	{
		if (CanReopen)
		{
			LuaHookBase.InvokeFromCLR(hk_Reopen);
		}
	}

	public void Show()
	{
		if (CanShow)
		{
			IsShown = true;
			LuaHookBase.InvokeFromCLR(hk_Show);
		}
	}

	public void Hide()
	{
		if (CanHide)
		{
			IsShown = false;
			LuaHookBase.InvokeFromCLR(hk_Hide);
		}
	}

	public void Reshow()
	{
		if (CanReshow)
		{
			LuaHookBase.InvokeFromCLR(hk_Reshow);
		}
	}

	public void Refresh()
	{
		if (CanRefresh)
		{
			LuaHookBase.InvokeFromCLR(hk_Refresh);
		}
	}

	public void Request()
	{
		if (CanRequest)
		{
			IsRequested = true;
			LuaHookBase.InvokeFromCLR(hk_Request);
		}
	}

	public void Release()
	{
		if (CanRelease)
		{
			IsRequested = false;
			LuaHookBase.InvokeFromCLR(hk_Release);
		}
	}

	public void Rerequest()
	{
		if (CanRerequest)
		{
			LuaHookBase.InvokeFromCLR(hk_Rerequest);
		}
	}

	public void Apply()
	{
		if (CanApply)
		{
			LuaHookBase.InvokeFromCLR(hk_Apply);
		}
	}

	private void AddToTable(Table t)
	{
		t["LuiRoot"] = new Table(t.OwnerScript);
		Table table = t.Get("LuiRoot").Table;
		table["hk_Destroy"] = hk_Destroy;
		table["hk_SetPath"] = hk_SetPath;
		table["hk_Open"] = hk_Open;
		table["hk_Close"] = hk_Close;
		table["hk_Reopen"] = hk_Reopen;
		table["hk_Show"] = hk_Show;
		table["hk_Hide"] = hk_Hide;
		table["hk_Reshow"] = hk_Reshow;
		table["hk_Refresh"] = hk_Refresh;
		table["hk_Request"] = hk_Request;
		table["hk_Rerequest"] = hk_Rerequest;
		table["hk_Release"] = hk_Release;
		table["hk_Apply"] = hk_Apply;
		table["ld"] = DynValue.FromObject(t.OwnerScript, GetComponent<LuaDriver>());
	}

	public bool DoLoadRoot()
	{
		if (string.IsNullOrEmpty(RootPath))
		{
			return false;
		}
		try
		{
			_dynValueMenu = LuaLoader.Load(out _script, RootPath, delegate(Script script)
			{
				LuaLibs.BigLib(script);
				AddToTable(script.Globals);
			}, null);
			_dynValueMenu.Table.Set(DynValueStrPath, DynValue.NewString(RootPath));
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
			return false;
		}
		return true;
	}

	public void DoUnloadRoot()
	{
		_dynValueMenu = DynValue.Nil;
		_script = null;
	}

	[LuaDrivable("LoadRoot")]
	public static DynValue lua_LoadRoot(ScriptExecutionContext sec, CallbackArguments ca)
	{
		LuiMenu luiMenu = ca.AsComponent<LuiMenu>(0, "LoadRoot");
		luiMenu.LoadRoot();
		return luiMenu._dynValueMenu;
	}

	[LuaDrivable("UnloadRoot")]
	public static DynValue lua_UnloadRoot(ScriptExecutionContext sec, CallbackArguments ca)
	{
		LuiMenu luiMenu = ca.AsComponent<LuiMenu>(0, "UnloadRoot");
		luiMenu.UnloadRoot();
		return DynValue.Void;
	}

	[LuaDrivable("LoadFromClosure")]
	public static DynValue lua_LoadFromClosure(ScriptExecutionContext sec, CallbackArguments ca)
	{
		return DynValue.Void;
	}
}
