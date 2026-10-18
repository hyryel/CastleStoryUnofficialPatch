using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public static class HasHandlesLuiExtensions
{
	public static void InitHasHandles(this IHasHandlesLui _this)
	{
		_this.T.Set("Handles", DynValue.NewTable(_this.OwnerScript));
	}

	public static Table GetHandles(this IHasHandlesLui _this)
	{
		return _this.T.Get("Handles").Table;
	}
}
