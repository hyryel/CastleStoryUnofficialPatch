using Brix.Utils.Logging;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class HasMenusLuiLuiExtensions
{
	public static BrixLogger logger = new BrixLogger(typeof(HasMenusLuiLuiExtensions).FullName);

	public static void InitHasMenus(this IHasMenusLui _this)
	{
		_this.T.Set("SetMenu", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.SetMenu(con, args)));
		_this.T.Set("ClearMenu", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.ClearMenu(con, args)));
		_this.T.Set("Menus", DynValue.NewTable(_this.OwnerScript));
		_this.T.Set("AddMenu", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.AddMenu(con, args)));
		_this.T.Set("RemoveAllMenus", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.RemoveAllMenus(con, args)));
	}

	public static DynValue SetMenu(this IHasMenusLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = args.AsTable(0, "SetMenu");
		if (LuiBuilder.IsDebug)
		{
			logger.Info("LuaLayoutPanelTable#" + _this.Id + ".SetMenuTable(#" + table.ReferenceID + ")\n");
		}
		if (LuiBuilder.IsDebug)
		{
			logger.Info("LuaMenuTable#" + table.ReferenceID + ".Flags.Count = " + table.Get("Flags").Table.Length + "\nIHasMenusLui.SetMenu()");
		}
		_this.T.Set("Menu", DynValue.NewTable(table));
		return _this.D;
	}

	public static DynValue ClearMenu(this IHasMenusLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		_this.T.Set("Menu", DynValue.Nil);
		return _this.D;
	}

	public static DynValue AddMenu(this IHasMenusLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = args.AsTable(0, "AddMenu");
		if (LuiBuilder.IsDebug)
		{
			logger.Info("LuaMenuTable#" + _this.Id + ".AddMenuTable(#" + table.ReferenceID + ")\n");
		}
		if (LuiBuilder.IsDebug)
		{
			logger.Info("LuaMenuTable#" + table.ReferenceID + ".Flags.Count = " + table.Get("Flags").Table.Length + "\nIHasMenusLui.AddMenu()");
		}
		Table table2 = _this.T.Get("Menus").Table;
		table2.Append(DynValue.NewTable(table));
		return _this.D;
	}

	public static DynValue RemoveAllMenus(this IHasMenusLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		_this.T.Get("Menus").Table.Clear();
		return _this.D;
	}
}
