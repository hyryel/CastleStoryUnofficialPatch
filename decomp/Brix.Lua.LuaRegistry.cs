using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Brix.Engine;
using Brix.External.Signals;
using Brix.IO.Serialization;
using Brix.Network;
using Brix.Utils;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Lua;

[RequireComponent(typeof(SerializationComponent))]
[JsonObject(MemberSerialization.OptIn)]
public class LuaRegistry : MonoBehaviour
{
	[JsonProperty]
	private static Dictionary<string, Dictionary<string, object>> _saveData;

	private static Dictionary<MoonSharp.Interpreter.Coroutine, Closure> _sleeping_Coroutines;

	private static Signal sig_start;

	private static Signal sig_update;

	private static Signal sig_fixedUpdate;

	private static Signal sig_slowUpdate;

	private static Signal sig_OnEnd;

	private static Signal sig_serializing;

	private static Signal sig_serialized;

	private static Signal sig_deserializing;

	private static Signal sig_deserialized;

	public static Signal<Table> sig_serverMessage;

	public static DynValue cb_connectLifecycle;

	public static DynValue cb_disconnectLifecycle;

	private static DynValue cb_coroutine_resumeWhen;

	public static DynValue cb_setSaveData;

	public static DynValue cb_getSaveData;

	private float _timeAtLastSlowUpdate;

	static LuaRegistry()
	{
		_saveData = new Dictionary<string, Dictionary<string, object>>();
		_sleeping_Coroutines = new Dictionary<MoonSharp.Interpreter.Coroutine, Closure>();
		sig_start = new Signal();
		sig_update = new Signal();
		sig_fixedUpdate = new Signal();
		sig_slowUpdate = new Signal();
		sig_OnEnd = new Signal();
		sig_serializing = new Signal();
		sig_serialized = new Signal();
		sig_deserializing = new Signal();
		sig_deserialized = new Signal();
		sig_serverMessage = new Signal<Table>();
		cb_connectLifecycle = DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			HookScript(con.OwnerScript);
			return DynValue.Void;
		});
		cb_disconnectLifecycle = DynValue.NewCallback(delegate(ScriptExecutionContext con, CallbackArguments args)
		{
			DishookScript(con.OwnerScript);
			return DynValue.Void;
		});
		cb_coroutine_resumeWhen = DynValue.NewCallback(CoroutineResumeCondition, "coroutine.resumeWhen");
		cb_setSaveData = DynValue.NewCallback(SetSaveData, "SetSaveData");
		cb_getSaveData = DynValue.NewCallback(GetSaveData, "GetSaveData");
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
	}

	private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		_saveData.Clear();
	}

	public static void HookScript(Script script)
	{
		script.Globals.Set("hk_Start", DynValue.FromObject(script, new LuaHook(sig_start)));
		script.Globals.Set("hk_Update", DynValue.FromObject(script, new LuaHook(sig_update)));
		script.Globals.Set("hk_FixedUpdate", DynValue.FromObject(script, new LuaHook(sig_fixedUpdate)));
		script.Globals.Set("hk_SlowUpdate", DynValue.FromObject(script, new LuaHook(sig_slowUpdate)));
		script.Globals.Set("hk_OnEnd", DynValue.FromObject(script, new LuaHook(sig_OnEnd)));
		script.Globals.Set("hk_OnSerializing", DynValue.FromObject(script, new LuaHook(sig_serializing)));
		script.Globals.Set("hk_OnSerialized", DynValue.FromObject(script, new LuaHook(sig_serializing)));
		script.Globals.Set("hk_OnDeserializing", DynValue.FromObject(script, new LuaHook(sig_deserializing)));
		script.Globals.Set("hk_OnDeserialized", DynValue.FromObject(script, new LuaHook(sig_deserialized)));
		script.Globals.Set("hk_OnServerMessage", DynValue.FromObject(script, new LuaHook<Table>(sig_serverMessage)));
		script.Globals.Set("coroutine.resumeWhen", DynValue.FromObject(script, cb_coroutine_resumeWhen));
	}

	public static void DishookScript(Script script)
	{
		if (script.Globals.Get("hk_Start").ToObject<LuaHook>() != null)
		{
			script.Globals.Get("hk_Start").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_Update").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_FixedUpdate").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_SlowUpdate").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_OnEnd").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_OnSerializing").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_OnSerialized").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_OnDeserializing").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_OnDeserialized").ToObject<LuaHook>().DisconnectAll();
			script.Globals.Get("hk_OnServerMessage").ToObject<LuaHook<Table>>().DisconnectAll();
			script.Globals.Set("hk_Start", DynValue.Nil);
			script.Globals.Set("hk_Update", DynValue.Nil);
			script.Globals.Set("hk_FixedUpdate", DynValue.Nil);
			script.Globals.Set("hk_SlowUpdate", DynValue.Nil);
			script.Globals.Set("hk_OnEnd", DynValue.Nil);
			script.Globals.Set("hk_OnSerializing", DynValue.Nil);
			script.Globals.Set("hk_OnSerialized", DynValue.Nil);
			script.Globals.Set("hk_OnDeserializing", DynValue.Nil);
			script.Globals.Set("hk_OnDeserialized", DynValue.Nil);
			script.Globals.Set("hk_OnServerMessage", DynValue.Nil);
			script.Globals.Set("coroutine.resumeWhen", DynValue.Nil);
		}
	}

	private static void CheckCoroutinesToResume()
	{
		List<MoonSharp.Interpreter.Coroutine> list = null;
		foreach (KeyValuePair<MoonSharp.Interpreter.Coroutine, Closure> sleeping_Coroutine in _sleeping_Coroutines)
		{
			if (sleeping_Coroutine.Value.Call().Boolean)
			{
				if (list == null)
				{
					list = CollectionPool.RequestList<MoonSharp.Interpreter.Coroutine>();
				}
				list.Add(sleeping_Coroutine.Key);
			}
		}
		if (list != null)
		{
			foreach (MoonSharp.Interpreter.Coroutine item in list)
			{
				_sleeping_Coroutines.Remove(item);
				item.Resume();
			}
		}
		list?.Dispose();
	}

	private static DynValue CoroutineResumeCondition(ScriptExecutionContext context, CallbackArguments args)
	{
		MoonSharp.Interpreter.Coroutine coroutine = args.AsType(0, "coroutine.resumeWhen", DataType.Thread).Coroutine;
		Closure function = args.AsType(1, "coroutine.resumeWhen", DataType.Function).Function;
		_sleeping_Coroutines.Add(coroutine, function);
		return DynValue.Void;
	}

	private static DynValue SetSaveData(ScriptExecutionContext context, CallbackArguments args)
	{
		string key = args.AsType(0, "SetSaveData", DataType.String).String;
		string key2 = args.AsType(1, "SetSaveData", DataType.String).String;
		DynValue dynValue = args.RawGet(2, translateVoids: true);
		object value = ((dynValue.Type != DataType.Table) ? dynValue.ToObject<object>() : LuaLibs.SerialiseTable(dynValue.Table));
		Dictionary<string, object> value2 = null;
		if (!_saveData.TryGetValue(key, out value2))
		{
			value2 = new Dictionary<string, object>();
			_saveData.Add(key, value2);
		}
		value2[key2] = value;
		return DynValue.Void;
	}

	private static DynValue GetSaveData(ScriptExecutionContext context, CallbackArguments args)
	{
		string key = args.AsType(0, "GetSaveData", DataType.String).String;
		string key2 = args.AsType(1, "GetSaveData", DataType.String).String;
		Dictionary<string, object> value = null;
		if (_saveData.TryGetValue(key, out value) && value.TryGetValue(key2, out var value2))
		{
			if (value2.GetType() == typeof(Dictionary<object, object>))
			{
				return DynValue.NewTable(LuaLibs.DeserialiseTable((Dictionary<object, object>)value2, context.OwnerScript));
			}
			return DynValue.FromObject(context.OwnerScript, value2);
		}
		return DynValue.Nil;
	}

	private void Awake()
	{
		sig_serverMessage.Connect(UNetManager.instance.LuaMessageHandler.BroadcastData);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (GameSignals.Count(GameSignals.LevelReady) <= 0)
		{
			return;
		}
		try
		{
			if (_sleeping_Coroutines.Count > 0)
			{
				CheckCoroutinesToResume();
			}
			if (sig_update.ConnectionCount > 0)
			{
				sig_update.Invoke();
			}
			if (sig_slowUpdate.ConnectionCount > 0 && Time.time - _timeAtLastSlowUpdate >= 1f)
			{
				_timeAtLastSlowUpdate = Time.time;
				sig_slowUpdate.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	private void FixedUpdate()
	{
		if (GameSignals.Count(GameSignals.LevelReady) <= 0)
		{
			return;
		}
		try
		{
			if (sig_fixedUpdate.ConnectionCount > 0)
			{
				sig_fixedUpdate.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	private void OnDestroy()
	{
		sig_OnEnd.Invoke();
		sig_serverMessage.Clear();
	}

	[OnSerializing]
	internal void OnSerializing(StreamingContext context)
	{
		try
		{
			if (sig_serializing.ConnectionCount > 0)
			{
				sig_serializing.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	[OnSerialized]
	internal void OnSerialized(StreamingContext context)
	{
		try
		{
			if (sig_serialized.ConnectionCount > 0)
			{
				sig_serialized.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	[OnDeserializing]
	internal void OnDeserializing(StreamingContext context)
	{
		try
		{
			if (sig_deserializing.ConnectionCount > 0)
			{
				sig_deserializing.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		try
		{
			if (sig_deserialized.ConnectionCount > 0)
			{
				sig_deserialized.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
