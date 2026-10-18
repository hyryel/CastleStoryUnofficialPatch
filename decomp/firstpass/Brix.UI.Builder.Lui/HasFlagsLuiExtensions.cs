using Brix.Lua;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class HasFlagsLuiExtensions
{
	public static BrixLogger logger = new BrixLogger(typeof(HasFlagsLuiExtensions).FullName);

	public static void InitHasFlags(this IHasBuilderFlagsLui _this)
	{
		_this.T.Set("Flags", DynValue.NewTable(_this.OwnerScript));
		_this.T.Set("SetFlag", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.SetFlag(con, args)));
		_this.T.Set("ClearFlag", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.ClearFlag(con, args)));
		_this.T.Set("ClearAllFlags", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.ClearAllFlags(con, args)));
	}

	public static DynValue SetFlag(this IHasBuilderFlagsLui _this, ScriptExecutionContext sec, CallbackArguments ca)
	{
		BaseMenu.BuilderFlag builderFlag = ca.AsEnum<BaseMenu.BuilderFlag>(0, "flag");
		DynValue value = ca[1];
		if (value.Type != DataType.Function)
		{
			value = DynValue.True;
		}
		if (LuiBuilder.IsDebug)
		{
			logger.Info(string.Concat("LuaMenuTable#", _this.Id, ".SetFlag(", builderFlag, ")\n"));
		}
		Table table = _this.T.Get("Flags").Table;
		table.Set(builderFlag, value);
		if (LuiBuilder.IsDebug)
		{
			logger.Info("LuaMenuTable#" + _this.Id + ".Flags.Count = " + _this.T.Get("Flags").Table.Length + "\nIHasBuilderFlagsLui.SetFlag()");
		}
		return _this.D;
	}

	public static DynValue ClearFlag(this IHasBuilderFlagsLui _this, ScriptExecutionContext sec, CallbackArguments ca)
	{
		BaseMenu.BuilderFlag builderFlag = ca.AsEnum<BaseMenu.BuilderFlag>(0, "flag");
		if (LuiBuilder.IsDebug)
		{
			logger.Info(string.Concat("LuaMenuTable#", _this.Id, ".ClearFlag(", builderFlag, ")\n"));
		}
		Table table = _this.T.Get("Flags").Table;
		table.Remove(builderFlag);
		if (LuiBuilder.IsDebug)
		{
			logger.Info("LuaMenuTable#" + _this.Id + ".Flags.Count = " + _this.T.Get("Flags").Table.Length + "\nIHasBuilderFlagsLui.ClearFlag()");
		}
		return _this.D;
	}

	public static DynValue ClearAllFlags(this IHasBuilderFlagsLui _this, ScriptExecutionContext sec, CallbackArguments ca)
	{
		Table table = _this.T.Get("Flags").Table;
		table.Clear();
		return _this.D;
	}
}
