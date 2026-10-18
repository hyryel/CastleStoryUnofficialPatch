using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.UI.Builder.Lui;

public static class IsMenuLuiExtensions
{
	public static void InitIsMenu(this IIsMenuLui _this)
	{
		_this.T.Set("Reload", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.Reload(con, args)));
		_this.T.Set("Open", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.Open(con, args)));
		_this.T.Set("Reopen", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.Reopen(con, args)));
		_this.T.Set("Close", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.Close(con, args)));
		_this.T.Set("Refresh", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.Refresh(con, args)));
		_this.T.Set("hk_OnNew", UserData.CreateWithDescriptor(new LuaHook(), LuaHookBase.desc));
		_this.T.Set("hk_OnLoad", UserData.CreateWithDescriptor(new LuaHook(), LuaHookBase.desc));
		_this.T.Set("hk_OnOpen", UserData.CreateWithDescriptor(new LuaHook(), LuaHookBase.desc));
	}

	public static DynValue Open(this IIsMenuLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (!LuiBuilder.MapHelper.HasMenu(_this.T))
		{
			Debug.LogWarning("Menu not yet instantiated.\n");
			return DynValue.Void;
		}
		LuiBuilder.MapHelper.GetMenu(_this.T).Open();
		return DynValue.Void;
	}

	public static DynValue Reopen(this IIsMenuLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (!LuiBuilder.MapHelper.HasMenu(_this.T))
		{
			Debug.LogWarning("Menu not yet instantiated.\n");
			return DynValue.Void;
		}
		LuiBuilder.MapHelper.GetMenu(_this.T).Reopen();
		return DynValue.Void;
	}

	public static DynValue Close(this IIsMenuLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (!LuiBuilder.MapHelper.HasMenu(_this.T))
		{
			Debug.LogWarning("Menu not yet instantiated.\n");
			return DynValue.Void;
		}
		LuiBuilder.MapHelper.GetMenu(_this.T).Close();
		return DynValue.Void;
	}

	public static DynValue Reload(this IIsMenuLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (!LuiBuilder.MapHelper.HasMenu(_this.T))
		{
			Debug.LogWarning("Menu not yet instantiated.\n");
			return DynValue.Void;
		}
		LuiBuilder.MapHelper.GetMenu(_this.T).Reload();
		return DynValue.Void;
	}

	public static DynValue Refresh(this IIsMenuLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (!LuiBuilder.MapHelper.HasMenu(_this.T))
		{
			Debug.LogWarning("Menu not yet instantiated.\n");
			return DynValue.Void;
		}
		LuiBuilder.MapHelper.GetMenu(_this.T).Refresh();
		return DynValue.Void;
	}
}
