using Brix.Lua;
using MoonSharp.Interpreter;
using Rewired;
using UnityEngine;

public class LuaInput : MonoBehaviour
{
	private static LuaObjectDescriptor<LuaInput> _desc = new LuaObjectDescriptor<LuaInput>();

	public static void RegisterClass(Script script)
	{
		script.Globals["Input"] = _desc.GetStaticProxy();
	}

	[LuaDrivable("GetAnyButton")]
	private static DynValue GetAnyButton(ScriptExecutionContext context, CallbackArguments args)
	{
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetAnyButton());
	}

	[LuaDrivable("GetAnyButtonDown")]
	private static DynValue GetAnyButtonDown(ScriptExecutionContext context, CallbackArguments args)
	{
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetAnyButtonDown());
	}

	[LuaDrivable("GetAnyButtonUp")]
	private static DynValue GetAnyButtonUp(ScriptExecutionContext context, CallbackArguments args)
	{
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetAnyButtonUp());
	}

	[LuaDrivable("GetButton")]
	private static DynValue GetButton(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetButton", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetButton(actionName));
	}

	[LuaDrivable("GetButtonDown")]
	private static DynValue GetButtonDown(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetButtonDown", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetButtonDown(actionName));
	}

	[LuaDrivable("GetButtonUp")]
	private static DynValue GetButtonUp(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetButtonUp", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetButtonUp(actionName));
	}

	[LuaDrivable("GetButtonLongPress")]
	private static DynValue GetButtonLongPress(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetButtonLongPress", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetButtonLongPress(actionName));
	}

	[LuaDrivable("GetButtonLongPressDown")]
	private static DynValue GetButtonLongPressDown(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetButtonLongPressDown", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetButtonLongPressDown(actionName));
	}

	[LuaDrivable("GetButtonLongPressUp")]
	private static DynValue GetButtonLongPressUp(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetButtonLongPressUp", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewBoolean(player.GetButtonLongPressUp(actionName));
	}

	[LuaDrivable("GetAxis")]
	private static DynValue GetAxis(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsType(0, "Input.GetAxis", DataType.String).String;
		Player player = ReInput.players.GetPlayer("local");
		return DynValue.NewNumber(player.GetAxis(actionName));
	}

	[LuaDrivable("GetMouseAxis")]
	private static DynValue GetMouseAxis(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, new Vector2(ReInput.controllers.Mouse.GetAxis(0), ReInput.controllers.Mouse.GetAxis(1)));
	}
}
