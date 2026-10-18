using Brix.Lua;
using Brix.UI.Builder.Menu;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Brix.UI.Builder.Lui;

public static class HasConnectionsLuiExtensions
{
	public static void InitHasConnections(this IHasConnectionsLui _this)
	{
		_this.T.Set("Connections", DynValue.NewTable(_this.OwnerScript));
		_this.T.Set("ConnectWhen", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.ConnectWhen(con, args)));
		_this.T.Set("DisconnectAll", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.DisconnectAll()));
	}

	public static DynValue ConnectWhen(this IHasConnectionsLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		LuaHookBase o = args.AsUserData<LuaHookBase>(0, "ConnectWhen");
		Closure function = args.AsType(1, "ConnectWhen", DataType.Function).Function;
		LuaEnumProxy<BaseMenu.MenuState> o2 = args.AsUserData<LuaEnumProxy<BaseMenu.MenuState>>(2, "ConnectWhen");
		DynValue value = DynValue.NewTable(new Table(_this.OwnerScript));
		value.Table.Set("Hook", UserData.CreateWithDescriptor(o, LuaHookBase.desc));
		value.Table.Set("Callback", DynValue.NewClosure(function));
		value.Table.Set("MenuEvent", UserData.Create(o2));
		_this.T.Get("Connections").Table.Append(value);
		return _this.D;
	}

	public static DynValue DisconnectAll(this IHasConnectionsLui _this)
	{
		_this.T.Get("Connections").Table.Clear();
		return _this.D;
	}
}
