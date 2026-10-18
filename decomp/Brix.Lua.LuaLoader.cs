using System;
using System.Collections.Generic;
using System.IO;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public class LuaLoader : MonoBehaviour
{
	public enum LoadingBehaviour
	{
		OnAwake,
		OnStart,
		OnFactoryReady,
		LevelFirstLoaded,
		LevelFirstReady,
		LevelLoaded,
		LevelReady,
		LevelSaveLoaded,
		LevelSaveReady
	}

	private static readonly DynValue[] _noArgs = new DynValue[0];

	public string scriptPath;

	public LoadingBehaviour loadingBehaviour;

	public Script runningScript;

	private Signal OnDestroyed = new Signal();

	private static GameObject _standaloneParent;

	public static GameObject standaloneParent
	{
		get
		{
			if (_standaloneParent == null)
			{
				_standaloneParent = GameObject.Find("Jeu/LuaScripts");
			}
			return _standaloneParent;
		}
	}

	public static DynValue Load(string path)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, path, null, _noArgs, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue Load(string path, IList<DynValue> args)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, path, null, args, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue Load(out Script outscript, string path)
	{
		return Load_Impl(out outscript, path, null, _noArgs, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
	}

	public static DynValue Load(out Script outscript, string path, IList<DynValue> args)
	{
		return Load_Impl(out outscript, path, null, args, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
	}

	public static DynValue Load(string path, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, path, null, _noArgs, preloadAction, postloadAction);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue Load(string path, IList<DynValue> args, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, path, null, args, preloadAction, postloadAction);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue Load(out Script outscript, string path, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		return Load_Impl(out outscript, path, null, _noArgs, preloadAction, postloadAction);
	}

	public static DynValue Load(out Script outscript, string path, IList<DynValue> args, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		return Load_Impl(out outscript, path, null, args, preloadAction, postloadAction);
	}

	public static DynValue LoadString(string code)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, null, code, _noArgs, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue LoadString(string code, IList<DynValue> args)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, null, code, args, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue LoadString(out Script outscript, string code)
	{
		return Load_Impl(out outscript, null, code, _noArgs, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
	}

	public static DynValue LoadString(out Script outscript, string code, IList<DynValue> args)
	{
		return Load_Impl(out outscript, null, code, args, delegate(Script script)
		{
			LuaLibs.BigLib(script);
		}, null);
	}

	public static DynValue LoadString(string code, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, null, code, _noArgs, preloadAction, postloadAction);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue LoadString(string code, IList<DynValue> args, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		Script outscript = null;
		DynValue result = Load_Impl(out outscript, null, code, args, preloadAction, postloadAction);
		Script.Kill(ref outscript);
		return result;
	}

	public static DynValue LoadString(out Script outscript, string code, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		return Load_Impl(out outscript, null, code, _noArgs, preloadAction, postloadAction);
	}

	public static DynValue LoadString(out Script outscript, string code, IList<DynValue> args, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		return Load_Impl(out outscript, null, code, args, preloadAction, postloadAction);
	}

	private static DynValue Load_Impl(out Script outscript, string path, string code, IList<DynValue> args, Action<Script> preloadAction, Action<Script> postloadAction)
	{
		Script script = null;
		script = new Script();
		script.Options.ColonOperatorClrCallbackBehaviour = ColonOperatorBehaviour.TreatAsColon;
		script.FriendlyName = path ?? "LuaString";
		if (path != null)
		{
			path = path.Replace("\\", "/");
			string directoryName = Path.GetDirectoryName(path);
			script.Options.ScriptLoader = new BrixScriptLoader(directoryName + "/?;" + directoryName + "/?.lua");
		}
		else
		{
			script.Options.ScriptLoader = new BrixScriptLoader(string.Empty);
		}
		script.Globals["print"] = LuaLibs.LuaPrint;
		preloadAction?.Invoke(script);
		DynValue result = script.Call((path != null) ? script.LoadFile(path, null, path) : script.LoadString(code, null, "LoadStringFunc"), args);
		postloadAction?.Invoke(script);
		outscript = script;
		return result;
	}

	public static DynValue LoadPure(string path, bool forReal = false)
	{
		Script tokill = new Script((!forReal) ? (CoreModules.Basic | CoreModules.LoadMethods) : CoreModules.None);
		tokill.Options.ScriptLoader = new BrixScriptLoader(string.Empty);
		DynValue result = tokill.DoFile(path);
		Script.Kill(ref tokill);
		return result;
	}

	public static DynValue LoadPureBytecode(string path)
	{
		Script tokill = new Script(CoreModules.None);
		tokill.Options.ScriptLoader = new BrixScriptLoader(string.Empty);
		DynValue result = tokill.DoFile(path);
		File.Delete(path + ".bytecode");
		string[] byteCode = tokill.GetByteCode();
		foreach (string text in byteCode)
		{
			File.AppendAllText(path + ".bytecode", text + "\n");
		}
		Script.Kill(ref tokill);
		return result;
	}

	public static void LoadUserKeyBindingLuaFile()
	{
		Load("userconfigs.lua");
		BindingsProvider.Reload();
	}

	private void Awake()
	{
		if (loadingBehaviour == LoadingBehaviour.OnFactoryReady)
		{
			FactorySignalInvoker.OnFactoriesReadySig.LateConnect(ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.OnAwake)
		{
			Load(out runningScript, scriptPath);
		}
	}

	private void Start()
	{
		if (loadingBehaviour == LoadingBehaviour.LevelFirstLoaded)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelFirstLoaded, ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.LevelFirstReady)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelFirstReady, ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.LevelLoaded)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelLoaded, ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.LevelReady)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelReady, ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.LevelSaveLoaded)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelSaveLoaded, ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.LevelSaveReady)
		{
			GameSignals.LateConnectOnce(GameSignals.LevelSaveReady, ConnectLoadToSignal).DisconnectWhen(OnDestroyed);
		}
		else if (loadingBehaviour == LoadingBehaviour.OnStart)
		{
			Load(out runningScript, scriptPath);
		}
	}

	private void OnDestroy()
	{
		if (runningScript != null)
		{
			Script.Kill(ref runningScript);
		}
		OnDestroyed.Invoke();
	}

	private void ConnectLoadToSignal()
	{
		Load(out runningScript, scriptPath);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
