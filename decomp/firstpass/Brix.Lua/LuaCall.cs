using System;
using System.Reflection;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaCall : InfoContainer
{
	public LuaCall(MethodInfo info)
	{
		_state |= InfoState.IsCalled;
		_state |= InfoState.IsStatic;
		_callback = DynValue.NewCallback((Func<ScriptExecutionContext, CallbackArguments, DynValue>)Delegate.CreateDelegate(typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>), info));
	}
}
