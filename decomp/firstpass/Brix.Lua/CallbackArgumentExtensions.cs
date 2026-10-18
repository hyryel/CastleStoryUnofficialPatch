using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

namespace Brix.Lua;

public static class CallbackArgumentExtensions
{
	public static T AsEnum<T>(this CallbackArguments args, int argNum, string funcName, bool allowNil = false) where T : struct
	{
		return args.AsUserData<LuaEnumProxy<T>>(argNum, funcName, allowNil).value;
	}

	public static T AsComponent<T>(this CallbackArguments args, int argNum, string funcName, bool allowNil = false) where T : Component
	{
		return args.AsUserData<ILuaDriverDescribable>(argNum, funcName, allowNil).CachedGetComponent<T>();
	}

	public static GameObject AsGameObject(this CallbackArguments args, int argNum, string funcName, bool allowNil = false)
	{
		return args.AsUserData<LuaDriver>(argNum, funcName, allowNil).gameObject;
	}
}
