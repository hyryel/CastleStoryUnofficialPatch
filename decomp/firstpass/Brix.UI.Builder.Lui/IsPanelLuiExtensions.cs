using Brix.UI.Builder.Menu;
using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class IsPanelLuiExtensions
{
	public static void InitIsPanel(this IIsPanelLui _this)
	{
		_this.T.Set("Refresh", DynValue.NewCallback((ScriptExecutionContext cont, CallbackArguments args) => _this.Refresh(cont, args)));
	}

	public static DynValue Refresh(this IIsPanelLui _this, ScriptExecutionContext context, CallbackArguments args)
	{
		if (!LuiBuilder.MapHelper.HasPanel(_this.T))
		{
			return DynValue.Void;
		}
		LuiBuilder.MapHelper.GetPanel(_this.T).QueueRefresh();
		return DynValue.Void;
	}
}
