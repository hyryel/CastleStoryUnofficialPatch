using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaHookBase
{
	public const int DefaultMaximumConnections = 1000;

	public static LuaObjectDescriptor<LuaHookBase> desc = new LuaObjectDescriptor<LuaHookBase>();

	private Dictionary<Closure, SignalConnection> _connections;

	private SignalConnection _mainDishook = SignalConnection.Empty;

	private SignalConnection _scriptDishook = SignalConnection.Empty;

	public static void RegisterClass(Script script)
	{
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("Connect", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => Connect(exec, args), "Connect"));
		value.Table.Set("ConnectOnce", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => ConnectOnce(exec, args), "ConnectOnce"));
		value.Table.Set("LateConnect", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => LateConnect(exec, args), "LateConnect"));
		value.Table.Set("LateConnectOnce", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => LateConnectOnce(exec, args), "LateConnectOnce"));
		value.Table.Set("Disconnect", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => Disconnect(exec, args), "Disconnect"));
		value.Table.Set("Invoke", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => Invoke(exec, args), "Invoke"));
		script.Globals.Set("Hooks", value);
		LuaHook onscriptkilled = new LuaHook();
		script.OnScriptKilled += delegate
		{
			InvokeFromCLR(onscriptkilled);
		};
		script.Globals.Set("hk_OnScriptKilled", UserData.CreateWithDescriptor(onscriptkilled, desc));
	}

	[LuaDrivable("__eq")]
	public static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaHookBase luaHookBase = args.AsUserData<LuaHookBase>(0, "LuaHook.__eq", allowNil: true);
		LuaHookBase luaHookBase2 = args.AsUserData<LuaHookBase>(1, "LuaHook.__eq", allowNil: true);
		return DynValue.NewBoolean(luaHookBase == luaHookBase2);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return base.ToString();
	}

	~LuaHookBase()
	{
		DisconnectAll();
	}

	public static DynValue Connect(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaHookBase luaHookBase = args.AsUserData<LuaHookBase>(0, "Hooks:Connect");
		Closure function = args.AsType(1, "Hooks:Connect", DataType.Function).Function;
		LuaHook dishook = args.AsUserData<LuaHook>(2, "Hooks:Connect", allowNil: true);
		luaHookBase.ConnectTemplated(function, dishook);
		return DynValue.Void;
	}

	protected virtual void ConnectTemplated(Closure act, LuaHook dishook)
	{
	}

	public static void ConnectFromCLR(LuaHookBase hook, Closure act, LuaHook dishook)
	{
		hook.ConnectTemplated(act, dishook);
	}

	public static void ConnectFromCLR(LuaHook hook, Closure act, LuaHook dishook)
	{
		Script ownerScript = act.OwnerScript;
		SignalConnection sigcon = hook._signal.Connect(delegate(Closure _act)
		{
			_act.Call();
		}, act);
		HookDisconnectWhen(sigcon, hook, act, dishook);
	}

	public static void ConnectFromCLR<T1>(LuaHook<T1> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.Connect(delegate(T1 t1, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1));
		}, act), hook, act, dishook);
	}

	public static void ConnectFromCLR<T1, T2>(LuaHook<T1, T2> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.Connect(delegate(T1 t1, T2 t2, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1), DynValue.FromObject(ownerScript, t2));
		}, act), hook, act, dishook);
	}

	public static void ConnectFromCLR<T1, T2, T3>(LuaHook<T1, T2, T3> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.Connect(delegate(T1 t1, T2 t2, T3 t3, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1), DynValue.FromObject(ownerScript, t2), DynValue.FromObject(ownerScript, t3));
		}, act), hook, act, dishook);
	}

	public static void ConnectFromCLR<T1, T2, T3, T4>(LuaHook<T1, T2, T3, T4> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.Connect(delegate(T1 t1, T2 t2, T3 t3, T4 t4, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1), DynValue.FromObject(ownerScript, t2), DynValue.FromObject(ownerScript, t3), DynValue.FromObject(ownerScript, t4));
		}, act), hook, act, dishook);
	}

	public static DynValue ConnectOnce(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaHookBase luaHookBase = args.AsUserData<LuaHookBase>(0, "Hooks:ConnectOnce");
		Closure function = args.AsType(1, "Hooks:ConnectOnce", DataType.Function).Function;
		LuaHook dishook = args.AsUserData<LuaHook>(2, "Hooks:ConnectOnce", allowNil: true);
		luaHookBase.ConnectOnceTemplated(function, dishook);
		return DynValue.Void;
	}

	protected virtual void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
	}

	public static void ConnectOnceFromCLR(LuaHookBase hook, Closure act, LuaHook dishook)
	{
		hook.ConnectOnceTemplated(act, dishook);
	}

	public static void ConnectOnceFromCLR(LuaHook hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.ConnectOnce(delegate(Closure _act)
		{
			_act.Call();
		}, act), hook, act, dishook);
	}

	public static void ConnectOnceFromCLR<T1>(LuaHook<T1> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.ConnectOnce(delegate(T1 t1, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1));
		}, act), hook, act, dishook);
	}

	public static void ConnectOnceFromCLR<T1, T2>(LuaHook<T1, T2> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.ConnectOnce(delegate(T1 t1, T2 t2, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1), DynValue.FromObject(ownerScript, t2));
		}, act), hook, act, dishook);
	}

	public static void ConnectOnceFromCLR<T1, T2, T3>(LuaHook<T1, T2, T3> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.ConnectOnce(delegate(T1 t1, T2 t2, T3 t3, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1), DynValue.FromObject(ownerScript, t2), DynValue.FromObject(ownerScript, t3));
		}, act), hook, act, dishook);
	}

	public static void ConnectOnceFromCLR<T1, T2, T3, T4>(LuaHook<T1, T2, T3, T4> hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.ConnectOnce(delegate(T1 t1, T2 t2, T3 t3, T4 t4, Closure _act)
		{
			Script ownerScript = _act.OwnerScript;
			_act.Call(DynValue.FromObject(ownerScript, t1), DynValue.FromObject(ownerScript, t2), DynValue.FromObject(ownerScript, t3), DynValue.FromObject(ownerScript, t4));
		}, act), hook, act, dishook);
	}

	public static DynValue LateConnect(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaHook hook = args.AsUserData<LuaHook>(0, "Hooks:LateConnect");
		Closure function = args.AsType(1, "Hooks:LateConnect", DataType.Function).Function;
		LuaHook dishook = args.AsUserData<LuaHook>(2, "Hooks:LateConnect", allowNil: true);
		LateConnectFromCLR(hook, function, dishook);
		return DynValue.Void;
	}

	public static void LateConnectFromCLR(LuaHook hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.LateConnect(delegate
		{
			act.Call();
		}), hook, act, dishook);
	}

	public static DynValue LateConnectOnce(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaHook hook = args.AsUserData<LuaHook>(0, "Hooks:LateConnectOnce");
		Closure function = args.AsType(1, "Hooks:LateConnectOnce", DataType.Function).Function;
		LuaHook dishook = args.AsUserData<LuaHook>(2, "Hooks:LateConnectOnce", allowNil: true);
		LateConnectOnceFromCLR(hook, function, dishook);
		return DynValue.Void;
	}

	public static void LateConnectOnceFromCLR(LuaHook hook, Closure act, LuaHook dishook)
	{
		HookDisconnectWhen(hook._signal.LateConnectOnce(delegate
		{
			act.Call();
		}), hook, act, dishook);
	}

	public static DynValue Disconnect(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaHookBase hook = args.AsUserData<LuaHookBase>(0, "Hooks:Disconnect");
		Closure function = args.AsType(1, "Hooks:Disconnect", DataType.Function).Function;
		DisconnectFromCLR(hook, function);
		return DynValue.Void;
	}

	public static void DisconnectFromCLR(LuaHookBase hook, Closure act)
	{
		if (hook._connections != null)
		{
			KeyValuePair<Closure, SignalConnection> andRemove = hook._connections.GetAndRemove(act);
			if (andRemove.Value != SignalConnection.Empty)
			{
				andRemove.Value.Disconnect();
			}
		}
	}

	public void DisconnectAll()
	{
		if (_connections == null)
		{
			return;
		}
		foreach (KeyValuePair<Closure, SignalConnection> connection in _connections)
		{
			connection.Value.Disconnect();
		}
		_connections.Clear();
	}

	private static void HookDisconnectWhen(SignalConnection sigcon, LuaHookBase hook, Closure act, LuaHook dishook)
	{
		if (dishook != null)
		{
			hook._mainDishook = dishook._signal.ConnectOnce(delegate(LuaHookBase _hook, Closure _act)
			{
				DisconnectFromCLR(_hook, _act);
				if (_hook._scriptDishook != SignalConnection.Empty)
				{
					_hook._scriptDishook.Disconnect();
				}
			}, hook, act);
		}
		LuaHook luaHook = act.OwnerScript.Globals.Get("hk_OnScriptKilled").ToObject<LuaHook>();
		if (luaHook != null)
		{
			hook._scriptDishook = luaHook._signal.ConnectOnce(delegate(LuaHookBase _hook, Closure _act)
			{
				DisconnectFromCLR(_hook, _act);
				if (_hook._mainDishook != SignalConnection.Empty)
				{
					_hook._mainDishook.Disconnect();
				}
			}, hook, act);
		}
		if (hook._connections == null)
		{
			hook._connections = new Dictionary<Closure, SignalConnection>(8);
		}
		hook._connections[act] = sigcon;
	}

	public static DynValue Invoke(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaHookBase luaHookBase = args.AsUserData<LuaHookBase>(0, "Hooks:Invoke");
		Table table = args.AsType(1, "Hooks:Invoke", DataType.Table, allowNil: true).Table;
		luaHookBase.InvokeTemplated(table);
		return DynValue.Void;
	}

	protected virtual void InvokeTemplated(Table args)
	{
	}

	public static void InvokeFromCLR(LuaHook hook)
	{
		try
		{
			Signal signal = hook._signal;
			if (signal.ConnectionCount != 0)
			{
				signal.Invoke();
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	public static void InvokeFromCLR<T1>(LuaHook<T1> hook, T1 arg1)
	{
		try
		{
			Signal<T1> signal = hook._signal;
			if (signal.ConnectionCount != 0)
			{
				signal.Invoke(arg1);
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	public static void InvokeFromCLR<T1, T2>(LuaHook<T1, T2> hook, T1 arg1, T2 arg2)
	{
		try
		{
			Signal<T1, T2> signal = hook._signal;
			if (signal.ConnectionCount != 0)
			{
				signal.Invoke(arg1, arg2);
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	public static void InvokeFromCLR<T1, T2, T3>(LuaHook<T1, T2, T3> hook, T1 arg1, T2 arg2, T3 arg3)
	{
		try
		{
			Signal<T1, T2, T3> signal = hook._signal;
			if (signal.ConnectionCount != 0)
			{
				signal.Invoke(arg1, arg2, arg3);
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	public static void InvokeFromCLR<T1, T2, T3, T4>(LuaHook<T1, T2, T3, T4> hook, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
	{
		try
		{
			Signal<T1, T2, T3, T4> signal = hook._signal;
			if (signal.ConnectionCount != 0)
			{
				signal.Invoke(arg1, arg2, arg3, arg4);
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	public static void InvokeFromCLR<T1, T2, T3, T4, T5>(LuaHook<T1, T2, T3, T4, T5> hook, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
	{
		try
		{
			Signal<T1, T2, T3, T4, T5> signal = hook._signal;
			if (signal.ConnectionCount != 0)
			{
				signal.Invoke(arg1, arg2, arg3, arg4, arg5);
			}
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
	}

	public static void InvokeWithDynValues<T1>(LuaHook<T1> hook, DynValue arg1)
	{
		InvokeFromCLR(hook, arg1.ToObject<T1>());
	}

	public static void InvokeWithDynValues<T1, T2>(LuaHook<T1, T2> hook, DynValue arg1, DynValue arg2)
	{
		InvokeFromCLR(hook, arg1.ToObject<T1>(), arg1.ToObject<T2>());
	}

	public static void InvokeWithDynValues<T1, T2, T3>(LuaHook<T1, T2, T3> hook, DynValue arg1, DynValue arg2, DynValue arg3)
	{
		InvokeFromCLR(hook, arg1.ToObject<T1>(), arg2.ToObject<T2>(), arg3.ToObject<T3>());
	}

	public static void InvokeWithDynValues<T1, T2, T3, T4>(LuaHook<T1, T2, T3, T4> hook, DynValue arg1, DynValue arg2, DynValue arg3, DynValue arg4)
	{
		InvokeFromCLR(hook, arg1.ToObject<T1>(), arg2.ToObject<T2>(), arg3.ToObject<T3>(), arg4.ToObject<T4>());
	}

	public static void InvokeWithDynValues<T1, T2, T3, T4, T5>(LuaHook<T1, T2, T3, T4, T5> hook, DynValue arg1, DynValue arg2, DynValue arg3, DynValue arg4, DynValue arg5)
	{
		InvokeFromCLR(hook, arg1.ToObject<T1>(), arg2.ToObject<T2>(), arg3.ToObject<T3>(), arg4.ToObject<T4>(), arg5.ToObject<T5>());
	}
}
