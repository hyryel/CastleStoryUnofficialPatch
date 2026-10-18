using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class HasPanelsLuiExtensions
{
	public static void InitHasPanels(this IHasPanelsLui _this)
	{
		_this.T.Set("Panels", DynValue.NewTable(_this.OwnerScript));
		_this.T.Set("AddPanel", DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => _this.AddPanel(con, args)));
	}

	public static DynValue AddPanel(this IHasPanelsLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = args.AsTable(0, "AddPanel");
		if (table.Equals(DynValue.Nil))
		{
			return _this.D;
		}
		Table table2 = _this.T.Get("Panels").Table;
		table2.Append(DynValue.NewTable(table));
		return _this.D;
	}
}
